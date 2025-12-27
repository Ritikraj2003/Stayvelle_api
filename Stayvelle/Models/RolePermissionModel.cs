namespace Stayvelle.Models
{
    public class RolePermissionModel : CommonModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public bool isactive { get; set; }
        public bool isdelete { get; set; }
        
        // Navigation properties
        public virtual RoleModel Role { get; set; }
        public virtual PermissionModel Permission { get; set; }
    }
}

