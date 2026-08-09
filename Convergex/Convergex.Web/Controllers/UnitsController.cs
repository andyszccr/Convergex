using Convergex.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convergex.Web.Controllers;

[Authorize]
public class UnitsController : Controller
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _unitService.GetAllAsync(cancellationToken);
        return View(items);
    }
}