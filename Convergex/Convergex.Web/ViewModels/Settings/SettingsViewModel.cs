using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.Settings;

public class SettingsViewModel
{
    public string Language { get; set; } = "es";

    public string Theme { get; set; } = "light";

    public string DefaultCurrencyCode { get; set; } = "CRC";

    public string TimeZoneId { get; set; } = "Central America Standard Time";

    public IEnumerable<SelectListItem> Currencies { get; set; }
        = [];
}