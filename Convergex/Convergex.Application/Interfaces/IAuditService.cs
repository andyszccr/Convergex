using Convergex.Application.DTOs.Audit;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditLogEntryDto entry, CancellationToken cancellationToken = default);
    Task<PagedAuditLogsDto> GetPagedAsync(
        string? keyword,
        AuditAction? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AuditLogDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
