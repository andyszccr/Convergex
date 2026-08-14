namespace Convergex.Application.DTOs.Reports;

public class RatesReportDto
{
    public IReadOnlyList<RatesReportRowDto> Items { get; set; } = [];
    public int TotalRecords { get; set; }
    public decimal AverageBuyRate { get; set; }
    public decimal AverageSellRate { get; set; }
    public decimal AverageSpreadPercent { get; set; }
    public decimal? MaxVariationPercent { get; set; }
    public decimal? MinVariationPercent { get; set; }
}
