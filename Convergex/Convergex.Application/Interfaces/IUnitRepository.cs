using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IUnitRepository
{
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Unit> Items, int TotalCount)> GetPagedAsync(
        UnitCategory? category,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SymbolExistsAsync(string symbol, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);
    Task UpdateAsync(Unit unit, CancellationToken cancellationToken = default);
    Task DeleteAsync(Unit unit, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
