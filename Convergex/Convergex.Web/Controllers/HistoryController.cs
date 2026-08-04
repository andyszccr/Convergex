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

    public HistoryController(ICurrencyConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    public async Task<IActionResult> Index(
        ConversionType? type,
        string? userName,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime().Date;
        DateTime? toUtc = toDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1);

        var items = await _conversionService.GetHistoryAsync(type, userName, fromUtc, toUtc, cancellationToken);

        return View(new ConversionHistoryFilterViewModel
        {
            Type = type,
            UserName = userName,
            FromDate = fromDate,
            ToDate = toDate,
            Items = items
        });
    }
}
