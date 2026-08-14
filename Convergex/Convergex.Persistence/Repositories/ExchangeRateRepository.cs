using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
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

    private IQueryable<ExchangeRate> Query()
        => _context.ExchangeRates.Include(x => x.BaseCurrency).Include(x => x.TargetCurrency);

    public Task<ExchangeRate?> GetActiveByPairAsync(int baseCurrencyId, int targetCurrencyId, CancellationToken cancellationToken = default)
        => Query().FirstOrDefaultAsync(
            x => x.BaseCurrencyId == baseCurrencyId && x.TargetCurrencyId == targetCurrencyId && x.Status == ExchangeRateStatus.Active,
            cancellationToken);

    public async Task<IReadOnlyList<ExchangeRate>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await Query().AsNoTracking()
            .Where(x => x.Status == ExchangeRateStatus.Active)
            .OrderBy(x => x.BaseCurrency.Code).ThenBy(x => x.TargetCurrency.Code)
            .ToListAsync(cancellationToken);

    public Task<ExchangeRate?> GetLatestActiveAsync(CancellationToken cancellationToken = default)
        => Query().AsNoTracking()
            .Where(x => x.Status == ExchangeRateStatus.Active)
            .OrderByDescending(x => x.EffectiveAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExchangeRate>> GetTopActiveAsync(int take, CancellationToken cancellationToken = default)
        => await Query().AsNoTracking()
            .Where(x => x.Status == ExchangeRateStatus.Active)
            .OrderByDescending(x => x.EffectiveAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<ExchangeRate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ExchangeRate?> GetPreviousAsync(
        int baseCurrencyId,
        int targetCurrencyId,
        DateTime beforeEffectiveAt,
        int excludeId,
        CancellationToken cancellationToken = default)
        => _context.ExchangeRates.AsNoTracking()
            .Where(x => x.BaseCurrencyId == baseCurrencyId
                && x.TargetCurrencyId == targetCurrencyId
                && x.Id != excludeId
                && x.EffectiveAt < beforeEffectiveAt)
            .OrderByDescending(x => x.EffectiveAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(IReadOnlyList<ExchangeRate> Items, int TotalCount)> GetHistoryAsync(
        int? baseCurrencyId,
        int? targetCurrencyId,
        DateTime? fromUtc,
        DateTime? toUtc,
        ExchangeRateSource? source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Query().AsNoTracking().AsQueryable();

        if (baseCurrencyId.HasValue)
        {
            query = query.Where(x => x.BaseCurrencyId == baseCurrencyId.Value);
        }

        if (targetCurrencyId.HasValue)
        {
            query = query.Where(x => x.TargetCurrencyId == targetCurrencyId.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.EffectiveAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.EffectiveAt <= toUtc.Value);
        }

        if (source.HasValue)
        {
            query = query.Where(x => x.Source == source.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.EffectiveAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
        => await _context.ExchangeRates.AddAsync(rate, cancellationToken);

    public Task UpdateAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
    {
        _context.ExchangeRates.Update(rate);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
