using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IConversionRepository
{
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<int> CountFromAsync(DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversion>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<DateOnly, int>> GetDailyCountsAsync(DateTime fromUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<ConversionType, int>> GetCountsByTypeAsync(CancellationToken cancellationToken = default);
}
