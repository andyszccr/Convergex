using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Convergex.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        var model = DashboardViewModel.FromDto(summary);
        model.UserDisplayName = User.Identity?.Name ?? model.UserDisplayName;
        model.UserRole = User.FindFirstValue(ClaimTypes.Role) ?? model.UserRole;
        return View(model);
    }
}
