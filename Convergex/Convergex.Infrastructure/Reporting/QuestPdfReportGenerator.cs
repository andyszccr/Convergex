using Convergex.Application.DTOs.Reports;
using Convergex.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Convergex.Infrastructure.Reporting;

public class QuestPdfReportGenerator : IPdfReportGenerator
{
    private static readonly byte[] LogoBytes = LoadLogo();

    public byte[] Generate(ReportDocument document)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        if (LogoBytes.Length > 0)
                        {
                            row.ConstantItem(50).Height(50).Image(LogoBytes).FitArea();
                        }

                        row.RelativeItem().PaddingLeft(10).Column(headerCol =>
                        {
                            headerCol.Item().Text("Convergex").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                            headerCol.Item().Text(document.Title).FontSize(13).SemiBold();
                        });

                        row.ConstantItem(220).Column(metaCol =>
                        {
                            metaCol.Item().AlignRight().Text($"Generado: {document.GeneratedAt.ToLocalTime():dd/MM/yyyy HH:mm}").FontSize(8);
                            metaCol.Item().AlignRight().Text($"Solicitado por: {document.GeneratedByUserName}").FontSize(8);
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(document.Subtitle))
                    {
                        col.Item().PaddingTop(4).Text(document.Subtitle).FontSize(9).FontColor(Colors.Grey.Darken1);
                    }

                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    if (document.SummaryItems.Count > 0)
                    {
                        col.Item().PaddingBottom(12).Row(row =>
                        {
                            foreach (var summary in document.SummaryItems)
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Background(Colors.Grey.Lighten5).Padding(8).Column(c =>
                                    {
                                        c.Item().Text(summary.Label).FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                                        c.Item().PaddingTop(2).Text(summary.Value).FontSize(12).Bold();
                                    });
                            }
                        });
                    }

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in document.Columns)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var column in document.Columns)
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text(column.Header).FontColor(Colors.White).Bold().FontSize(8);
                            }
                        });

                        var rowIndex = 0;
                        foreach (var row in document.Rows)
                        {
                            var background = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                            for (var i = 0; i < row.Count; i++)
                            {
                                var isNumeric = document.Columns[i].Type is ReportColumnType.Number or ReportColumnType.Currency;
                                var cell = table.Cell().Background(background).BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2).Padding(5);
                                if (isNumeric)
                                {
                                    cell.AlignRight().Text(row[i].DisplayText).FontSize(8);
                                }
                                else
                                {
                                    cell.Text(row[i].DisplayText).FontSize(8);
                                }
                            }

                            rowIndex++;
                        }

                        if (document.Rows.Count == 0)
                        {
                            table.Cell().ColumnSpan((uint)document.Columns.Count).Padding(10)
                                .AlignCenter().Text("Sin datos para los filtros seleccionados.").FontColor(Colors.Grey.Darken1);
                        }
                    });
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Convergex © {DateTime.Now.Year}").FontSize(7).FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static byte[] LoadLogo()
    {
        var assembly = typeof(QuestPdfReportGenerator).Assembly;
        var resourceName = $"{assembly.GetName().Name}.Reporting.Assets.logo.png";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return [];
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
