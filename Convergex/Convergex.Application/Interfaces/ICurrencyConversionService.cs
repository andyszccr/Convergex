using Convergex.Application.DTOs.Conversions;
using Convergex.Domain.Enums;

namespace Convergex.Application.Interfaces;

public interface ICurrencyConversionService
{
    Task<(bool Success, string Message, CurrencyConversionResultDto? Result)> ConvertAsync(
        CurrencyConversionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversionHistoryItemDto>> GetHistoryAsync(
        ConversionType? type = null,
        string? userName = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
}
