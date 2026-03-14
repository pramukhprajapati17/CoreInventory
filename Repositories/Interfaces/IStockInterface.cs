using Repositories.Models;

namespace Repositories.Interfaces;

public interface IStockInterface
{
    Task<StockRecord?> GetByProductLocationAsync(long productId, long locationId, CancellationToken cancellationToken = default);
    Task<bool> UpsertAsync(StockRecord stock, CancellationToken cancellationToken = default);
}
