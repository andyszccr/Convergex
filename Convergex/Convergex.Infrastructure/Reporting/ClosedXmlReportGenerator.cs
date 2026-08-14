using ClosedXML.Excel;
using Convergex.Application.DTOs.Reports;
using Convergex.Application.Interfaces;

namespace Convergex.Infrastructure.Reporting;

public class ClosedXmlReportGenerator : IExcelReportGenerator
{
    public byte[] Generate(ReportDocument document)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(TruncateSheetName(document.Title));

        sheet.Cell(1, 1).Value = "Convergex";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;
        sheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#2563EB");

        sheet.Cell(2, 1).Value = document.Title;
        sheet.Cell(2, 1).Style.Font.Bold = true;
        sheet.Cell(2, 1).Style.Font.FontSize = 12;

        sheet.Cell(3, 1).Value = document.Subtitle;
        sheet.Cell(4, 1).Value = $"Generado: {document.GeneratedAt.ToLocalTime():dd/MM/yyyy HH:mm} · Solicitado por: {document.GeneratedByUserName}";
        sheet.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml("#64748B");

        var currentRow = 6;

        if (document.SummaryItems.Count > 0)
        {
            sheet.Cell(currentRow, 1).Value = "Resumen";
            sheet.Cell(currentRow, 1).Style.Font.Bold = true;
            currentRow++;

            foreach (var item in document.SummaryItems)
            {
                sheet.Cell(currentRow, 1).Value = item.Label;
                sheet.Cell(currentRow, 2).Value = item.Value;
                sheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.FromHtml("#64748B");
                currentRow++;
            }

            currentRow++;
        }

        var headerRow = currentRow;
        for (var i = 0; i < document.Columns.Count; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = document.Columns[i].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
        }

        var dataStartRow = headerRow + 1;
        for (var r = 0; r < document.Rows.Count; r++)
        {
            var row = document.Rows[r];
            for (var c = 0; c < row.Count; c++)
            {
                var cell = sheet.Cell(dataStartRow + r, c + 1);
                var value = row[c];
                var columnType = document.Columns[c].Type;

                if (value.DateValue.HasValue)
                {
                    cell.Value = value.DateValue.Value;
                    cell.Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
                }
                else if (value.NumericValue.HasValue)
                {
                    cell.Value = value.NumericValue.Value;
                    cell.Style.NumberFormat.Format = columnType == ReportColumnType.Currency
                        ? "#,##0.00"
                        : "#,##0.######";
                }
                else
                {
                    cell.Value = value.DisplayText;
                }
            }
        }

        if (document.Rows.Count > 0)
        {
            var dataRange = sheet.Range(headerRow, 1, dataStartRow + document.Rows.Count - 1, document.Columns.Count);
            dataRange.SetAutoFilter();
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        }

        sheet.SheetView.FreezeRows(headerRow);
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string TruncateSheetName(string value)
    {
        var sanitized = value.Length <= 31 ? value : value[..31];
        foreach (var invalid in new[] { '\\', '/', '?', '*', '[', ']', ':' })
        {
            sanitized = sanitized.Replace(invalid, '-');
        }

        return sanitized;
    }
}
