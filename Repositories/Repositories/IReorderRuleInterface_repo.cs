using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IReorderRuleInterface_repo : IReorderRuleInterface
{
    private readonly string _connectionString;

    public IReorderRuleInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(ReorderRuleRecord rule, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_reorder_rule (c_product_id, c_location_id, c_min_qty, c_max_qty, c_is_active)
            values (@product_id, @location_id, @min_qty, @max_qty, @is_active)
            returning c_reorder_rule_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@product_id", rule.ProductId);
        command.Parameters.AddWithValue("@location_id", (object?)rule.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@min_qty", rule.MinQty);
        command.Parameters.AddWithValue("@max_qty", (object?)rule.MaxQty ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_active", rule.IsActive);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<ReorderRuleRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_reorder_rule_id, c_product_id, c_location_id, c_min_qty, c_max_qty, c_is_active
            from t_reorder_rule
            where c_reorder_rule_id = @id;
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

        return new ReorderRuleRecord
        {
            ReorderRuleId = reader.GetInt64(0),
            ProductId = reader.GetInt64(1),
            LocationId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
            MinQty = reader.GetDecimal(3),
            MaxQty = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
            IsActive = reader.GetBoolean(5),
        };
    }

    public async Task<IReadOnlyList<ReorderRuleRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_reorder_rule_id, c_product_id, c_location_id, c_min_qty, c_max_qty, c_is_active
            from t_reorder_rule
            order by c_reorder_rule_id desc;
            """;

        var results = new List<ReorderRuleRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ReorderRuleRecord
            {
                ReorderRuleId = reader.GetInt64(0),
                ProductId = reader.GetInt64(1),
                LocationId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                MinQty = reader.GetDecimal(3),
                MaxQty = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                IsActive = reader.GetBoolean(5),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(ReorderRuleRecord rule, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_reorder_rule
            set c_product_id = @product_id,
                c_location_id = @location_id,
                c_min_qty = @min_qty,
                c_max_qty = @max_qty,
                c_is_active = @is_active
            where c_reorder_rule_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", rule.ReorderRuleId);
        command.Parameters.AddWithValue("@product_id", rule.ProductId);
        command.Parameters.AddWithValue("@location_id", (object?)rule.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@min_qty", rule.MinQty);
        command.Parameters.AddWithValue("@max_qty", (object?)rule.MaxQty ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_active", rule.IsActive);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_reorder_rule
            where c_reorder_rule_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
