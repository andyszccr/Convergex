using Convergex.Application.DTOs.Conversions;
using Convergex.Domain.Enums;

namespace Convergex.Web.ViewModels.Conversions;

public class ConversionHistoryFilterViewModel
{
    public ConversionType? Type { get; set; }

    public string? UserName { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string TimeZoneId { get; set; } =
        "Central America Standard Time";

    public IReadOnlyList<ConversionHistoryItemDto> Items { get; set; } = [];
}