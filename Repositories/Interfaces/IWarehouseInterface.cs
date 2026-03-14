using Repositories.Models;

namespace Repositories.Interfaces;

public interface IWarehouseInterface
{
    Task<long> CreateAsync(WarehouseRecord warehouse, CancellationToken cancellationToken = default);
    Task<WarehouseRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(WarehouseRecord warehouse, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
