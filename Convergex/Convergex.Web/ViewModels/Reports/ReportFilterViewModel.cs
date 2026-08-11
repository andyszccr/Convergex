using Convergex.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.Reports;

public class ReportFilterViewModel
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Conversiones
    public ConversionType? Type { get; set; }
    public string? CurrencyCode { get; set; }
    public string? UserName { get; set; }

    // Tasas
    public int? BaseCurrencyId { get; set; }
    public int? TargetCurrencyId { get; set; }
    public ExchangeRateSource? Source { get; set; }

    // Auditoría
    public string? Keyword { get; set; }
    public AuditAction? EventType { get; set; }
    public string? EntityName { get; set; }

    // Usuarios
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public IEnumerable<SelectListItem> Currencies { get; set; } = [];
    public IEnumerable<SelectListItem> CurrencyCodes { get; set; } = [];
    public IEnumerable<SelectListItem> Roles { get; set; } = [];
}
