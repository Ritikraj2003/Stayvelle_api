using System.ComponentModel.DataAnnotations.Schema;
using Stayvelle.Models.DTOs;

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
}
