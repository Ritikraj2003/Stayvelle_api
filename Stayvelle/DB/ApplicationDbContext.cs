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
        public DbSet<BookingModel> BookingModel { get; set; }
        public DbSet<GuestDetailsModel> GuestDetailsModel { get; set; }
        public DbSet<HousekeepingTask> HousekeepingTask { get; set; }
        
        // New Models
        public DbSet<ServiceModel> ServiceModel { get; set; }
        public DbSet<BookingServiceModel> BookingServiceModel { get; set; }
        public DbSet<RoomDiscountModel> RoomDiscountModel { get; set; }
        public DbSet<DocumentModel> DocumentModel { get; set; }
        public DbSet<BookingSessionModel> BookingSessionModel { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure RoomModel
            modelBuilder.Entity<RoomModel>()
                .HasKey(r => r.Id);
            
            modelBuilder.Entity<RoomModel>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            // Configure BookingModel
            modelBuilder.Entity<BookingModel>()
                .HasKey(b => b.BookingId);

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.BookingId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<BookingModel>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure DateTime properties to be stored as UTC
            modelBuilder.Entity<BookingModel>()
                .Property(b => b.CheckInDate)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.CheckOutDate)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.ActualCheckInTime)
                .HasConversion(
                    v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.ActualCheckOutTime)
                .HasConversion(
                    v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.CreatedOn)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.ModifiedOn)
                .HasConversion(
                    v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            // Configure GuestDetailsModel
            modelBuilder.Entity<GuestDetailsModel>()
                .HasKey(g => g.GuestId);

            modelBuilder.Entity<GuestDetailsModel>()
                .Property(g => g.GuestId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<GuestDetailsModel>()
                .HasOne(g => g.Booking)
                .WithMany(b => b.Guests)
                .HasForeignKey(g => g.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CommonModel fields for BookingModel
            modelBuilder.Entity<BookingModel>()
                .Property(b => b.CreatedBy)
                .HasDefaultValue("system");

            modelBuilder.Entity<BookingModel>()
                .Property(b => b.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure CommonModel fields for GuestDetailsModel
            modelBuilder.Entity<GuestDetailsModel>()
                .Property(g => g.CreatedBy)
                .HasDefaultValue("system");

            modelBuilder.Entity<GuestDetailsModel>()
                .Property(g => g.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<GuestDetailsModel>()
                .Property(g => g.ModifiedOn)
                .HasConversion(
                    v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            
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

            // Configure HousekeepingTask
            modelBuilder.Entity<HousekeepingTask>()
                .HasKey(t => t.TaskId);

            modelBuilder.Entity<HousekeepingTask>()
                .Property(t => t.TaskId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<HousekeepingTask>()
                .HasOne(t => t.Room)
                .WithMany()
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HousekeepingTask>()
                .HasOne(t => t.Booking)
                .WithMany()
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure DateTime properties for HousekeepingTask
            modelBuilder.Entity<HousekeepingTask>()
                .Property(t => t.CreatedOn)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<HousekeepingTask>()
                .Property(t => t.ModifiedOn)
                .HasConversion(
                    v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            // Configure CommonModel fields for HousekeepingTask
            modelBuilder.Entity<HousekeepingTask>()
                .Property(t => t.CreatedBy)
                .HasDefaultValue("system");

            modelBuilder.Entity<HousekeepingTask>()
                .Property(t => t.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
