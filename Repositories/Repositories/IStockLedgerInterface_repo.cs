using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IStockLedgerInterface_repo : IStockLedgerInterface
{
    private readonly string _connectionString;

    public IStockLedgerInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> AddEntryAsync(StockLedgerRecord entry, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_stock_ledger (c_product_id, c_location_id, c_doc_type, c_doc_id, c_qty_change, c_created_at)
            values (@product_id, @location_id, @doc_type, @doc_id, @qty_change, now())
            returning c_ledger_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@product_id", entry.ProductId);
        command.Parameters.AddWithValue("@location_id", (object?)entry.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@doc_type", entry.DocType);
        command.Parameters.AddWithValue("@doc_id", entry.DocId);
        command.Parameters.AddWithValue("@qty_change", entry.QtyChange);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<IReadOnlyList<StockLedgerRecord>> ListByProductAsync(long productId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_ledger_id, c_product_id, c_location_id, c_doc_type, c_doc_id, c_qty_change, c_created_at
            from t_stock_ledger
            where c_product_id = @product_id
            order by c_ledger_id desc;
            """;

        var results = new List<StockLedgerRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@product_id", productId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new StockLedgerRecord
            {
                LedgerId = reader.GetInt64(0),
                ProductId = reader.GetInt64(1),
                LocationId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                DocType = reader.GetString(3),
                DocId = reader.GetInt64(4),
                QtyChange = reader.GetDecimal(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<StockLedgerRecord>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_ledger_id, c_product_id, c_location_id, c_doc_type, c_doc_id, c_qty_change, c_created_at
            from t_stock_ledger
            order by c_ledger_id desc;
            """;

        var results = new List<StockLedgerRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new StockLedgerRecord
            {
                LedgerId = reader.GetInt64(0),
                ProductId = reader.GetInt64(1),
                LocationId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                DocType = reader.GetString(3),
                DocId = reader.GetInt64(4),
                QtyChange = reader.GetDecimal(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }
}
