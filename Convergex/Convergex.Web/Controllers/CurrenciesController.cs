using Convergex.Application.DTOs.Currencies;
using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convergex.Web.Controllers;

[Authorize]
public class CurrenciesController : Controller
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _currencyService.GetAllAsync(cancellationToken);
        return View(items);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create() => View(new CurrencyFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(CurrencyFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _currencyService.CreateAsync(ToDto(model), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var item = await _currencyService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new CurrencyFormViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Symbol = item.Symbol,
            IsActive = item.IsActive,
            IsEdit = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, CurrencyFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        model.IsEdit = true;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _currencyService.UpdateAsync(ToDto(model), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _currencyService.DeleteAsync(id, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private static CurrencyFormDto ToDto(CurrencyFormViewModel model) => new()
    {
        Id = model.Id,
        Code = model.Code,
        Name = model.Name,
        Symbol = model.Symbol,
        IsActive = model.IsActive
    };
}
