using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.ExchangeRates;

public class ExchangeRateFormViewModel
{
    [Required(ErrorMessage = "Selecciona la moneda origen.")]
    [Display(Name = "Moneda origen")]
    public int BaseCurrencyId { get; set; }

    [Required(ErrorMessage = "Selecciona la moneda destino.")]
    [Display(Name = "Moneda destino")]
    public int TargetCurrencyId { get; set; }

    [Required(ErrorMessage = "La tasa de compra es obligatoria.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "La tasa de compra debe ser mayor que cero.")]
    [Display(Name = "Tasa de compra (Buy)")]
    public decimal BuyRate { get; set; }

    [Required(ErrorMessage = "La tasa de venta es obligatoria.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "La tasa de venta debe ser mayor que cero.")]
    [Display(Name = "Tasa de venta (Sell)")]
    public decimal SellRate { get; set; }

    [Required(ErrorMessage = "La fecha de vigencia es obligatoria.")]
    [Display(Name = "Vigente desde")]
    public DateTime EffectiveAt { get; set; } = DateTime.Now;

    public bool IsEdit { get; set; }
    public IEnumerable<SelectListItem> Currencies { get; set; } = [];
}
