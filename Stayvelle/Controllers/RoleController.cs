using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRole _roleRepository;
        private readonly IRolePermission _rolePermissionRepository;
        private readonly IPermission _permissionRepository;
        private readonly ApplicationDbContext _context;

        public RoleController(
            IRole roleRepository,
            IRolePermission rolePermissionRepository,
            IPermission permissionRepository,
            ApplicationDbContext context)
        {
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
            _context = context;
        }

        // GET: api/Role
        [HttpGet]
        public async Task<ActionResult<List<RoleResponseDTO>>> GetAllRoles([FromQuery] string? search = null)
        {
            var response = await _roleRepository.GetAllRolesAsync();
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }

            var roles = response.Data;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                roles = roles.Where(r => 
                    r.role_name.ToLower().Contains(search) || 
                    r.Id.ToString().Contains(search)
                ).ToList();
            }

            // Get permissions for each role
            var roleResponseList = new List<RoleResponseDTO>();
            foreach (var role in roles)
            {
                var permissionsResponse = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);
                var permissions = permissionsResponse.Data ?? new List<PermissionModel>();

                roleResponseList.Add(new RoleResponseDTO
                {
                    Id = role.Id,
                    role_name = role.role_name,
                    isactive = role.isactive,
                    Permissions = permissions.Select(p => new PermissionDTO
                    {
                        Id = p.Id,
                        Name = p.permission_name,
                        Description = string.Empty,
                        Module = p.permission_code,
                        Action = string.Empty
                    }).ToList()
                });
            }

            return Ok(roleResponseList);
        }

        // GET: api/Role/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleResponseDTO>> GetRole(int id)
        {
            var response = await _roleRepository.GetRoleByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Role with ID {id} not found" });
            }

            var role = response.Data;
            var permissionsResponse = await _permissionRepository.GetPermissionsByRoleIdAsync(id);
            var permissions = permissionsResponse.Data ?? new List<PermissionModel>();

            var roleResponse = new RoleResponseDTO
            {
                Id = role.Id,
                role_name = role.role_name,
                isactive = role.isactive,
                Permissions = permissions.Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    Name = p.permission_name,
                    Description = string.Empty,
                    Module = p.permission_code,
                    Action = string.Empty
                }).ToList()
            };

            return Ok(roleResponse);
        }

        // POST: api/Role
        [HttpPost]
        public async Task<ActionResult<RoleResponseDTO>> CreateRole([FromBody] CreateRoleDTO createRoleDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if role name already exists
            var existingRoleResponse = await _roleRepository.GetRoleByNameAsync(createRoleDTO.role_name);
            if (existingRoleResponse.Success && existingRoleResponse.Data != null)
            {
                return Conflict(new { message = "Role with this name already exists" });
            }

            // Create role
            var role = new RoleModel
            {
                role_name = createRoleDTO.role_name,
                isactive = createRoleDTO.isactive,
                isdelete = false,
                CreatedBy = createRoleDTO.CreatedBy,
                CreatedOn = DateTime.UtcNow
            };

            var roleResponse = await _roleRepository.CreateRoleAsync(role);
            if (!roleResponse.Success || roleResponse.Data == null)
            {
                return BadRequest(new { message = roleResponse.Message });
            }

            var createdRole = roleResponse.Data;

            // Assign permissions to role
            if (createRoleDTO.PermissionIds != null && createRoleDTO.PermissionIds.Count > 0)
            {
                var assignResponse = await _rolePermissionRepository.AssignPermissionsToRoleAsync(
                    createdRole.Id,
                    createRoleDTO.PermissionIds,
                    createRoleDTO.CreatedBy
                );

                if (!assignResponse.Success)
                {
                    // Role created but permissions failed - you might want to handle this differently
                    return BadRequest(new { message = $"Role created but failed to assign permissions: {assignResponse.Message}" });
                }
            }

            // Get the created role with permissions
            var getRoleResponse = await GetRole(createdRole.Id);
            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, getRoleResponse.Value);
        }

        // PUT: api/Role/5
        [HttpPut("{id}")]
        public async Task<ActionResult<RoleResponseDTO>> UpdateRole(int id, [FromBody] UpdateRoleDTO updateRoleDTO)
        {
            var existingRoleResponse = await _roleRepository.GetRoleByIdAsync(id);
            if (!existingRoleResponse.Success || existingRoleResponse.Data == null)
            {
                return NotFound(new { message = existingRoleResponse.Message ?? $"Role with ID {id} not found" });
            }

            var existingRole = existingRoleResponse.Data;

            // Check if name is being changed and already exists
            if (!string.IsNullOrEmpty(updateRoleDTO.role_name) && updateRoleDTO.role_name != existingRole.role_name)
            {
                var nameCheckResponse = await _roleRepository.GetRoleByNameAsync(updateRoleDTO.role_name);
                if (nameCheckResponse.Success && nameCheckResponse.Data != null && nameCheckResponse.Data.Id != id)
                {
                    return Conflict(new { message = "Role with this name already exists" });
                }
            }

            // Update role
            var roleToUpdate = new RoleModel
            {
                Id = id,
                role_name = updateRoleDTO.role_name ?? existingRole.role_name,
                isactive = updateRoleDTO.isactive ?? existingRole.isactive,
                isdelete = existingRole.isdelete,
                ModifiedBy = updateRoleDTO.ModifiedBy,
                ModifiedOn = DateTime.UtcNow
            };

            var updateResponse = await _roleRepository.UpdateRoleAsync(id, roleToUpdate);
            if (!updateResponse.Success || updateResponse.Data == null)
            {
                return BadRequest(new { message = updateResponse.Message });
            }

            // Update permissions if provided
            if (updateRoleDTO.PermissionIds != null)
            {
                var assignResponse = await _rolePermissionRepository.UpdateRolePermissionsAsync(
                    id,
                    updateRoleDTO.PermissionIds,
                    updateRoleDTO.ModifiedBy
                );

                if (!assignResponse.Success)
                {
                    return BadRequest(new { message = $"Role updated but failed to update permissions: {assignResponse.Message}" });
                }
            }

            // Get the updated role with permissions
            return await GetRole(id);
        }

        // DELETE: api/Role/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            var result = await _roleRepository.SoftDeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Role with ID {id} not found" });
            }
            return Ok(new { message = $"Role with ID {id} deleted successfully" });
        }

        // DELETE: api/Role/5/hard
        [HttpDelete("{id}/hard")]
        public async Task<ActionResult> HardDeleteRole(int id)
        {
            var result = await _roleRepository.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Role with ID {id} not found" });
            }
            return Ok(new { message = $"Role with ID {id} permanently deleted" });
        }
    }
}

