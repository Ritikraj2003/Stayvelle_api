using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogin _loginRepository;

        public LoginController(ILogin loginRepository)
        {
            _loginRepository = loginRepository;
        }

        // POST: api/Login
        [HttpPost]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var response = await _loginRepository.LoginAsync(loginRequest);
            
            if (!response.Success || response.Data == null)
            {
                return Unauthorized(new { message = response.Message ?? "Invalid credentials" });
            }

            return Ok(response.Data);
        }
    }
}

