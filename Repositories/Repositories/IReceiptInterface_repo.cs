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
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@receipt_id", line.ReceiptId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@location_id", (object?)line.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@qty", line.Quantity);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<bool> UpdateLineAsync(ReceiptLineRecord line, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_receipt_line
            set c_product_id = @product_id,
                c_location_id = @location_id,
                c_qty = @qty
            where c_receipt_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", line.ReceiptLineId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@location_id", (object?)line.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@qty", line.Quantity);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteLineAsync(long lineId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_receipt_line
            where c_receipt_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", lineId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
