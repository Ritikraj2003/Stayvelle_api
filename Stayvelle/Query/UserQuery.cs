namespace Stayvelle.Query
{
    public static class UserQuery
    {
        public static string GetAllUsers = @"
            SELECT *
            FROM ""UsersModel""
            WHERE ""isdelete"" = false
            ORDER BY ""Id"" ASC";

        public static string UpdateUser = @"
            UPDATE ""UsersModel""
            SET 
                ""Name"" = @Name,
                ""Email"" = @Email,
                ""Password"" = @Password,
                ""Username"" = @Username,
                ""Phone"" = @Phone,
                ""isactive"" = @isactive,
                ""isstaff"" = @isstaff,
                ""isadmin"" = @isadmin,
                ""role_id"" = @role_id,
                ""role_name"" = @role_name
            WHERE ""Id"" = @Id AND ""isdelete"" = false;";

        public static string SoftDeleteUser = @"
            UPDATE ""UsersModel""
            SET ""isdelete"" = true
            WHERE ""Id"" = @Id AND ""isdelete"" = false;";

        public static string HardDeleteUser = @"
            DELETE FROM ""UsersModel""
            WHERE ""Id"" = @Id;";
    }
}

