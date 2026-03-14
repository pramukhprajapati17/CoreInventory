using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsApiController : ControllerBase
{
    private readonly IProductInterface _products;
    private readonly IProductCategoryInterface _categories;
    private readonly IUomInterface _uoms;

    public ProductsApiController(IProductInterface products, IProductCategoryInterface categories, IUomInterface uoms)
    {
        _products = products;
        _categories = categories;
        _uoms = uoms;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _products.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRecord product, CancellationToken cancellationToken)
    {
        var id = await _products.CreateAsync(product, cancellationToken);
        product.ProductId = id;
        return Ok(product);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductRecord product, CancellationToken cancellationToken)
    {
        product.ProductId = id;
        var updated = await _products.UpdateAsync(product, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Product not found." });
        }

        return Ok(product);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _products.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return Conflict(new { success = false, message = "Product is linked to stock and cannot be deleted." });
        }

        return Ok(new { success = true });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken)
    {
        var data = await _categories.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("uoms")]
    public async Task<IActionResult> Uoms(CancellationToken cancellationToken)
    {
        var data = await _uoms.ListAsync(cancellationToken);
        return Ok(data);
    }
}
