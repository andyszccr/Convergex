using Convergex.Application.DTOs.Audits;

namespace Convergex.Web.ViewModels.Reports;

public class AuditReportViewModel
{
    public string? UserName { get; set; }

    public string? Module { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public IReadOnlyList<AuditLogDto> Items { get; set; } = [];

    public int TotalRecords => Items.Count;
}