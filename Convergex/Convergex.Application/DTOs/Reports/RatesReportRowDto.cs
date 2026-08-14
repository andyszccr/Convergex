namespace Convergex.Application.DTOs.Reports;

public class RatesReportRowDto
{
    public DateTime EffectiveAt { get; set; }
    public string Pair { get; set; } = string.Empty;
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal? VariationPercent { get; set; }
}
