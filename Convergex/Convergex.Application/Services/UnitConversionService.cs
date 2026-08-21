using Convergex.Application.Common;
using Convergex.Application.DTOs.Units;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;

namespace Convergex.Application.Services;

public class UnitConversionService : IUnitConversionService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IConversionRepository _conversionRepository;

    public UnitConversionService(
        IUnitRepository unitRepository,
        IConversionRepository conversionRepository)
    {
        _unitRepository = unitRepository;
        _conversionRepository = conversionRepository;
    }

    public async Task<(bool Success, string Message, UnitConversionResultDto? Result)> ConvertAsync(
        UnitConversionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return (false, "La cantidad debe ser mayor que cero.", null);
        }

        if (request.FromUnitId == request.ToUnitId)
        {
            return (false, "Selecciona unidades diferentes para convertir.", null);
        }

        var from = await _unitRepository.GetByIdAsync(request.FromUnitId, cancellationToken);
        var to = await _unitRepository.GetByIdAsync(request.ToUnitId, cancellationToken);

        if (from is null || to is null)
        {
            return (false, "Una de las unidades seleccionadas no existe.", null);
        }

        if (!from.IsActive || !to.IsActive)
        {
            return (false, "Solo se pueden convertir unidades activas.", null);
        }

        if (from.Category != to.Category)
        {
            return (false, "Las unidades deben pertenecer a la misma categoría.", null);
        }

        if (to.FactorToBase == 0)
        {
            return (false, "No se puede convertir entre estas unidades.", null);
        }

        var rate = Math.Round(from.FactorToBase / to.FactorToBase, 12);
        if (rate == 0)
        {
            return (false, "No se puede convertir entre estas unidades.", null);
        }

        var resultValue = DecimalFormatter.Round(request.Amount * rate, to.DecimalPrecision, to.RoundingMode);

        var conversion = new Conversion
        {
            Type = ConversionType.Unit,
            FromCode = from.Code,
            ToCode = to.Code,
            Amount = request.Amount,
            RateApplied = rate,
            Result = resultValue,
            UserName = request.UserName,
            CreatedAt = DateTime.UtcNow
        };

        await _conversionRepository.AddAsync(conversion, cancellationToken);
        await _conversionRepository.SaveChangesAsync(cancellationToken);

        return (true, "Conversión realizada y guardada en el historial.", new UnitConversionResultDto
        {
            ConversionId = conversion.Id,
            FromCode = from.Code,
            ToCode = to.Code,
            FromName = from.Name,
            ToName = to.Name,
            Amount = request.Amount,
            Rate = rate,
            Result = resultValue,
            CreatedAt = conversion.CreatedAt
        });
    }
}