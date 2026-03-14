using Repositories.Models;

namespace Repositories.Interfaces;

public interface ICustomerInterface
{
    Task<long> CreateAsync(CustomerRecord customer, CancellationToken cancellationToken = default);
    Task<CustomerRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CustomerRecord customer, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
