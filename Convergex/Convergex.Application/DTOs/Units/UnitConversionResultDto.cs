namespace Convergex.Application.DTOs.Units;

public class UnitConversionResultDto
{
    public int ConversionId { get; set; }
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public decimal Result { get; set; }
    public DateTime CreatedAt { get; set; }
}