namespace Repositories.Models;

public sealed class ProductCategoryRecord
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
}
