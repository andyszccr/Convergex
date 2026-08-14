using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Audit;

public class AuditLogEntryDto
{
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Detail { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public AuditStatus Status { get; set; } = AuditStatus.Success;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
}
