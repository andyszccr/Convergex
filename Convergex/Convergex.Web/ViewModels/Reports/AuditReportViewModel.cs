using Convergex.Application.DTOs.Reports;

namespace Convergex.Web.ViewModels.Reports;

public class AuditReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public AuditReportDto Report { get; set; } = new();
    public int TotalPages { get; set; }
}
