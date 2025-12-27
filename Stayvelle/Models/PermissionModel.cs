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
}

