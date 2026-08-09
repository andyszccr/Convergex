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
        CancellationToken cancellationToken,
        int page = 1)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime();
        DateTime? toUtc = toDate?.ToUniversalTime().AddDays(1);

        const int pageSize = 10;
        var result = await _conversionService.GetHistoryPagedAsync(type, userName, fromUtc, toUtc, page, pageSize, cancellationToken);

        return View(new ConversionHistoryFilterViewModel
        {
            Type = type,
            UserName = userName,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize,
            TotalItems = result.Total,
            Items = result.Items
        });
    }
}
