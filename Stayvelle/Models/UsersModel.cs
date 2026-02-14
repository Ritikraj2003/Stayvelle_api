using System.ComponentModel.DataAnnotations.Schema;
using Stayvelle.Models;

namespace Stayvelle.Models
{
    public class UsersModel : CommonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public bool isactive { get; set; }
        public string Phone { get; set; }
        public int role_id { get; set; }
        public string role_name { get; set; }
        public bool isstaff { get; set; }
        public bool isadmin { get; set; }
        public bool isdelete { get; set; }
        
        public bool IsHousekeeping { get; set; }

        [NotMapped]
        public List<DocumentDto> Documents { get; set; } = new();
    }

    public class CreateUserDTO : CommonModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool isactive { get; set; } = true;
        public bool isstaff { get; set; } = false;
        public bool isadmin { get; set; } = false;
        public int role_id { get; set; }
        public string role_name { get; set; }
        public string Phone { get; set; }
        
        public bool IsHousekeeping { get; set; } = false;
        public List<DocumentDto> Documents { get; set; } = new();
    }

    public class UpdateUserDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
        public int? role_id { get; set; }
        public string? role_name { get; set; }
        public bool? isactive { get; set; }
        public bool? isstaff { get; set; }
        public bool? isadmin { get; set; }
        
        public bool? IsHousekeeping { get; set; }
        public List<DocumentDto>? Documents { get; set; }
    }

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
        // ImageUrl removed
        public List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
        public string Token { get; set; } = string.Empty; // Will be implemented with JWT later
    }
}
