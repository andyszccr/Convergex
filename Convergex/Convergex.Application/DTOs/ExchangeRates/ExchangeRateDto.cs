using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.ExchangeRates;

public class ExchangeRateDto
{
    public int Id { get; set; }
    public int BaseCurrencyId { get; set; }
    public int TargetCurrencyId { get; set; }
    public string BaseCode { get; set; } = string.Empty;
    public string TargetCode { get; set; } = string.Empty;
    public string Pair => $"{BaseCode}/{TargetCode}";
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public decimal MidRate => Math.Round((BuyRate + SellRate) / 2, 6);
    public DateTime EffectiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ExchangeRateSource Source { get; set; }
    public string SourceName => Source == ExchangeRateSource.Api ? "API" : "Manual";
    public ExchangeRateStatus Status { get; set; }
    public string StatusName => Status switch
    {
        ExchangeRateStatus.Active => "Activa",
        ExchangeRateStatus.Inactive => "Inactiva",
        _ => "Histórica"
    };
    public string? CreatedByName { get; set; }
    public decimal? VariationPercent { get; set; }
}
