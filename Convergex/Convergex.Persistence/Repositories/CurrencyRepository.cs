using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly ConvergexDbContext _context;

    public CurrencyRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
        => _context.Currencies.CountAsync(c => c.IsActive, cancellationToken);

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Currencies.AsNoTracking().OrderBy(c => c.Code).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Currency>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Currencies.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);

    public Task<Currency?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Currencies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => _context.Currencies.FirstOrDefaultAsync(c => c.Code == code.ToUpper(), cancellationToken);

    public Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var query = _context.Currencies.Where(c => c.Code == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Currency currency, CancellationToken cancellationToken = default)
        => await _context.Currencies.AddAsync(currency, cancellationToken);

    public Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        _context.Currencies.Update(currency);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Currency currency, CancellationToken cancellationToken = default)
    {
        _context.Currencies.Remove(currency);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
