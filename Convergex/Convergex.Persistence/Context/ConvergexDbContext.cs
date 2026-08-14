using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Context;

public class ConvergexDbContext : DbContext
{
    public ConvergexDbContext(DbContextOptions<ConvergexDbContext> options)
        : base(options)
    {
    }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Conversion> Conversions => Set<Conversion>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

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

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Category).HasConversion<int>();
            entity.Property(x => x.FactorToBase).HasPrecision(18, 12);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.Symbol).IsUnique();
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BuyRate).HasPrecision(18, 6);
            entity.Property(x => x.SellRate).HasPrecision(18, 6);
            entity.Property(x => x.CreatedByName).HasMaxLength(120);
            entity.HasIndex(x => new { x.BaseCurrencyId, x.TargetCurrencyId, x.Status });
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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(200);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ResetToken).HasMaxLength(100);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(60).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(40);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.HasIndex(x => x.Timestamp);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.EntityName);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Language).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Theme).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DefaultCurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        });
    }
}
