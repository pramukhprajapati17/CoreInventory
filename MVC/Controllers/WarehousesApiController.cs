using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/warehouses")]
public sealed class WarehousesApiController : ControllerBase
{
    private readonly IWarehouseInterface _warehouses;

    public WarehousesApiController(IWarehouseInterface warehouses)
    {
        _warehouses = warehouses;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _warehouses.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WarehouseRecord warehouse, CancellationToken cancellationToken)
    {
        var id = await _warehouses.CreateAsync(warehouse, cancellationToken);
        warehouse.WarehouseId = id;
        return Ok(warehouse);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] WarehouseRecord warehouse, CancellationToken cancellationToken)
    {
        warehouse.WarehouseId = id;
        var updated = await _warehouses.UpdateAsync(warehouse, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Warehouse not found." });
        }

        return Ok(warehouse);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _warehouses.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Warehouse not found." });
        }

        return Ok(new { success = true });
    }
}
