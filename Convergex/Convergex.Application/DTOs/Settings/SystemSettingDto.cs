namespace Convergex.Application.DTOs.Settings;

public class SystemSettingDto
{
    public string Language { get; set; } = "es";

    public string Theme { get; set; } = "light";

    public string DefaultCurrencyCode { get; set; } = "CRC";

    public string TimeZoneId { get; set; } = "Central America Standard Time";

    public DateTime UpdatedAt { get; set; }
}