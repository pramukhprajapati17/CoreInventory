using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IUomInterface_repo : IUomInterface
{
    private readonly string _connectionString;

    public IUomInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(UomRecord uom, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_uom (c_uom_name, c_uom_code)
            values (@name, @code)
            returning c_uom_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", uom.UomName);
        command.Parameters.AddWithValue("@code", uom.UomCode);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<UomRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_uom_id, c_uom_name, c_uom_code
            from t_uom
            where c_uom_id = @id;
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

        return new UomRecord
        {
            UomId = reader.GetInt64(0),
            UomName = reader.GetString(1),
            UomCode = reader.GetString(2),
        };
    }

    public async Task<IReadOnlyList<UomRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_uom_id, c_uom_name, c_uom_code
            from t_uom
            order by c_uom_name;
            """;

        var results = new List<UomRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new UomRecord
            {
                UomId = reader.GetInt64(0),
                UomName = reader.GetString(1),
                UomCode = reader.GetString(2),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(UomRecord uom, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_uom
            set c_uom_name = @name,
                c_uom_code = @code
            where c_uom_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", uom.UomId);
        command.Parameters.AddWithValue("@name", uom.UomName);
        command.Parameters.AddWithValue("@code", uom.UomCode);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_uom
            where c_uom_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
