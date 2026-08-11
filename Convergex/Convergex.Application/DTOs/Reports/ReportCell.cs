using System.Globalization;

namespace Convergex.Application.DTOs.Reports;

public class ReportCell
{
    public string DisplayText { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }

    public static ReportCell Text(string? value) => new() { DisplayText = value ?? "—" };

    public static ReportCell Number(decimal value, string displayFormat = "N2") => new()
    {
        DisplayText = value.ToString(displayFormat, CultureInfo.InvariantCulture),
        NumericValue = value
    };

    public static ReportCell DateAndTime(DateTime value)
    {
        var local = value.ToLocalTime();
        return new ReportCell { DisplayText = local.ToString("dd/MM/yyyy HH:mm"), DateValue = local };
    }
}
