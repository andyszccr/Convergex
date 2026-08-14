using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Units;

public class UnitFormDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public UnitCategory Category { get; set; }
    public decimal ConversionFactor { get; set; } = 1m;
    public int DecimalPrecision { get; set; } = 2;
    public RoundingMode RoundingMode { get; set; } = RoundingMode.HalfUp;
    public bool IsActive { get; set; } = true;
}
