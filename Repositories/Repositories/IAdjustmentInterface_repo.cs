using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IAdjustmentInterface_repo : IAdjustmentInterface
{
    private readonly string _connectionString;

    public IAdjustmentInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(AdjustmentRecord adjustment, CancellationToken cancellationToken = default)
    {
        const string insertHeader = """
            insert into t_adjustment (c_adjustment_no, c_location_id, c_status, c_reason, c_created_by, c_created_at)
            values (@no, @location_id, @status, @reason, @created_by, now())
            returning c_adjustment_id;
            """;

        const string insertLine = """
            insert into t_adjustment_line (c_adjustment_id, c_product_id, c_counted_qty, c_system_qty)
            values (@adjustment_id, @product_id, @counted_qty, @system_qty);
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var headerCommand = new NpgsqlCommand(insertHeader, connection, transaction);
        headerCommand.Parameters.AddWithValue("@no", adjustment.AdjustmentNo);
        headerCommand.Parameters.AddWithValue("@location_id", adjustment.LocationId);
        headerCommand.Parameters.AddWithValue("@status", adjustment.Status);
        headerCommand.Parameters.AddWithValue("@reason", (object?)adjustment.Reason ?? DBNull.Value);
        headerCommand.Parameters.AddWithValue("@created_by", (object?)adjustment.CreatedBy ?? DBNull.Value);
        var headerResult = await headerCommand.ExecuteScalarAsync(cancellationToken);
        var adjustmentId = headerResult is long id ? id : Convert.ToInt64(headerResult);

        foreach (var line in adjustment.Lines)
        {
            await using var lineCommand = new NpgsqlCommand(insertLine, connection, transaction);
            lineCommand.Parameters.AddWithValue("@adjustment_id", adjustmentId);
            lineCommand.Parameters.AddWithValue("@product_id", line.ProductId);
            lineCommand.Parameters.AddWithValue("@counted_qty", line.CountedQty);
            lineCommand.Parameters.AddWithValue("@system_qty", line.SystemQty);
            await lineCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return adjustmentId;
    }

    public async Task<AdjustmentRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string headerSql = """
            select c_adjustment_id, c_adjustment_no, c_location_id, c_status, c_reason, c_created_by, c_created_at
            from t_adjustment
            where c_adjustment_id = @id;
            """;

        const string linesSql = """
            select c_adjustment_line_id, c_adjustment_id, c_product_id, c_counted_qty, c_system_qty
            from t_adjustment_line
            where c_adjustment_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        AdjustmentRecord? adjustment = null;
        await using (var headerCommand = new NpgsqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("@id", id);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                adjustment = new AdjustmentRecord
                {
                    AdjustmentId = reader.GetInt64(0),
                    AdjustmentNo = reader.GetString(1),
                    LocationId = reader.GetInt64(2),
                    Status = reader.GetString(3),
                    Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                    CreatedAt = reader.GetDateTime(6),
                };
            }
        }

        if (adjustment is null)
        {
            return null;
        }

        await using var linesCommand = new NpgsqlCommand(linesSql, connection);
        linesCommand.Parameters.AddWithValue("@id", id);
        await using var linesReader = await linesCommand.ExecuteReaderAsync(cancellationToken);
        while (await linesReader.ReadAsync(cancellationToken))
        {
            adjustment.Lines.Add(new AdjustmentLineRecord
            {
                AdjustmentLineId = linesReader.GetInt64(0),
                AdjustmentId = linesReader.GetInt64(1),
                ProductId = linesReader.GetInt64(2),
                CountedQty = linesReader.GetDecimal(3),
                SystemQty = linesReader.GetDecimal(4),
            });
        }

        return adjustment;
    }

    public async Task<IReadOnlyList<AdjustmentRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_adjustment_id, c_adjustment_no, c_location_id, c_status, c_reason, c_created_by, c_created_at
            from t_adjustment
            order by c_adjustment_id desc;
            """;

        var results = new List<AdjustmentRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AdjustmentRecord
            {
                AdjustmentId = reader.GetInt64(0),
                AdjustmentNo = reader.GetString(1),
                LocationId = reader.GetInt64(2),
                Status = reader.GetString(3),
                Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(AdjustmentRecord adjustment, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_adjustment
            set c_location_id = @location_id,
                c_status = @status,
                c_reason = @reason
            where c_adjustment_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", adjustment.AdjustmentId);
        command.Parameters.AddWithValue("@location_id", adjustment.LocationId);
        command.Parameters.AddWithValue("@status", adjustment.Status);
        command.Parameters.AddWithValue("@reason", (object?)adjustment.Reason ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_adjustment
            set c_status = @status
            where c_adjustment_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@status", status);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<AdjustmentLineRecord>> ListLinesAsync(long adjustmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_adjustment_line_id, c_adjustment_id, c_product_id, c_counted_qty, c_system_qty
            from t_adjustment_line
            where c_adjustment_id = @id;
            """;

        var results = new List<AdjustmentLineRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", adjustmentId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AdjustmentLineRecord
            {
                AdjustmentLineId = reader.GetInt64(0),
                AdjustmentId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                CountedQty = reader.GetDecimal(3),
                SystemQty = reader.GetDecimal(4),
            });
        }

        return results;
    }

    public async Task<long> AddLineAsync(AdjustmentLineRecord line, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_adjustment_line (c_adjustment_id, c_product_id, c_counted_qty, c_system_qty)
            values (@adjustment_id, @product_id, @counted_qty, @system_qty)
            returning c_adjustment_line_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var locationId = await GetAdjustmentLocationAsync(connection, transaction, line.AdjustmentId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@adjustment_id", line.AdjustmentId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@counted_qty", line.CountedQty);
        command.Parameters.AddWithValue("@system_qty", line.SystemQty);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        var lineId = result is long id ? id : Convert.ToInt64(result);

        var delta = line.CountedQty - line.SystemQty;
        if (delta != 0)
        {
            await ApplyStockChangeAsync(connection, transaction, line.ProductId, locationId, delta, "adjustment", line.AdjustmentId, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return lineId;
    }

    public async Task<bool> UpdateLineAsync(AdjustmentLineRecord line, CancellationToken cancellationToken = default)
    {
        const string selectSql = """
            select c_product_id, c_counted_qty, c_system_qty, c_adjustment_id
            from t_adjustment_line
            where c_adjustment_line_id = @id;
            """;

        const string sql = """
            update t_adjustment_line
            set c_product_id = @product_id,
                c_counted_qty = @counted_qty,
                c_system_qty = @system_qty
            where c_adjustment_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        long oldProductId;
        decimal oldCounted;
        decimal oldSystem;
        long adjustmentId;
        await using (var selectCommand = new NpgsqlCommand(selectSql, connection, transaction))
        {
            selectCommand.Parameters.AddWithValue("@id", line.AdjustmentLineId);
            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return false;
            }
            oldProductId = reader.GetInt64(0);
            oldCounted = reader.GetDecimal(1);
            oldSystem = reader.GetDecimal(2);
            adjustmentId = reader.GetInt64(3);
        }

        await using (var command = new NpgsqlCommand(sql, connection, transaction))
        {
            command.Parameters.AddWithValue("@id", line.AdjustmentLineId);
            command.Parameters.AddWithValue("@product_id", line.ProductId);
            command.Parameters.AddWithValue("@counted_qty", line.CountedQty);
            command.Parameters.AddWithValue("@system_qty", line.SystemQty);
            var updated = await command.ExecuteNonQueryAsync(cancellationToken) > 0;
            if (!updated)
            {
                return false;
            }
        }

        var locationId = await GetAdjustmentLocationAsync(connection, transaction, adjustmentId, cancellationToken);
        var oldDelta = oldCounted - oldSystem;
        var newDelta = line.CountedQty - line.SystemQty;

        if (oldProductId == line.ProductId)
        {
            var delta = newDelta - oldDelta;
            if (delta != 0)
            {
                await ApplyStockChangeAsync(connection, transaction, line.ProductId, locationId, delta, "adjustment", adjustmentId, cancellationToken);
            }
        }
        else
        {
            if (oldDelta != 0)
            {
                await ApplyStockChangeAsync(connection, transaction, oldProductId, locationId, -oldDelta, "adjustment", adjustmentId, cancellationToken);
            }
            if (newDelta != 0)
            {
                await ApplyStockChangeAsync(connection, transaction, line.ProductId, locationId, newDelta, "adjustment", adjustmentId, cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteLineAsync(long lineId, CancellationToken cancellationToken = default)
    {
        const string selectSql = """
            select c_product_id, c_counted_qty, c_system_qty, c_adjustment_id
            from t_adjustment_line
            where c_adjustment_line_id = @id;
            """;

        const string sql = """
            delete from t_adjustment_line
            where c_adjustment_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        long productId;
        decimal counted;
        decimal system;
        long adjustmentId;
        await using (var selectCommand = new NpgsqlCommand(selectSql, connection, transaction))
        {
            selectCommand.Parameters.AddWithValue("@id", lineId);
            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return false;
            }
            productId = reader.GetInt64(0);
            counted = reader.GetDecimal(1);
            system = reader.GetDecimal(2);
            adjustmentId = reader.GetInt64(3);
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

        var locationId = await GetAdjustmentLocationAsync(connection, transaction, adjustmentId, cancellationToken);
        var delta = counted - system;
        if (delta != 0)
        {
            await ApplyStockChangeAsync(connection, transaction, productId, locationId, -delta, "adjustment", adjustmentId, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static async Task<long> GetAdjustmentLocationAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long adjustmentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            select c_location_id
            from t_adjustment
            where c_adjustment_id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@id", adjustmentId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
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
