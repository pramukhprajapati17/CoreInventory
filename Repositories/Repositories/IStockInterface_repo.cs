using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IStockInterface_repo : IStockInterface
{
    private readonly string _connectionString;

    public IStockInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<StockRecord?> GetByProductLocationAsync(long productId, long locationId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_stock_id, c_product_id, c_location_id, c_qty
            from t_stock
            where c_product_id = @product_id
              and c_location_id = @location_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@product_id", productId);
        command.Parameters.AddWithValue("@location_id", locationId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new StockRecord
        {
            StockId = reader.GetInt64(0),
            ProductId = reader.GetInt64(1),
            LocationId = reader.GetInt64(2),
            Quantity = reader.GetDecimal(3),
        };
    }

    public async Task<bool> UpsertAsync(StockRecord stock, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_stock (c_product_id, c_location_id, c_qty)
            values (@product_id, @location_id, @qty)
            on conflict (c_product_id, c_location_id)
            do update set c_qty = excluded.c_qty;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@product_id", stock.ProductId);
        command.Parameters.AddWithValue("@location_id", stock.LocationId);
        command.Parameters.AddWithValue("@qty", stock.Quantity);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
