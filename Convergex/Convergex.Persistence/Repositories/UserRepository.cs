using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Convergex.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ConvergexDbContext _context;

    public UserRepository(ConvergexDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower(), cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable().Where(u => u.Email == email.ToLower());
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
        => await _context.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public Task<Role?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
