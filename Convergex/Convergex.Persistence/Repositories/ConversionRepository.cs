using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class ConversionRepository : IConversionRepository
{
    private readonly ConvergexDbContext _context;

    public ConversionRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _context.Conversions.CountAsync(cancellationToken);

    public Task<int> CountFromAsync(DateTime fromUtc, CancellationToken cancellationToken = default)
        => _context.Conversions.CountAsync(c => c.CreatedAt >= fromUtc, cancellationToken);

    public Task<int> CountBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => _context.Conversions.CountAsync(c => c.CreatedAt >= fromUtc && c.CreatedAt < toUtc, cancellationToken);

    public async Task<IReadOnlyList<Conversion>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
        => await _context.Conversions
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<DateOnly, int>> GetDailyCountsAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Conversions
            .AsNoTracking()
            .Where(c => c.CreatedAt >= fromUtc)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => DateOnly.FromDateTime(x.Date),
            x => x.Count);
    }

    public async Task<IReadOnlyDictionary<ConversionType, int>> GetCountsByTypeAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Conversions
            .AsNoTracking()
            .GroupBy(c => c.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Type, x => x.Count);
    }

    public async Task<IReadOnlyList<Conversion>> GetHistoryAsync(
        ConversionType? type = null,
        string? userName = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Conversions.AsNoTracking().AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(c => c.UserName != null && c.UserName.Contains(userName));
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= toUtc.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Conversion conversion, CancellationToken cancellationToken = default)
        => await _context.Conversions.AddAsync(conversion, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
