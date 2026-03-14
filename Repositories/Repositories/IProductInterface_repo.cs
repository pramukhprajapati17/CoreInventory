using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IProductInterface_repo : IProductInterface
{
    private readonly string _connectionString;

    public IProductInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(ProductRecord product, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_product (c_product_name, c_sku, c_category_id, c_uom_id, c_is_active, c_created_at, c_updated_at)
            values (@name, @sku, @category_id, @uom_id, @is_active, now(), now())
            returning c_product_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", product.ProductName);
        command.Parameters.AddWithValue("@sku", product.Sku);
        command.Parameters.AddWithValue("@category_id", (object?)product.CategoryId ?? DBNull.Value);
        command.Parameters.AddWithValue("@uom_id", (object?)product.UomId ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_active", product.IsActive);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<ProductRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_product_id, c_product_name, c_sku, c_category_id, c_uom_id, c_is_active
            from t_product
            where c_product_id = @id;
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

        return new ProductRecord
        {
            ProductId = reader.GetInt64(0),
            ProductName = reader.GetString(1),
            Sku = reader.GetString(2),
            CategoryId = reader.IsDBNull(3) ? null : reader.GetInt64(3),
            UomId = reader.IsDBNull(4) ? null : reader.GetInt64(4),
            IsActive = reader.GetBoolean(5),
        };
    }

    public async Task<IReadOnlyList<ProductRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_product_id, c_product_name, c_sku, c_category_id, c_uom_id, c_is_active
            from t_product
            order by c_product_name;
            """;

        var results = new List<ProductRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProductRecord
            {
                ProductId = reader.GetInt64(0),
                ProductName = reader.GetString(1),
                Sku = reader.GetString(2),
                CategoryId = reader.IsDBNull(3) ? null : reader.GetInt64(3),
                UomId = reader.IsDBNull(4) ? null : reader.GetInt64(4),
                IsActive = reader.GetBoolean(5),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(ProductRecord product, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_product
            set c_product_name = @name,
                c_sku = @sku,
                c_category_id = @category_id,
                c_uom_id = @uom_id,
                c_is_active = @is_active,
                c_updated_at = now()
            where c_product_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", product.ProductId);
        command.Parameters.AddWithValue("@name", product.ProductName);
        command.Parameters.AddWithValue("@sku", product.Sku);
        command.Parameters.AddWithValue("@category_id", (object?)product.CategoryId ?? DBNull.Value);
        command.Parameters.AddWithValue("@uom_id", (object?)product.UomId ?? DBNull.Value);
        command.Parameters.AddWithValue("@is_active", product.IsActive);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string existsSql = """
            select 1
            from t_stock
            where c_product_id = @id
            limit 1;
            """;

        const string sql = """
            update t_product
            set c_is_active = false,
                c_updated_at = now()
            where c_product_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var existsCommand = new NpgsqlCommand(existsSql, connection))
        {
            existsCommand.Parameters.AddWithValue("@id", id);
            var exists = await existsCommand.ExecuteScalarAsync(cancellationToken);
            if (exists is not null)
            {
                return false;
            }
        }

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
