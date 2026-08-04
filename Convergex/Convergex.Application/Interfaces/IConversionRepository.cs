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
    Task<IReadOnlyList<Conversion>> GetHistoryAsync(
        ConversionType? type = null,
        string? userName = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Conversion conversion, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
