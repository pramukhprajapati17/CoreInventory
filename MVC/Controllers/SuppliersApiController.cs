using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/suppliers")]
public sealed class SuppliersApiController : ControllerBase
{
    private readonly ISupplierInterface _suppliers;

    public SuppliersApiController(ISupplierInterface suppliers)
    {
        _suppliers = suppliers;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _suppliers.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupplierRecord supplier, CancellationToken cancellationToken)
    {
        var id = await _suppliers.CreateAsync(supplier, cancellationToken);
        supplier.SupplierId = id;
        return Ok(supplier);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] SupplierRecord supplier, CancellationToken cancellationToken)
    {
        supplier.SupplierId = id;
        var updated = await _suppliers.UpdateAsync(supplier, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Supplier not found." });
        }

        return Ok(supplier);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _suppliers.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Supplier not found." });
        }

        return Ok(new { success = true });
    }
}
