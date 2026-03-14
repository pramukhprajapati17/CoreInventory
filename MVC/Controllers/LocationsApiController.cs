using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/locations")]
public sealed class LocationsApiController : ControllerBase
{
    private readonly ILocationInterface _locations;
    private readonly IWarehouseInterface _warehouses;

    public LocationsApiController(ILocationInterface locations, IWarehouseInterface warehouses)
    {
        _locations = locations;
        _warehouses = warehouses;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] long warehouseId, CancellationToken cancellationToken)
    {
        var data = await _locations.ListByWarehouseAsync(warehouseId, cancellationToken);
        return Ok(data);
    }

    [HttpGet("all")]
    public async Task<IActionResult> ListAll(CancellationToken cancellationToken)
    {
        var data = await _locations.ListAllAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> Warehouses(CancellationToken cancellationToken)
    {
        var data = await _warehouses.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LocationRecord location, CancellationToken cancellationToken)
    {
        var id = await _locations.CreateAsync(location, cancellationToken);
        location.LocationId = id;
        return Ok(location);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] LocationRecord location, CancellationToken cancellationToken)
    {
        location.LocationId = id;
        var updated = await _locations.UpdateAsync(location, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Location not found." });
        }

        return Ok(location);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _locations.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Location not found." });
        }

        return Ok(new { success = true });
    }
}
