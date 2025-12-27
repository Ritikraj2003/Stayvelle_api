namespace Stayvelle.Query
{
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
}

