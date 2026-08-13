using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using Convergex.Web.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Convergex.Application.Helpers;

namespace Convergex.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ICurrencyConversionService _conversionService;
    private readonly IAuditService _auditService;
    private readonly ISystemSettingService _settingService;

    public ReportsController(
    ICurrencyConversionService conversionService,
    IAuditService auditService,
    ISystemSettingService settingService)
    {
        _conversionService = conversionService;
        _auditService = auditService;
        _settingService = settingService;
    }

    public async Task<IActionResult> Index(
        ConversionType? type,
        string? userName,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime().Date;
        DateTime? toUtc = toDate?
            .ToUniversalTime()
            .Date
            .AddDays(1)
            .AddTicks(-1);

        var items = await _conversionService.GetHistoryAsync(
            type,
            userName,
            fromUtc,
            toUtc,
            cancellationToken);

        var viewModel = new HistoryReportViewModel
        {
            Type = type,
            UserName = userName,
            FromDate = fromDate,
            ToDate = toDate,
            Items = items
        };

        return View(viewModel);
    }

    public async Task<IActionResult> ExportHistoryPdf(
    ConversionType? type,
    string? userName,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime().Date;

        DateTime? toUtc = toDate?
            .ToUniversalTime()
            .Date
            .AddDays(1)
            .AddTicks(-1);

        var items = await _conversionService.GetHistoryAsync(
            type,
            userName,
            fromUtc,
            toUtc,
            cancellationToken);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("CONVERGEX")
                            .FontSize(20)
                            .Bold();

                        column.Item()
                            .Text("Reporte de Historial de Conversiones")
                            .FontSize(14)
                            .SemiBold();

                        column.Item()
                            .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9);
                    });

                page.Content()
                    .PaddingVertical(15)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        column.Item()
                            .Text($"Total de registros: {items.Count}")
                            .Bold();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Fecha");
                                header.Cell().Element(HeaderCell).Text("Tipo");
                                header.Cell().Element(HeaderCell).Text("De");
                                header.Cell().Element(HeaderCell).Text("A");
                                header.Cell().Element(HeaderCell).Text("Monto");
                                header.Cell().Element(HeaderCell).Text("Resultado");
                                header.Cell().Element(HeaderCell).Text("Tasa");
                                header.Cell().Element(HeaderCell).Text("Usuario");
                            });

                            foreach (var item in items)
                            {
                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.CreatedAt
                                        .ToLocalTime()
                                        .ToString("dd/MM/yyyy HH:mm"));

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.Type == "Currency"
                                        ? "Moneda"
                                        : "Unidad");

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.FromCode);

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.ToCode);

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.Amount.ToString("N2"));

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.Result.ToString("N4"));

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.RateApplied.ToString("N6"));

                                table.Cell()
                                    .Element(DataCell)
                                    .Text(item.UserName ?? "—");
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
            });
        });

        byte[] pdfBytes = document.GeneratePdf();

        string fileName =
            $"Reporte_Historial_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

        return File(
            pdfBytes,
            "application/pdf",
            fileName);

        static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Border(0.5f)
                .BorderColor(Colors.Grey.Medium)
                .Padding(5)
                .DefaultTextStyle(x => x.SemiBold());
        }

        static IContainer DataCell(IContainer container)
        {
            return container
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }

    public async Task<IActionResult> ExportHistoryExcel(
    ConversionType? type,
    string? userName,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?.ToUniversalTime().Date;

        DateTime? toUtc = toDate?
            .ToUniversalTime()
            .Date
            .AddDays(1)
            .AddTicks(-1);

        var items = await _conversionService.GetHistoryAsync(
            type,
            userName,
            fromUtc,
            toUtc,
            cancellationToken);

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Historial");



        worksheet.Cell("A1").Value = "CONVERGEX";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 18;

        worksheet.Cell("A2").Value = "Reporte de Historial de Conversiones";
        worksheet.Cell("A2").Style.Font.Bold = true;
        worksheet.Cell("A2").Style.Font.FontSize = 14;

        worksheet.Cell("A3").Value =
            $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        worksheet.Cell("A4").Value =
            $"Total de registros: {items.Count}";



        worksheet.Cell("A6").Value = "Filtros aplicados";
        worksheet.Cell("A6").Style.Font.Bold = true;

        worksheet.Cell("A7").Value = "Tipo:";
        worksheet.Cell("B7").Value =
            type.HasValue
                ? (type.Value == ConversionType.Currency
                    ? "Moneda"
                    : "Unidad")
                : "Todos";

        worksheet.Cell("C7").Value = "Usuario:";
        worksheet.Cell("D7").Value =
            string.IsNullOrWhiteSpace(userName)
                ? "Todos"
                : userName;

        worksheet.Cell("E7").Value = "Desde:";
        worksheet.Cell("F7").Value =
            fromDate.HasValue
                ? fromDate.Value.ToString("dd/MM/yyyy")
                : "Sin filtro";

        worksheet.Cell("G7").Value = "Hasta:";
        worksheet.Cell("H7").Value =
            toDate.HasValue
                ? toDate.Value.ToString("dd/MM/yyyy")
                : "Sin filtro";



        int headerRow = 9;

        worksheet.Cell(headerRow, 1).Value = "Fecha";
        worksheet.Cell(headerRow, 2).Value = "Tipo";
        worksheet.Cell(headerRow, 3).Value = "De";
        worksheet.Cell(headerRow, 4).Value = "A";
        worksheet.Cell(headerRow, 5).Value = "Monto";
        worksheet.Cell(headerRow, 6).Value = "Resultado";
        worksheet.Cell(headerRow, 7).Value = "Tasa";
        worksheet.Cell(headerRow, 8).Value = "Usuario";

        var headerRange = worksheet.Range(
            headerRow,
            1,
            headerRow,
            8);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#E9ECEF");

        headerRange.Style.Border.BottomBorder =
            XLBorderStyleValues.Thin;



        int currentRow = headerRow + 1;

        foreach (var item in items)
        {
            worksheet.Cell(currentRow, 1).Value =
                item.CreatedAt.ToLocalTime();

            worksheet.Cell(currentRow, 2).Value =
                item.Type == "Currency"
                    ? "Moneda"
                    : "Unidad";

            worksheet.Cell(currentRow, 3).Value =
                item.FromCode;

            worksheet.Cell(currentRow, 4).Value =
                item.ToCode;

            worksheet.Cell(currentRow, 5).Value =
                item.Amount;

            worksheet.Cell(currentRow, 6).Value =
                item.Result;

            worksheet.Cell(currentRow, 7).Value =
                item.RateApplied;

            worksheet.Cell(currentRow, 8).Value =
                item.UserName ?? "—";

            currentRow++;
        }


        if (items.Count > 0)
        {
            worksheet.Range(
                    headerRow + 1,
                    1,
                    currentRow - 1,
                    1)
                .Style.DateFormat.Format =
                    "dd/MM/yyyy HH:mm";

            worksheet.Range(
                    headerRow + 1,
                    5,
                    currentRow - 1,
                    5)
                .Style.NumberFormat.Format =
                    "#,##0.00";

            worksheet.Range(
                    headerRow + 1,
                    6,
                    currentRow - 1,
                    6)
                .Style.NumberFormat.Format =
                    "#,##0.0000";

            worksheet.Range(
                    headerRow + 1,
                    7,
                    currentRow - 1,
                    7)
                .Style.NumberFormat.Format =
                    "#,##0.000000";


            worksheet.Range(
                    headerRow,
                    1,
                    currentRow - 1,
                    8)
                .SetAutoFilter();
        }


        worksheet.SheetView.FreezeRows(headerRow);

        worksheet.Columns().AdjustToContents();

        worksheet.Column(1).Width = Math.Min(
            worksheet.Column(1).Width,
            22);

        worksheet.Column(8).Width = Math.Min(
            worksheet.Column(8).Width,
            30);


        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        var content = stream.ToArray();

        string fileName =
            $"Reporte_Historial_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    public async Task<IActionResult> Audit(
    string? userName,
    string? module,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate?
            .ToUniversalTime()
            .Date;

        DateTime? toUtc = toDate?
            .ToUniversalTime()
            .Date
            .AddDays(1)
            .AddTicks(-1);

        var items = await _auditService.GetAsync(
            userName,
            module,
            fromUtc,
            toUtc,
            cancellationToken);

        var viewModel = new AuditReportViewModel
        {
            UserName = userName,
            Module = module,
            FromDate = fromDate,
            ToDate = toDate,
            Items = items
        };

        return View(viewModel);
    }

}