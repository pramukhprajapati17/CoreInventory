using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repositories;

public sealed class IDeliveryInterface_repo : IDeliveryInterface
{
    private readonly string _connectionString;

    public IDeliveryInterface_repo(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(DeliveryRecord delivery, CancellationToken cancellationToken = default)
    {
        const string insertHeader = """
            insert into t_delivery (c_delivery_no, c_customer_id, c_status, c_expected_date, c_created_by, c_created_at)
            values (@no, @customer_id, @status, @expected_date, @created_by, now())
            returning c_delivery_id;
            """;

        const string insertLine = """
            insert into t_delivery_line (c_delivery_id, c_product_id, c_location_id, c_qty)
            values (@delivery_id, @product_id, @location_id, @qty);
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var headerCommand = new NpgsqlCommand(insertHeader, connection, transaction);
        headerCommand.Parameters.AddWithValue("@no", delivery.DeliveryNo);
        headerCommand.Parameters.AddWithValue("@customer_id", (object?)delivery.CustomerId ?? DBNull.Value);
        headerCommand.Parameters.AddWithValue("@status", delivery.Status);
        headerCommand.Parameters.AddWithValue("@expected_date", (object?)delivery.ExpectedDate ?? DBNull.Value);
        headerCommand.Parameters.AddWithValue("@created_by", (object?)delivery.CreatedBy ?? DBNull.Value);
        var headerResult = await headerCommand.ExecuteScalarAsync(cancellationToken);
        var deliveryId = headerResult is long id ? id : Convert.ToInt64(headerResult);

        foreach (var line in delivery.Lines)
        {
            await using var lineCommand = new NpgsqlCommand(insertLine, connection, transaction);
            lineCommand.Parameters.AddWithValue("@delivery_id", deliveryId);
            lineCommand.Parameters.AddWithValue("@product_id", line.ProductId);
            lineCommand.Parameters.AddWithValue("@location_id", (object?)line.LocationId ?? DBNull.Value);
            lineCommand.Parameters.AddWithValue("@qty", line.Quantity);
            await lineCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return deliveryId;
    }

    public async Task<DeliveryRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string headerSql = """
            select c_delivery_id, c_delivery_no, c_customer_id, c_status, c_expected_date, c_created_by, c_created_at
            from t_delivery
            where c_delivery_id = @id;
            """;

        const string linesSql = """
            select c_delivery_line_id, c_delivery_id, c_product_id, c_location_id, c_qty
            from t_delivery_line
            where c_delivery_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        DeliveryRecord? delivery = null;
        await using (var headerCommand = new NpgsqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("@id", id);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                delivery = new DeliveryRecord
                {
                    DeliveryId = reader.GetInt64(0),
                    DeliveryNo = reader.GetString(1),
                    CustomerId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                    Status = reader.GetString(3),
                    ExpectedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                    CreatedAt = reader.GetDateTime(6),
                };
            }
        }

        if (delivery is null)
        {
            return null;
        }

        await using var linesCommand = new NpgsqlCommand(linesSql, connection);
        linesCommand.Parameters.AddWithValue("@id", id);
        await using var linesReader = await linesCommand.ExecuteReaderAsync(cancellationToken);
        while (await linesReader.ReadAsync(cancellationToken))
        {
            delivery.Lines.Add(new DeliveryLineRecord
            {
                DeliveryLineId = linesReader.GetInt64(0),
                DeliveryId = linesReader.GetInt64(1),
                ProductId = linesReader.GetInt64(2),
                LocationId = linesReader.IsDBNull(3) ? null : linesReader.GetInt64(3),
                Quantity = linesReader.GetDecimal(4),
            });
        }

        return delivery;
    }

    public async Task<IReadOnlyList<DeliveryRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select c_delivery_id, c_delivery_no, c_customer_id, c_status, c_expected_date, c_created_by, c_created_at
            from t_delivery
            order by c_delivery_id desc;
            """;

        var results = new List<DeliveryRecord>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DeliveryRecord
            {
                DeliveryId = reader.GetInt64(0),
                DeliveryNo = reader.GetString(1),
                CustomerId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                Status = reader.GetString(3),
                ExpectedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                CreatedBy = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }

        return results;
    }

    public async Task<bool> UpdateAsync(DeliveryRecord delivery, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_delivery
            set c_customer_id = @customer_id,
                c_status = @status,
                c_expected_date = @expected_date
            where c_delivery_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", delivery.DeliveryId);
        command.Parameters.AddWithValue("@customer_id", (object?)delivery.CustomerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@status", delivery.Status);
        command.Parameters.AddWithValue("@expected_date", (object?)delivery.ExpectedDate ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateStatusAsync(long id, string status, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update t_delivery
            set c_status = @status
            where c_delivery_id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@status", status);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
