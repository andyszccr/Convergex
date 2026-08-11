using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Convergex.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ConvergexDbContext context, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(context, cancellationToken);

        await SeedRolesAndUsersAsync(context, cancellationToken);
        await SeedCatalogAsync(context, cancellationToken);
    }

    private static async Task EnsureSchemaAsync(ConvergexDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (await TableExistsAsync(context, "Users", cancellationToken))
        {
            return;
        }

        // SQLite mantiene el archivo bloqueado si hay conexiones abiertas/pool.
        await context.Database.CloseConnectionAsync();
        SqliteConnection.ClearAllPools();

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(
        ConvergexDbContext context,
        string tableName,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name=$name;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task SeedRolesAndUsersAsync(ConvergexDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Roles.AnyAsync(cancellationToken))
        {
            context.Roles.AddRange(
                new Role { Name = "Administrador", Description = "Acceso completo al sistema" },
                new Role { Name = "Operador", Description = "Puede realizar conversiones y consultar historial" },
                new Role { Name = "Consulta", Description = "Solo lectura de información" });
            await context.SaveChangesAsync(cancellationToken);
        }

        if (await context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrador", cancellationToken);
        var operatorRole = await context.Roles.FirstAsync(r => r.Name == "Operador", cancellationToken);
        var hasher = new PasswordHasher<User>();

        var admin = new User
        {
            FullName = "Alexander Navarro",
            Email = "admin@convergex.com",
            RoleId = adminRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var operatorUser = new User
        {
            FullName = "María Rodríguez",
            Email = "operador@convergex.com",
            RoleId = operatorRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        operatorUser.PasswordHash = hasher.HashPassword(operatorUser, "Operador123!");

        context.Users.AddRange(admin, operatorUser);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCatalogAsync(ConvergexDbContext context, CancellationToken cancellationToken)
    {
        await SeedUnitsAsync(context, cancellationToken);

        if (await context.Currencies.AnyAsync(cancellationToken))
        {
            return;
        }

        var usd = new Currency { Code = "USD", Name = "Dólar estadounidense", Symbol = "$", IsActive = true };
        var crc = new Currency { Code = "CRC", Name = "Colón costarricense", Symbol = "₡", IsActive = true };
        var eur = new Currency { Code = "EUR", Name = "Euro", Symbol = "€", IsActive = true };
        var mxn = new Currency { Code = "MXN", Name = "Peso mexicano", Symbol = "$", IsActive = true };

        context.Currencies.AddRange(usd, crc, eur, mxn);
        await context.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        const string seedAdminName = "Alexander Navarro";
        context.ExchangeRates.AddRange(
            new ExchangeRate
            {
                BaseCurrencyId = usd.Id, TargetCurrencyId = crc.Id, BuyRate = 505.20m, SellRate = 515.80m,
                EffectiveAt = now.AddDays(-2), CreatedAt = now.AddDays(-2),
                Source = ExchangeRateSource.Manual, Status = ExchangeRateStatus.Historical, CreatedByName = seedAdminName
            },
            new ExchangeRate
            {
                BaseCurrencyId = usd.Id, TargetCurrencyId = crc.Id, BuyRate = 508.10m, SellRate = 518.45m,
                EffectiveAt = now.AddMinutes(-15), CreatedAt = now.AddMinutes(-15),
                Source = ExchangeRateSource.Manual, Status = ExchangeRateStatus.Active, CreatedByName = seedAdminName
            },
            new ExchangeRate
            {
                BaseCurrencyId = usd.Id, TargetCurrencyId = eur.Id, BuyRate = 0.900m, SellRate = 0.940m,
                EffectiveAt = now.AddDays(-2), CreatedAt = now.AddDays(-2),
                Source = ExchangeRateSource.Manual, Status = ExchangeRateStatus.Historical, CreatedByName = seedAdminName
            },
            new ExchangeRate
            {
                BaseCurrencyId = usd.Id, TargetCurrencyId = eur.Id, BuyRate = 0.905m, SellRate = 0.935m,
                EffectiveAt = now.AddHours(-2), CreatedAt = now.AddHours(-2),
                Source = ExchangeRateSource.Manual, Status = ExchangeRateStatus.Active, CreatedByName = seedAdminName
            },
            new ExchangeRate
            {
                BaseCurrencyId = eur.Id, TargetCurrencyId = crc.Id, BuyRate = 548.00m, SellRate = 566.00m,
                EffectiveAt = now.AddDays(-2), CreatedAt = now.AddDays(-2),
                Source = ExchangeRateSource.Manual, Status = ExchangeRateStatus.Historical, CreatedByName = seedAdminName
            },
            new ExchangeRate
            {
                BaseCurrencyId = eur.Id, TargetCurrencyId = crc.Id, BuyRate = 552.30m, SellRate = 562.90m,
                EffectiveAt = now.AddHours(-5), CreatedAt = now.AddHours(-5),
                Source = ExchangeRateSource.Manual, Status = ExchangeRateStatus.Active, CreatedByName = seedAdminName
            });

        var conversions = new List<Conversion>();
        var users = new[] { "andre", "maria", "carlos", "sofia" };
        var pairs = new[]
        {
            ("USD", "CRC", 512.45m),
            ("EUR", "USD", 1.09m),
            ("USD", "MXN", 17.20m),
            ("CRC", "USD", 0.00195m),
            ("m", "km", 0.001m),
            ("kg", "lb", 2.20462m)
        };

        var random = new Random(42);
        for (var dayOffset = 6; dayOffset >= 0; dayOffset--)
        {
            var day = now.Date.AddDays(-dayOffset);
            var count = dayOffset == 0 ? 5 : random.Next(2, 7);

            for (var i = 0; i < count; i++)
            {
                var pair = pairs[random.Next(pairs.Length)];
                var isUnit = pair.Item1 is "m" or "kg";
                var amount = isUnit
                    ? Math.Round((decimal)(random.NextDouble() * 100 + 1), 2)
                    : Math.Round((decimal)(random.NextDouble() * 500 + 10), 2);

                conversions.Add(new Conversion
                {
                    Type = isUnit ? ConversionType.Unit : ConversionType.Currency,
                    FromCode = pair.Item1,
                    ToCode = pair.Item2,
                    Amount = amount,
                    RateApplied = pair.Item3,
                    Result = Math.Round(amount * pair.Item3, 4),
                    UserName = users[random.Next(users.Length)],
                    CreatedAt = day.AddHours(random.Next(8, 20)).AddMinutes(random.Next(0, 59))
                });
            }
        }

        context.Conversions.AddRange(conversions);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUnitsAsync(ConvergexDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Units.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Units.AddRange(
            new Unit { Name = "Onza troy", Symbol = "oz t", Category = UnitCategory.Peso, ConversionFactor = 31.1035m, DecimalPrecision = 4, RoundingMode = RoundingMode.HalfUp, IsActive = true },
            new Unit { Name = "Gramo", Symbol = "g", Category = UnitCategory.Peso, ConversionFactor = 1m, DecimalPrecision = 2, RoundingMode = RoundingMode.HalfUp, IsActive = true },
            new Unit { Name = "Kilogramo", Symbol = "kg", Category = UnitCategory.Peso, ConversionFactor = 1000m, DecimalPrecision = 2, RoundingMode = RoundingMode.HalfUp, IsActive = true },
            new Unit { Name = "Metro", Symbol = "m", Category = UnitCategory.Longitud, ConversionFactor = 1m, DecimalPrecision = 2, RoundingMode = RoundingMode.HalfUp, IsActive = true },
            new Unit { Name = "Centímetro", Symbol = "cm", Category = UnitCategory.Longitud, ConversionFactor = 0.01m, DecimalPrecision = 2, RoundingMode = RoundingMode.Truncate, IsActive = true },
            new Unit { Name = "Lote de transacción", Symbol = "lote", Category = UnitCategory.Volumen, ConversionFactor = 1000m, DecimalPrecision = 0, RoundingMode = RoundingMode.Truncate, IsActive = true },
            new Unit { Name = "Fracción monetaria", Symbol = "frac", Category = UnitCategory.Volumen, ConversionFactor = 0.01m, DecimalPrecision = 2, RoundingMode = RoundingMode.HalfUp, IsActive = true });

        await context.SaveChangesAsync(cancellationToken);
    }
}
