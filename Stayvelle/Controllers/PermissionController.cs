using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermission _permissionRepository;

        public PermissionController(IPermission permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        // GET: api/Permission
        [HttpGet]
        public async Task<ActionResult<List<PermissionModel>>> GetAllPermissions([FromQuery] string? search = null)
        {
            var response = await _permissionRepository.GetAllPermissionsAsync();
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }

            var permissions = response.Data;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                permissions = permissions.Where(p =>
                    p.permission_name.ToLower().Contains(search) ||
                    p.permission_code.ToLower().Contains(search)
                ).ToList();
            }

            return Ok(permissions);
        }

        // GET: api/Permission/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionModel>> GetPermission(int id)
        {
            var response = await _permissionRepository.GetPermissionByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Permission with ID {id} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/Permission/role/5
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<List<PermissionModel>>> GetPermissionsByRole(int roleId)
        {
            var response = await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
        }

        // POST: api/Permission
        [HttpPost]
        public async Task<ActionResult<PermissionModel>> CreatePermission([FromBody] CreatePermissionDTO createPermissionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permission = new PermissionModel
            {
                permission_name = createPermissionDTO.permission_name,
                permission_code = createPermissionDTO.permission_code,
                isdelete = false,
                CreatedBy = createPermissionDTO.CreatedBy,
                CreatedOn = DateTime.UtcNow
            };

            var response = await _permissionRepository.CreatePermissionAsync(permission);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }

            return CreatedAtAction(nameof(GetPermission), new { id = response.Data.Id }, response.Data);
        }

        // PUT: api/Permission/5
        [HttpPut("{id}")]
        public async Task<ActionResult<PermissionModel>> UpdatePermission(int id, [FromBody] UpdatePermissionDTO updatePermissionDTO)
        {
            var existingPermissionResponse = await _permissionRepository.GetPermissionByIdAsync(id);
            if (!existingPermissionResponse.Success || existingPermissionResponse.Data == null)
            {
                return NotFound(new { message = existingPermissionResponse.Message ?? $"Permission with ID {id} not found" });
            }

            var existingPermission = existingPermissionResponse.Data;

            var permissionToUpdate = new PermissionModel
            {
                Id = id,
                permission_name = updatePermissionDTO.permission_name ?? existingPermission.permission_name,
                permission_code = updatePermissionDTO.permission_code ?? existingPermission.permission_code,
                isdelete = existingPermission.isdelete,
                ModifiedBy = updatePermissionDTO.ModifiedBy,
                ModifiedOn = DateTime.UtcNow
            };

            var response = await _permissionRepository.UpdatePermissionAsync(id, permissionToUpdate);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response.Data);
        }

        // DELETE: api/Permission/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePermission(int id)
        {
            var result = await _permissionRepository.SoftDeletePermissionAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Permission with ID {id} not found" });
            }
            return Ok(new { message = $"Permission with ID {id} deleted successfully" });
        }

        // DELETE: api/Permission/5/hard
        [HttpDelete("{id}/hard")]
        public async Task<ActionResult> HardDeletePermission(int id)
        {
            var result = await _permissionRepository.DeletePermissionAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Permission with ID {id} not found" });
            }
            return Ok(new { message = $"Permission with ID {id} permanently deleted" });
        }
    }
}

