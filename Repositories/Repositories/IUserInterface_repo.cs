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
}
