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

    [HttpGet("{id:long}/lines")]
    public async Task<IActionResult> Lines(long id, CancellationToken cancellationToken)
    {
        var data = await _receipts.ListLinesAsync(id, cancellationToken);
        return Ok(data);
    }

    [HttpPost("{id:long}/lines")]
    public async Task<IActionResult> AddLine(long id, [FromBody] ReceiptLineRecord line, CancellationToken cancellationToken)
    {
        line.ReceiptId = id;
        var lineId = await _receipts.AddLineAsync(line, cancellationToken);
        line.ReceiptLineId = lineId;
        return Ok(line);
    }

    [HttpPut("lines/{lineId:long}")]
    public async Task<IActionResult> UpdateLine(long lineId, [FromBody] ReceiptLineRecord line, CancellationToken cancellationToken)
    {
        line.ReceiptLineId = lineId;
        var updated = await _receipts.UpdateLineAsync(line, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Line not found." });
        }

        return Ok(line);
    }

    [HttpDelete("lines/{lineId:long}")]
    public async Task<IActionResult> DeleteLine(long lineId, CancellationToken cancellationToken)
    {
        var deleted = await _receipts.DeleteLineAsync(lineId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Line not found." });
        }

        return Ok(new { success = true });
    }
}
