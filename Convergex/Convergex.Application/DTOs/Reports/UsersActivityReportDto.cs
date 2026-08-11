namespace Convergex.Application.DTOs.Reports;

public class UsersActivityReportDto
{
    public IReadOnlyList<UserActivityRowDto> Items { get; set; } = [];
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalOperations { get; set; }
}
