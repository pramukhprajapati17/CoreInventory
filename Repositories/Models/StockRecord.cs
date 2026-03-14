namespace Repositories.Models;

public sealed class StockRecord
{
    public long StockId { get; set; }
    public long ProductId { get; set; }
    public long LocationId { get; set; }
    public decimal Quantity { get; set; }
}
