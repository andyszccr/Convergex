using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface IExchangeRateService
{
    Task<IReadOnlyList<ExchangeRateDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ExchangeRateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExchangeRateDto?> GetActiveByPairAsync(int baseCurrencyId, int targetCurrencyId, CancellationToken cancellationToken = default);
    Task<PagedExchangeRatesDto> GetHistoryAsync(
        int? baseCurrencyId,
        int? targetCurrencyId,
        DateTime? fromUtc,
        DateTime? toUtc,
        ExchangeRateSource? source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> PublishManualAsync(ExchangeRateFormDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, int Published)> SyncFromApiAsync(CancellationToken cancellationToken = default);
}
