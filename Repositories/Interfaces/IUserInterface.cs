using Repositories.Models;

namespace Repositories.Interfaces;

public interface IUserInterface
{
    Task<long> CreateUserAsync(UserRecord user, CancellationToken cancellationToken = default);
    Task<UserRecord?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserRecord?> GetUserByIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(long userId, string fullName, string? phone, string? password, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
