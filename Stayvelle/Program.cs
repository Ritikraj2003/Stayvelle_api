using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.RepositoryImpl;
using Stayvelle.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references by ignoring them
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove delay of token when expire
    };
});

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StayVelle API",
        Version = "v1",
        Description = "StayVelle Hotel Management System API"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DBConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Services
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Repositories
builder.Services.AddScoped<IUsers, UserRepository>();
builder.Services.AddScoped<IRole, RoleRepository>();
builder.Services.AddScoped<IPermission, PermissionRepository>();
builder.Services.AddScoped<ILogin, LoginRepository>();
builder.Services.AddScoped<IRolePermission, RolePermissionRepository>();
builder.Services.AddScoped<IRoom, RoomRepository>();
builder.Services.AddScoped<IBooking, BookingRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();

// Automatic database schema management
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
        
        // Automatic database schema management (NO MIGRATIONS NEEDED!)
        // This will:
        // - Create new tables if they don't exist
        // - Add missing columns to existing tables automatically
        // - Preserve all existing data
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // First, ensure all tables exist (creates new tables only, doesn't delete existing ones)
        // Note: EnsureCreated() only works if database is completely empty
        // So we also use DatabaseHelper to create missing tables
        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: EnsureCreated() may have skipped some tables: {ex.Message}");
        }
        
        // Then, automatically create missing tables and add missing columns to existing tables
        // This handles new properties added to models without deleting data
        DatabaseHelper.EnsureColumnsExist(context);
        
        Console.WriteLine("✓ Database tables and columns verified/updated successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
