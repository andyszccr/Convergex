using Convergex.Application.DTOs.Units;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IUnitService
{
    Task<IReadOnlyList<UnitDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnitDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnitDto>> GetActiveByCategoryAsync(UnitCategory category, CancellationToken cancellationToken = default);
    Task<UnitDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}