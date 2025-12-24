using Microsoft.EntityFrameworkCore;
using Npgsql;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.RepositoryImpl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DBConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<IUsers, UserRepository>();

var app = builder.Build();

// Automatically create database and tables based on models
// This will create the database if it doesn't exist, then create tables
// Note: To apply schema changes (add/remove columns), you may need to:
// 1. Manually drop the table, or
// 2. Set "RecreateDatabaseOnStartup": true in appsettings.Development.json
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // First, ensure the database exists
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database;
        
        // Create connection string to 'postgres' database to check/create target database
        connectionStringBuilder.Database = "postgres";
        var masterConnectionString = connectionStringBuilder.ToString();
        
        using (var masterConnection = new NpgsqlConnection(masterConnectionString))
        {
            masterConnection.Open();
            
            // Check if database exists
            var checkDbCommand = new NpgsqlCommand(
                $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", 
                masterConnection);
            var dbExists = checkDbCommand.ExecuteScalar() != null;
            
            if (!dbExists)
            {
                // Create the database
                var createDbCommand = new NpgsqlCommand(
                    $"CREATE DATABASE \"{databaseName}\"", 
                    masterConnection);
                createDbCommand.ExecuteNonQuery();
                Console.WriteLine($"Database '{databaseName}' created successfully!");
            }
        }
        
        // Now create/update tables in the target database
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Check if we should recreate database (useful for development when models change)
        var recreateDatabase = builder.Configuration.GetValue<bool>("RecreateDatabaseOnStartup", false);
        
        if (recreateDatabase && app.Environment.IsDevelopment())
        {
            // WARNING: This will delete all data!
            context.Database.EnsureDeleted();
            Console.WriteLine("Database deleted. Recreating...");
        }
        
        // Create tables if they don't exist
        // This automatically creates tables based on all DbSet properties in ApplicationDbContext
        context.Database.EnsureCreated();
        
        Console.WriteLine("Database and tables created/verified successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating/updating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
