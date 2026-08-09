using System.ComponentModel.DataAnnotations;
using Convergex.Application.DTOs.Units;
using Convergex.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.Units;

public class UnitConversionViewModel
{
    [Display(Name = "Categoría")]
    public UnitCategory Category { get; set; } = UnitCategory.Length;

    [Required(ErrorMessage = "Selecciona la unidad de origen.")]
    [Display(Name = "Unidad origen")]
    public int FromUnitId { get; set; }

    [Required(ErrorMessage = "Selecciona la unidad de destino.")]
    [Display(Name = "Unidad destino")]
    public int ToUnitId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    [Display(Name = "Cantidad")]
    public decimal Amount { get; set; }

    public decimal? CurrentFactor { get; set; }
    public UnitConversionResultDto? LastResult { get; set; }
    public IEnumerable<SelectListItem> FromUnits { get; set; } = [];
    public IEnumerable<SelectListItem> ToUnits { get; set; } = [];
}