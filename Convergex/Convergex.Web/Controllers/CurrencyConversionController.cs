using Convergex.Application.DTOs.Conversions;
using Convergex.Application.DTOs.ExternalApis;
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
    private readonly IExternalExchangeRateService _externalExchangeRateService;
    private readonly ISystemSettingService _settingService;

    public CurrencyConversionController(
        ICurrencyConversionService conversionService,
        ICurrencyService currencyService,
        IExchangeRateService exchangeRateService,
        IExternalExchangeRateService externalExchangeRateService,
        ISystemSettingService settingService)
    {
        _conversionService = conversionService;
        _currencyService = currencyService;
        _exchangeRateService = exchangeRateService;
        _externalExchangeRateService = externalExchangeRateService;
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new CurrencyConversionViewModel();
        var settings = await _settingService.GetAsync(cancellationToken);
        var currencies = await _currencyService.GetActiveAsync(cancellationToken);
        var defaultCurrency = currencies.FirstOrDefault(x =>
            string.Equals(x.Code, settings.DefaultCurrencyCode, StringComparison.OrdinalIgnoreCase));

        if (defaultCurrency is not null)
        {
            model.FromCurrencyId = defaultCurrency.Id;
        }

        return View(await BuildAsync(model, cancellationToken));
    }

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
            var direct = await _exchangeRateService.GetActiveByPairAsync(model.FromCurrencyId, model.ToCurrencyId, cancellationToken);
            if (direct is not null)
            {
                model.CurrentRate = direct.BuyRate;
                ViewData["RateSource"] = "Base de datos";
            }
            else
            {
                var inverse = await _exchangeRateService.GetActiveByPairAsync(model.ToCurrencyId, model.FromCurrencyId, cancellationToken);
                if (inverse is not null && inverse.SellRate != 0)
                {
                    model.CurrentRate = Math.Round(1m / inverse.SellRate, 8);
                    ViewData["RateSource"] = "Base de datos (inversa)";
                }
                else
                {
                    var fromCurrency = currencies.FirstOrDefault(c => c.Id == model.FromCurrencyId);
                    var toCurrency = currencies.FirstOrDefault(c => c.Id == model.ToCurrencyId);

                    if (fromCurrency != null && toCurrency != null)
                    {
                        ExternalExchangeRateDto? externalRate = null;
                        if (fromCurrency.Code == "USD" && toCurrency.Code == "CRC")
                        {
                            externalRate = await _externalExchangeRateService.GetTdcRateAsync(cancellationToken);
                            if (externalRate != null)
                            {
                                model.CurrentRate = Math.Round(externalRate.Venta, 6);
                                model.ExternalCompraRate = externalRate.Compra;
                                model.ExternalVentaRate = externalRate.Venta;
                                model.ExternalRateDate = externalRate.VentaDate;
                                model.HasExternalRate = true;
                                ViewData["RateSource"] = "API Externa (TDC)";
                            }
                        }
                        else if (fromCurrency.Code == "CRC" && toCurrency.Code == "USD")
                        {
                            externalRate = await _externalExchangeRateService.GetTdcRateAsync(cancellationToken);
                            if (externalRate != null)
                            {
                                model.CurrentRate = Math.Round(1m / externalRate.Compra, 6);
                                model.ExternalCompraRate = externalRate.Compra;
                                model.ExternalVentaRate = externalRate.Venta;
                                model.ExternalRateDate = externalRate.VentaDate;
                                model.HasExternalRate = true;
                                ViewData["RateSource"] = "API Externa (TDC)";
                            }
                        }
                    }
                }
            }
        }

        return model;
    }
}
