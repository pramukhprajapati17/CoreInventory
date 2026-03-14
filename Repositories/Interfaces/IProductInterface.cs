using Repositories.Models;

namespace Repositories.Interfaces;

public interface IProductInterface
{
    Task<long> CreateAsync(ProductRecord product, CancellationToken cancellationToken = default);
    Task<ProductRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ProductRecord product, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
