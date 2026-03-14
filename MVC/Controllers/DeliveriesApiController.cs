using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/deliveries")]
public sealed class DeliveriesApiController : ControllerBase
{
    private readonly IDeliveryInterface _deliveries;
    private readonly ICustomerInterface _customers;

    public DeliveriesApiController(IDeliveryInterface deliveries, ICustomerInterface customers)
    {
        _deliveries = deliveries;
        _customers = customers;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _deliveries.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("customers")]
    public async Task<IActionResult> Customers(CancellationToken cancellationToken)
    {
        var data = await _customers.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DeliveryRecord delivery, CancellationToken cancellationToken)
    {
        var id = await _deliveries.CreateAsync(delivery, cancellationToken);
        delivery.DeliveryId = id;
        return Ok(delivery);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] DeliveryRecord delivery, CancellationToken cancellationToken)
    {
        delivery.DeliveryId = id;
        var updated = await _deliveries.UpdateAsync(delivery, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Delivery not found." });
        }

        return Ok(delivery);
    }
}
