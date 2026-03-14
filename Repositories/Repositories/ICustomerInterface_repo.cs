using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class ICustomerInterface_repo : ICustomerInterface
{
    private readonly string _connectionString;

    public ICustomerInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(CustomerRecord customer, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_customer (c_customer_name, c_email, c_phone)
            values (@name, @email, @phone)
            returning c_customer_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", customer.CustomerName);
        command.Parameters.AddWithValue("@email", (object?)customer.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@phone", (object?)customer.Phone ?? DBNull.Value);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<CustomerRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_customer_id, c_customer_name, c_email, c_phone
            from t_customer
            where c_customer_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new CustomerRecord
        {
            CustomerId = reader.GetInt64(0),
            CustomerName = reader.GetString(1),
            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
        };
    }

    public async Task<IReadOnlyList<CustomerRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_customer_id, c_customer_name, c_email, c_phone
            from t_customer
            order by c_customer_name;
            """;

        var results = new List<CustomerRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CustomerRecord
            {
                CustomerId = reader.GetInt64(0),
                CustomerName = reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(CustomerRecord customer, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_customer
            set c_customer_name = @name,
                c_email = @email,
                c_phone = @phone
            where c_customer_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", customer.CustomerId);
        command.Parameters.AddWithValue("@name", customer.CustomerName);
        command.Parameters.AddWithValue("@email", (object?)customer.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@phone", (object?)customer.Phone ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_customer
            where c_customer_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
