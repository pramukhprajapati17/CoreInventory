using Repositories.Models;

namespace Repositories.Interfaces;

public interface IReorderRuleInterface
{
    Task<long> CreateAsync(ReorderRuleRecord rule, CancellationToken cancellationToken = default);
    Task<ReorderRuleRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReorderRuleRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ReorderRuleRecord rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
