using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetActiveByPairAsync(int baseCurrencyId, int targetCurrencyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetLatestActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetTopActiveAsync(int take, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetPreviousAsync(
        int baseCurrencyId,
        int targetCurrencyId,
        DateTime beforeEffectiveAt,
        int excludeId,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ExchangeRate> Items, int TotalCount)> GetHistoryAsync(
        int? baseCurrencyId,
        int? targetCurrencyId,
        DateTime? fromUtc,
        DateTime? toUtc,
        ExchangeRateSource? source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(ExchangeRate rate, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExchangeRate rate, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
