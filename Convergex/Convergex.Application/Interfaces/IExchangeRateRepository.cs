using Convergex.Domain.Entities;

namespace Convergex.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetTopAsync(int take, CancellationToken cancellationToken = default);
}
