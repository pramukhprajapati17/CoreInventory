using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/adjustments")]
public sealed class AdjustmentsApiController : ControllerBase
{
    private readonly IAdjustmentInterface _adjustments;
    private readonly ILocationInterface _locations;

    public AdjustmentsApiController(IAdjustmentInterface adjustments, ILocationInterface locations)
    {
        _adjustments = adjustments;
        _locations = locations;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _adjustments.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("locations")]
    public async Task<IActionResult> Locations(CancellationToken cancellationToken)
    {
        var data = await _locations.ListAllAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdjustmentRecord adjustment, CancellationToken cancellationToken)
    {
        var id = await _adjustments.CreateAsync(adjustment, cancellationToken);
        adjustment.AdjustmentId = id;
        return Ok(adjustment);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] AdjustmentRecord adjustment, CancellationToken cancellationToken)
    {
        adjustment.AdjustmentId = id;
        var updated = await _adjustments.UpdateAsync(adjustment, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Adjustment not found." });
        }

        return Ok(adjustment);
    }
}
