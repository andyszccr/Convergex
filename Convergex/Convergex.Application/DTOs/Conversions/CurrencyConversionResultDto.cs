namespace Convergex.Application.DTOs.Conversions;

public class CurrencyConversionResultDto
{
    public int ConversionId { get; set; }
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public decimal Result { get; set; }
    public DateTime CreatedAt { get; set; }
}
