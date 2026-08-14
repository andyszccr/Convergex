using Convergex.Application.DTOs.Units;

namespace Convergex.Application.Interfaces;

public interface IUnitConversionService
{
    Task<(bool Success, string Message, UnitConversionResultDto? Result)> ConvertAsync(
        UnitConversionRequestDto request,
        CancellationToken cancellationToken = default);
}