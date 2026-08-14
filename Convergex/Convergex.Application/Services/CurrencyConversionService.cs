using System.Text.Json;
using Convergex.Application.DTOs.Audit;
using Convergex.Application.DTOs.Conversions;
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
    private readonly IAuditService _auditService;
    private readonly ILogger<CurrencyConversionService> _logger;

    public CurrencyConversionService(
        ICurrencyRepository currencyRepository,
        IExchangeRateRepository exchangeRateRepository,
        IConversionRepository conversionRepository,
        IExternalExchangeRateService externalExchangeRateService,
        IAuditService auditService,
        ILogger<CurrencyConversionService> logger)
    {
        _currencyRepository = currencyRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _conversionRepository = conversionRepository;
        _externalExchangeRateService = externalExchangeRateService;
        _auditService = auditService;
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

        var rateInfo = await GetRateQuoteAsync(from.Id, to.Id, cancellationToken);
        if (rateInfo is null)
        {
            return (false, $"No hay tasa de cambio disponible para {from.Code}/{to.Code}.", null);
        }

        var resultValue = Math.Round(request.Amount * rateInfo.Rate, 6);

        var conversion = new Conversion
        {
            Type = ConversionType.Currency,
            FromCode = from.Code,
            ToCode = to.Code,
            Amount = request.Amount,
            RateApplied = rateInfo.Rate,
            Result = resultValue,
            UserName = request.UserName,
            CreatedAt = DateTime.UtcNow
        };

        await _conversionRepository.AddAsync(conversion, cancellationToken);
        await _conversionRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            Action = AuditAction.Conversion,
            EntityName = "Conversion",
            EntityId = conversion.Id.ToString(),
            Detail = $"Conversión {from.Code} → {to.Code}: {request.Amount:N2} → {resultValue:N4} (tasa {rateInfo.Rate:N6})",
            NewValues = JsonSerializer.Serialize(new
            {
                conversion.FromCode,
                conversion.ToCode,
                conversion.Amount,
                conversion.RateApplied,
                conversion.Result,
                conversion.UserName
            }),
            Status = AuditStatus.Success,
            UserName = request.UserName
        }, cancellationToken);

        return (true, "Conversión realizada y guardada en el historial.", new CurrencyConversionResultDto
        {
            ConversionId = conversion.Id,
            FromCode = from.Code,
            ToCode = to.Code,
            Amount = request.Amount,
            Rate = rateInfo.Rate,
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

    public async Task<RateQuoteDto?> GetRateQuoteAsync(int fromCurrencyId, int toCurrencyId, CancellationToken cancellationToken = default)
    {
        var direct = await _exchangeRateRepository.GetActiveByPairAsync(fromCurrencyId, toCurrencyId, cancellationToken);
        if (direct is not null)
        {
            _logger.LogInformation("Tasa encontrada en BD: {From}/{To} = {Rate}", fromCurrencyId, toCurrencyId, direct.BuyRate);
            return new RateQuoteDto { Rate = direct.BuyRate, Source = "Base de datos" };
        }

        var inverse = await _exchangeRateRepository.GetActiveByPairAsync(toCurrencyId, fromCurrencyId, cancellationToken);
        if (inverse is not null && inverse.SellRate != 0)
        {
            var rate = Math.Round(1m / inverse.SellRate, 8);
            _logger.LogInformation("Tasa inversa encontrada en BD: {From}/{To} = {Rate}", fromCurrencyId, toCurrencyId, rate);
            return new RateQuoteDto { Rate = rate, Source = "Base de datos (inversa)" };
        }

        _logger.LogInformation("Consultando API externa para {From}/{To}", fromCurrencyId, toCurrencyId);
        var externalQuote = await GetExternalRateQuoteAsync(fromCurrencyId, toCurrencyId, cancellationToken);
        if (externalQuote is not null)
        {
            _logger.LogInformation("Tasa obtenida de API externa: {From}/{To} = {Rate}", fromCurrencyId, toCurrencyId, externalQuote.Rate);
            return externalQuote;
        }

        _logger.LogWarning("No se encontró tasa para {From}/{To}", fromCurrencyId, toCurrencyId);
        return null;
    }

    private async Task<RateQuoteDto?> GetExternalRateQuoteAsync(int fromId, int toId, CancellationToken cancellationToken)
    {
        try
        {
            var from = await _currencyRepository.GetByIdAsync(fromId, cancellationToken);
            var to = await _currencyRepository.GetByIdAsync(toId, cancellationToken);

            if (from is null || to is null)
            {
                return null;
            }

            if (from.Code == "USD" && to.Code == "CRC")
            {
                var data = await _externalExchangeRateService.GetTdcRateAsync(cancellationToken);
                if (data is not null)
                {
                    return new RateQuoteDto
                    {
                        Rate = Math.Round(data.Venta, 6),
                        Source = "API Externa (TDC)",
                        HasExternalRate = true,
                        ExternalCompraRate = data.Compra,
                        ExternalVentaRate = data.Venta,
                        ExternalRateDate = data.VentaDate
                    };
                }
            }
            else if (from.Code == "CRC" && to.Code == "USD")
            {
                var data = await _externalExchangeRateService.GetTdcRateAsync(cancellationToken);
                if (data is not null)
                {
                    return new RateQuoteDto
                    {
                        Rate = Math.Round(1m / data.Compra, 6),
                        Source = "API Externa (TDC)",
                        HasExternalRate = true,
                        ExternalCompraRate = data.Compra,
                        ExternalVentaRate = data.Venta,
                        ExternalRateDate = data.VentaDate
                    };
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
