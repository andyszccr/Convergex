namespace Convergex.Application.DTOs.Reports;

public class ConversionsReportDto
{
    public IReadOnlyList<ConversionsReportRowDto> Items { get; set; } = [];
    public int TotalOperations { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalResult { get; set; }
    public decimal AverageRate { get; set; }
    public IReadOnlyList<CurrencyTotalDto> TotalsByCurrency { get; set; } = [];
}
