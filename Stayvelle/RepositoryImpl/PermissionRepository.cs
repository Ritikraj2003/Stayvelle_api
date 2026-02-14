using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.RepositoryImpl
{
    public class PermissionRepository : IPermission
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<PermissionModel>> CreatePermissionAsync(PermissionModel permission)
        {
            var response = new Response<PermissionModel>();
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(permission.permission_name))
                {
                    response.Success = false;
                    response.Message = "Permission name is required";
                    response.Data = null;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(permission.permission_code))
                {
                    response.Success = false;
                    response.Message = "Permission code is required";
                    response.Data = null;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(permission.CreatedBy))
                {
                    response.Success = false;
                    response.Message = "CreatedBy is required";
                    response.Data = null;
                    return response;
                }

                _context.PermissionModel.Add(permission);
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "Permission created successfully";
                response.Data = permission;
                return response;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                response.Success = false;
                response.Message = $"Database error: {dbEx.Message}. Inner exception: {dbEx.InnerException?.Message ?? "No inner exception"}";
                response.Data = null;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}. Inner exception: {ex.InnerException?.Message ?? "No inner exception"}";
                response.Data = null;
                return response;
            }
        }

        public async Task<Response<List<PermissionModel>>> GetAllPermissionsAsync()
        {
            var response = new Response<List<PermissionModel>>();
            try
            {
                var permissions = await _context.PermissionModel
                    .Where(p => !p.isdelete)
                    .ToListAsync();
                
                response.Success = true;
                response.Message = "Success";
                response.Data = permissions;
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

        public async Task<Response<PermissionModel?>> GetPermissionByIdAsync(int id)
        {
            var response = new Response<PermissionModel?>();
            try
            {
                var permission = await _context.PermissionModel
                    .FirstOrDefaultAsync(p => p.Id == id && !p.isdelete);

                if (permission == null)
                {
                    response.Success = false;
                    response.Message = "Permission not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = permission;
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

        public async Task<Response<List<PermissionModel>>> GetPermissionsByRoleIdAsync(int roleId)
        {
            var response = new Response<List<PermissionModel>>();
            try
            {
                var permissions = await _context.RolePermissionModel
                    .Where(rp => rp.RoleId == roleId && !rp.isdelete && rp.isactive)
                    .Include(rp => rp.Permission)
                    .Select(rp => rp.Permission)
                    .Where(p => p != null && !p.isdelete)
                    .ToListAsync();

                response.Success = true;
                response.Message = "Success";
                response.Data = permissions;
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

        public async Task<Response<PermissionModel?>> UpdatePermissionAsync(int id, PermissionModel permission)
        {
            var response = new Response<PermissionModel?>();
            try
            {
                var existingPermission = await _context.PermissionModel.FindAsync(id);
                if (existingPermission == null || existingPermission.isdelete)
                {
                    response.Success = false;
                    response.Message = "Permission not found";
                    response.Data = null;
                    return response;
                }

                existingPermission.permission_name = permission.permission_name;
                existingPermission.permission_code = permission.permission_code;
                existingPermission.ModifiedBy = permission.ModifiedBy;
                existingPermission.ModifiedOn = permission.ModifiedOn;

                await _context.SaveChangesAsync();
                return await GetPermissionByIdAsync(id);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            try
            {
                var permission = await _context.PermissionModel.FindAsync(id);
                if (permission == null) return false;

                _context.PermissionModel.Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeletePermissionAsync(int id)
        {
            try
            {
                var permission = await _context.PermissionModel.FindAsync(id);
                if (permission == null || permission.isdelete) return false;

                permission.isdelete = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

