using System.ComponentModel.DataAnnotations;
using Convergex.Domain.Enums;

namespace Convergex.Web.ViewModels.Units;

public class UnitFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El símbolo es obligatorio.")]
    [StringLength(10, ErrorMessage = "El símbolo debe tener como máximo 10 caracteres.")]
    [Display(Name = "Símbolo")]
    public string Symbol { get; set; } = string.Empty;

    [Display(Name = "Categoría")]
    public UnitCategory Category { get; set; } = UnitCategory.Peso;

    [Required(ErrorMessage = "El factor de conversión es obligatorio.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "El factor de conversión debe ser mayor que cero.")]
    [Display(Name = "Factor de conversión")]
    public decimal ConversionFactor { get; set; } = 1m;

    [Required(ErrorMessage = "La precisión decimal es obligatoria.")]
    [Range(0, 8, ErrorMessage = "La precisión decimal debe estar entre 0 y 8.")]
    [Display(Name = "Precisión (decimales)")]
    public int DecimalPrecision { get; set; } = 2;

    [Display(Name = "Regla de redondeo")]
    public RoundingMode RoundingMode { get; set; } = RoundingMode.HalfUp;

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    public bool IsEdit { get; set; }
}
