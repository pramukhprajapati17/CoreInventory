using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/uoms")]
public sealed class UomsApiController : ControllerBase
{
    private readonly IUomInterface _uoms;

    public UomsApiController(IUomInterface uoms)
    {
        _uoms = uoms;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _uoms.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UomRecord uom, CancellationToken cancellationToken)
    {
        var id = await _uoms.CreateAsync(uom, cancellationToken);
        uom.UomId = id;
        return Ok(uom);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UomRecord uom, CancellationToken cancellationToken)
    {
        uom.UomId = id;
        var updated = await _uoms.UpdateAsync(uom, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "UoM not found." });
        }

        return Ok(uom);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _uoms.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "UoM not found." });
        }

        return Ok(new { success = true });
    }
}
