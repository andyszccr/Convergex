using Convergex.Application;
using Convergex.Application.Interfaces;
using Convergex.Infrastructure.BackgroundServices;
using Convergex.Infrastructure.ExternalServices;
using Convergex.Infrastructure.Reporting;
using Convergex.Persistence.Auditing;
using Convergex.Persistence.Context;
using Convergex.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace Convergex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=convergex.db";

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<ConvergexDbContext>((sp, options) =>
            options.UseSqlite(connectionString)
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IConversionRepository, ConversionRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

        services.AddScoped<IPdfReportGenerator, QuestPdfReportGenerator>();
        services.AddScoped<IExcelReportGenerator, ClosedXmlReportGenerator>();

        services.AddHttpClient<IExchangeRateProviderClient, ExchangeRateApiClient>((sp, client) =>
        {
            var baseUrl = configuration["ExchangeRateApi:BaseUrl"] ?? "https://open.er-api.com/v6/latest/";
            var timeoutSeconds = int.TryParse(configuration["ExchangeRateApi:TimeoutSeconds"], out var seconds) ? seconds : 10;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        services.AddHostedService<ExchangeRateSyncBackgroundService>();

        services.AddApplication();

        return services;
    }
}
