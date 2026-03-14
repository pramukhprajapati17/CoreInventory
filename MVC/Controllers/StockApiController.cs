using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockApiController : ControllerBase
{
    private readonly string _connectionString;
    private readonly IReorderRuleInterface _rules;

    public StockApiController(IConfiguration configuration, IReorderRuleInterface rules)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _rules = rules;
    }

    [HttpGet("availability")]
    public async Task<IActionResult> Availability(CancellationToken cancellationToken)
    {
        const string sql = """
            select
                s.c_stock_id,
                s.c_product_id,
                p.c_product_name,
                s.c_location_id,
                l.c_location_name,
                w.c_warehouse_name,
                s.c_qty,
                r.c_min_qty,
                r.c_max_qty
            from t_stock s
            join t_product p on p.c_product_id = s.c_product_id
            join t_location l on l.c_location_id = s.c_location_id
            join t_warehouse w on w.c_warehouse_id = l.c_warehouse_id
            left join t_reorder_rule r on r.c_product_id = s.c_product_id and (r.c_location_id is null or r.c_location_id = s.c_location_id);
            """;

        var results = new List<object>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new
            {
                StockId = reader.GetInt64(0),
                ProductId = reader.GetInt64(1),
                ProductName = reader.GetString(2),
                LocationId = reader.GetInt64(3),
                LocationName = reader.GetString(4),
                WarehouseName = reader.GetString(5),
                Qty = reader.GetDecimal(6),
                MinQty = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                MaxQty = reader.IsDBNull(8) ? (decimal?)null : reader.GetDecimal(8)
            });
        }

        return Ok(results);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> Alerts(CancellationToken cancellationToken)
    {
        const string sql = """
            select
                s.c_stock_id,
                s.c_product_id,
                p.c_product_name,
                s.c_location_id,
                l.c_location_name,
                w.c_warehouse_name,
                s.c_qty,
                r.c_min_qty
            from t_stock s
            join t_product p on p.c_product_id = s.c_product_id
            join t_location l on l.c_location_id = s.c_location_id
            join t_warehouse w on w.c_warehouse_id = l.c_warehouse_id
            join t_reorder_rule r on r.c_product_id = s.c_product_id and (r.c_location_id is null or r.c_location_id = s.c_location_id)
            where s.c_qty <= r.c_min_qty;
            """;

        var results = new List<object>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new
            {
                StockId = reader.GetInt64(0),
                ProductId = reader.GetInt64(1),
                ProductName = reader.GetString(2),
                LocationId = reader.GetInt64(3),
                LocationName = reader.GetString(4),
                WarehouseName = reader.GetString(5),
                Qty = reader.GetDecimal(6),
                MinQty = reader.GetDecimal(7)
            });
        }

        return Ok(results);
    }

    [HttpGet("rules")]
    public async Task<IActionResult> ListRules(CancellationToken cancellationToken)
    {
        var data = await _rules.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] ReorderRuleRecord rule, CancellationToken cancellationToken)
    {
        var id = await _rules.CreateAsync(rule, cancellationToken);
        rule.ReorderRuleId = id;
        return Ok(rule);
    }

    [HttpPut("rules/{id:long}")]
    public async Task<IActionResult> UpdateRule(long id, [FromBody] ReorderRuleRecord rule, CancellationToken cancellationToken)
    {
        rule.ReorderRuleId = id;
        var updated = await _rules.UpdateAsync(rule, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Rule not found." });
        }

        return Ok(rule);
    }

    [HttpDelete("rules/{id:long}")]
    public async Task<IActionResult> DeleteRule(long id, CancellationToken cancellationToken)
    {
        var deleted = await _rules.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Rule not found." });
        }

        return Ok(new { success = true });
    }
}
