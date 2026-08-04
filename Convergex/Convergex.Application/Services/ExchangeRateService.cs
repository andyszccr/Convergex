using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;

namespace Convergex.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ICurrencyRepository _currencyRepository;

    public ExchangeRateService(
        IExchangeRateRepository exchangeRateRepository,
        ICurrencyRepository currencyRepository)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<IReadOnlyList<ExchangeRateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _exchangeRateRepository.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<ExchangeRateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _exchangeRateRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<ExchangeRateDto?> GetByPairAsync(
        int baseCurrencyId,
        int targetCurrencyId,
        CancellationToken cancellationToken = default)
    {
        var item = await _exchangeRateRepository.GetByPairAsync(baseCurrencyId, targetCurrencyId, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<(bool Success, string Message)> CreateAsync(
        ExchangeRateFormDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(dto, cancellationToken);
        if (validation is not null)
        {
            return (false, validation);
        }

        var existing = await _exchangeRateRepository.GetByPairAsync(dto.BaseCurrencyId, dto.TargetCurrencyId, cancellationToken);
        if (existing is not null)
        {
            return (false, "Ya existe una tasa para ese par. Edítala en su lugar.");
        }

        await _exchangeRateRepository.AddAsync(new ExchangeRate
        {
            BaseCurrencyId = dto.BaseCurrencyId,
            TargetCurrencyId = dto.TargetCurrencyId,
            Rate = dto.Rate,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _exchangeRateRepository.SaveChangesAsync(cancellationToken);
        return (true, "Tasa de cambio creada correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(
        ExchangeRateFormDto dto,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(dto, cancellationToken);
        if (validation is not null)
        {
            return (false, validation);
        }

        var rate = await _exchangeRateRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (rate is null)
        {
            return (false, "Tasa de cambio no encontrada.");
        }

        var existing = await _exchangeRateRepository.GetByPairAsync(dto.BaseCurrencyId, dto.TargetCurrencyId, cancellationToken);
        if (existing is not null && existing.Id != dto.Id)
        {
            return (false, "Ya existe otra tasa para ese par.");
        }

        rate.BaseCurrencyId = dto.BaseCurrencyId;
        rate.TargetCurrencyId = dto.TargetCurrencyId;
        rate.Rate = dto.Rate;
        rate.UpdatedAt = DateTime.UtcNow;

        await _exchangeRateRepository.UpdateAsync(rate, cancellationToken);
        await _exchangeRateRepository.SaveChangesAsync(cancellationToken);
        return (true, "Tasa de cambio actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rate = await _exchangeRateRepository.GetByIdAsync(id, cancellationToken);
        if (rate is null)
        {
            return (false, "Tasa de cambio no encontrada.");
        }

        await _exchangeRateRepository.DeleteAsync(rate, cancellationToken);
        await _exchangeRateRepository.SaveChangesAsync(cancellationToken);
        return (true, "Tasa de cambio eliminada correctamente.");
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

        if (dto.Rate <= 0)
        {
            return "La tasa debe ser mayor que cero.";
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

    private static ExchangeRateDto Map(ExchangeRate rate) => new()
    {
        Id = rate.Id,
        BaseCurrencyId = rate.BaseCurrencyId,
        TargetCurrencyId = rate.TargetCurrencyId,
        BaseCode = rate.BaseCurrency.Code,
        TargetCode = rate.TargetCurrency.Code,
        Rate = rate.Rate,
        UpdatedAt = rate.UpdatedAt
    };
}
