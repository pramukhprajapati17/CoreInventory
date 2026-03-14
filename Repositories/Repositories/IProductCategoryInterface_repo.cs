using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IProductCategoryInterface_repo : IProductCategoryInterface
{
    private readonly string _connectionString;

    public IProductCategoryInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(ProductCategoryRecord category, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_product_category (c_category_name, c_category_code)
            values (@name, @code)
            returning c_category_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", category.CategoryName);
        command.Parameters.AddWithValue("@code", category.CategoryCode);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<ProductCategoryRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_category_id, c_category_name, c_category_code
            from t_product_category
            where c_category_id = @id;
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

        return new ProductCategoryRecord
        {
            CategoryId = reader.GetInt64(0),
            CategoryName = reader.GetString(1),
            CategoryCode = reader.GetString(2),
        };
    }

    public async Task<IReadOnlyList<ProductCategoryRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_category_id, c_category_name, c_category_code
            from t_product_category
            order by c_category_name;
            """;

        var results = new List<ProductCategoryRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProductCategoryRecord
            {
                CategoryId = reader.GetInt64(0),
                CategoryName = reader.GetString(1),
                CategoryCode = reader.GetString(2),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(ProductCategoryRecord category, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_product_category
            set c_category_name = @name,
                c_category_code = @code
            where c_category_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", category.CategoryId);
        command.Parameters.AddWithValue("@name", category.CategoryName);
        command.Parameters.AddWithValue("@code", category.CategoryCode);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_product_category
            where c_category_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
