using System.Globalization;
using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace Convergex.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IExchangeRateProviderClient _providerClient;
    private readonly IConfiguration _configuration;

    public ExchangeRateService(
        IExchangeRateRepository exchangeRateRepository,
        ICurrencyRepository currencyRepository,
        IExchangeRateProviderClient providerClient,
        IConfiguration configuration)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _currencyRepository = currencyRepository;
        _providerClient = providerClient;
        _configuration = configuration;
    }

    private string HomeCurrencyCode => _configuration["ExchangeRateApi:BaseCurrencyCode"] ?? "USD";

    private decimal DefaultSpreadPercent =>
        decimal.TryParse(_configuration["ExchangeRateApi:DefaultSpreadPercent"], NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 2m;

    public async Task<IReadOnlyList<ExchangeRateDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = await _exchangeRateRepository.GetActiveAsync(cancellationToken);
        var result = new List<ExchangeRateDto>(items.Count);
        foreach (var item in items)
        {
            result.Add(await MapAsync(item, cancellationToken));
        }

        return result;
    }

    public async Task<ExchangeRateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _exchangeRateRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<ExchangeRateDto?> GetActiveByPairAsync(int baseCurrencyId, int targetCurrencyId, CancellationToken cancellationToken = default)
    {
        var item = await _exchangeRateRepository.GetActiveByPairAsync(baseCurrencyId, targetCurrencyId, cancellationToken);
        return item is null ? null : await MapAsync(item, cancellationToken);
    }

    public async Task<PagedExchangeRatesDto> GetHistoryAsync(
        int? baseCurrencyId,
        int? targetCurrencyId,
        DateTime? fromUtc,
        DateTime? toUtc,
        ExchangeRateSource? source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var (items, totalCount) = await _exchangeRateRepository.GetHistoryAsync(
            baseCurrencyId, targetCurrencyId, fromUtc, toUtc, source, page, pageSize, cancellationToken);

        var mapped = new List<ExchangeRateDto>(items.Count);
        foreach (var item in items)
        {
            mapped.Add(await MapAsync(item, cancellationToken));
        }

        return new PagedExchangeRatesDto
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<(bool Success, string Message)> PublishManualAsync(ExchangeRateFormDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(dto, cancellationToken);
        if (validation is not null)
        {
            return (false, validation);
        }

        await PublishAsync(
            dto.BaseCurrencyId, dto.TargetCurrencyId, dto.BuyRate, dto.SellRate,
            dto.EffectiveAt, ExchangeRateSource.Manual, dto.CreatedByName, cancellationToken);

        return (true, "Tasa publicada correctamente. La tasa anterior para este par pasó a estado histórico.");
    }

    public async Task<(bool Success, string Message)> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var rate = await _exchangeRateRepository.GetByIdAsync(id, cancellationToken);
        if (rate is null)
        {
            return (false, "Tasa de cambio no encontrada.");
        }

        if (rate.Status != ExchangeRateStatus.Active)
        {
            return (false, "Solo se pueden desactivar tasas activas.");
        }

        rate.Status = ExchangeRateStatus.Inactive;
        await _exchangeRateRepository.UpdateAsync(rate, cancellationToken);
        await _exchangeRateRepository.SaveChangesAsync(cancellationToken);
        return (true, "Tasa desactivada. No se usará para conversiones hasta publicar una nueva para este par.");
    }

    public async Task<(bool Success, string Message, int Published)> SyncFromApiAsync(CancellationToken cancellationToken = default)
    {
        var currencies = await _currencyRepository.GetActiveAsync(cancellationToken);
        var home = currencies.FirstOrDefault(c => string.Equals(c.Code, HomeCurrencyCode, StringComparison.OrdinalIgnoreCase));
        if (home is null)
        {
            return (false, $"No se encontró la moneda base configurada ({HomeCurrencyCode}) entre las monedas activas.", 0);
        }

        var targets = currencies.Where(c => c.Id != home.Id).ToList();
        if (targets.Count == 0)
        {
            return (false, "No hay monedas destino activas para sincronizar.", 0);
        }

        var quote = await _providerClient.GetLatestRatesAsync(home.Code, targets.Select(t => t.Code).ToList(), cancellationToken);
        if (!quote.Success)
        {
            return (false, quote.ErrorMessage ?? "No se pudo sincronizar con el proveedor externo.", 0);
        }

        var spread = DefaultSpreadPercent / 100m;
        var published = 0;

        foreach (var target in targets)
        {
            if (!quote.Rates.TryGetValue(target.Code, out var mid) || mid <= 0)
            {
                continue;
            }

            var buyRate = Math.Round(mid * (1 - spread / 2), 6);
            var sellRate = Math.Round(mid * (1 + spread / 2), 6);

            await PublishAsync(
                home.Id, target.Id, buyRate, sellRate,
                DateTime.UtcNow, ExchangeRateSource.Api, "Sincronización automática", cancellationToken);
            published++;
        }

        return published > 0
            ? (true, $"Se publicaron {published} tasa(s) desde el proveedor externo.", published)
            : (false, "El proveedor externo no devolvió tasas para las monedas configuradas.", 0);
    }

    private async Task PublishAsync(
        int baseCurrencyId,
        int targetCurrencyId,
        decimal buyRate,
        decimal sellRate,
        DateTime effectiveAt,
        ExchangeRateSource source,
        string? createdByName,
        CancellationToken cancellationToken)
    {
        var existingActive = await _exchangeRateRepository.GetActiveByPairAsync(baseCurrencyId, targetCurrencyId, cancellationToken);
        if (existingActive is not null)
        {
            existingActive.Status = ExchangeRateStatus.Historical;
            await _exchangeRateRepository.UpdateAsync(existingActive, cancellationToken);
        }

        await _exchangeRateRepository.AddAsync(new ExchangeRate
        {
            BaseCurrencyId = baseCurrencyId,
            TargetCurrencyId = targetCurrencyId,
            BuyRate = buyRate,
            SellRate = sellRate,
            EffectiveAt = effectiveAt,
            CreatedAt = DateTime.UtcNow,
            Source = source,
            Status = ExchangeRateStatus.Active,
            CreatedByName = createdByName
        }, cancellationToken);

        await _exchangeRateRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> ValidateAsync(ExchangeRateFormDto dto, CancellationToken cancellationToken)
    {
        if (dto.BaseCurrencyId <= 0 || dto.TargetCurrencyId <= 0)
        {
            return "Debes seleccionar moneda origen y destino.";
        }

        if (dto.BaseCurrencyId == dto.TargetCurrencyId)
        {
            return "La moneda origen y destino deben ser diferentes.";
        }

        if (dto.BuyRate <= 0 || dto.SellRate <= 0)
        {
            return "Las tasas de compra y venta deben ser mayores que cero.";
        }

        if (dto.SellRate <= dto.BuyRate)
        {
            return "La tasa de venta debe ser mayor que la tasa de compra (spread inválido).";
        }

        var baseCurrency = await _currencyRepository.GetByIdAsync(dto.BaseCurrencyId, cancellationToken);
        var targetCurrency = await _currencyRepository.GetByIdAsync(dto.TargetCurrencyId, cancellationToken);

        if (baseCurrency is null || targetCurrency is null)
        {
            return "Una de las monedas seleccionadas no existe.";
        }

        if (!baseCurrency.IsActive || !targetCurrency.IsActive)
        {
            return "Solo se pueden usar monedas activas.";
        }

        return null;
    }

    private async Task<ExchangeRateDto> MapAsync(ExchangeRate rate, CancellationToken cancellationToken)
    {
        var previous = await _exchangeRateRepository.GetPreviousAsync(
            rate.BaseCurrencyId, rate.TargetCurrencyId, rate.EffectiveAt, rate.Id, cancellationToken);

        decimal? variation = null;
        if (previous is not null)
        {
            var previousMid = (previous.BuyRate + previous.SellRate) / 2;
            var currentMid = (rate.BuyRate + rate.SellRate) / 2;
            if (previousMid != 0)
            {
                variation = Math.Round((currentMid - previousMid) * 100m / previousMid, 2);
            }
        }

        return new ExchangeRateDto
        {
            Id = rate.Id,
            BaseCurrencyId = rate.BaseCurrencyId,
            TargetCurrencyId = rate.TargetCurrencyId,
            BaseCode = rate.BaseCurrency.Code,
            TargetCode = rate.TargetCurrency.Code,
            BuyRate = rate.BuyRate,
            SellRate = rate.SellRate,
            EffectiveAt = rate.EffectiveAt,
            CreatedAt = rate.CreatedAt,
            Source = rate.Source,
            Status = rate.Status,
            CreatedByName = rate.CreatedByName,
            VariationPercent = variation
        };
    }
}
