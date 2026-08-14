using Convergex.Application;
using Convergex.Application.Interfaces;
using Convergex.Infrastructure;
using Convergex.Persistence.Context;
using Convergex.Persistence.Seed;
using Convergex.Web.Middleware;
using Convergex.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;
using Serilog.Events;

namespace Convergex.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "Logs/convergex-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true)
            .CreateLogger();

        builder.Services.AddSerilog();

        try
        {
            Log.Information("Iniciando aplicación Convergex");

            builder.Services
                .AddControllersWithViews(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                });

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/Login";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ConvergexDbContext>();
                await DbSeeder.SeedAsync(db);
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex is not null || httpContext.Response.StatusCode >= 500)
                    {
                        return LogEventLevel.Error;
                    }

                    if (httpContext.Response.StatusCode >= 400)
                    {
                        return LogEventLevel.Warning;
                    }

                    return LogEventLevel.Information;
                };
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
                    diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anónimo");
                };
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "La aplicación Convergex terminó inesperadamente");
        }
        finally
        {
            Log.Information("Cerrando aplicación Convergex");
            await Log.CloseAndFlushAsync();
        }
    }
}
