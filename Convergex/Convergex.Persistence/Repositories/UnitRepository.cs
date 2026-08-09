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

    public async Task<IReadOnlyList<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Units.AsNoTracking()
            .OrderBy(u => u.Category)
            .ThenBy(u => u.Code)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Unit>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Units.AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Category)
            .ThenBy(u => u.Code)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Unit>> GetActiveByCategoryAsync(
        UnitCategory category,
        CancellationToken cancellationToken = default)
        => await _context.Units.AsNoTracking()
            .Where(u => u.IsActive && u.Category == category)
            .OrderBy(u => u.Code)
            .ToListAsync(cancellationToken);

    public Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Units.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Unit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => _context.Units.FirstOrDefaultAsync(u => u.Code == code, cancellationToken);
}