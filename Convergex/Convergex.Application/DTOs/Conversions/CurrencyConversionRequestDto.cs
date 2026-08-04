namespace Convergex.Application.DTOs.Conversions;

public class CurrencyConversionRequestDto
{
    public int FromCurrencyId { get; set; }
    public int ToCurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string? UserName { get; set; }
}
