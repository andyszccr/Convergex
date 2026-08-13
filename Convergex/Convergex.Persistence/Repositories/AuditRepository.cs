using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly ConvergexDbContext _context;

    public AuditRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        AuditLog auditLog,
        CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(
            auditLog,
            cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAsync(
        string? userName = null,
        string? module = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(x =>
                x.UserName.Contains(userName));
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x =>
                x.Module.Contains(module));
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAt <= toUtc.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}