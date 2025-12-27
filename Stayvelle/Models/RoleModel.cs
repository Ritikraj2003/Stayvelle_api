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
}

