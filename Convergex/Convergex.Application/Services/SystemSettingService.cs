using Convergex.Application.DTOs.Settings;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;

namespace Convergex.Application.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly ISystemSettingRepository _repository;

    public SystemSettingService(
        ISystemSettingRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemSettingDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetAsync(
            cancellationToken);

        if (setting is null)
        {
            return new SystemSettingDto();
        }

        return new SystemSettingDto
        {
            Language = setting.Language,
            Theme = setting.Theme,
            DefaultCurrencyCode = setting.DefaultCurrencyCode,
            TimeZoneId = setting.TimeZoneId,
            UpdatedAt = setting.UpdatedAt
        };
    }

    public async Task SaveAsync(
        SystemSettingDto dto,
        CancellationToken cancellationToken = default)
    {
        var current = await _repository.GetAsync(
            cancellationToken);

        if (current is null)
        {
            var setting = new SystemSetting
            {
                Language = dto.Language,
                Theme = dto.Theme,
                DefaultCurrencyCode = dto.DefaultCurrencyCode,
                TimeZoneId = dto.TimeZoneId,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(
                setting,
                cancellationToken);
        }
        else
        {
            var setting = new SystemSetting
            {
                Id = current.Id,
                Language = dto.Language,
                Theme = dto.Theme,
                DefaultCurrencyCode = dto.DefaultCurrencyCode,
                TimeZoneId = dto.TimeZoneId,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.UpdateAsync(
                setting,
                cancellationToken);
        }

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}