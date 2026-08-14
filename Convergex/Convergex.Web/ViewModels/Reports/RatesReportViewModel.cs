using Convergex.Application.DTOs.Reports;

namespace Convergex.Web.ViewModels.Reports;

public class RatesReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public RatesReportDto Report { get; set; } = new();
    public int TotalPages { get; set; }
}
