namespace Repositories.Models;

public sealed class ProductRecord
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public long? CategoryId { get; set; }
    public long? UomId { get; set; }
    public bool IsActive { get; set; } = true;
}
