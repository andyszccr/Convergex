using Convergex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Context;

public class ConvergexDbContext : DbContext
{
    public ConvergexDbContext(DbContextOptions<ConvergexDbContext> options)
        : base(options)
    {
    }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Conversion> Conversions => Set<Conversion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Symbol).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Rate).HasPrecision(18, 6);
            entity.HasOne(x => x.BaseCurrency)
                .WithMany(x => x.BaseRates)
                .HasForeignKey(x => x.BaseCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TargetCurrency)
                .WithMany(x => x.TargetRates)
                .HasForeignKey(x => x.TargetCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Conversion>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ToCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 6);
            entity.Property(x => x.Result).HasPrecision(18, 6);
            entity.Property(x => x.RateApplied).HasPrecision(18, 6);
            entity.Property(x => x.UserName).HasMaxLength(100);
        });
    }
}
