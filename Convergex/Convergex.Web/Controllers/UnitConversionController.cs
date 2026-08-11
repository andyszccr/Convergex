using Convergex.Application.DTOs.Units;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Units;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize]
public class UnitConversionController : Controller
{
    private readonly IUnitConversionService _conversionService;
    private readonly IUnitService _unitService;

    public UnitConversionController(IUnitConversionService conversionService, IUnitService unitService)
    {
        _conversionService = conversionService;
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(UnitCategory? category, CancellationToken cancellationToken)
        => View(await BuildAsync(new UnitConversionViewModel
        {
            Category = category ?? UnitCategory.Length
        }, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UnitConversionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildAsync(model, cancellationToken));
        }

        var result = await _conversionService.ConvertAsync(new UnitConversionRequestDto
        {
            FromUnitId = model.FromUnitId,
            ToUnitId = model.ToUnitId,
            Amount = model.Amount,
            UserName = User.Identity?.Name
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildAsync(model, cancellationToken));
        }

        TempData["Success"] = result.Message;
        model.LastResult = result.Result;
        return View(await BuildAsync(model, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetFactor(int fromId, int toId, CancellationToken cancellationToken)
    {
        if (fromId <= 0 || toId <= 0 || fromId == toId)
        {
            return Json(new { found = false, message = "Selecciona un par válido." });
        }

        var from = await _unitService.GetByIdAsync(fromId, cancellationToken);
        var to = await _unitService.GetByIdAsync(toId, cancellationToken);

        if (from is null || to is null)
        {
            return Json(new { found = false, message = "Una de las unidades no existe." });
        }

        if (from.Category != to.Category)
        {
            return Json(new { found = false, message = "Las unidades deben ser de la misma categoría." });
        }

        if (to.FactorToBase == 0)
        {
            return Json(new { found = false, message = "Factor no disponible para ese par." });
        }

        var factor = Math.Round(from.FactorToBase / to.FactorToBase, 12);
        return Json(new { found = true, factor });
    }

    private async Task<UnitConversionViewModel> BuildAsync(
        UnitConversionViewModel model,
        CancellationToken cancellationToken)
    {
        var category = Enum.IsDefined(model.Category) && model.Category != 0
            ? model.Category
            : UnitCategory.Length;

        var units = await _unitService.GetActiveByCategoryAsync(category, cancellationToken);

        model.Category = category;
        model.FromUnits = units.Select(u => new SelectListItem
        {
            Value = u.Id.ToString(),
            Text = $"{u.Code} - {u.Name} ({u.Symbol})"
        });
        model.ToUnits = units.Select(u => new SelectListItem
        {
            Value = u.Id.ToString(),
            Text = $"{u.Code} - {u.Name} ({u.Symbol})"
        });

        return model;
    }
}