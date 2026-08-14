namespace Convergex.Application.DTOs.Reports;

public class ReportDocument
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedByUserName { get; set; } = string.Empty;
    public IReadOnlyList<ReportColumn> Columns { get; set; } = [];
    public IReadOnlyList<IReadOnlyList<ReportCell>> Rows { get; set; } = [];
    public IReadOnlyList<ReportSummaryItem> SummaryItems { get; set; } = [];
}
