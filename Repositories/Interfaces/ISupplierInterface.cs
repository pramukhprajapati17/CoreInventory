using Repositories.Models;

namespace Repositories.Interfaces;

public interface ISupplierInterface
{
    Task<long> CreateAsync(SupplierRecord supplier, CancellationToken cancellationToken = default);
    Task<SupplierRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(SupplierRecord supplier, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
