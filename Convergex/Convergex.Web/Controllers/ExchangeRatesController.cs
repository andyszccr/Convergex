using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.ExchangeRates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize]
public class ExchangeRatesController : Controller
{
    private const int HistoryPageSize = 10;

    private readonly IExchangeRateService _exchangeRateService;
    private readonly ICurrencyService _currencyService;
    private readonly ICurrencyConversionService _conversionService;

    public ExchangeRatesController(
        IExchangeRateService exchangeRateService,
        ICurrencyService currencyService,
        ICurrencyConversionService conversionService)
    {
        _exchangeRateService = exchangeRateService;
        _currencyService = currencyService;
        _conversionService = conversionService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _exchangeRateService.GetActiveAsync(cancellationToken);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> GetRate(int fromId, int toId, CancellationToken cancellationToken)
    {
        if (fromId <= 0 || toId <= 0 || fromId == toId)
        {
            return Json(new { found = false, message = "Selecciona un par válido." });
        }

        var quote = await _conversionService.GetRateQuoteAsync(fromId, toId, cancellationToken);
        if (quote is null)
        {
            return Json(new { found = false, message = "No hay tasa de cambio disponible para ese par." });
        }

        return Json(new { found = true, rate = quote.Rate, source = quote.Source });
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
        => View(await BuildFormAsync(new ExchangeRateFormViewModel(), cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(ExchangeRateFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var result = await _exchangeRateService.PublishManualAsync(ToDto(model, User.Identity?.Name), cancellationToken);
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

        var model = new ExchangeRateFormViewModel
        {
            BaseCurrencyId = item.BaseCurrencyId,
            TargetCurrencyId = item.TargetCurrencyId,
            BuyRate = item.BuyRate,
            SellRate = item.SellRate,
            EffectiveAt = DateTime.Now,
            IsEdit = true
        };

        return View(await BuildFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, ExchangeRateFormViewModel model, CancellationToken cancellationToken)
    {
        model.IsEdit = true;

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var result = await _exchangeRateService.PublishManualAsync(ToDto(model, User.Identity?.Name), cancellationToken);
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
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.DeactivateAsync(id, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SyncNow(CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.SyncFromApiAsync(cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> History(
        int? baseCurrencyId,
        int? targetCurrencyId,
        DateTime? fromDate,
        DateTime? toDate,
        ExchangeRateSource? source,
        int page,
        CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime().Date;
        DateTime? toUtc = toDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1);

        var result = await _exchangeRateService.GetHistoryAsync(
            baseCurrencyId, targetCurrencyId, fromUtc, toUtc, source, page < 1 ? 1 : page, HistoryPageSize, cancellationToken);

        var currencies = await _currencyService.GetAllAsync(cancellationToken);

        var model = new ExchangeRateHistoryFilterViewModel
        {
            BaseCurrencyId = baseCurrencyId,
            TargetCurrencyId = targetCurrencyId,
            FromDate = fromDate,
            ToDate = toDate,
            Source = source,
            Result = result,
            Currencies = currencies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Code} - {c.Name}" })
        };

        return View(model);
    }

    private async Task<ExchangeRateFormViewModel> BuildFormAsync(
        ExchangeRateFormViewModel model,
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

    private static ExchangeRateFormDto ToDto(ExchangeRateFormViewModel model, string? userName) => new()
    {
        BaseCurrencyId = model.BaseCurrencyId,
        TargetCurrencyId = model.TargetCurrencyId,
        BuyRate = model.BuyRate,
        SellRate = model.SellRate,
        EffectiveAt = model.EffectiveAt.ToUniversalTime(),
        CreatedByName = userName
    };
}
