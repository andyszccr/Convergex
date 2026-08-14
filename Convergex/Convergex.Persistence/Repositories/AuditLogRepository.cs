using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ConvergexDbContext _context;

    public AuditLogRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AddAsync(log, cancellationToken);

    public Task<AuditLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        string? keyword,
        AuditAction? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.UserName, $"%{term}%") ||
                (x.IpAddress != null && EF.Functions.Like(x.IpAddress, $"%{term}%")) ||
                (x.Detail != null && EF.Functions.Like(x.Detail, $"%{term}%")));
        }

        if (action.HasValue)
        {
            query = query.Where(x => x.Action == action.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(x => x.EntityName == entityName);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.Timestamp <= toUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
