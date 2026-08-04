using Convergex.Application.Interfaces;
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
}
