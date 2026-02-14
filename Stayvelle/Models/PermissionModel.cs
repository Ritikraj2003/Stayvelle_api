namespace Stayvelle.Models
{
    public class PermissionModel : CommonModel
    {
        public int Id { get; set; }
        public string permission_name { get; set; } = string.Empty;
        public string permission_code { get; set; } = string.Empty; // e.g., "Users", "Reservations", "Settings"
        public bool isdelete { get; set; }
        
        // Navigation property for many-to-many relationship
        public virtual ICollection<RolePermissionModel>? RolePermissionModel { get; set; }
    }

    public class CreatePermissionDTO
    {
        public string permission_name { get; set; } = string.Empty;
        public string permission_code { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdatePermissionDTO
    {
        public string? permission_name { get; set; }
        public string? permission_code { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }

    public class PermissionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}

