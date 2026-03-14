using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IWarehouseInterface_repo : IWarehouseInterface
{
    private readonly string _connectionString;

    public IWarehouseInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(WarehouseRecord warehouse, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_warehouse (c_warehouse_name, c_code, c_is_active)
            values (@name, @code, @is_active)
            returning c_warehouse_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", warehouse.WarehouseName);
        command.Parameters.AddWithValue("@code", warehouse.Code);
        command.Parameters.AddWithValue("@is_active", warehouse.IsActive);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<WarehouseRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_warehouse_id, c_warehouse_name, c_code, c_is_active
            from t_warehouse
            where c_warehouse_id = @id;
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

        return new WarehouseRecord
        {
            WarehouseId = reader.GetInt64(0),
            WarehouseName = reader.GetString(1),
            Code = reader.GetString(2),
            IsActive = reader.GetBoolean(3),
        };
    }

    public async Task<IReadOnlyList<WarehouseRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_warehouse_id, c_warehouse_name, c_code, c_is_active
            from t_warehouse
            order by c_warehouse_name;
            """;

        var results = new List<WarehouseRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new WarehouseRecord
            {
                WarehouseId = reader.GetInt64(0),
                WarehouseName = reader.GetString(1),
                Code = reader.GetString(2),
                IsActive = reader.GetBoolean(3),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(WarehouseRecord warehouse, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_warehouse
            set c_warehouse_name = @name,
                c_code = @code,
                c_is_active = @is_active
            where c_warehouse_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", warehouse.WarehouseId);
        command.Parameters.AddWithValue("@name", warehouse.WarehouseName);
        command.Parameters.AddWithValue("@code", warehouse.Code);
        command.Parameters.AddWithValue("@is_active", warehouse.IsActive);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_warehouse
            where c_warehouse_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
