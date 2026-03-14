using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MVC.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardApiController : ControllerBase
{
    private readonly string _connectionString;

    public DashboardApiController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis([FromQuery] string? docType, [FromQuery] string? status, [FromQuery] long? warehouseId, [FromQuery] long? locationId, [FromQuery] long? categoryId, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var totalProducts = await ExecuteScalarAsync(connection, """
            select count(*) from t_product where c_is_active = true;
            """, cancellationToken);

        var lowStock = await ExecuteScalarAsync(connection, """
            select count(*) from t_stock s
            join t_reorder_rule r on r.c_product_id = s.c_product_id and (r.c_location_id is null or r.c_location_id = s.c_location_id)
            join t_product p on p.c_product_id = s.c_product_id
            join t_location l on l.c_location_id = s.c_location_id
            where s.c_qty <= r.c_min_qty
              and (@category_id::bigint is null or p.c_category_id = @category_id::bigint)
              and (@location_id::bigint is null or s.c_location_id = @location_id::bigint)
              and (@warehouse_id::bigint is null or l.c_warehouse_id = @warehouse_id::bigint);
            """, cancellationToken, ("@category_id", (object?)categoryId ?? DBNull.Value), ("@location_id", (object?)locationId ?? DBNull.Value), ("@warehouse_id", (object?)warehouseId ?? DBNull.Value));

        var outOfStock = await ExecuteScalarAsync(connection, """
            select count(*) from t_stock s
            join t_product p on p.c_product_id = s.c_product_id
            join t_location l on l.c_location_id = s.c_location_id
            where s.c_qty <= 0
              and (@category_id::bigint is null or p.c_category_id = @category_id::bigint)
              and (@location_id::bigint is null or s.c_location_id = @location_id::bigint)
              and (@warehouse_id::bigint is null or l.c_warehouse_id = @warehouse_id::bigint);
            """, cancellationToken, ("@category_id", (object?)categoryId ?? DBNull.Value), ("@location_id", (object?)locationId ?? DBNull.Value), ("@warehouse_id", (object?)warehouseId ?? DBNull.Value));

        var pendingReceipts = await CountByDocTypeAsync(connection, "receipt", status, cancellationToken);
        var pendingDeliveries = await CountByDocTypeAsync(connection, "delivery", status, cancellationToken);
        var pendingTransfers = await CountByDocTypeAsync(connection, "transfer", status, cancellationToken);

        if (!string.IsNullOrWhiteSpace(docType))
        {
            pendingReceipts = docType.Equals("receipt", StringComparison.OrdinalIgnoreCase) ? pendingReceipts : 0;
            pendingDeliveries = docType.Equals("delivery", StringComparison.OrdinalIgnoreCase) ? pendingDeliveries : 0;
            pendingTransfers = docType.Equals("transfer", StringComparison.OrdinalIgnoreCase) ? pendingTransfers : 0;
        }

        return Ok(new
        {
            totalProducts,
            lowStock,
            outOfStock,
            pendingReceipts,
            pendingDeliveries,
            pendingTransfers
        });
    }

    private static async Task<long> ExecuteScalarAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken, params (string, object)[] parameters)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }

    private static async Task<long> CountByDocTypeAsync(NpgsqlConnection connection, string docType, string? status, CancellationToken cancellationToken)
    {
        var sql = docType switch
        {
            "receipt" => """
                select count(*) from t_receipt
                where (
                    @status::text is null and c_status in ('Draft', 'Waiting', 'Ready')
                    or @status::text is not null and c_status = @status::text
                );
                """,
            "delivery" => """
                select count(*) from t_delivery
                where (
                    @status::text is null and c_status in ('Draft', 'Waiting', 'Ready')
                    or @status::text is not null and c_status = @status::text
                );
                """,
            "transfer" => """
                select count(*) from t_transfer
                where (
                    @status::text is null and c_status in ('Draft', 'Waiting', 'Ready')
                    or @status::text is not null and c_status = @status::text
                );
                """,
            _ => "select 0;"
        };

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@status", (object?)status ?? DBNull.Value);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is long id ? id : Convert.ToInt64(result);
    }
}
