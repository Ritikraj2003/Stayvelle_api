using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

            // Check if email already exists
            var existingEmailResponse = await _userRepository.GetUserByEmailAsync(createUserDTO.Email);
            if (existingEmailResponse.Success && existingEmailResponse.Data != null)
            {
                return Conflict(new { message = "User with this email already exists" });
            }

            // Check if username already exists
            var existingUsername = await _userRepository.GetUserByUsernameAsync(createUserDTO.Username);
            if (existingUsername != null)
            {
                return Conflict(new { message = "User with this username already exists" });
            }

            var user = new UsersModel
            {
                Name = createUserDTO.Name,
                Email = createUserDTO.Email,
                Password = createUserDTO.Password,
                Username = createUserDTO.Username,
                isactive = createUserDTO.isactive,
                isstaff = createUserDTO.isstaff,
                isadmin = createUserDTO.isadmin,
                isdelete = false
            };

            var response = await _userRepository.CreateUserAsync(user);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return CreatedAtAction(nameof(GetUser), new { id = response.Data.Id }, response.Data);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ActionResult<UsersModel>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            var existingUserResponse = await _userRepository.GetUserByIdAsync(id);
            if (!existingUserResponse.Success || existingUserResponse.Data == null)
            {
                return NotFound(new { message = existingUserResponse.Message ?? $"User with ID {id} not found" });
            }

            var existingUser = existingUserResponse.Data;

            // Check if email is being changed and already exists
            if (!string.IsNullOrEmpty(updateUserDTO.Email) && updateUserDTO.Email != existingUser.Email)
            {
                var emailExistsResponse = await _userRepository.GetUserByEmailAsync(updateUserDTO.Email);
                if (emailExistsResponse.Success && emailExistsResponse.Data != null)
                {
                    return Conflict(new { message = "User with this email already exists" });
                }
            }

            // Check if username is being changed and already exists
            if (!string.IsNullOrEmpty(updateUserDTO.Username) && updateUserDTO.Username != existingUser.Username)
            {
                var usernameExists = await _userRepository.GetUserByUsernameAsync(updateUserDTO.Username);
                if (usernameExists != null)
                {
                    return Conflict(new { message = "User with this username already exists" });
                }
            }

            // Update only provided fields
            var userToUpdate = new UsersModel
            {
                Id = id,
                Name = updateUserDTO.Name ?? existingUser.Name,
                Email = updateUserDTO.Email ?? existingUser.Email,
                Password = updateUserDTO.Password ?? existingUser.Password,
                Username = updateUserDTO.Username ?? existingUser.Username,
                isactive = updateUserDTO.isactive ?? existingUser.isactive,
                isstaff = updateUserDTO.isstaff ?? existingUser.isstaff,
                isadmin = updateUserDTO.isadmin ?? existingUser.isadmin,
                isdelete = existingUser.isdelete
            };

            var updatedUserResponse = await _userRepository.UpdateUserAsync(id, userToUpdate);
            if (!updatedUserResponse.Success || updatedUserResponse.Data == null)
            {
                return NotFound(new { message = updatedUserResponse.Message ?? $"User with ID {id} not found" });
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
