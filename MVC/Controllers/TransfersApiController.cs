using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/transfers")]
public sealed class TransfersApiController : ControllerBase
{
    private readonly ITransferInterface _transfers;
    private readonly ILocationInterface _locations;

    public TransfersApiController(ITransferInterface transfers, ILocationInterface locations)
    {
        _transfers = transfers;
        _locations = locations;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _transfers.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("locations")]
    public async Task<IActionResult> Locations(CancellationToken cancellationToken)
    {
        var data = await _locations.ListAllAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransferRecord transfer, CancellationToken cancellationToken)
    {
        var id = await _transfers.CreateAsync(transfer, cancellationToken);
        transfer.TransferId = id;
        return Ok(transfer);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] TransferRecord transfer, CancellationToken cancellationToken)
    {
        transfer.TransferId = id;
        var updated = await _transfers.UpdateAsync(transfer, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Transfer not found." });
        }

        return Ok(transfer);
    }
}
