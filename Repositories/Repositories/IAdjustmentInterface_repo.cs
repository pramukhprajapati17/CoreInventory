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
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@adjustment_id", line.AdjustmentId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@counted_qty", line.CountedQty);
        command.Parameters.AddWithValue("@system_qty", line.SystemQty);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<bool> UpdateLineAsync(AdjustmentLineRecord line, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_adjustment_line
            set c_product_id = @product_id,
                c_counted_qty = @counted_qty,
                c_system_qty = @system_qty
            where c_adjustment_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", line.AdjustmentLineId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@counted_qty", line.CountedQty);
        command.Parameters.AddWithValue("@system_qty", line.SystemQty);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteLineAsync(long lineId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_adjustment_line
            where c_adjustment_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", lineId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
