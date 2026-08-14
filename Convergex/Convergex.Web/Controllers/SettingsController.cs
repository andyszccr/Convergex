using Convergex.Application.DTOs.Audit;
using Convergex.Application.DTOs.Settings;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ISystemSettingService _settingService;
    private readonly ICurrencyService _currencyService;
    private readonly IAuditService _auditService;

    public SettingsController(
        ISystemSettingService settingService,
        ICurrencyService currencyService,
        IAuditService auditService)
    {
        _settingService = settingService;
        _currencyService = currencyService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var setting = await _settingService.GetAsync(
            cancellationToken);

        var model = new SettingsViewModel
        {
            Language = setting.Language,
            Theme = setting.Theme,
            DefaultCurrencyCode = setting.DefaultCurrencyCode,
            TimeZoneId = setting.TimeZoneId
        };

        await LoadCurrenciesAsync(
            model,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        SettingsViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCurrenciesAsync(
                model,
                cancellationToken);

            return View(model);
        }

        await _settingService.SaveAsync(
            new SystemSettingDto
            {
                Language = model.Language,
                Theme = model.Theme,
                DefaultCurrencyCode = model.DefaultCurrencyCode,
                TimeZoneId = model.TimeZoneId
            },
            cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            Action = AuditAction.Update,
            EntityName = "Settings",
            Detail = $"Idioma: {model.Language}, Tema: {model.Theme}, Moneda: {model.DefaultCurrencyCode}, Zona horaria: {model.TimeZoneId}.",
            Status = AuditStatus.Success,
            UserName = User.Identity?.Name,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);

        TempData["Success"] =
            "Configuración actualizada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCurrenciesAsync(
        SettingsViewModel model,
        CancellationToken cancellationToken)
    {
        var currencies =
            await _currencyService.GetActiveAsync(
                cancellationToken);

        model.Currencies =
            currencies.Select(x =>
                new SelectListItem
                {
                    Value = x.Code,
                    Text = $"{x.Code} - {x.Name}"
                });
    }
}