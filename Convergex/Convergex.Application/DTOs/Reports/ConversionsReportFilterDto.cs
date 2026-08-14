using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Reports;

public class ConversionsReportFilterDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public ConversionType? Type { get; set; }
    public string? CurrencyCode { get; set; }
    public string? UserName { get; set; }
}
