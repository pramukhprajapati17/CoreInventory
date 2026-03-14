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

    [HttpGet("{id:long}/lines")]
    public async Task<IActionResult> Lines(long id, CancellationToken cancellationToken)
    {
        var data = await _adjustments.ListLinesAsync(id, cancellationToken);
        return Ok(data);
    }

    [HttpPost("{id:long}/lines")]
    public async Task<IActionResult> AddLine(long id, [FromBody] AdjustmentLineRecord line, CancellationToken cancellationToken)
    {
        line.AdjustmentId = id;
        var lineId = await _adjustments.AddLineAsync(line, cancellationToken);
        line.AdjustmentLineId = lineId;
        return Ok(line);
    }

    [HttpPut("lines/{lineId:long}")]
    public async Task<IActionResult> UpdateLine(long lineId, [FromBody] AdjustmentLineRecord line, CancellationToken cancellationToken)
    {
        line.AdjustmentLineId = lineId;
        var updated = await _adjustments.UpdateLineAsync(line, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Line not found." });
        }

        return Ok(line);
    }

    [HttpDelete("lines/{lineId:long}")]
    public async Task<IActionResult> DeleteLine(long lineId, CancellationToken cancellationToken)
    {
        var deleted = await _adjustments.DeleteLineAsync(lineId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Line not found." });
        }

        return Ok(new { success = true });
    }
}
