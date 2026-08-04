namespace Convergex.Application.DTOs.Dashboard;

public class ConversionChartPointDto
{
    public DateOnly Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}
