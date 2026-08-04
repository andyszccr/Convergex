using Convergex.Application.DTOs.Dashboard;
using System.Globalization;
using System.Text.Json;

namespace Convergex.Web.ViewModels.Dashboard;

public class DashboardViewModel
{
    public string UserDisplayName { get; set; } = "Alexander Navarro";
    public string UserRole { get; set; } = "Administrador";
    public int TotalConversions { get; set; }
    public int ConversionsToday { get; set; }
    public int ActiveCurrencies { get; set; }
    public int ActiveUnits { get; set; }
    public decimal TodayGrowthPercent { get; set; }
    public ExchangeRateSummaryDto? LatestExchangeRate { get; set; }
    public IReadOnlyList<ConversionChartPointDto> ChartPoints { get; set; } = [];
    public IReadOnlyList<ConversionTypeShareDto> TypeShares { get; set; } = [];
    public IReadOnlyList<RecentConversionDto> RecentConversions { get; set; } = [];
    public IReadOnlyList<ExchangeRateSummaryDto> TopRates { get; set; } = [];
    public string ChartLabelsJson { get; set; } = "[]";
    public string ChartValuesJson { get; set; } = "[]";
    public string TypeLabelsJson { get; set; } = "[]";
    public string TypeValuesJson { get; set; } = "[]";
    public string CurrentDateLabel { get; set; } = string.Empty;

    public static DashboardViewModel FromDto(DashboardSummaryDto dto)
    {
        var culture = new CultureInfo("es-CR");

        return new DashboardViewModel
        {
            TotalConversions = dto.TotalConversions,
            ConversionsToday = dto.ConversionsToday,
            ActiveCurrencies = dto.ActiveCurrencies,
            ActiveUnits = dto.ActiveUnits,
            TodayGrowthPercent = dto.TodayGrowthPercent,
            LatestExchangeRate = dto.LatestExchangeRate,
            ChartPoints = dto.ChartPoints,
            TypeShares = dto.TypeShares,
            RecentConversions = dto.RecentConversions,
            TopRates = dto.TopRates,
            ChartLabelsJson = JsonSerializer.Serialize(dto.ChartPoints.Select(p => p.Label)),
            ChartValuesJson = JsonSerializer.Serialize(dto.ChartPoints.Select(p => p.Count)),
            TypeLabelsJson = JsonSerializer.Serialize(dto.TypeShares.Select(p => p.Type)),
            TypeValuesJson = JsonSerializer.Serialize(dto.TypeShares.Select(p => p.Count)),
            CurrentDateLabel = DateTime.Now.ToString("d 'de' MMMM 'de' yyyy", culture)
        };
    }
}
