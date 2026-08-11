using Convergex.Application.DTOs.Conversions;
using Convergex.Application.DTOs.ExternalApis;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Convergex.Application.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IConversionRepository _conversionRepository;
    private readonly IExternalExchangeRateService _externalExchangeRateService;
    private readonly ILogger<CurrencyConversionService> _logger;

    public CurrencyConversionService(
        ICurrencyRepository currencyRepository,
        IExchangeRateRepository exchangeRateRepository,
        IConversionRepository conversionRepository,
        IExternalExchangeRateService externalExchangeRateService,
        ILogger<CurrencyConversionService> logger)
    {
        _currencyRepository = currencyRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _conversionRepository = conversionRepository;
        _externalExchangeRateService = externalExchangeRateService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, CurrencyConversionResultDto? Result)> ConvertAsync(
        CurrencyConversionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return (false, "El monto debe ser mayor que cero.", null);
        }

        if (request.FromCurrencyId == request.ToCurrencyId)
        {
            return (false, "Selecciona monedas diferentes para convertir.", null);
        }

        var from = await _currencyRepository.GetByIdAsync(request.FromCurrencyId, cancellationToken);
        var to = await _currencyRepository.GetByIdAsync(request.ToCurrencyId, cancellationToken);

        if (from is null || to is null)
        {
            return (false, "Una de las monedas seleccionadas no existe.", null);
        }

        if (!from.IsActive || !to.IsActive)
        {
            return (false, "Solo se pueden convertir monedas activas.", null);
        }

        var rateInfo = await ResolveRateAsync(from.Id, to.Id, cancellationToken);
        if (rateInfo is null)
        {
            return (false, $"No hay tasa de cambio disponible para {from.Code}/{to.Code}.", null);
        }

        var resultValue = Math.Round(request.Amount * rateInfo.Value, 6);

        var conversion = new Conversion
        {
            Type = ConversionType.Currency,
            FromCode = from.Code,
            ToCode = to.Code,
            Amount = request.Amount,
            RateApplied = rateInfo.Value,
            Result = resultValue,
            UserName = request.UserName,
            CreatedAt = DateTime.UtcNow
        };

        await _conversionRepository.AddAsync(conversion, cancellationToken);
        await _conversionRepository.SaveChangesAsync(cancellationToken);

        return (true, "Conversión realizada y guardada en el historial.", new CurrencyConversionResultDto
        {
            ConversionId = conversion.Id,
            FromCode = from.Code,
            ToCode = to.Code,
            Amount = request.Amount,
            Rate = rateInfo.Value,
            Result = resultValue,
            CreatedAt = conversion.CreatedAt
        });
    }

    public async Task<IReadOnlyList<ConversionHistoryItemDto>> GetHistoryAsync(
        ConversionType? type = null,
        string? userName = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _conversionRepository.GetHistoryAsync(type, userName, fromUtc, toUtc, cancellationToken);
        return items.Select(c => new ConversionHistoryItemDto
        {
            Id = c.Id,
            Type = c.Type.ToString(),
            FromCode = c.FromCode,
            ToCode = c.ToCode,
            Amount = c.Amount,
            Result = c.Result,
            RateApplied = c.RateApplied,
            UserName = c.UserName,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<(IReadOnlyList<ConversionHistoryItemDto> Items, int Total)> GetHistoryPagedAsync(
        ConversionType? type = null,
        string? userName = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _conversionRepository.GetHistoryPagedAsync(type, userName, fromUtc, toUtc, page, pageSize, cancellationToken);

        var items = result.Items.Select(c => new ConversionHistoryItemDto
        {
            Id = c.Id,
            Type = c.Type.ToString(),
            FromCode = c.FromCode,
            ToCode = c.ToCode,
            Amount = c.Amount,
            Result = c.Result,
            RateApplied = c.RateApplied,
            UserName = c.UserName,
            CreatedAt = c.CreatedAt
        }).ToList();

        return (items, result.Total);
    }

    private async Task<decimal?> ResolveRateAsync(int fromId, int toId, CancellationToken cancellationToken)
    {
        // Intentar obtener tasa directa de la base de datos
        var direct = await _exchangeRateRepository.GetByPairAsync(fromId, toId, cancellationToken);
        if (direct is not null)
        {
            _logger.LogInformation("Tasa encontrada en BD: {From}/{To} = {Rate}", fromId, toId, direct.Rate);
            return direct.Rate;
        }

        // Intentar obtener tasa inversa de la base de datos
        var inverse = await _exchangeRateRepository.GetByPairAsync(toId, fromId, cancellationToken);
        if (inverse is not null && inverse.Rate != 0)
        {
            var rate = Math.Round(1m / inverse.Rate, 8);
            _logger.LogInformation("Tasa inversa encontrada en BD: {From}/{To} = {Rate}", fromId, toId, rate);
            return rate;
        }

        // Si no hay tasa en BD, consultar API externa (solo para USD/CRC)
        _logger.LogInformation("Consultando API externa para {From}/{To}", fromId, toId);
        var externalRate = await GetExternalRateAsync(fromId, toId, cancellationToken);
        if (externalRate.HasValue)
        {
            _logger.LogInformation("Tasa obtenida de API externa: {From}/{To} = {Rate}", fromId, toId, externalRate.Value);
            return externalRate.Value;
        }

        _logger.LogWarning("No se encontró tasa para {From}/{To}", fromId, toId);
        return null;
    }

    private async Task<decimal?> GetExternalRateAsync(int fromId, int toId, CancellationToken cancellationToken)
    {
        try
        {
            // Obtener los códigos de moneda
            var from = await _currencyRepository.GetByIdAsync(fromId, cancellationToken);
            var to = await _currencyRepository.GetByIdAsync(toId, cancellationToken);

            if (from is null || to is null)
            {
                return null;
            }

            // Solo soportamos USD/CRC por ahora desde la API externa
            if (from.Code == "USD" && to.Code == "CRC")
            {
                var data = await _externalExchangeRateService.GetTdcRateAsync(cancellationToken);
                if (data is not null)
                {
                    // Usar la tasa de venta para comprar USD con CRC
                    return Math.Round(data.Venta, 6);
                }
            }
            else if (from.Code == "CRC" && to.Code == "USD")
            {
                var data = await _externalExchangeRateService.GetTdcRateAsync(cancellationToken);
                if (data is not null)
                {
                    // Usar la tasa de compra para vender USD por CRC
                    return Math.Round(1m / data.Compra, 6);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar API externa para tasas de cambio");
            return null;
        }
    }
}
