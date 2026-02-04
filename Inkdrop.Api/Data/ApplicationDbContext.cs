using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Inkdrop.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Printer> Printers => Set<Printer>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            printer.HasOne<Location>()
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
        }
        );
    }

}