using Earthquakes.Domain;
using Microsoft.EntityFrameworkCore;

namespace Earthquakes.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Earthquake> Earthquakes { get; set; }

    public DbSet<EphemerisEntry> EphemerisEntries { get; set; }

    public DbSet<SunSpot> SunSpots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<EphemerisEntry>()
            .HasKey(e => new
            {
                e.Day,
                e.CenterBody,
                e.TargetBody
            });

        modelBuilder
            .Entity<EphemerisEntry>()
            .HasIndex(e => new
            {
                e.CenterBody,
                e.TargetBody,
                e.Minimum
            });

        modelBuilder
            .Entity<EphemerisEntry>()
            .HasIndex(e => new
            {
                e.CenterBody,
                e.TargetBody,
                e.OnsideMinimum
            });

        modelBuilder
            .Entity<EphemerisEntry>()
            .HasIndex(e => new
            {
                e.CenterBody,
                e.TargetBody,
                e.OffsideMinimum
            });
    }
}
