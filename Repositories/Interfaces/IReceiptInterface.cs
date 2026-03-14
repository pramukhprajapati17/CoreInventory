using Repositories.Models;

namespace Repositories.Interfaces;

public interface IReceiptInterface
{
    Task<long> CreateAsync(ReceiptRecord receipt, CancellationToken cancellationToken = default);
    Task<ReceiptRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReceiptRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ReceiptRecord receipt, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReceiptLineRecord>> ListLinesAsync(long receiptId, CancellationToken cancellationToken = default);
    Task<long> AddLineAsync(ReceiptLineRecord line, CancellationToken cancellationToken = default);
    Task<bool> UpdateLineAsync(ReceiptLineRecord line, CancellationToken cancellationToken = default);
    Task<bool> DeleteLineAsync(long lineId, CancellationToken cancellationToken = default);
}
