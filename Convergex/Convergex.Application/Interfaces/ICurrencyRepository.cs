using Convergex.Domain.Entities;

namespace Convergex.Application.Interfaces;

public interface ICurrencyRepository
{
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Currency?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Currency currency, CancellationToken cancellationToken = default);
    Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default);
    Task DeleteAsync(Currency currency, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
