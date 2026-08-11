namespace Convergex.Application.DTOs.Reports;

public class ReportColumn
{
    public string Header { get; set; } = string.Empty;
    public ReportColumnType Type { get; set; } = ReportColumnType.Text;
}
