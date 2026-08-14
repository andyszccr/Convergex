using Convergex.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Convergex.Infrastructure.BackgroundServices;

public class ExchangeRateSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExchangeRateSyncBackgroundService> _logger;

    public ExchangeRateSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ExchangeRateSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = true;
        if (bool.TryParse(_configuration["ExchangeRateSync:Enabled"], out var parsedEnabled))
        {
            enabled = parsedEnabled;
        }

        if (!enabled)
        {
            _logger.LogInformation("Sincronización automática de tasas de cambio deshabilitada por configuración.");
            return;
        }

        var intervalMinutes = 60;
        if (int.TryParse(_configuration["ExchangeRateSync:IntervalMinutes"], out var parsedInterval) && parsedInterval > 0)
        {
            intervalMinutes = parsedInterval;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

        do
        {
            await RunSyncAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
            var result = await exchangeRateService.SyncFromApiAsync(cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Sincronización automática de tasas de cambio completada: {Published} tasa(s) publicadas.", result.Published);
            }
            else
            {
                _logger.LogWarning("Sincronización automática de tasas de cambio no completada: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la sincronización automática de tasas de cambio.");
        }
    }
}
