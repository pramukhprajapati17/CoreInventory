using Repositories.Models;

namespace Repositories.Interfaces;

public interface ILocationInterface
{
    Task<long> CreateAsync(LocationRecord location, CancellationToken cancellationToken = default);
    Task<LocationRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationRecord>> ListByWarehouseAsync(long warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationRecord>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(LocationRecord location, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
