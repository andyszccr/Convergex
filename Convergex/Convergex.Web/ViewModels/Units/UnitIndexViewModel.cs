using Convergex.Application.DTOs.Units;
using Convergex.Domain.Enums;

namespace Convergex.Web.ViewModels.Units;

public class UnitIndexViewModel
{
    public UnitCategory? Category { get; set; }
    public string? Search { get; set; }
    public PagedUnitsDto Result { get; set; } = new();
}
