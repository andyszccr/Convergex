namespace Convergex.Application.DTOs.Reports;

public class ConversionsReportRowDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RateApplied { get; set; }
    public decimal Result { get; set; }
    public string? UserName { get; set; }
}
