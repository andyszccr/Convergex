using Convergex.Application.DTOs.Audits;

namespace Convergex.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string userName,
        string action,
        string module,
        string description,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? userName = null,
        string? module = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
}