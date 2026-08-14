using Convergex.Application.DTOs.Settings;

namespace Convergex.Application.Interfaces;

public interface ISystemSettingService
{
    Task<SystemSettingDto> GetAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        SystemSettingDto dto,
        CancellationToken cancellationToken = default);
}