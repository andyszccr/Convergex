using Convergex.Application.DTOs.Currencies;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;

namespace Convergex.Application.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;

    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _currencyRepository.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = await _currencyRepository.GetActiveAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _currencyRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CurrencyFormDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
        {
            return (false, validation);
        }

        if (await _currencyRepository.CodeExistsAsync(dto.Code, null, cancellationToken))
        {
            return (false, "Ya existe una moneda con ese código.");
        }

        await _currencyRepository.AddAsync(new Currency
        {
            Code = dto.Code.Trim().ToUpperInvariant(),
            Name = dto.Name.Trim(),
            Symbol = dto.Symbol.Trim(),
            IsActive = dto.IsActive
        }, cancellationToken);

        await _currencyRepository.SaveChangesAsync(cancellationToken);
        return (true, "Moneda creada correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(CurrencyFormDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
        {
            return (false, validation);
        }

        var currency = await _currencyRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (currency is null)
        {
            return (false, "Moneda no encontrada.");
        }

        if (await _currencyRepository.CodeExistsAsync(dto.Code, dto.Id, cancellationToken))
        {
            return (false, "Ya existe una moneda con ese código.");
        }

        currency.Code = dto.Code.Trim().ToUpperInvariant();
        currency.Name = dto.Name.Trim();
        currency.Symbol = dto.Symbol.Trim();
        currency.IsActive = dto.IsActive;

        await _currencyRepository.UpdateAsync(currency, cancellationToken);
        await _currencyRepository.SaveChangesAsync(cancellationToken);
        return (true, "Moneda actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var currency = await _currencyRepository.GetByIdAsync(id, cancellationToken);
        if (currency is null)
        {
            return (false, "Moneda no encontrada.");
        }

        try
        {
            await _currencyRepository.DeleteAsync(currency, cancellationToken);
            await _currencyRepository.SaveChangesAsync(cancellationToken);
            return (true, "Moneda eliminada correctamente.");
        }
        catch
        {
            return (false, "No se puede eliminar la moneda porque está relacionada con tasas de cambio.");
        }
    }

    public async Task<IReadOnlyList<CurrencySuggestionDto>> GetSuggestionsAsync(
        CancellationToken cancellationToken = default)
    {
        var existing = await _currencyRepository.GetAllAsync(cancellationToken);
        var codes = existing
            .Select(c => c.Code.ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return CurrencyCatalog.Common
            .Where(c => !codes.Contains(c.Code))
            .ToList();
    }

    private static string? Validate(CurrencyFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || dto.Code.Trim().Length is < 3 or > 10)
        {
            return "El código debe tener entre 3 y 10 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "El nombre es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(dto.Symbol))
        {
            return "El símbolo es obligatorio.";
        }

        return null;
    }

    private static CurrencyDto Map(Currency currency) => new()
    {
        Id = currency.Id,
        Code = currency.Code,
        Name = currency.Name,
        Symbol = currency.Symbol,
        IsActive = currency.IsActive
    };
}
