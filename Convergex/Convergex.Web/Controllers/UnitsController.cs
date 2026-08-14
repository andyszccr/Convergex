using Convergex.Application.DTOs.Units;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Units;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convergex.Web.Controllers;

[Authorize]
public class UnitsController : Controller
{
    private const int PageSize = 8;

    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    public async Task<IActionResult> Index(
        UnitCategory? category,
        string? search,
        int page,
        CancellationToken cancellationToken)
    {
        var result = await _unitService.GetPagedAsync(category, search, page < 1 ? 1 : page, PageSize, cancellationToken);

        return View(new UnitIndexViewModel
        {
            Category = category,
            Search = search,
            Result = result
        });
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Create() => View(new UnitFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(UnitFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _unitService.CreateAsync(ToDto(model), cancellationToken);
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
        var item = await _unitService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new UnitFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Symbol = item.Symbol,
            Category = item.Category,
            ConversionFactor = item.ConversionFactor,
            DecimalPrecision = item.DecimalPrecision,
            RoundingMode = item.RoundingMode,
            IsActive = item.IsActive,
            IsEdit = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, UnitFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        model.IsEdit = true;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _unitService.UpdateAsync(ToDto(model), cancellationToken);
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
        var result = await _unitService.DeleteAsync(id, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private static UnitFormDto ToDto(UnitFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Symbol = model.Symbol,
        Category = model.Category,
        ConversionFactor = model.ConversionFactor,
        DecimalPrecision = model.DecimalPrecision,
        RoundingMode = model.RoundingMode,
        IsActive = model.IsActive
    };
}
