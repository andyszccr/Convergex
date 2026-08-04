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
        => _context.ExchangeRates
            .AsNoTracking()
            .Include(x => x.BaseCurrency)
            .Include(x => x.TargetCurrency)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExchangeRate>> GetTopAsync(int take, CancellationToken cancellationToken = default)
        => await _context.ExchangeRates
            .AsNoTracking()
            .Include(x => x.BaseCurrency)
            .Include(x => x.TargetCurrency)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
