using Convergex.Application.DTOs.Units;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Services;

public class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepository;

    public UnitService(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<IReadOnlyList<UnitDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitRepository.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<UnitDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitRepository.GetActiveAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<UnitDto>> GetActiveByCategoryAsync(
        UnitCategory category,
        CancellationToken cancellationToken = default)
    {
        var items = await _unitRepository.GetActiveByCategoryAsync(category, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<UnitDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    private static UnitDto Map(Unit unit) => new()
    {
        Id = unit.Id,
        Code = unit.Code,
        Name = unit.Name,
        Symbol = unit.Symbol,
        Category = unit.Category,
        FactorToBase = unit.FactorToBase,
        IsActive = unit.IsActive
    };
}