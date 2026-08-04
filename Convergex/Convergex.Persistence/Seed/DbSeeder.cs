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
        context.ExchangeRates.AddRange(
            new ExchangeRate
            {
                BaseCurrencyId = usd.Id,
                TargetCurrencyId = crc.Id,
                Rate = 512.45m,
                UpdatedAt = now.AddMinutes(-15)
            },
            new ExchangeRate
            {
                BaseCurrencyId = usd.Id,
                TargetCurrencyId = eur.Id,
                Rate = 0.92m,
                UpdatedAt = now.AddHours(-2)
            },
            new ExchangeRate
            {
                BaseCurrencyId = eur.Id,
                TargetCurrencyId = crc.Id,
                Rate = 557.10m,
                UpdatedAt = now.AddHours(-5)
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
}
