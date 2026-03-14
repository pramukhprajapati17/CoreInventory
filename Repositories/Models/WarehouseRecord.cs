namespace Repositories.Models;

public sealed class WarehouseRecord
{
    public long WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
