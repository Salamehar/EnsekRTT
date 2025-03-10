using Microsoft.EntityFrameworkCore;
using MeterReadings.Core.Models;

namespace MeterReadings.Data.Context;

public class MeterReadingDbContext : DbContext
{
    public MeterReadingDbContext(DbContextOptions<MeterReadingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        // MeterReading configuration
        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.MeterReadingDateTime).IsRequired();
            entity.Property(e => e.MeterReadValue).IsRequired();

            // Create index on AccountId + MeterReadingDateTime for faster lookups
            entity.HasIndex(e => new { e.AccountId, e.MeterReadingDateTime }).IsUnique();

            // Relationship with Account
            entity.HasOne(e => e.Account)
                  .WithMany(a => a.MeterReadings)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}