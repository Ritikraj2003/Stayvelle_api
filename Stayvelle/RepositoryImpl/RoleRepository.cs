using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.RepositoryImpl
{
    public class RoleRepository : IRole
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // Helper to map RoleModel (with permissions loaded) to RoleResponseDTO
        private RoleResponseDTO MapToDTO(RoleModel role)
        {
            return new RoleResponseDTO
            {
                Id = role.Id,
                role_name = role.role_name,
                isactive = role.isactive,
                Permissions = role.RolePermissions?
                    .Where(rp => !rp.isdelete && rp.isactive && rp.Permission != null)
                    .Select(rp => new PermissionDTO
                    {
                        Id = rp.Permission.Id,
                        Name = rp.Permission.permission_name,
                        Module = rp.Permission.permission_code,
                        // Assuming Description/Action might not be in PermissionModel or mapping from existing fields
                        Description = string.Empty, 
                        Action = string.Empty
                    }).ToList() ?? new List<PermissionDTO>()
            };
        }

        public async Task<Response<RoleResponseDTO>> CreateRoleAsync(CreateRoleDTO createRoleDTO)
        {
            Response<RoleResponseDTO> response = new Response<RoleResponseDTO>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if role name exists
                var existingRole = await _context.RoleModel
                    .FirstOrDefaultAsync(r => r.role_name == createRoleDTO.role_name && !r.isdelete);
                
                if (existingRole != null)
                {
                    response.Success = false;
                    response.Message = "Role with this name already exists";
                    return response;
                }

                var role = new RoleModel
                {
                    role_name = createRoleDTO.role_name,
                    isactive = createRoleDTO.isactive,
                    isdelete = false,
                    CreatedBy = createRoleDTO.CreatedBy,
                    CreatedOn = DateTime.UtcNow
                };

                _context.RoleModel.Add(role);
                await _context.SaveChangesAsync();

                // Assign permissions if any
                if (createRoleDTO.PermissionIds != null && createRoleDTO.PermissionIds.Count > 0)
                {
                    var rolePermissions = createRoleDTO.PermissionIds.Select(pId => new RolePermissionModel
                    {
                        RoleId = role.Id, // Use RoleId property
                        PermissionId = pId, // Use PermissionId property
                        CreatedBy = createRoleDTO.CreatedBy,
                        CreatedOn = DateTime.UtcNow,
                        isactive = true,
                        isdelete = false
                    }).ToList();

                    _context.RolePermissionModel.AddRange(rolePermissions);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Fetch created role with permissions to return full DTO
                return await GetRoleByIdAsync(role.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<Response<List<RoleResponseDTO>>> GetAllRolesAsync(string? search = null)
        {
            var response = new Response<List<RoleResponseDTO>>();
            try
            {
                var query = _context.RoleModel
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .Where(r => !r.isdelete);

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(r => r.role_name.ToLower().Contains(search) || r.Id.ToString().Contains(search));
                }

                var roles = await query
                    .OrderBy(r => r.role_name)
                    .ToListAsync();
                
                var roleDTOs = roles.Select(role => MapToDTO(role)).ToList();

                response.Success = true;
                response.Message = "Success";
                response.Data = roleDTOs;
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

        public async Task<Response<RoleResponseDTO?>> GetRoleByIdAsync(int id)
        {
            var response = new Response<RoleResponseDTO?>();
            try
            {
                var role = await _context.RoleModel
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.isdelete);

                if (role == null)
                {
                    response.Success = false;
                    response.Message = "Role not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = MapToDTO(role);
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

        public async Task<Response<RoleResponseDTO?>> GetRoleByNameAsync(string name)
        {
            var response = new Response<RoleResponseDTO?>();
            try
            {
                var role = await _context.RoleModel
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.role_name == name && !r.isdelete);

                response.Success = true;
                response.Message = "Success";
                response.Data = role != null ? MapToDTO(role) : null;
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

        public async Task<Response<RoleResponseDTO?>> UpdateRoleAsync(int id, UpdateRoleDTO updateRoleDTO)
        {
            var response = new Response<RoleResponseDTO?>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRole = await _context.RoleModel.FindAsync(id);
                if (existingRole == null || existingRole.isdelete)
                {
                    response.Success = false;
                    response.Message = "Role not found";
                    response.Data = null;
                    return response;
                }

                // Check naming conflict if name changed
                if (!string.IsNullOrEmpty(updateRoleDTO.role_name) && updateRoleDTO.role_name != existingRole.role_name)
                {
                     var nameExists = await _context.RoleModel
                        .AnyAsync(r => r.role_name == updateRoleDTO.role_name && !r.isdelete && r.Id != id);
                     if (nameExists)
                     {
                        response.Success = false;
                        response.Message = "Role with this name already exists";
                        return response;
                     }
                }

                // Update fields
                existingRole.role_name = updateRoleDTO.role_name ?? existingRole.role_name;
                existingRole.isactive = updateRoleDTO.isactive ?? existingRole.isactive;
                existingRole.ModifiedBy = updateRoleDTO.ModifiedBy;
                existingRole.ModifiedOn = DateTime.UtcNow;

                _context.RoleModel.Update(existingRole);
                
                // Update permissions if provided
                if (updateRoleDTO.PermissionIds != null)
                {
                    // Existing permissions
                    var existingPermissions = await _context.RolePermissionModel
                        .Where(rp => rp.RoleId == id && !rp.isdelete)
                        .ToListAsync();
                    
                    // Soft delete all existing permissions
                    foreach(var perm in existingPermissions)
                    {
                        perm.isdelete = true;
                        perm.isactive = false;
                        perm.ModifiedBy = updateRoleDTO.ModifiedBy;
                        perm.ModifiedOn = DateTime.UtcNow;
                    }
                    
                    // Add new ones
                    var newPermissions = updateRoleDTO.PermissionIds.Select(pId => new RolePermissionModel
                    {
                        RoleId = id,
                        PermissionId = pId,
                        CreatedBy = updateRoleDTO.ModifiedBy, 
                        CreatedOn = DateTime.UtcNow,
                        isactive = true,
                        isdelete = false
                    }).ToList();
                    
                    _context.RolePermissionModel.AddRange(newPermissions);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetRoleByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }
        
        public async Task<bool> DeleteRoleAsync(int id)
        {
             // ... existing delete logic ...
             try
             {
                 var role = await _context.RoleModel.FindAsync(id);
                 if (role == null) return false;

                 _context.RoleModel.Remove(role);
                 await _context.SaveChangesAsync();
                 return true;
             }
             catch
             {
                 return false;
             }
        }

        public async Task<bool> SoftDeleteRoleAsync(int id)
        {
             // ... existing soft delete logic ...
             try
             {
                 var role = await _context.RoleModel.FindAsync(id);
                 if (role == null || role.isdelete) return false;

                 role.isdelete = true;
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

