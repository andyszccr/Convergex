using Convergex.Application.DTOs.Audits;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;

namespace Convergex.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task LogAsync(
        string userName,
        string action,
        string module,
        string description,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserName = userName,
            Action = action,
            Module = module,
            Description = description,
            IpAddress = ipAddress,

            // Siempre guardamos UTC en la BD
            CreatedAt = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(
            auditLog,
            cancellationToken);

        await _auditRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? userName = null,
        string? module = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _auditRepository.GetAsync(
            userName,
            module,
            fromUtc,
            toUtc,
            cancellationToken);

        return items.Select(x => new AuditLogDto
        {
            Id = x.Id,
            UserName = x.UserName,
            Action = x.Action,
            Module = x.Module,
            Description = x.Description,
            IpAddress = x.IpAddress,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}