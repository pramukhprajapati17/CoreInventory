namespace Repositories.Models;

public sealed class LocationRecord
{
    public long LocationId { get; set; }
    public long WarehouseId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
