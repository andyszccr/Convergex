using System.Text.Json;
using Convergex.Application.DTOs.Audit;
using Convergex.Application.DTOs.Conversions;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IConversionRepository _conversionRepository;
    private readonly IAuditService _auditService;

    public CurrencyConversionService(
        ICurrencyRepository currencyRepository,
        IExchangeRateRepository exchangeRateRepository,
        IConversionRepository conversionRepository,
        IAuditService auditService)
    {
        _currencyRepository = currencyRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _conversionRepository = conversionRepository;
        _auditService = auditService;
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

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            Action = AuditAction.Conversion,
            EntityName = "Conversion",
            EntityId = conversion.Id.ToString(),
            Detail = $"Conversión {from.Code} → {to.Code}: {request.Amount:N2} → {resultValue:N4} (tasa {rateInfo.Value:N6})",
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

    private async Task<decimal?> ResolveRateAsync(int fromId, int toId, CancellationToken cancellationToken)
    {
        // Par almacenado Base->Destino: convertir Base->Destino es el cambista comprando la Base (BuyRate);
        // convertir Destino->Base (par inverso) es el cambista vendiendo la Base (1 / SellRate).
        var direct = await _exchangeRateRepository.GetActiveByPairAsync(fromId, toId, cancellationToken);
        if (direct is not null)
        {
            return direct.BuyRate;
        }

        var inverse = await _exchangeRateRepository.GetActiveByPairAsync(toId, fromId, cancellationToken);
        if (inverse is not null && inverse.SellRate != 0)
        {
            return Math.Round(1m / inverse.SellRate, 8);
        }

        return null;
    }
}
