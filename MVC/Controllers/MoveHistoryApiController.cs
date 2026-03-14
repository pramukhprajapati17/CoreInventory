using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace MVC.Controllers;

[ApiController]
[Route("api/movehistory")]
public sealed class MoveHistoryApiController : ControllerBase
{
    private readonly IStockLedgerInterface _ledger;
    private readonly IProductInterface _products;
    private readonly ILocationInterface _locations;

    public MoveHistoryApiController(IStockLedgerInterface ledger, IProductInterface products, ILocationInterface locations)
    {
        _ledger = ledger;
        _products = products;
        _locations = locations;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _ledger.ListAllAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products(CancellationToken cancellationToken)
    {
        var data = await _products.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("locations")]
    public async Task<IActionResult> Locations(CancellationToken cancellationToken)
    {
        var data = await _locations.ListAllAsync(cancellationToken);
        return Ok(data);
    }
}
