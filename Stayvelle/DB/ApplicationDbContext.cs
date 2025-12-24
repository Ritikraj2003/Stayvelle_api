using Microsoft.EntityFrameworkCore;
using Stayvelle.Models;

namespace Stayvelle.DB
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        // Add all your DbSet properties here for each model
        // Tables will be automatically created based on these models
        public DbSet<UsersModel> UsersModel { get; set; }
        
        // Add more DbSet properties here as you create new models
        // Example:
        // public DbSet<YourNewModel> YourNewModel { get; set; }
    }
}
