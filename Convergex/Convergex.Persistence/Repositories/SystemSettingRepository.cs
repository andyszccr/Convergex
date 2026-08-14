using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly ConvergexDbContext _context;

    public SystemSettingRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public Task<SystemSetting?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        SystemSetting setting,
        CancellationToken cancellationToken = default)
    {
        await _context.SystemSettings.AddAsync(
            setting,
            cancellationToken);
    }

    public Task UpdateAsync(
        SystemSetting setting,
        CancellationToken cancellationToken = default)
    {
        _context.SystemSettings.Update(setting);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}