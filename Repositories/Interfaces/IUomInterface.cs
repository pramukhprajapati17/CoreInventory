using Repositories.Models;

namespace Repositories.Interfaces;

public interface IUomInterface
{
    Task<long> CreateAsync(UomRecord uom, CancellationToken cancellationToken = default);
    Task<UomRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UomRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UomRecord uom, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
