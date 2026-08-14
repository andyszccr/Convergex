namespace Convergex.Application.DTOs.Conversions;

public class RateQuoteDto
{
    public decimal Rate { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal? ExternalCompraRate { get; set; }
    public decimal? ExternalVentaRate { get; set; }
    public string? ExternalRateDate { get; set; }
    public bool HasExternalRate { get; set; }
}
