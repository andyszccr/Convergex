namespace Convergex.Application.DTOs.ExternalApis;

public class ExternalExchangeRateDto
{
    public string Garantia { get; set; } = string.Empty;
    public string Licence { get; set; } = string.Empty;
    public decimal Compra { get; set; }
    public string Updated { get; set; } = string.Empty;
    public string VentaDate { get; set; } = string.Empty;
    public decimal Venta { get; set; }
    public string CompraDate { get; set; } = string.Empty;
    public string HostedBy { get; set; } = string.Empty;
}