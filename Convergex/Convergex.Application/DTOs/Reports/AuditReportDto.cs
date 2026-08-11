namespace Convergex.Application.DTOs.Reports;

public class AuditReportDto
{
    public IReadOnlyList<AuditReportRowDto> Items { get; set; } = [];
    public int TotalEvents { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public IReadOnlyList<ActionCountDto> CountsByAction { get; set; } = [];
}
