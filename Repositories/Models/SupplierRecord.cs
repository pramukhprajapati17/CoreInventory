namespace Repositories.Models;

public sealed class SupplierRecord
{
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
