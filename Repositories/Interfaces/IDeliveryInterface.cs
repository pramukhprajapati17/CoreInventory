using Repositories.Models;

namespace Repositories.Interfaces;

public interface IDeliveryInterface
{
    Task<long> CreateAsync(DeliveryRecord delivery, CancellationToken cancellationToken = default);
    Task<DeliveryRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(DeliveryRecord delivery, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default);
}
