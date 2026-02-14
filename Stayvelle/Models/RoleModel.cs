namespace Stayvelle.Models
{
    public class RoleModel : CommonModel
    {
        public int Id { get; set; }
        public string role_name { get; set; } = string.Empty;
        public bool isactive { get; set; }
        public bool isdelete { get; set; }
        
        // Navigation property for many-to-many relationship
        public virtual ICollection<RolePermissionModel>? RolePermissions { get; set; }
    }

    public class CreateRoleDTO
    {
        public string role_name { get; set; } = string.Empty;
        public bool isactive { get; set; } = true;
        public List<int> PermissionIds { get; set; } = new List<int>();
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdateRoleDTO
    {
        public string? role_name { get; set; }
        public bool? isactive { get; set; }
        public List<int>? PermissionIds { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }

    public class RoleResponseDTO
    {
        public int Id { get; set; }
        public string role_name { get; set; } = string.Empty;
        public bool isactive { get; set; }
        public List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
    }
}

