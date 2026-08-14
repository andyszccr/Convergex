using Convergex.Application.Interfaces;
using Convergex.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Convergex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
        services.AddHttpClient<IExternalExchangeRateService, ExternalExchangeRateService>();
        services.AddScoped<IUnitConversionService, UnitConversionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
