namespace Convergex.Application.DTOs.Reports;

public class AuditReportRowDto
{
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
