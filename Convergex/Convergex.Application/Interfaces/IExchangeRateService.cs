using Convergex.Application.DTOs.ExchangeRates;

namespace Convergex.Application.Interfaces;

public interface IExchangeRateService
{
    Task<IReadOnlyList<ExchangeRateDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExchangeRateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExchangeRateDto?> GetByPairAsync(int baseCurrencyId, int targetCurrencyId, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CreateAsync(ExchangeRateFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateAsync(ExchangeRateFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
