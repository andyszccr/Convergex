using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IUnitRepository
{
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> GetActiveByCategoryAsync(UnitCategory category, CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Unit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}