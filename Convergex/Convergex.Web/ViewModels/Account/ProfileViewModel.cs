using System.ComponentModel.DataAnnotations;

namespace Convergex.Web.ViewModels.Account;

public class ProfileViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public string RoleName { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string? CurrentPassword { get; set; }

    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string? ConfirmNewPassword { get; set; }
}
