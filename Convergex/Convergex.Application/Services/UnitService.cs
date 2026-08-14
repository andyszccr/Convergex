using Convergex.Application.Common;
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

    public async Task<PagedUnitsDto> GetPagedAsync(
        UnitCategory? category,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var (items, totalCount) = await _unitRepository.GetPagedAsync(category, search, page, pageSize, cancellationToken);

        return new PagedUnitsDto
        {
            Items = items.Select(Map).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
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

    public async Task<(bool Success, string Message)> CreateAsync(UnitFormDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
        {
            return (false, validation);
        }

        if (await _unitRepository.NameExistsAsync(dto.Name, null, cancellationToken))
        {
            return (false, "Ya existe una unidad con ese nombre.");
        }

        if (await _unitRepository.SymbolExistsAsync(dto.Symbol, null, cancellationToken))
        {
            return (false, "Ya existe una unidad con ese símbolo.");
        }

        await _unitRepository.AddAsync(new Unit
        {
            Code = dto.Symbol.Trim(),
            Name = dto.Name.Trim(),
            Symbol = dto.Symbol.Trim(),
            Category = dto.Category,
            FactorToBase = dto.ConversionFactor,
            DecimalPrecision = dto.DecimalPrecision,
            RoundingMode = dto.RoundingMode,
            IsActive = dto.IsActive
        }, cancellationToken);

        await _unitRepository.SaveChangesAsync(cancellationToken);
        return (true, "Unidad creada correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UnitFormDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto);
        if (validation is not null)
        {
            return (false, validation);
        }

        var unit = await _unitRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (unit is null)
        {
            return (false, "Unidad no encontrada.");
        }

        if (await _unitRepository.NameExistsAsync(dto.Name, dto.Id, cancellationToken))
        {
            return (false, "Ya existe una unidad con ese nombre.");
        }

        if (await _unitRepository.SymbolExistsAsync(dto.Symbol, dto.Id, cancellationToken))
        {
            return (false, "Ya existe una unidad con ese símbolo.");
        }

        unit.Code = dto.Symbol.Trim();
        unit.Name = dto.Name.Trim();
        unit.Symbol = dto.Symbol.Trim();
        unit.Category = dto.Category;
        unit.FactorToBase = dto.ConversionFactor;
        unit.DecimalPrecision = dto.DecimalPrecision;
        unit.RoundingMode = dto.RoundingMode;
        unit.IsActive = dto.IsActive;

        await _unitRepository.UpdateAsync(unit, cancellationToken);
        await _unitRepository.SaveChangesAsync(cancellationToken);
        return (true, "Unidad actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
        if (unit is null)
        {
            return (false, "Unidad no encontrada.");
        }

        try
        {
            await _unitRepository.DeleteAsync(unit, cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);
            return (true, "Unidad eliminada correctamente.");
        }
        catch
        {
            return (false, "No se puede eliminar la unidad porque está relacionada con operaciones existentes.");
        }
    }

    private static string? Validate(UnitFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "El nombre es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(dto.Symbol) || dto.Symbol.Trim().Length > 10)
        {
            return "El símbolo es obligatorio y debe tener como máximo 10 caracteres.";
        }

        if (dto.ConversionFactor <= 0)
        {
            return "El factor de conversión debe ser mayor que cero.";
        }

        if (dto.DecimalPrecision is < 0 or > 8)
        {
            return "La precisión decimal debe estar entre 0 y 8.";
        }

        return null;
    }

    private static UnitDto Map(Unit unit) => new()
    {
        Id = unit.Id,
        Code = unit.Code,
        Name = unit.Name,
        Symbol = unit.Symbol,
        Category = unit.Category,
        FactorToBase = unit.FactorToBase,
        ConversionFactor = unit.FactorToBase,
        FormattedConversionFactor = DecimalFormatter.Format(unit.FactorToBase, unit.DecimalPrecision, unit.RoundingMode),
        DecimalPrecision = unit.DecimalPrecision,
        RoundingMode = unit.RoundingMode,
        IsActive = unit.IsActive
    };
}
