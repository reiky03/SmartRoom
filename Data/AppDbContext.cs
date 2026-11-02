using Microsoft.EntityFrameworkCore;
using SmartRoom.Models;

namespace SmartRoom.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Room entity configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.RoomCode)
                .IsUnique();

            entity.Property(r => r.IsActive)
                .HasDefaultValue(true);
        });

        // Booking entity configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(b => b.BookingReference)
                .IsUnique();

            entity.HasIndex(b => new { b.RoomId, b.StartTime, b.EndTime, b.Status });

            entity.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
