using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IReceiptInterface_repo : IReceiptInterface
{
    private readonly string _connectionString;

    public IReceiptInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(ReceiptRecord receipt, CancellationToken cancellationToken = default)
    {
        const string insertHeader = """
            insert into t_receipt (c_receipt_no, c_supplier_id, c_status, c_expected_date, c_created_by, c_created_at)
            values (@no, @supplier_id, @status, @expected_date, @created_by, now())
            returning c_receipt_id;
            """;

        const string insertLine = """
            insert into t_receipt_line (c_receipt_id, c_product_id, c_location_id, c_qty)
            values (@receipt_id, @product_id, @location_id, @qty);
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var headerCommand = new NpgsqlCommand(insertHeader, connection, transaction);
        headerCommand.Parameters.AddWithValue("@no", receipt.ReceiptNo);
        headerCommand.Parameters.AddWithValue("@supplier_id", (object?)receipt.SupplierId ?? DBNull.Value);
        headerCommand.Parameters.AddWithValue("@status", receipt.Status);
        headerCommand.Parameters.AddWithValue("@expected_date", (object?)receipt.ExpectedDate ?? DBNull.Value);
        headerCommand.Parameters.AddWithValue("@created_by", (object?)receipt.CreatedBy ?? DBNull.Value);
        var headerResult = await headerCommand.ExecuteScalarAsync(cancellationToken);
        var receiptId = headerResult is long id ? id : Convert.ToInt64(headerResult);

        foreach (var line in receipt.Lines)
        {
            await using var lineCommand = new NpgsqlCommand(insertLine, connection, transaction);
            lineCommand.Parameters.AddWithValue("@receipt_id", receiptId);
            lineCommand.Parameters.AddWithValue("@product_id", line.ProductId);
            lineCommand.Parameters.AddWithValue("@location_id", (object?)line.LocationId ?? DBNull.Value);
            lineCommand.Parameters.AddWithValue("@qty", line.Quantity);
            await lineCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return receiptId;
    }

    public async Task<ReceiptRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string headerSql = """
            select c_receipt_id, c_receipt_no, c_supplier_id, c_status, c_expected_date, c_created_by, c_created_at
            from t_receipt
            where c_receipt_id = @id;
            """;

        const string linesSql = """
            select c_receipt_line_id, c_receipt_id, c_product_id, c_location_id, c_qty
            from t_receipt_line
            where c_receipt_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        ReceiptRecord? receipt = null;
        await using (var headerCommand = new NpgsqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("@id", id);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                receipt = new ReceiptRecord
                {
                    ReceiptId = reader.GetInt64(0),
                    ReceiptNo = reader.GetString(1),
                    SupplierId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                    Status = reader.GetString(3),
                    ExpectedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                    CreatedAt = reader.GetDateTime(6),
                };
            }
        }

        if (receipt is null)
        {
            return null;
        }

        await using var linesCommand = new NpgsqlCommand(linesSql, connection);
        linesCommand.Parameters.AddWithValue("@id", id);
        await using var linesReader = await linesCommand.ExecuteReaderAsync(cancellationToken);
        while (await linesReader.ReadAsync(cancellationToken))
        {
            receipt.Lines.Add(new ReceiptLineRecord
            {
                ReceiptLineId = linesReader.GetInt64(0),
                ReceiptId = linesReader.GetInt64(1),
                ProductId = linesReader.GetInt64(2),
                LocationId = linesReader.IsDBNull(3) ? null : linesReader.GetInt64(3),
                Quantity = linesReader.GetDecimal(4),
            });
        }

        return receipt;
    }

    public async Task<IReadOnlyList<ReceiptRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_receipt_id, c_receipt_no, c_supplier_id, c_status, c_expected_date, c_created_by, c_created_at
            from t_receipt
            order by c_receipt_id desc;
            """;

        var results = new List<ReceiptRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ReceiptRecord
            {
                ReceiptId = reader.GetInt64(0),
                ReceiptNo = reader.GetString(1),
                SupplierId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                Status = reader.GetString(3),
                ExpectedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(ReceiptRecord receipt, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_receipt
            set c_supplier_id = @supplier_id,
                c_status = @status,
                c_expected_date = @expected_date
            where c_receipt_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", receipt.ReceiptId);
        command.Parameters.AddWithValue("@supplier_id", (object?)receipt.SupplierId ?? DBNull.Value);
        command.Parameters.AddWithValue("@status", receipt.Status);
        command.Parameters.AddWithValue("@expected_date", (object?)receipt.ExpectedDate ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_receipt
            set c_status = @status
            where c_receipt_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@status", status);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<ReceiptLineRecord>> ListLinesAsync(long receiptId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_receipt_line_id, c_receipt_id, c_product_id, c_location_id, c_qty
            from t_receipt_line
            where c_receipt_id = @id;
            """;

        var results = new List<ReceiptLineRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", receiptId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ReceiptLineRecord
            {
                ReceiptLineId = reader.GetInt64(0),
                ReceiptId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                LocationId = reader.IsDBNull(3) ? null : reader.GetInt64(3),
                Quantity = reader.GetDecimal(4),
            });
        }

        return results;
    }

    public async Task<long> AddLineAsync(ReceiptLineRecord line, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_receipt_line (c_receipt_id, c_product_id, c_location_id, c_qty)
            values (@receipt_id, @product_id, @location_id, @qty)
            returning c_receipt_line_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@receipt_id", line.ReceiptId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@location_id", (object?)line.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@qty", line.Quantity);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        var lineId = result is long id ? id : Convert.ToInt64(result);

        if (line.LocationId.HasValue)
        {
            await ApplyStockChangeAsync(connection, transaction, line.ProductId, line.LocationId.Value, line.Quantity, "receipt", line.ReceiptId, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return lineId;
    }

    public async Task<bool> UpdateLineAsync(ReceiptLineRecord line, CancellationToken cancellationToken = default)
    {
        const string selectSql = """
            select c_product_id, c_location_id, c_qty, c_receipt_id
            from t_receipt_line
            where c_receipt_line_id = @id;
            """;

        const string sql = """
            update t_receipt_line
            set c_product_id = @product_id,
                c_location_id = @location_id,
                c_qty = @qty
            where c_receipt_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        long oldProductId;
        long? oldLocationId;
        decimal oldQty;
        long receiptId;
        await using (var selectCommand = new NpgsqlCommand(selectSql, connection, transaction))
        {
            selectCommand.Parameters.AddWithValue("@id", line.ReceiptLineId);
            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return false;
            }
            oldProductId = reader.GetInt64(0);
            oldLocationId = reader.IsDBNull(1) ? null : reader.GetInt64(1);
            oldQty = reader.GetDecimal(2);
            receiptId = reader.GetInt64(3);
        }

        await using (var command = new NpgsqlCommand(sql, connection, transaction))
        {
            command.Parameters.AddWithValue("@id", line.ReceiptLineId);
            command.Parameters.AddWithValue("@product_id", line.ProductId);
            command.Parameters.AddWithValue("@location_id", (object?)line.LocationId ?? DBNull.Value);
            command.Parameters.AddWithValue("@qty", line.Quantity);
            var updated = await command.ExecuteNonQueryAsync(cancellationToken) > 0;
            if (!updated)
            {
                return false;
            }
        }

        if (oldLocationId.HasValue && (oldProductId != line.ProductId || oldLocationId != line.LocationId))
        {
            await ApplyStockChangeAsync(connection, transaction, oldProductId, oldLocationId.Value, -oldQty, "receipt", receiptId, cancellationToken);
        }

        if (line.LocationId.HasValue)
        {
            if (oldProductId == line.ProductId && oldLocationId == line.LocationId)
            {
                var delta = line.Quantity - oldQty;
                if (delta != 0)
                {
                    await ApplyStockChangeAsync(connection, transaction, line.ProductId, line.LocationId.Value, delta, "receipt", receiptId, cancellationToken);
                }
            }
            else
            {
                await ApplyStockChangeAsync(connection, transaction, line.ProductId, line.LocationId.Value, line.Quantity, "receipt", receiptId, cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteLineAsync(long lineId, CancellationToken cancellationToken = default)
    {
        const string selectSql = """
            select c_product_id, c_location_id, c_qty, c_receipt_id
            from t_receipt_line
            where c_receipt_line_id = @id;
            """;

        const string sql = """
            delete from t_receipt_line
            where c_receipt_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        long productId;
        long? locationId;
        decimal qty;
        long receiptId;
        await using (var selectCommand = new NpgsqlCommand(selectSql, connection, transaction))
        {
            selectCommand.Parameters.AddWithValue("@id", lineId);
            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return false;
            }
            productId = reader.GetInt64(0);
            locationId = reader.IsDBNull(1) ? null : reader.GetInt64(1);
            qty = reader.GetDecimal(2);
            receiptId = reader.GetInt64(3);
        }

        await using (var command = new NpgsqlCommand(sql, connection, transaction))
        {
            command.Parameters.AddWithValue("@id", lineId);
            var deleted = await command.ExecuteNonQueryAsync(cancellationToken) > 0;
            if (!deleted)
            {
                return false;
            }
        }

        if (locationId.HasValue)
        {
            await ApplyStockChangeAsync(connection, transaction, productId, locationId.Value, -qty, "receipt", receiptId, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static async Task ApplyStockChangeAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long productId,
        long locationId,
        decimal qtyChange,
        string docType,
        long docId,
        CancellationToken cancellationToken)
    {
        const string stockSql = """
            insert into t_stock (c_product_id, c_location_id, c_qty)
            values (@product_id, @location_id, @qty)
            on conflict (c_product_id, c_location_id)
            do update set c_qty = t_stock.c_qty + excluded.c_qty;
            """;

        const string ledgerSql = """
            insert into t_stock_ledger (c_product_id, c_location_id, c_doc_type, c_doc_id, c_qty_change)
            values (@product_id, @location_id, @doc_type, @doc_id, @qty_change);
            """;

        await using (var stockCommand = new NpgsqlCommand(stockSql, connection, transaction))
        {
            stockCommand.Parameters.AddWithValue("@product_id", productId);
            stockCommand.Parameters.AddWithValue("@location_id", locationId);
            stockCommand.Parameters.AddWithValue("@qty", qtyChange);
            await stockCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var ledgerCommand = new NpgsqlCommand(ledgerSql, connection, transaction))
        {
            ledgerCommand.Parameters.AddWithValue("@product_id", productId);
            ledgerCommand.Parameters.AddWithValue("@location_id", locationId);
            ledgerCommand.Parameters.AddWithValue("@doc_type", docType);
            ledgerCommand.Parameters.AddWithValue("@doc_id", docId);
            ledgerCommand.Parameters.AddWithValue("@qty_change", qtyChange);
            await ledgerCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
