using Convergex.Application.DTOs.Currencies;

namespace Convergex.Application.Interfaces;

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurrencyDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CreateAsync(CurrencyFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateAsync(CurrencyFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurrencySuggestionDto>> GetSuggestionsAsync(CancellationToken cancellationToken = default);
}
