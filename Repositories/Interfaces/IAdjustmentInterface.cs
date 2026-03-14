using Repositories.Models;

namespace Repositories.Interfaces;

public interface IAdjustmentInterface
{
    Task<long> CreateAsync(AdjustmentRecord adjustment, CancellationToken cancellationToken = default);
    Task<AdjustmentRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdjustmentRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(AdjustmentRecord adjustment, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default);
}
