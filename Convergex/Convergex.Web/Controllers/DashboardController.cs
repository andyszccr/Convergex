using Convergex.Application.Interfaces;
using Convergex.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

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
        return View(DashboardViewModel.FromDto(summary));
    }
}
