using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class ILocationInterface_repo : ILocationInterface
{
    private readonly string _connectionString;

    public ILocationInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(LocationRecord location, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_location (c_warehouse_id, c_location_name, c_location_code, c_is_active)
            values (@warehouse_id, @name, @code, @is_active)
            returning c_location_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@warehouse_id", location.WarehouseId);
        command.Parameters.AddWithValue("@name", location.LocationName);
        command.Parameters.AddWithValue("@code", location.LocationCode);
        command.Parameters.AddWithValue("@is_active", location.IsActive);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<LocationRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_location_id, c_warehouse_id, c_location_name, c_location_code, c_is_active
            from t_location
            where c_location_id = @id;
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

        return new LocationRecord
        {
            LocationId = reader.GetInt64(0),
            WarehouseId = reader.GetInt64(1),
            LocationName = reader.GetString(2),
            LocationCode = reader.GetString(3),
            IsActive = reader.GetBoolean(4),
        };
    }

    public async Task<IReadOnlyList<LocationRecord>> ListByWarehouseAsync(long warehouseId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_location_id, c_warehouse_id, c_location_name, c_location_code, c_is_active
            from t_location
            where c_warehouse_id = @warehouse_id
            order by c_location_name;
            """;

        var results = new List<LocationRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@warehouse_id", warehouseId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new LocationRecord
            {
                LocationId = reader.GetInt64(0),
                WarehouseId = reader.GetInt64(1),
                LocationName = reader.GetString(2),
                LocationCode = reader.GetString(3),
                IsActive = reader.GetBoolean(4),
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<LocationRecord>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_location_id, c_warehouse_id, c_location_name, c_location_code, c_is_active
            from t_location
            order by c_location_name;
            """;

        var results = new List<LocationRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new LocationRecord
            {
                LocationId = reader.GetInt64(0),
                WarehouseId = reader.GetInt64(1),
                LocationName = reader.GetString(2),
                LocationCode = reader.GetString(3),
                IsActive = reader.GetBoolean(4),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(LocationRecord location, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_location
            set c_warehouse_id = @warehouse_id,
                c_location_name = @name,
                c_location_code = @code,
                c_is_active = @is_active
            where c_location_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", location.LocationId);
        command.Parameters.AddWithValue("@warehouse_id", location.WarehouseId);
        command.Parameters.AddWithValue("@name", location.LocationName);
        command.Parameters.AddWithValue("@code", location.LocationCode);
        command.Parameters.AddWithValue("@is_active", location.IsActive);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_location
            where c_location_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
