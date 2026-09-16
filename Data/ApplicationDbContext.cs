using Microsoft.EntityFrameworkCore;
using wad_project.Models;

namespace wad_project.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Vehicle
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(v => v.LicensePlate).IsUnique();
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(b => b.BookingReference).IsUnique();

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Vehicle)
                  .WithMany(v => v.Bookings)
                  .HasForeignKey(b => b.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.PromoCode)
                  .WithMany()
                  .HasForeignKey(b => b.PromoCodeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment (1-to-1 with Booking)
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(p => p.Booking)
                  .WithOne(b => b.Payment)
                  .HasForeignKey<Payment>(p => p.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Review (1-to-1 with Booking, 1-to-Many with User & Vehicle)
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Booking)
                  .WithOne(b => b.Review)
                  .HasForeignKey<Review>(r => r.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Vehicle)
                  .WithMany(v => v.Reviews)
                  .HasForeignKey(r => r.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PromoCode
        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
        });
    }
}
