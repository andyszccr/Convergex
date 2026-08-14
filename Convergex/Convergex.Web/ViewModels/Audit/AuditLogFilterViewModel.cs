using Convergex.Application.DTOs.Audit;
using Convergex.Domain.Enums;

namespace Convergex.Web.ViewModels.Audit;

public class AuditLogFilterViewModel
{
    public string? Keyword { get; set; }
    public AuditAction? EventType { get; set; }
    public string? EntityName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public PagedAuditLogsDto Result { get; set; } = new();
}
