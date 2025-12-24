namespace Stayvelle.Query
{
    public static class UserQuery
    {
        // GET Queries - To retrieve data
        
        // Get all users (excluding soft-deleted)
        public static string GetAllUsers = @"
            SELECT 
                ""Id"", 
                ""Name"", 
                ""Email"", 
                ""Password"", 
                ""Username"", 
                ""isactive"", 
                ""isstaff"", 
                ""isadmin"", 
                ""isdelete""
            FROM ""UsersModel""
            WHERE ""isdelete"" = false
            ORDER BY ""Id"" ASC";

        // Get user by ID
        public static string GetUserById = @"
            SELECT 
                ""Id"", 
                ""Name"", 
                ""Email"", 
                ""Password"", 
                ""Username"", 
                ""isactive"", 
                ""isstaff"", 
                ""isadmin"", 
                ""isdelete""
            FROM ""UsersModel""
            WHERE ""Id"" = @Id AND ""isdelete"" = false";

        // Get user by Email
        public static string GetUserByEmail = @"
            SELECT 
                ""Id"", 
                ""Name"", 
                ""Email"", 
                ""Password"", 
                ""Username"", 
                ""isactive"", 
                ""isstaff"", 
                ""isadmin"", 
                ""isdelete""
            FROM ""UsersModel""
            WHERE ""Email"" = @Email AND ""isdelete"" = false";

        // Get user by Username
        public static string GetUserByUsername = @"
            SELECT 
                ""Id"", 
                ""Name"", 
                ""Email"", 
                ""Password"", 
                ""Username"", 
                ""isactive"", 
                ""isstaff"", 
                ""isadmin"", 
                ""isdelete""
            FROM ""UsersModel""
            WHERE ""Username"" = @Username AND ""isdelete"" = false";

        // Check if email exists
        public static string CheckEmailExists = @"
            SELECT COUNT(*) 
            FROM ""UsersModel""
            WHERE ""Email"" = @Email AND ""isdelete"" = false;";

        // Check if username exists
        public static string CheckUsernameExists = @"
            SELECT COUNT(*) 
            FROM ""UsersModel""
            WHERE ""Username"" = @Username AND ""isdelete"" = false;";

        // UPDATE Queries - To update data
        
        // Update user by ID (full update)
        public static string UpdateUser = @"
            UPDATE ""UsersModel""
            SET 
                ""Name"" = @Name,
                ""Email"" = @Email,
                ""Password"" = @Password,
                ""Username"" = @Username,
                ""isactive"" = @isactive,
                ""isstaff"" = @isstaff,
                ""isadmin"" = @isadmin
            WHERE ""Id"" = @Id AND ""isdelete"" = false;";

        // Update user - partial update (only provided fields)
        public static string UpdateUserPartial = @"
            UPDATE ""UsersModel""
            SET 
                ""Name"" = COALESCE(@Name, ""Name""),
                ""Email"" = COALESCE(@Email, ""Email""),
                ""Password"" = COALESCE(@Password, ""Password""),
                ""Username"" = COALESCE(@Username, ""Username""),
                ""isactive"" = COALESCE(@isactive, ""isactive""),
                ""isstaff"" = COALESCE(@isstaff, ""isstaff""),
                ""isadmin"" = COALESCE(@isadmin, ""isadmin"")
            WHERE ""Id"" = @Id AND ""isdelete"" = false;";

        // Soft delete user (set isdelete flag)
        public static string SoftDeleteUser = @"
            UPDATE ""UsersModel""
            SET ""isdelete"" = true
            WHERE ""Id"" = @Id AND ""isdelete"" = false;";

        // Hard delete user (permanently remove)
        public static string HardDeleteUser = @"
            DELETE FROM ""UsersModel""
            WHERE ""Id"" = @Id;";

        // Activate/Deactivate user
        public static string UpdateUserStatus = @"
            UPDATE ""UsersModel""
            SET ""isactive"" = @isactive
            WHERE ""Id"" = @Id AND ""isdelete"" = false;";
    }
}

