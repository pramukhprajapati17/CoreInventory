using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IUserInterface_repo : IUserInterface
{
    private readonly string _connectionString;

    public IUserInterface_repo(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task<long> CreateUserAsync(UserRecord user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_user
                (c_full_name, c_email, c_password, c_phone, c_is_active, c_created_at, c_updated_at)
            values
                (@full_name, @email, @password, @phone, true, now(), now())
            returning c_user_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@full_name", user.FullName);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@password", user.PasswordHash);
        command.Parameters.AddWithValue("@phone", (object?)user.Phone ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<UserRecord?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                c_user_id,
                c_full_name,
                c_email,
                c_password,
                c_phone,
                c_is_active
            from t_user
            where lower(c_email) = lower(@email)
            limit 1;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserRecord
        {
            UserId = reader.GetInt64(reader.GetOrdinal("c_user_id")),
            FullName = reader.GetString(reader.GetOrdinal("c_full_name")),
            Email = reader.GetString(reader.GetOrdinal("c_email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("c_password")),
            Phone = reader.IsDBNull(reader.GetOrdinal("c_phone")) ? null : reader.GetString(reader.GetOrdinal("c_phone")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("c_is_active")),
        };
    }

    public async Task<UserRecord?> GetUserByIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                c_user_id,
                c_full_name,
                c_email,
                c_password,
                c_phone,
                c_is_active
            from t_user
            where c_user_id = @id
            limit 1;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserRecord
        {
            UserId = reader.GetInt64(reader.GetOrdinal("c_user_id")),
            FullName = reader.GetString(reader.GetOrdinal("c_full_name")),
            Email = reader.GetString(reader.GetOrdinal("c_email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("c_password")),
            Phone = reader.IsDBNull(reader.GetOrdinal("c_phone")) ? null : reader.GetString(reader.GetOrdinal("c_phone")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("c_is_active")),
        };
    }

    public async Task<bool> UpdateProfileAsync(long userId, string fullName, string? phone, string? password, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_user
            set c_full_name = @full_name,
                c_phone = @phone,
                c_password = coalesce(@password, c_password),
                c_updated_at = now()
            where c_user_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@full_name", fullName);
        command.Parameters.AddWithValue("@phone", (object?)phone ?? DBNull.Value);
        command.Parameters.AddWithValue("@password", string.IsNullOrWhiteSpace(password) ? DBNull.Value : password);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdatePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_user
            set c_password = @new_password,
                c_updated_at = now()
            where c_user_id = @id
              and c_password = @current_password;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@current_password", currentPassword);
        command.Parameters.AddWithValue("@new_password", newPassword);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
