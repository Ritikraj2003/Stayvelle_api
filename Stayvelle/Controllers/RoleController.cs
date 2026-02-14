using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRole _roleRepository;

        public RoleController(IRole roleRepository)
        {
            _roleRepository = roleRepository;
        }

        // GET: api/Role
        [HttpGet]
        public async Task<ActionResult<List<RoleResponseDTO>>> GetAllRoles([FromQuery] string? search = null)
        {
            var response = await _roleRepository.GetAllRolesAsync(search);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
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
            return Ok(response.Data);
        }

        // POST: api/Role
        [HttpPost]
        public async Task<ActionResult<RoleResponseDTO>> CreateRole([FromBody] CreateRoleDTO createRoleDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roleResponse = await _roleRepository.CreateRoleAsync(createRoleDTO);
            if (!roleResponse.Success || roleResponse.Data == null)
            {
                if (roleResponse.Message != null && roleResponse.Message.Contains("already exists"))
                {
                    return Conflict(new { message = roleResponse.Message });
                }
                return BadRequest(new { message = roleResponse.Message });
            }

            // Repository now returns full DTO
            return CreatedAtAction(nameof(GetRole), new { id = roleResponse.Data.Id }, roleResponse.Data);
        }

        // PUT: api/Role/5
        [HttpPut("{id}")]
        public async Task<ActionResult<RoleResponseDTO>> UpdateRole(int id, [FromBody] UpdateRoleDTO updateRoleDTO)
        {
            var updateResponse = await _roleRepository.UpdateRoleAsync(id, updateRoleDTO);
            if (!updateResponse.Success || updateResponse.Data == null)
            {
                if (updateResponse.Message != null && updateResponse.Message.Contains("already exists"))
                {
                    return Conflict(new { message = updateResponse.Message });
                }
                if (updateResponse.Message != null && updateResponse.Message.Contains("not found"))
                {
                    return NotFound(new { message = updateResponse.Message });
                }
                return BadRequest(new { message = updateResponse.Message });
            }

            return Ok(updateResponse.Data);
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

