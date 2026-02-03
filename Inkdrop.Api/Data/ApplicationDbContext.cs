using Inkdrop.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Inkdrop.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
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
    }

}