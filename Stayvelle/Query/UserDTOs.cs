namespace Stayvelle.Query
{
    // DTOs for User operations (for API requests/responses)
    
    public class CreateUserDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool isactive { get; set; } = true;
        public bool isstaff { get; set; } = false;
        public bool isadmin { get; set; } = false;
    }

    public class UpdateUserDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Username { get; set; }
        public bool? isactive { get; set; }
        public bool? isstaff { get; set; }
        public bool? isadmin { get; set; }
    }
}

