namespace Convergex.Application.DTOs.Reports;

public class CurrencyTotalDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public int Operations { get; set; }
    public decimal TotalAmount { get; set; }
}
