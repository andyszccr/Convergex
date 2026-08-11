using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Units;

public class UnitDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public UnitCategory Category { get; set; }
    public decimal FactorToBase { get; set; }
    public bool IsActive { get; set; }
}