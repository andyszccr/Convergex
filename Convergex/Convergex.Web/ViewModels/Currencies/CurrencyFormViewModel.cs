using System.ComponentModel.DataAnnotations;

namespace Convergex.Web.ViewModels.Currencies;

public class CurrencyFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "El código debe tener entre 3 y 10 caracteres.")]
    [Display(Name = "Código")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El símbolo es obligatorio.")]
    [Display(Name = "Símbolo")]
    public string Symbol { get; set; } = string.Empty;

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    public bool IsEdit { get; set; }
}
