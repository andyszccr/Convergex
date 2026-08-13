using Convergex.Application.DTOs.Conversions;
using Convergex.Domain.Enums;

namespace Convergex.Web.ViewModels.Reports;

public class HistoryReportViewModel
{
    public ConversionType? Type { get; set; }
    public string? UserName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public IReadOnlyList<ConversionHistoryItemDto> Items { get; set; } = [];

    public int TotalRecords => Items.Count;

    public decimal TotalAmount => Items.Sum(x => x.Amount);

    public decimal TotalResult => Items.Sum(x => x.Result);
}