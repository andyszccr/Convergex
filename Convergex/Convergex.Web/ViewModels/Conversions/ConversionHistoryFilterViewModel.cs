using Convergex.Application.DTOs.Conversions;
using Convergex.Domain.Enums;

namespace Convergex.Web.ViewModels.Conversions;

public class ConversionHistoryFilterViewModel
{
    public ConversionType? Type { get; set; }
    public string? UserName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public IReadOnlyList<ConversionHistoryItemDto> Items { get; set; } = [];
}