using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.Users;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Selecciona un rol.")]
    [Display(Name = "Rol")]
    public int RoleId { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    public bool IsEdit { get; set; }

    public IEnumerable<SelectListItem> Roles { get; set; } = [];
}
