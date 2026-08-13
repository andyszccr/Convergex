using Convergex.Domain.Entities;

namespace Convergex.Application.Interfaces;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SystemSetting setting,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SystemSetting setting,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}