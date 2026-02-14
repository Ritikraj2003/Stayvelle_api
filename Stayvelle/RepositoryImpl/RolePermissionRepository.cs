using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.RepositoryImpl
{
    public class RolePermissionRepository : IRolePermission
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<bool>> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds, string createdBy)
        {
            var response = new Response<bool>();
            try
            {
                // Get existing permissions for this role
                var existingRolePermissions = await _context.RolePermissionModel
                    .Where(rp => rp.RoleId == roleId && !rp.isdelete)
                    .ToListAsync();

                // Soft delete all existing permissions
                foreach (var existing in existingRolePermissions)
                {
                    existing.isdelete = true;
                    existing.isactive = false;
                }

                // Add new permissions
                foreach (var permissionId in permissionIds.Distinct())
                {
                    // Check if permission exists
                    var permission = await _context.PermissionModel
                        .FirstOrDefaultAsync(p => p.Id == permissionId && !p.isdelete);

                    if (permission != null)
                    {
                        // Check if this mapping already exists (even if deleted)
                        var existingMapping = existingRolePermissions
                            .FirstOrDefault(rp => rp.PermissionId == permissionId);

                        if (existingMapping != null)
                        {
                            // Reactivate existing mapping
                            existingMapping.isdelete = false;
                            existingMapping.isactive = true;
                            existingMapping.ModifiedBy = createdBy;
                            existingMapping.ModifiedOn = DateTime.UtcNow;
                        }
                        else
                        {
                            // Create new mapping
                            var rolePermission = new RolePermissionModel
                            {
                                RoleId = roleId,
                                PermissionId = permissionId,
                                isactive = true,
                                isdelete = false,
                                CreatedBy = createdBy,
                                CreatedOn = DateTime.UtcNow
                            };
                            _context.RolePermissionModel.Add(rolePermission);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                response.Success = true;
                response.Message = "Permissions assigned successfully";
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = false;
                return response;
            }
        }

        public async Task<Response<bool>> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds, string modifiedBy)
        {
            return await AssignPermissionsToRoleAsync(roleId, permissionIds, modifiedBy);
        }

        public async Task<Response<List<RolePermissionModel>>> GetRolePermissionsByRoleIdAsync(int roleId)
        {
            var response = new Response<List<RolePermissionModel>>();
            try
            {
                var rolePermissions = await _context.RolePermissionModel
                    .Where(rp => rp.RoleId == roleId && !rp.isdelete && rp.isactive)
                    .Include(rp => rp.Permission)
                    .Include(rp => rp.Role)
                    .ToListAsync();

                response.Success = true;
                response.Message = "Success";
                response.Data = rolePermissions;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<Response<bool>> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var response = new Response<bool>();
            try
            {
                var rolePermission = await _context.RolePermissionModel
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.isdelete);

                if (rolePermission == null)
                {
                    response.Success = false;
                    response.Message = "Role permission not found";
                    response.Data = false;
                    return response;
                }

                rolePermission.isdelete = true;
                rolePermission.isactive = false;
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "Permission removed successfully";
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = false;
                return response;
            }
        }
    }
}

