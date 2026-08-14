using Convergex.Application.DTOs.Reports;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportsController : Controller
{
    private const int PreviewPageSize = 15;

    private readonly IReportService _reportService;
    private readonly IPdfReportGenerator _pdfReportGenerator;
    private readonly IExcelReportGenerator _excelReportGenerator;
    private readonly ICurrencyService _currencyService;
    private readonly IUserService _userService;

    public ReportsController(
        IReportService reportService,
        IPdfReportGenerator pdfReportGenerator,
        IExcelReportGenerator excelReportGenerator,
        ICurrencyService currencyService,
        IUserService userService)
    {
        _reportService = reportService;
        _pdfReportGenerator = pdfReportGenerator;
        _excelReportGenerator = excelReportGenerator;
        _currencyService = currencyService;
        _userService = userService;
    }

    public IActionResult Index() => RedirectToAction(nameof(Conversions));

    public async Task<IActionResult> Conversions(ReportFilterViewModel filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetConversionsReportAsync(ToConversionsFilter(filter), cancellationToken);
        filter.CurrencyCodes = await BuildCurrencyCodeItemsAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var totalPages = (int)Math.Ceiling(report.Items.Count / (double)PreviewPageSize);
        var pagedReport = new ConversionsReportDto
        {
            Items = report.Items.Skip((page - 1) * PreviewPageSize).Take(PreviewPageSize).ToList(),
            TotalOperations = report.TotalOperations,
            TotalAmount = report.TotalAmount,
            TotalResult = report.TotalResult,
            AverageRate = report.AverageRate,
            TotalsByCurrency = report.TotalsByCurrency
        };

        return View(new ConversionsReportViewModel { Filter = filter, Report = pagedReport, TotalPages = totalPages });
    }

    public async Task<IActionResult> Rates(ReportFilterViewModel filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetRatesReportAsync(ToRatesFilter(filter), cancellationToken);
        filter.Currencies = await BuildCurrencyItemsAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var totalPages = (int)Math.Ceiling(report.Items.Count / (double)PreviewPageSize);
        var pagedReport = new RatesReportDto
        {
            Items = report.Items.Skip((page - 1) * PreviewPageSize).Take(PreviewPageSize).ToList(),
            TotalRecords = report.TotalRecords,
            AverageBuyRate = report.AverageBuyRate,
            AverageSellRate = report.AverageSellRate,
            AverageSpreadPercent = report.AverageSpreadPercent,
            MaxVariationPercent = report.MaxVariationPercent,
            MinVariationPercent = report.MinVariationPercent
        };

        return View(new RatesReportViewModel { Filter = filter, Report = pagedReport, TotalPages = totalPages });
    }

    public async Task<IActionResult> Audit(ReportFilterViewModel filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetAuditReportAsync(ToAuditFilter(filter), cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var totalPages = (int)Math.Ceiling(report.Items.Count / (double)PreviewPageSize);
        var pagedReport = new AuditReportDto
        {
            Items = report.Items.Skip((page - 1) * PreviewPageSize).Take(PreviewPageSize).ToList(),
            TotalEvents = report.TotalEvents,
            SuccessCount = report.SuccessCount,
            FailedCount = report.FailedCount,
            CountsByAction = report.CountsByAction
        };

        return View(new AuditReportViewModel { Filter = filter, Report = pagedReport, TotalPages = totalPages });
    }

    public async Task<IActionResult> Users(ReportFilterViewModel filter, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetUsersActivityReportAsync(ToUsersFilter(filter), cancellationToken);
        filter.Roles = await BuildRoleItemsAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var totalPages = (int)Math.Ceiling(report.Items.Count / (double)PreviewPageSize);
        var pagedReport = new UsersActivityReportDto
        {
            Items = report.Items.Skip((page - 1) * PreviewPageSize).Take(PreviewPageSize).ToList(),
            TotalUsers = report.TotalUsers,
            ActiveUsers = report.ActiveUsers,
            TotalOperations = report.TotalOperations
        };

        return View(new UsersActivityReportViewModel { Filter = filter, Report = pagedReport, TotalPages = totalPages });
    }

    [HttpGet]
    public async Task<IActionResult> Export(string report, string format, ReportFilterViewModel filter, CancellationToken cancellationToken)
    {
        var userName = User.Identity?.Name ?? "Sistema";

        ReportDocument document;
        string fileNamePrefix;

        switch (report?.ToLowerInvariant())
        {
            case "conversions":
                document = _reportService.BuildConversionsDocument(
                    await _reportService.GetConversionsReportAsync(ToConversionsFilter(filter), cancellationToken), userName);
                fileNamePrefix = "reporte-conversiones";
                break;
            case "rates":
                document = _reportService.BuildRatesDocument(
                    await _reportService.GetRatesReportAsync(ToRatesFilter(filter), cancellationToken), userName);
                fileNamePrefix = "reporte-tasas";
                break;
            case "audit":
                document = _reportService.BuildAuditDocument(
                    await _reportService.GetAuditReportAsync(ToAuditFilter(filter), cancellationToken), userName);
                fileNamePrefix = "reporte-auditoria";
                break;
            case "users":
                document = _reportService.BuildUsersActivityDocument(
                    await _reportService.GetUsersActivityReportAsync(ToUsersFilter(filter), cancellationToken), userName);
                fileNamePrefix = "reporte-usuarios";
                break;
            default:
                return BadRequest("Tipo de reporte no válido.");
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");

        if (string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _excelReportGenerator.Generate(document);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileNamePrefix}-{timestamp}.xlsx");
        }

        var pdfBytes = _pdfReportGenerator.Generate(document);
        return File(pdfBytes, "application/pdf", $"{fileNamePrefix}-{timestamp}.pdf");
    }

    private static ConversionsReportFilterDto ToConversionsFilter(ReportFilterViewModel filter) => new()
    {
        FromUtc = filter.FromDate?.ToUniversalTime().Date,
        ToUtc = filter.ToDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1),
        Type = filter.Type,
        CurrencyCode = filter.CurrencyCode,
        UserName = filter.UserName
    };

    private static RatesReportFilterDto ToRatesFilter(ReportFilterViewModel filter) => new()
    {
        FromUtc = filter.FromDate?.ToUniversalTime().Date,
        ToUtc = filter.ToDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1),
        BaseCurrencyId = filter.BaseCurrencyId,
        TargetCurrencyId = filter.TargetCurrencyId,
        Source = filter.Source
    };

    private static AuditReportFilterDto ToAuditFilter(ReportFilterViewModel filter) => new()
    {
        FromUtc = filter.FromDate?.ToUniversalTime().Date,
        ToUtc = filter.ToDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1),
        Keyword = filter.Keyword,
        Action = filter.EventType,
        EntityName = filter.EntityName
    };

    private static UsersActivityReportFilterDto ToUsersFilter(ReportFilterViewModel filter) => new()
    {
        FromUtc = filter.FromDate?.ToUniversalTime().Date,
        ToUtc = filter.ToDate?.ToUniversalTime().Date.AddDays(1).AddTicks(-1),
        RoleId = filter.RoleId,
        IsActive = filter.IsActive
    };

    private async Task<IEnumerable<SelectListItem>> BuildCurrencyItemsAsync(CancellationToken cancellationToken)
    {
        var currencies = await _currencyService.GetAllAsync(cancellationToken);
        return currencies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Code} - {c.Name}" });
    }

    private async Task<IEnumerable<SelectListItem>> BuildCurrencyCodeItemsAsync(CancellationToken cancellationToken)
    {
        var currencies = await _currencyService.GetAllAsync(cancellationToken);
        return currencies.Select(c => new SelectListItem { Value = c.Code, Text = $"{c.Code} - {c.Name}" });
    }

    private async Task<IEnumerable<SelectListItem>> BuildRoleItemsAsync(CancellationToken cancellationToken)
    {
        var roles = await _userService.GetRolesAsync(cancellationToken);
        return roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name });
    }
}
