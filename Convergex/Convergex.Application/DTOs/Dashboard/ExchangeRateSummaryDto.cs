namespace Convergex.Application.DTOs.Dashboard;

public class ExchangeRateSummaryDto
{
    public string BaseCode { get; set; } = string.Empty;
    public string TargetCode { get; set; } = string.Empty;
    public string Pair => $"{BaseCode}/{TargetCode}";
    public decimal Rate { get; set; }
    public decimal VariationPercent { get; set; }
    public DateTime UpdatedAt { get; set; }
}
