using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;
using Stayvelle.Services;

namespace Stayvelle.RepositoryImpl
{
    public class LoginRepository : ILogin
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermission _permissionRepository;
        private readonly IJwtService _jwtService;

        public LoginRepository(ApplicationDbContext context, IPermission permissionRepository, IJwtService jwtService)
        {
            _context = context;
            _permissionRepository = permissionRepository;
            _jwtService = jwtService;
        }

        public async Task<Response<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequest)
        {
            var response = new Response<LoginResponseDTO>();
            try
            {
                // Find user by email
                var user = await _context.UsersModel
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && !u.isdelete && u.isactive);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Invalid email or password";
                    response.Data = null;
                    return response;
                }

                // Verify password (Note: In production, use hashed passwords)
                if (user.Password != loginRequest.Password)
                {
                    response.Success = false;
                    response.Message = "Invalid email or password";
                    response.Data = null;
                    return response;
                }

                // Get role information
                var role = await _context.RoleModel
                    .FirstOrDefaultAsync(r => r.Id == user.role_id && !r.isdelete && r.isactive);

                if (role == null)
                {
                    response.Success = false;
                    response.Message = "User role not found or inactive";
                    response.Data = null;
                    return response;
                }

                // Get permissions for the role
                var permissionsResponse = await _permissionRepository.GetPermissionsByRoleIdAsync(user.role_id);
                var permissions = permissionsResponse.Data ?? new List<Models.PermissionModel>();

                // Build response DTO
                var loginResponse = new LoginResponseDTO
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    Phone = user.Phone ?? string.Empty,
                    RoleId = user.role_id,
                    RoleName = role.role_name,
                    IsActive = user.isactive,
                    IsStaff = user.isstaff,
                    IsAdmin = user.isadmin,
                    ImageUrl = user.ImageUrl,
                    Permissions = permissions.Select(p => new PermissionDTO
                    {
                        Id = p.Id,
                        Name = p.permission_name,
                        Description = string.Empty,
                        Module = p.permission_code,
                        Action = string.Empty
                    }).ToList(),
                    Token = _jwtService.GenerateToken(user.Id, user.Email, user.Username, role.role_name, user.role_id)
                };

                response.Success = true;
                response.Message = "Login successful";
                response.Data = loginResponse;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Login failed: {ex.Message}";
                response.Data = null;
                return response;
            }
        }
    }
}

