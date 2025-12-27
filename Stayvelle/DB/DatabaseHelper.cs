using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace Stayvelle.DB
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Automatically adds missing columns to existing tables based on model properties
        /// This ensures new properties are added without deleting existing data
        /// </summary>
        public static void EnsureColumnsExist(ApplicationDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                var wasClosed = connection.State != System.Data.ConnectionState.Open;
                
                if (wasClosed)
                {
                    connection.Open();
                }

                try
                {
                    // Get all entity types from the model
                    var entityTypes = context.Model.GetEntityTypes();

                    foreach (var entityType in entityTypes)
                    {
                        var tableName = entityType.GetTableName();
                        if (string.IsNullOrEmpty(tableName)) continue;

                        // Check if table exists first
                        var checkTableSql = $@"
                            SELECT COUNT(*) 
                            FROM information_schema.tables 
                            WHERE table_name = '{tableName}'";
                        
                        var checkTableCmd = new NpgsqlCommand(checkTableSql, (NpgsqlConnection)connection);
                        var tableExists = Convert.ToInt32(checkTableCmd.ExecuteScalar()) > 0;

                        if (!tableExists)
                        {
                            // Table doesn't exist, create it
                            CreateTable(context, entityType, (NpgsqlConnection)connection);
                            continue;
                        }

                        // Get all properties for this entity
                        var properties = entityType.GetProperties();

                        foreach (var property in properties)
                        {
                            var columnName = property.GetColumnName();
                            if (string.IsNullOrEmpty(columnName)) continue;

                            // Skip Id column (primary key) - it should already exist
                            if (property.IsPrimaryKey()) continue;

                            // Check if column exists
                            var checkColumnSql = $@"
                                SELECT COUNT(*) 
                                FROM information_schema.columns 
                                WHERE table_name = '{tableName}' 
                                AND column_name = '{columnName}'";

                            var checkCmd = new NpgsqlCommand(checkColumnSql, (NpgsqlConnection)connection);
                            var columnExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

                            if (!columnExists)
                            {
                                // Determine column type based on property type
                                var columnType = GetPostgreSQLType(property);
                                var isNullable = property.IsNullable ? "NULL" : "NOT NULL DEFAULT ''";

                                // Add the column
                                var addColumnSql = $@"ALTER TABLE ""{tableName}"" ADD COLUMN ""{columnName}"" {columnType} {isNullable}";
                                
                                var addCmd = new NpgsqlCommand(addColumnSql, (NpgsqlConnection)connection);
                                addCmd.ExecuteNonQuery();

                                Console.WriteLine($"✓ Added column '{columnName}' ({columnType}) to table '{tableName}'");
                            }
                        }
                    }
                }
                finally
                {
                    if (wasClosed)
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Error ensuring columns exist: {ex.Message}");
                // Don't throw - allow application to continue
                // The error might be due to permissions or connection issues
            }
        }

        /// <summary>
        /// Maps .NET types to PostgreSQL column types
        /// </summary>
        private static string GetPostgreSQLType(IProperty property)
        {
            var clrType = property.ClrType;

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

            if (underlyingType == typeof(string))
            {
                return "TEXT";
            }
            else if (underlyingType == typeof(int))
            {
                return "INTEGER";
            }
            else if (underlyingType == typeof(long))
            {
                return "BIGINT";
            }
            else if (underlyingType == typeof(bool))
            {
                return "BOOLEAN";
            }
            else if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTime?))
            {
                return "TIMESTAMP";
            }
            else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
            {
                return "NUMERIC";
            }
            else if (underlyingType == typeof(byte[]))
            {
                return "BYTEA";
            }
            else
            {
                // Default to TEXT for unknown types
                return "TEXT";
            }
        }

        /// <summary>
        /// Creates a table for an entity type if it doesn't exist
        /// </summary>
        private static void CreateTable(ApplicationDbContext context, IEntityType entityType, NpgsqlConnection connection)
        {
            try
            {
                var tableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(tableName)) return;

                var properties = entityType.GetProperties();
                var primaryKey = entityType.FindPrimaryKey();
                var primaryKeyProperty = primaryKey?.Properties.FirstOrDefault();

                var columns = new List<string>();

                // Add Id column (primary key with auto-increment)
                if (primaryKeyProperty != null)
                {
                    var idColumnName = primaryKeyProperty.GetColumnName();
                    var clrType = Nullable.GetUnderlyingType(primaryKeyProperty.ClrType) ?? primaryKeyProperty.ClrType;
                    
                    // Use SERIAL for integer types (auto-increment)
                    if (clrType == typeof(int))
                    {
                        columns.Add($"\"{idColumnName}\" SERIAL PRIMARY KEY");
                    }
                    else if (clrType == typeof(long))
                    {
                        columns.Add($"\"{idColumnName}\" BIGSERIAL PRIMARY KEY");
                    }
                    else
                    {
                        var idColumnType = GetPostgreSQLType(primaryKeyProperty);
                        columns.Add($"\"{idColumnName}\" {idColumnType} PRIMARY KEY");
                    }
                }

                // Add other columns
                foreach (var property in properties)
                {
                    // Skip primary key (already added)
                    if (property.IsPrimaryKey()) continue;

                    var columnName = property.GetColumnName();
                    if (string.IsNullOrEmpty(columnName)) continue;

                    var columnType = GetPostgreSQLType(property);
                    var isNullable = property.IsNullable ? "NULL" : "NOT NULL";

                    // Add default values for non-nullable types
                    string defaultValue = "";
                    if (!property.IsNullable)
                    {
                        var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                        if (clrType == typeof(string))
                        {
                            defaultValue = "DEFAULT ''";
                        }
                        else if (clrType == typeof(bool))
                        {
                            defaultValue = "DEFAULT false";
                        }
                        else if (clrType == typeof(int) || clrType == typeof(long))
                        {
                            defaultValue = "DEFAULT 0";
                        }
                        else if (clrType == typeof(decimal) || clrType == typeof(double) || clrType == typeof(float))
                        {
                            defaultValue = "DEFAULT 0";
                        }
                        else if (clrType == typeof(DateTime))
                        {
                            defaultValue = "DEFAULT CURRENT_TIMESTAMP";
                        }
                    }

                    columns.Add($"\"{columnName}\" {columnType} {isNullable} {defaultValue}".Trim());
                }

                if (columns.Any())
                {
                    var createTableSql = $@"CREATE TABLE IF NOT EXISTS ""{tableName}"" ({string.Join(", ", columns)})";
                    var createCmd = new NpgsqlCommand(createTableSql, connection);
                    createCmd.ExecuteNonQuery();

                    Console.WriteLine($"✓ Created table '{tableName}' with {columns.Count} columns");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Error creating table '{entityType.GetTableName()}': {ex.Message}");
            }
        }
    }
}

