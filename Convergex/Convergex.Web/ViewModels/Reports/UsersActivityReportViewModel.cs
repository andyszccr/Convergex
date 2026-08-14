using Convergex.Application.DTOs.Reports;

namespace Convergex.Web.ViewModels.Reports;

public class UsersActivityReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public UsersActivityReportDto Report { get; set; } = new();
    public int TotalPages { get; set; }
}
