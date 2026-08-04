using Convergex.Domain.Entities;

namespace Convergex.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetTopAsync(int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetByPairAsync(int baseCurrencyId, int targetCurrencyId, CancellationToken cancellationToken = default);
    Task AddAsync(ExchangeRate rate, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExchangeRate rate, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExchangeRate rate, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
