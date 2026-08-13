using Convergex.Domain.Entities;

namespace Convergex.Application.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(
        AuditLog auditLog,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetAsync(
        string? userName = null,
        string? module = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}