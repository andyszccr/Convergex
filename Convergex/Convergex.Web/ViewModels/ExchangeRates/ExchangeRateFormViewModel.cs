using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.ExchangeRates;

public class ExchangeRateFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecciona la moneda origen.")]
    [Display(Name = "Moneda origen")]
    public int BaseCurrencyId { get; set; }

    [Required(ErrorMessage = "Selecciona la moneda destino.")]
    [Display(Name = "Moneda destino")]
    public int TargetCurrencyId { get; set; }

    [Required(ErrorMessage = "La tasa es obligatoria.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "La tasa debe ser mayor que cero.")]
    [Display(Name = "Tasa")]
    public decimal Rate { get; set; }

    public bool IsEdit { get; set; }
    public IEnumerable<SelectListItem> Currencies { get; set; } = [];
}
