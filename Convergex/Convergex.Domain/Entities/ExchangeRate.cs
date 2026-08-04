namespace Convergex.Domain.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public int BaseCurrencyId { get; set; }
    public int TargetCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Currency BaseCurrency { get; set; } = null!;
    public Currency TargetCurrency { get; set; } = null!;
}
