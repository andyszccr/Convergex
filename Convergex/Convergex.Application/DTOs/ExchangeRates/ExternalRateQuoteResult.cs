namespace Convergex.Application.DTOs.ExchangeRates;

public class ExternalRateQuoteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string BaseCode { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}
