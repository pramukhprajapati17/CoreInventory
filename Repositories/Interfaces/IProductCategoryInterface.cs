using Repositories.Models;

namespace Repositories.Interfaces;

public interface IProductCategoryInterface
{
    Task<long> CreateAsync(ProductCategoryRecord category, CancellationToken cancellationToken = default);
    Task<ProductCategoryRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductCategoryRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ProductCategoryRecord category, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
