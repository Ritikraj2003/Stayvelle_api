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
    public class UsersController : ControllerBase
    {
        private readonly IUsers _userRepository;

        public UsersController(IUsers userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<List<UsersModel>>> GetAllUsers()
        {
            var response = await _userRepository.GetAllUsersAsync();
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsersModel>> GetUser(int id)
        {
            var response = await _userRepository.GetUserByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"User with ID {id} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/Users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UsersModel>> GetUserByEmail(string email)
        {
            var response = await _userRepository.GetUserByEmailAsync(email);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"User with email {email} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/Users/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UsersModel>> GetUserByUsername(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new { message = $"User with username {username} not found" });
            }
            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UsersModel>> CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userRepository.CreateUserAsync(createUserDTO);
            if (!response.Success || response.Data == null)
            {
                if (response.Message != null && response.Message.Contains("already exists"))
                {
                     return Conflict(new { message = response.Message });
                }
                return BadRequest(new { message = response.Message });
            }
            return CreatedAtAction(nameof(GetUser), new { id = response.Data.Id }, response.Data);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ActionResult<UsersModel>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            var updatedUserResponse = await _userRepository.UpdateUserAsync(id, updateUserDTO);
            if (!updatedUserResponse.Success || updatedUserResponse.Data == null)
            {
                 if (updatedUserResponse.Message != null && updatedUserResponse.Message.Contains("already exists"))
                {
                     return Conflict(new { message = updatedUserResponse.Message });
                }
                if (updatedUserResponse.Message != null && updatedUserResponse.Message.Contains("not found"))
                {
                    return NotFound(new { message = updatedUserResponse.Message });
                }
                return BadRequest(new { message = updatedUserResponse.Message ?? $"Error updating user with ID {id}" });
            }

            return Ok(updatedUserResponse.Data);
        }

        // DELETE: api/Users/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userRepository.SoftDeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            return Ok(new { message = $"User with ID {id} deleted successfully" });
        }

        // DELETE: api/Users/5/hard (Hard Delete - permanently remove)
        [HttpDelete("{id}/hard")]
        public async Task<ActionResult> HardDeleteUser(int id)
        {
            var result = await _userRepository.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }
            return Ok(new { message = $"User with ID {id} permanently deleted" });
        }
    }
}
