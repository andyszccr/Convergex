using Convergex.Application.DTOs.ExchangeRates;

namespace Convergex.Application.Interfaces;

public interface IExchangeRateProviderClient
{
    Task<ExternalRateQuoteResult> GetLatestRatesAsync(
        string baseCode,
        IReadOnlyCollection<string> targetCodes,
        CancellationToken cancellationToken = default);
}
