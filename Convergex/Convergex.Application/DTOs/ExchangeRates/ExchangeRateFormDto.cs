namespace Convergex.Application.DTOs.ExchangeRates;

public class ExchangeRateFormDto
{
    public int BaseCurrencyId { get; set; }
    public int TargetCurrencyId { get; set; }
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public DateTime EffectiveAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByName { get; set; }
}
