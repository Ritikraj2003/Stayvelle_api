using Stayvelle.Models;

namespace Stayvelle.Query
{
    // DTOs for User operations (for API requests/responses)
    
    public class CreateUserDTO: CommonModel
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
        public string? ImageUrl { get; set; } // Optional: image as base64 string or file path

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
        public string? ImageUrl { get; set; } // Optional: 
                                              // - If provided (not null): updates image (empty string = remove image)
                                              // - If null: preserves existing image
    }
}

