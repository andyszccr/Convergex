using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Audit;

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string ActionName => Action switch
    {
        AuditAction.Create => "Creación",
        AuditAction.Update => "Edición",
        AuditAction.Delete => "Eliminación",
        AuditAction.Login => "Inicio de sesión",
        AuditAction.Logout => "Cierre de sesión",
        AuditAction.LoginFailed => "Inicio fallido",
        AuditAction.Conversion => "Conversión",
        _ => Action.ToString()
    };
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Detail { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public AuditStatus Status { get; set; }
    public string StatusName => Status == AuditStatus.Success ? "Éxito" : "Fallido";
}
