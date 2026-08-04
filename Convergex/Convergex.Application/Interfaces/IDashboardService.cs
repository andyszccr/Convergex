using Convergex.Application.DTOs.Dashboard;

namespace Convergex.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
