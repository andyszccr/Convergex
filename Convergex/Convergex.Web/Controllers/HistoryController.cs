using Convergex.Application.Helpers;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Conversions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convergex.Web.Controllers;

[Authorize]
public class HistoryController : Controller
{
    private readonly ICurrencyConversionService _conversionService;
    private readonly ISystemSettingService _settingService;

    public HistoryController(
        ICurrencyConversionService conversionService,
        ISystemSettingService settingService)
    {
        _conversionService = conversionService;
        _settingService = settingService;
    }

    public async Task<IActionResult> Index(
        ConversionType? type,
        string? userName,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var settings =
            await _settingService.GetAsync(
                cancellationToken);

        DateTime? fromUtc = null;
        DateTime? toUtc = null;

        if (fromDate.HasValue)
        {
            fromUtc =
                TimeZoneHelper.LocalToUtc(
                    fromDate.Value.Date,
                    settings.TimeZoneId);
        }

        if (toDate.HasValue)
        {
            toUtc =
                TimeZoneHelper.LocalToUtc(
                    toDate.Value.Date.AddDays(1),
                    settings.TimeZoneId)
                .AddTicks(-1);
        }

        var items =
            await _conversionService.GetHistoryAsync(
                type,
                userName,
                fromUtc,
                toUtc,
                cancellationToken);

        return View(
            new ConversionHistoryFilterViewModel
            {
                Type = type,
                UserName = userName,
                FromDate = fromDate,
                ToDate = toDate,
                TimeZoneId = settings.TimeZoneId,
                Items = items
            });
    }
}