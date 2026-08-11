namespace Convergex.Application.DTOs.Units;

public class UnitConversionRequestDto
{
    public int FromUnitId { get; set; }
    public int ToUnitId { get; set; }
    public decimal Amount { get; set; }
    public string? UserName { get; set; }
}