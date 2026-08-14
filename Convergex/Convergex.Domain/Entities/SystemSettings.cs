namespace Convergex.Domain.Entities;

public class SystemSetting
{
    public int Id { get; set; }

    public string Language { get; set; } = "es";

    public string Theme { get; set; } = "light";

    public string DefaultCurrencyCode { get; set; } = "CRC";

    public string TimeZoneId { get; set; } = "Central America Standard Time";

    public DateTime UpdatedAt { get; set; }
}