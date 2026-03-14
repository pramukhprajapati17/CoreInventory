using Repositories.Models;

namespace Repositories.Interfaces;

public interface IStockLedgerInterface
{
    Task<long> AddEntryAsync(StockLedgerRecord entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockLedgerRecord>> ListByProductAsync(long productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockLedgerRecord>> ListAllAsync(CancellationToken cancellationToken = default);
}
