using Convergex.Application.DTOs.Units;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IUnitService
{
    Task<PagedUnitsDto> GetPagedAsync(
        UnitCategory? category,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnitDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<UnitDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CreateAsync(UnitFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> UpdateAsync(UnitFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
