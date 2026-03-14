using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesApiController : ControllerBase
{
    private readonly IProductCategoryInterface _categories;

    public CategoriesApiController(IProductCategoryInterface categories)
    {
        _categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var data = await _categories.ListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCategoryRecord category, CancellationToken cancellationToken)
    {
        var id = await _categories.CreateAsync(category, cancellationToken);
        category.CategoryId = id;
        return Ok(category);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductCategoryRecord category, CancellationToken cancellationToken)
    {
        category.CategoryId = id;
        var updated = await _categories.UpdateAsync(category, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "Category not found." });
        }

        return Ok(category);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await _categories.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Category not found." });
        }

        return Ok(new { success = true });
    }
}
