using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class ITransferInterface_repo : ITransferInterface
{
    private readonly string _connectionString;

    public ITransferInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(TransferRecord transfer, CancellationToken cancellationToken = default)
    {
        const string insertHeader = """
            insert into t_transfer (c_transfer_no, c_from_location_id, c_to_location_id, c_status, c_created_by, c_created_at)
            values (@no, @from_location_id, @to_location_id, @status, @created_by, now())
            returning c_transfer_id;
            """;

        const string insertLine = """
            insert into t_transfer_line (c_transfer_id, c_product_id, c_qty)
            values (@transfer_id, @product_id, @qty);
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var headerCommand = new NpgsqlCommand(insertHeader, connection, transaction);
        headerCommand.Parameters.AddWithValue("@no", transfer.TransferNo);
        headerCommand.Parameters.AddWithValue("@from_location_id", transfer.FromLocationId);
        headerCommand.Parameters.AddWithValue("@to_location_id", transfer.ToLocationId);
        headerCommand.Parameters.AddWithValue("@status", transfer.Status);
        headerCommand.Parameters.AddWithValue("@created_by", (object?)transfer.CreatedBy ?? DBNull.Value);
        var headerResult = await headerCommand.ExecuteScalarAsync(cancellationToken);
        var transferId = headerResult is long id ? id : Convert.ToInt64(headerResult);

        foreach (var line in transfer.Lines)
        {
            await using var lineCommand = new NpgsqlCommand(insertLine, connection, transaction);
            lineCommand.Parameters.AddWithValue("@transfer_id", transferId);
            lineCommand.Parameters.AddWithValue("@product_id", line.ProductId);
            lineCommand.Parameters.AddWithValue("@qty", line.Quantity);
            await lineCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return transferId;
    }

    public async Task<TransferRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string headerSql = """
            select c_transfer_id, c_transfer_no, c_from_location_id, c_to_location_id, c_status, c_created_by, c_created_at
            from t_transfer
            where c_transfer_id = @id;
            """;

        const string linesSql = """
            select c_transfer_line_id, c_transfer_id, c_product_id, c_qty
            from t_transfer_line
            where c_transfer_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        TransferRecord? transfer = null;
        await using (var headerCommand = new NpgsqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("@id", id);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                transfer = new TransferRecord
                {
                    TransferId = reader.GetInt64(0),
                    TransferNo = reader.GetString(1),
                    FromLocationId = reader.GetInt64(2),
                    ToLocationId = reader.GetInt64(3),
                    Status = reader.GetString(4),
                    CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                    CreatedAt = reader.GetDateTime(6),
                };
            }
        }

        if (transfer is null)
        {
            return null;
        }

        await using var linesCommand = new NpgsqlCommand(linesSql, connection);
        linesCommand.Parameters.AddWithValue("@id", id);
        await using var linesReader = await linesCommand.ExecuteReaderAsync(cancellationToken);
        while (await linesReader.ReadAsync(cancellationToken))
        {
            transfer.Lines.Add(new TransferLineRecord
            {
                TransferLineId = linesReader.GetInt64(0),
                TransferId = linesReader.GetInt64(1),
                ProductId = linesReader.GetInt64(2),
                Quantity = linesReader.GetDecimal(3),
            });
        }

        return transfer;
    }

    public async Task<IReadOnlyList<TransferRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_transfer_id, c_transfer_no, c_from_location_id, c_to_location_id, c_status, c_created_by, c_created_at
            from t_transfer
            order by c_transfer_id desc;
            """;

        var results = new List<TransferRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new TransferRecord
            {
                TransferId = reader.GetInt64(0),
                TransferNo = reader.GetString(1),
                FromLocationId = reader.GetInt64(2),
                ToLocationId = reader.GetInt64(3),
                Status = reader.GetString(4),
                CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(TransferRecord transfer, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_transfer
            set c_from_location_id = @from_location_id,
                c_to_location_id = @to_location_id,
                c_status = @status
            where c_transfer_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", transfer.TransferId);
        command.Parameters.AddWithValue("@from_location_id", transfer.FromLocationId);
        command.Parameters.AddWithValue("@to_location_id", transfer.ToLocationId);
        command.Parameters.AddWithValue("@status", transfer.Status);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_transfer
            set c_status = @status
            where c_transfer_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@status", status);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IReadOnlyList<TransferLineRecord>> ListLinesAsync(long transferId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_transfer_line_id, c_transfer_id, c_product_id, c_qty
            from t_transfer_line
            where c_transfer_id = @id;
            """;

        var results = new List<TransferLineRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", transferId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new TransferLineRecord
            {
                TransferLineId = reader.GetInt64(0),
                TransferId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                Quantity = reader.GetDecimal(3),
            });
        }

        return results;
    }

    public async Task<long> AddLineAsync(TransferLineRecord line, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into t_transfer_line (c_transfer_id, c_product_id, c_qty)
            values (@transfer_id, @product_id, @qty)
            returning c_transfer_line_id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@transfer_id", line.TransferId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@qty", line.Quantity);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    public async Task<bool> UpdateLineAsync(TransferLineRecord line, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_transfer_line
            set c_product_id = @product_id,
                c_qty = @qty
            where c_transfer_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", line.TransferLineId);
        command.Parameters.AddWithValue("@product_id", line.ProductId);
        command.Parameters.AddWithValue("@qty", line.Quantity);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteLineAsync(long lineId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            delete from t_transfer_line
            where c_transfer_line_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", lineId);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
