using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Reports;

public class AuditReportFilterDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? Keyword { get; set; }
    public AuditAction? Action { get; set; }
    public string? EntityName { get; set; }
}
