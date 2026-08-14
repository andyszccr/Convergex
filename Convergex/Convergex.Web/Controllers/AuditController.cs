using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convergex.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class AuditController : Controller
{
    private const int PageSize = 15;

    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(
        string? keyword,
        AuditAction? eventType,
        string? entityName,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime().Date;
        DateTime? toUtc = toDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1);

        var result = await _auditService.GetPagedAsync(
            keyword, eventType, entityName, fromUtc, toUtc, page < 1 ? 1 : page, PageSize, cancellationToken);

        return View(new AuditLogFilterViewModel
        {
            Keyword = keyword,
            EventType = eventType,
            EntityName = entityName,
            FromDate = fromDate,
            ToDate = toDate,
            Result = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var item = await _auditService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Json(item);
    }
}
