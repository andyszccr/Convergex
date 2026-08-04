namespace Convergex.Application.DTOs.ExchangeRates;

public class ExchangeRateDto
{
    public int Id { get; set; }
    public int BaseCurrencyId { get; set; }
    public int TargetCurrencyId { get; set; }
    public string BaseCode { get; set; } = string.Empty;
    public string TargetCode { get; set; } = string.Empty;
    public string Pair => $"{BaseCode}/{TargetCode}";
    public decimal Rate { get; set; }
    public DateTime UpdatedAt { get; set; }
}
