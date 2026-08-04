namespace Convergex.Application.DTOs.Dashboard;

public class RecentConversionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
