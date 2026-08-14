using Convergex.Application.DTOs.Audit;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Convergex.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        ICurrentUserContext currentUserContext,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task LogAsync(AuditLogEntryDto entry, CancellationToken cancellationToken = default)
    {
        try
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = entry.UserId ?? _currentUserContext.UserId,
                UserName = entry.UserName ?? _currentUserContext.UserName ?? "Sistema",
                Action = entry.Action,
                EntityName = entry.EntityName,
                EntityId = entry.EntityId,
                Detail = entry.Detail,
                OldValues = entry.OldValues,
                NewValues = entry.NewValues,
                IpAddress = entry.IpAddress ?? _currentUserContext.IpAddress,
                Timestamp = DateTime.UtcNow,
                Status = entry.Status
            }, cancellationToken);

            await _auditLogRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // La auditoría nunca debe interrumpir la operación principal: se registra el fallo y se continúa.
            _logger.LogWarning(ex, "No se pudo registrar el evento de auditoría {Action} sobre {EntityName}.", entry.Action, entry.EntityName);
        }
    }

    public async Task<PagedAuditLogsDto> GetPagedAsync(
        string? keyword,
        AuditAction? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 15 : pageSize;

        var (items, totalCount) = await _auditLogRepository.GetPagedAsync(
            keyword, action, entityName, fromUtc, toUtc, page, pageSize, cancellationToken);

        return new PagedAuditLogsDto
        {
            Items = items.Select(Map).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AuditLogDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _auditLogRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    private static AuditLogDto Map(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserName = log.UserName,
        Action = log.Action,
        EntityName = log.EntityName,
        EntityId = log.EntityId,
        Detail = log.Detail,
        OldValues = log.OldValues,
        NewValues = log.NewValues,
        IpAddress = log.IpAddress,
        Timestamp = log.Timestamp,
        Status = log.Status
    };
}
