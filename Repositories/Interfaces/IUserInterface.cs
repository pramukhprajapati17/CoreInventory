using Repositories.Models;

namespace Repositories.Interfaces;

public interface IUserInterface
{
    Task<long> CreateUserAsync(UserRecord user, CancellationToken cancellationToken = default);
    Task<UserRecord?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}
