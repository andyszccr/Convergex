namespace Convergex.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalConversions { get; set; }
    public int ConversionsToday { get; set; }
    public int ConversionsYesterday { get; set; }
    public int ActiveCurrencies { get; set; }
    public int ActiveUnits { get; set; }
    public decimal TodayGrowthPercent { get; set; }
    public ExchangeRateSummaryDto? LatestExchangeRate { get; set; }
    public IReadOnlyList<ConversionChartPointDto> ChartPoints { get; set; } = [];
    public IReadOnlyList<ConversionTypeShareDto> TypeShares { get; set; } = [];
    public IReadOnlyList<RecentConversionDto> RecentConversions { get; set; } = [];
    public IReadOnlyList<ExchangeRateSummaryDto> TopRates { get; set; } = [];
}
