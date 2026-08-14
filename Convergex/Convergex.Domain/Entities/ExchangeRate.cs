using Convergex.Domain.Enums;

namespace Convergex.Domain.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public int BaseCurrencyId { get; set; }
    public int TargetCurrencyId { get; set; }
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public DateTime EffectiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ExchangeRateSource Source { get; set; } = ExchangeRateSource.Manual;
    public ExchangeRateStatus Status { get; set; } = ExchangeRateStatus.Active;
    public string? CreatedByName { get; set; }

    public Currency BaseCurrency { get; set; } = null!;
    public Currency TargetCurrency { get; set; } = null!;
}
