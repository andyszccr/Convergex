using Convergex.Application;
using Convergex.Application.Interfaces;
using Convergex.Application.Services;
using Convergex.Persistence.Context;
using Convergex.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Convergex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=convergex.db";

        services.AddDbContext<ConvergexDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IConversionRepository, ConversionRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddApplication();

        return services;
    }
}
