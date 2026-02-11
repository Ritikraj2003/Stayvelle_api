using Stayvelle.Models;

namespace Stayvelle.Query
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsStaff { get; set; }
        public bool IsAdmin { get; set; }
        public string? ImageUrl { get; set; } // User profile image
        public List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
        public string Token { get; set; } = string.Empty; // Will be implemented with JWT later
    }

}

