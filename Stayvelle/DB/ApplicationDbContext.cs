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
        public DbSet<RoleModel> RoleModel { get; set; }
        public DbSet<PermissionModel> PermissionModel { get; set; }
        public DbSet<RolePermissionModel> RolePermissionModel { get; set; }
        public DbSet<RoomModel> RoomModel { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure RoomModel
            modelBuilder.Entity<RoomModel>()
                .HasKey(r => r.Id);
            
            modelBuilder.Entity<RoomModel>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();
            
            // Configure many-to-many relationship
            modelBuilder.Entity<RolePermissionModel>()
                .HasKey(rp => rp.Id);
            
            modelBuilder.Entity<RolePermissionModel>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);
            
            modelBuilder.Entity<RolePermissionModel>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissionModel)
                .HasForeignKey(rp => rp.PermissionId);
        }
    }
}
