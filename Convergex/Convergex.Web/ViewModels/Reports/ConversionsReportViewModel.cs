using Convergex.Application.DTOs.Reports;

namespace Convergex.Web.ViewModels.Reports;

public class ConversionsReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public ConversionsReportDto Report { get; set; } = new();
    public int TotalPages { get; set; }
}
