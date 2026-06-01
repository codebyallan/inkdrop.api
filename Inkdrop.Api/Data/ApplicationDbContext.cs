using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<Toner> Toners => Set<Toner>();
    public DbSet<Movements> Movements => Set<Movements>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<PrinterTelemetry> PrinterTelemetries => Set<PrinterTelemetry>();
    public DbSet<TelemetrySupply> TelemetrySupplies => Set<TelemetrySupply>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.Username)
                .HasMaxLength(50)
                .IsRequired();
            user.Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired();
            user.Property(u => u.PasswordHash)
                .IsRequired();
            user.Property(u => u.Salt)
                .IsRequired();
            user.Property(u => u.CreatedAt)
                .HasColumnType("timestamp with time zone");
            user.Property(u => u.UpdatedAt)
                .HasColumnType("timestamp with time zone");
            user.Property(u => u.DeletedAt)
                .HasColumnType("timestamp with time zone");
            user.HasQueryFilter(u => u.DeletedAt == null);
            user.HasIndex(u => u.Username)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");
            user.HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(l => l.CreatedAt)
                .HasColumnType("timestamp with time zone");
            entity.Property(l => l.UpdatedAt)
                .HasColumnType("timestamp with time zone");
            entity.Property(l => l.DeletedAt)
                .HasColumnType("timestamp with time zone");
            entity.HasQueryFilter(l => l.DeletedAt == null);
            entity.HasIndex(l => l.Name)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");
        }
        );
        modelBuilder.Entity<Printer>(printer =>
        {
            printer.HasKey(l => l.Id);
            printer.Property(l => l.Name)
                .HasMaxLength(100)
                .IsRequired();
            printer.Property(l => l.Model)
                .HasMaxLength(100)
                .IsRequired();
            printer.Property(l => l.Manufacturer)
                .HasMaxLength(100)
                .IsRequired();
            printer.Property(l => l.IpAddress)
                .HasMaxLength(45)
                .IsRequired();
            printer.HasOne(l => l.Location)
                .WithMany()
                .HasForeignKey(p => p.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            printer.Property(l => l.IsActive)
                .HasDefaultValue(true);
            printer.Property(l => l.CreatedAt)
                .HasColumnType("timestamp with time zone");
            printer.Property(l => l.UpdatedAt)
                .HasColumnType("timestamp with time zone");
            printer.Property(l => l.DeletedAt)
                .HasColumnType("timestamp with time zone");
            printer.HasQueryFilter(l => l.DeletedAt == null);
            printer.HasIndex(l => l.IpAddress)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");
        }
        );
        modelBuilder.Entity<Toner>(toner =>
        {
            toner.HasKey(l => l.Id);
            toner.Property(l => l.Model)
                .HasMaxLength(100)
                .IsRequired();
            toner.Property(l => l.Manufacturer)
                .HasMaxLength(100)
                .IsRequired();
            toner.Property(l => l.Quantity)
                .HasDefaultValue(0);
            toner.Property(l => l.Color)
                .HasMaxLength(50)
                .IsRequired();
            toner.Property(l => l.CreatedAt)
                .HasColumnType("timestamp with time zone");
            toner.Property(l => l.UpdatedAt)
                .HasColumnType("timestamp with time zone");
            toner.Property(l => l.DeletedAt)
                .HasColumnType("timestamp with time zone");
            toner.HasQueryFilter(l => l.DeletedAt == null);
            toner.HasIndex(l => new { l.Model, l.Manufacturer, l.Color })
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");
        }
        );
        modelBuilder.Entity<Movements>(movement =>
        {
            movement.HasKey(l => l.Id);
            movement.Property(l => l.Quantity)
                .IsRequired();
            movement.Property(l => l.Description)
                .HasMaxLength(100)
                .IsRequired(false);
            movement.Property(l => l.Type)
                .HasMaxLength(50)
                .IsRequired();
            movement.HasOne<Toner>()
                .WithMany()
                .HasForeignKey(m => m.TonerId)
                .OnDelete(DeleteBehavior.Restrict);
            movement.HasOne<Printer>()
                .WithMany()
                .HasForeignKey(m => m.PrinterId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            movement.Property(l => l.CreatedAt)
                .HasColumnType("timestamp with time zone");
        }
        );

        modelBuilder.Entity<ApiKey>(apiKey =>
        {
            apiKey.HasKey(a => a.Id);
            apiKey.Property(a => a.Name).HasMaxLength(100).IsRequired();
            apiKey.Property(a => a.KeyHash).IsRequired();
            apiKey.Property(a => a.CreatedAt).HasColumnType("timestamp with time zone");
            apiKey.Property(a => a.LastUsedAt).HasColumnType("timestamp with time zone");
            apiKey.Property(a => a.DeletedAt).HasColumnType("timestamp with time zone");
            apiKey.HasQueryFilter(a => a.DeletedAt == null);
            apiKey.HasIndex(a => a.KeyHash).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
        });

        modelBuilder.Entity<PrinterTelemetry>(telemetry =>
        {
            telemetry.HasKey(t => t.Id);
            telemetry.Property(t => t.PrinterId).IsRequired();
            telemetry.Property(t => t.TotalPages).IsRequired();
            telemetry.Property(t => t.CollectedAt).HasColumnType("timestamp with time zone").IsRequired();
            telemetry.Property(t => t.CreatedAt).HasColumnType("timestamp with time zone");
            telemetry.HasIndex(t => new { t.PrinterId, t.CollectedAt })
                .IsUnique();
            telemetry.HasOne<Printer>()
                .WithMany()
                .HasForeignKey(t => t.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TelemetrySupply>(supply =>
        {
            supply.HasKey(s => s.Id);
            supply.Property(s => s.Color).HasMaxLength(50).IsRequired();
            supply.Property(s => s.Level).IsRequired();
            supply.Property(s => s.CreatedAt).HasColumnType("timestamp with time zone");
            supply.HasOne<PrinterTelemetry>()
                .WithMany(t => t.Supplies)
                .HasForeignKey(s => s.TelemetryId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Notifications.Notifiable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Ignore("Notifications");
                modelBuilder.Entity(entityType.ClrType).Ignore("IsValid");
            }
        }
    }

}