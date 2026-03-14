using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersApiController : ControllerBase
{
    private readonly ICustomerInterface _customers;

    public CustomersApiController(ICustomerInterface customers)
    {
        _customers = customers;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _customers.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerRecord customer, CancellationToken cancellationToken)
    {
        var id = await _customers.CreateAsync(customer, cancellationToken);
        customer.CustomerId = id;
        return Ok(customer);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] CustomerRecord customer, CancellationToken cancellationToken)
    {
        customer.CustomerId = id;
        var updated = await _customers.UpdateAsync(customer, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Customer not found." });
        }

        return Ok(customer);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _customers.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Customer not found." });
        }

        return Ok(new { success = true });
    }
}
