using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ConvergexDbContext _context;

    public ExchangeRateRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public Task<ExchangeRate?> GetLatestAsync(CancellationToken cancellationToken = default)
        => Query().OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExchangeRate>> GetTopAsync(int take, CancellationToken cancellationToken = default)
        => await Query().OrderByDescending(x => x.UpdatedAt).Take(take).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Query().OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);

    public Task<ExchangeRate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ExchangeRate?> GetByPairAsync(
        int baseCurrencyId,
        int targetCurrencyId,
        CancellationToken cancellationToken = default)
        => Query().FirstOrDefaultAsync(
            x => x.BaseCurrencyId == baseCurrencyId && x.TargetCurrencyId == targetCurrencyId,
            cancellationToken);

    public async Task AddAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
        => await _context.ExchangeRates.AddAsync(rate, cancellationToken);

    public Task UpdateAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
    {
        _context.ExchangeRates.Update(rate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
    {
        _context.ExchangeRates.Remove(rate);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    private IQueryable<ExchangeRate> Query()
        => _context.ExchangeRates
            .Include(x => x.BaseCurrency)
            .Include(x => x.TargetCurrency);
}
