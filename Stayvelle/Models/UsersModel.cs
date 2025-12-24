namespace Stayvelle.Models
{
    public class UsersModel:CommonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public bool isactive { get; set; }
        public bool isstaff { get; set; } 
        public bool isadmin { get; set; } 
        public bool isdelete {  get; set; }
    }
}
