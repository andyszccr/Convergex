using Convergex.Application.DTOs.Conversions;
using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.Conversions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize]
public class CurrencyConversionController : Controller
{
    private readonly ICurrencyConversionService _conversionService;
    private readonly ICurrencyService _currencyService;
    private readonly IExchangeRateService _exchangeRateService;

    public CurrencyConversionController(
        ICurrencyConversionService conversionService,
        ICurrencyService currencyService,
        IExchangeRateService exchangeRateService)
    {
        _conversionService = conversionService;
        _currencyService = currencyService;
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await BuildAsync(new CurrencyConversionViewModel(), cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CurrencyConversionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildAsync(model, cancellationToken));
        }

        var result = await _conversionService.ConvertAsync(new CurrencyConversionRequestDto
        {
            FromCurrencyId = model.FromCurrencyId,
            ToCurrencyId = model.ToCurrencyId,
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

    private async Task<CurrencyConversionViewModel> BuildAsync(
        CurrencyConversionViewModel model,
        CancellationToken cancellationToken)
    {
        var currencies = await _currencyService.GetActiveAsync(cancellationToken);
        model.Currencies = currencies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Code} - {c.Name} ({c.Symbol})"
        });

        if (model.FromCurrencyId > 0 && model.ToCurrencyId > 0 && model.FromCurrencyId != model.ToCurrencyId)
        {
            var direct = await _exchangeRateService.GetByPairAsync(model.FromCurrencyId, model.ToCurrencyId, cancellationToken);
            if (direct is not null)
            {
                model.CurrentRate = direct.Rate;
            }
            else
            {
                var inverse = await _exchangeRateService.GetByPairAsync(model.ToCurrencyId, model.FromCurrencyId, cancellationToken);
                if (inverse is not null && inverse.Rate != 0)
                {
                    model.CurrentRate = Math.Round(1m / inverse.Rate, 8);
                }
            }
        }

        return model;
    }
}
