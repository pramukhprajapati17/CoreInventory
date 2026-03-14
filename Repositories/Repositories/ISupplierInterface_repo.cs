using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class ISupplierInterface_repo : ISupplierInterface
{
    private readonly string _connectionString;

    public ISupplierInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(SupplierRecord supplier, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_supplier (c_supplier_name, c_email, c_phone)
            values (@name, @email, @phone)
            returning c_supplier_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", supplier.SupplierName);
        command.Parameters.AddWithValue("@email", (object?)supplier.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@phone", (object?)supplier.Phone ?? DBNull.Value);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<SupplierRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_supplier_id, c_supplier_name, c_email, c_phone
            from t_supplier
            where c_supplier_id = @id;
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

        return new SupplierRecord
        {
            SupplierId = reader.GetInt64(0),
            SupplierName = reader.GetString(1),
            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
        };
    }

    public async Task<IReadOnlyList<SupplierRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_supplier_id, c_supplier_name, c_email, c_phone
            from t_supplier
            order by c_supplier_name;
            """;

        var results = new List<SupplierRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new SupplierRecord
            {
                SupplierId = reader.GetInt64(0),
                SupplierName = reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(SupplierRecord supplier, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_supplier
            set c_supplier_name = @name,
                c_email = @email,
                c_phone = @phone
            where c_supplier_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", supplier.SupplierId);
        command.Parameters.AddWithValue("@name", supplier.SupplierName);
        command.Parameters.AddWithValue("@email", (object?)supplier.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@phone", (object?)supplier.Phone ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_supplier
            where c_supplier_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
