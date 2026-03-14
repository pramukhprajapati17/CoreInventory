using Repositories.Models;

namespace Repositories.Interfaces;

public interface ITransferInterface
{
    Task<long> CreateAsync(TransferRecord transfer, CancellationToken cancellationToken = default);
    Task<TransferRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransferRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TransferRecord transfer, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default);
}
