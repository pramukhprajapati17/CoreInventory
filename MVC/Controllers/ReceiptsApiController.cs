using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/receipts")]
public sealed class ReceiptsApiController : ControllerBase
{
    private readonly IReceiptInterface _receipts;
    private readonly ISupplierInterface _suppliers;

    public ReceiptsApiController(IReceiptInterface receipts, ISupplierInterface suppliers)
    {
        _receipts = receipts;
        _suppliers = suppliers;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _receipts.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers(CancellationToken cancellationToken)
    {
        var data = await _suppliers.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReceiptRecord receipt, CancellationToken cancellationToken)
    {
        var id = await _receipts.CreateAsync(receipt, cancellationToken);
        receipt.ReceiptId = id;
        return Ok(receipt);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ReceiptRecord receipt, CancellationToken cancellationToken)
    {
        receipt.ReceiptId = id;
        var updated = await _receipts.UpdateAsync(receipt, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Receipt not found." });
        }

        return Ok(receipt);
    }
}
