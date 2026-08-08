using System.ComponentModel.DataAnnotations;
using Convergex.Application.DTOs.Conversions;
using Convergex.Application.DTOs.ExternalApis;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.Conversions;

public class CurrencyConversionViewModel
{
    [Required(ErrorMessage = "Selecciona la moneda origen.")]
    [Display(Name = "Moneda origen")]
    public int FromCurrencyId { get; set; }

    [Required(ErrorMessage = "Selecciona la moneda destino.")]
    [Display(Name = "Moneda destino")]
    public int ToCurrencyId { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.000001, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }

    public decimal? CurrentRate { get; set; }
    public CurrencyConversionResultDto? LastResult { get; set; }
    public IEnumerable<SelectListItem> Currencies { get; set; } = [];
    
    // Precios de la API externa para USD/CRC
    public decimal? ExternalCompraRate { get; set; }
    public decimal? ExternalVentaRate { get; set; }
    public string? ExternalRateDate { get; set; }
    public bool HasExternalRate { get; set; }
}
