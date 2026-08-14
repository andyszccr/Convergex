using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Convergex.Web.ViewModels.ExchangeRates;

public class ExchangeRateHistoryFilterViewModel
{
    public int? BaseCurrencyId { get; set; }
    public int? TargetCurrencyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public ExchangeRateSource? Source { get; set; }
    public PagedExchangeRatesDto Result { get; set; } = new();
    public IEnumerable<SelectListItem> Currencies { get; set; } = [];
}
