using Convergex.Application.DTOs.Users;
using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return View(users);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View(await BuildFormAsync(new UserFormViewModel(), cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "La contraseña es obligatoria.");
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var result = await _userService.CreateAsync(new CreateUserDto
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password!,
            RoleId = model.RoleId,
            IsActive = model.IsActive
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildFormAsync(model, cancellationToken));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var model = new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleId = user.RoleId,
            IsActive = user.IsActive,
            IsEdit = true
        };

        return View(await BuildFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        model.IsEdit = true;

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var result = await _userService.UpdateAsync(new UpdateUserDto
        {
            Id = id,
            FullName = model.FullName,
            Email = model.Email,
            RoleId = model.RoleId,
            IsActive = model.IsActive,
            NewPassword = model.Password
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildFormAsync(model, cancellationToken));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteAsync(id, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        var roles = await _userService.GetRolesAsync(cancellationToken);
        return View(roles);
    }

    private async Task<UserFormViewModel> BuildFormAsync(UserFormViewModel model, CancellationToken cancellationToken)
    {
        var roles = await _userService.GetRolesAsync(cancellationToken);
        model.Roles = roles.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name,
            Selected = r.Id == model.RoleId
        });
        return model;
    }
}
