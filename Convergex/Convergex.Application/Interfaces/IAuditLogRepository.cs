using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<AuditLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        string? keyword,
        AuditAction? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
