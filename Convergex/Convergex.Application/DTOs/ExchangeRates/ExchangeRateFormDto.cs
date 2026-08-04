namespace Convergex.Application.DTOs.ExchangeRates;

public class ExchangeRateFormDto
{
    public int Id { get; set; }
    public int BaseCurrencyId { get; set; }
    public int TargetCurrencyId { get; set; }
    public decimal Rate { get; set; }
}
