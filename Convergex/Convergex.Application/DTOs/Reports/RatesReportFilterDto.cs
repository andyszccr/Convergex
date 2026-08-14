using Convergex.Domain.Enums;

namespace Convergex.Application.DTOs.Reports;

public class RatesReportFilterDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? BaseCurrencyId { get; set; }
    public int? TargetCurrencyId { get; set; }
    public ExchangeRateSource? Source { get; set; }
}
