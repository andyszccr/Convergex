namespace Convergex.Application.Interfaces;

public interface ICurrencyRepository
{
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}
