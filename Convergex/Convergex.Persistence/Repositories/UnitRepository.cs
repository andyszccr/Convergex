using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly ConvergexDbContext _context;

    public UnitRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
        => _context.Units.CountAsync(u => u.IsActive, cancellationToken);

    public async Task<(IReadOnlyList<Unit> Items, int TotalCount)> GetPagedAsync(
        UnitCategory? category,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Units.AsNoTracking().AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(u => u.Category == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => EF.Functions.Like(u.Name, $"%{term}%") || EF.Functions.Like(u.Symbol, $"%{term}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.Category).ThenBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Unit>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Units.AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Category).ThenBy(u => u.Name)
            .ToListAsync(cancellationToken);

    public Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Units.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        var query = _context.Units.Where(u => u.Name == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> SymbolExistsAsync(string symbol, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = symbol.Trim();
        var query = _context.Units.Where(u => u.Symbol == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Unit unit, CancellationToken cancellationToken = default)
        => await _context.Units.AddAsync(unit, cancellationToken);

    public Task UpdateAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        _context.Units.Update(unit);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        _context.Units.Remove(unit);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
