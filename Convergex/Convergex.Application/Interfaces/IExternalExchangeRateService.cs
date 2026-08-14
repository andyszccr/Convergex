using Convergex.Application.DTOs.ExternalApis;

namespace Convergex.Application.Interfaces;

public interface IExternalExchangeRateService
{
    Task<ExternalExchangeRateDto?> GetTdcRateAsync(CancellationToken cancellationToken = default);
}