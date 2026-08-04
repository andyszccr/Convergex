using Convergex.Domain.Enums;

namespace Convergex.Domain.Entities;

public class Conversion
{
    public int Id { get; set; }
    public ConversionType Type { get; set; }
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public decimal RateApplied { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
