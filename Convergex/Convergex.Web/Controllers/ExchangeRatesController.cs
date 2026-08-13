using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.ExchangeRates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize]
public class ExchangeRatesController : Controller
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ICurrencyService _currencyService;

    public ExchangeRatesController(IExchangeRateService exchangeRateService, ICurrencyService currencyService)
    {
        _exchangeRateService = exchangeRateService;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _exchangeRateService.GetAllAsync(cancellationToken);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> GetRate(int fromId, int toId, CancellationToken cancellationToken)
    {
        if (fromId <= 0 || toId <= 0 || fromId == toId)
        {
            return Json(new { found = false, message = "Selecciona un par válido." });
        }

        var direct = await _exchangeRateService.GetByPairAsync(fromId, toId, cancellationToken);
        if (direct is not null)
        {
            return Json(new { found = true, rate = direct.Rate, pair = direct.Pair, source = "directa" });
        }

        var inverse = await _exchangeRateService.GetByPairAsync(toId, fromId, cancellationToken);
        if (inverse is not null && inverse.Rate != 0)
        {
            var rate = Math.Round(1m / inverse.Rate, 8);
            return Json(new { found = true, rate, pair = $"{inverse.TargetCode}/{inverse.BaseCode}", source = "inversa" });
        }

        return Json(new { found = false, message = "No hay tasa para ese par." });
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
        => View(await BuildFormAsync(new HistoryReportViewModel(), cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(HistoryReportViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var result = await _exchangeRateService.CreateAsync(ToDto(model), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildFormAsync(model, cancellationToken));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var item = await _exchangeRateService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        var model = new HistoryReportViewModel
        {
            Id = item.Id,
            BaseCurrencyId = item.BaseCurrencyId,
            TargetCurrencyId = item.TargetCurrencyId,
            Rate = item.Rate,
            IsEdit = true
        };

        return View(await BuildFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, HistoryReportViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        model.IsEdit = true;

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var result = await _exchangeRateService.UpdateAsync(ToDto(model), cancellationToken);
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
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.DeleteAsync(id, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task<HistoryReportViewModel> BuildFormAsync(
        HistoryReportViewModel model,
        CancellationToken cancellationToken)
    {
        var currencies = await _currencyService.GetActiveAsync(cancellationToken);
        model.Currencies = currencies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Code} - {c.Name}"
        });
        return model;
    }

    private static ExchangeRateFormDto ToDto(HistoryReportViewModel model) => new()
    {
        Id = model.Id,
        BaseCurrencyId = model.BaseCurrencyId,
        TargetCurrencyId = model.TargetCurrencyId,
        Rate = model.Rate
    };
}
