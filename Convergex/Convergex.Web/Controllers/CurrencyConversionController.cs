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
    private readonly IAuditService _auditService;
    private readonly ISystemSettingService _settingService;

    public CurrencyConversionController(
        ICurrencyConversionService conversionService,
        ICurrencyService currencyService,
        IExchangeRateService exchangeRateService,
        IAuditService auditService,
        ISystemSettingService settingService)
    {
        _conversionService = conversionService;
        _currencyService = currencyService;
        _exchangeRateService = exchangeRateService;
        _auditService = auditService;
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = new CurrencyConversionViewModel();

        var settings = await _settingService.GetAsync(
            cancellationToken);

        var currencies = await _currencyService.GetActiveAsync(
            cancellationToken);

        var defaultCurrency = currencies.FirstOrDefault(
            x => string.Equals(
                x.Code,
                settings.DefaultCurrencyCode,
                StringComparison.OrdinalIgnoreCase));

        if (defaultCurrency is not null)
        {
            model.FromCurrencyId = defaultCurrency.Id;
        }

        return View(
            await BuildAsync(
                model,
                cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        CurrencyConversionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(
                await BuildAsync(
                    model,
                    cancellationToken));
        }

        var result = await _conversionService.ConvertAsync(
            new CurrencyConversionRequestDto
            {
                FromCurrencyId = model.FromCurrencyId,
                ToCurrencyId = model.ToCurrencyId,
                Amount = model.Amount,
                UserName = User.Identity?.Name
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(
                await BuildAsync(
                    model,
                    cancellationToken));
        }

        if (result.Result is not null)
        {
            var userName =
                User.Identity?.Name ?? "Usuario desconocido";

            var ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString();

            await _auditService.LogAsync(
                userName: userName,
                action: "Conversión realizada",
                module: "Conversiones",
                description:
                    $"Conversión de {result.Result.Amount:N2} " +
                    $"{result.Result.FromCode} a " +
                    $"{result.Result.ToCode}. " +
                    $"Resultado: {result.Result.Result:N4}.",
                ipAddress: ipAddress,
                cancellationToken: cancellationToken);
        }

        TempData["Success"] = result.Message;
        model.LastResult = result.Result;

        return View(
            await BuildAsync(
                model,
                cancellationToken));
    }

    private async Task<CurrencyConversionViewModel> BuildAsync(
        CurrencyConversionViewModel model,
        CancellationToken cancellationToken)
    {
        var currencies =
            await _currencyService.GetActiveAsync(
                cancellationToken);

        model.Currencies =
            currencies.Select(c =>
                new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} - {c.Name} ({c.Symbol})"
                });

        if (model.FromCurrencyId > 0
            && model.ToCurrencyId > 0
            && model.FromCurrencyId != model.ToCurrencyId)
        {
            var direct =
                await _exchangeRateService.GetByPairAsync(
                    model.FromCurrencyId,
                    model.ToCurrencyId,
                    cancellationToken);

            if (direct is not null)
            {
                model.CurrentRate = direct.Rate;
            }
            else
            {
                var inverse =
                    await _exchangeRateService.GetByPairAsync(
                        model.ToCurrencyId,
                        model.FromCurrencyId,
                        cancellationToken);

                if (inverse is not null
                    && inverse.Rate != 0)
                {
                    model.CurrentRate =
                        Math.Round(
                            1m / inverse.Rate,
                            8);
                }
            }
        }

        return model;
    }
}