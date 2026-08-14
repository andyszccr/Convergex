namespace Convergex.Application.DTOs.Reports;

public class UsersActivityReportFilterDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}
