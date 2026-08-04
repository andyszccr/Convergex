using Convergex.Application.Interfaces;
using Convergex.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Convergex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
