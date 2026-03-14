namespace Repositories.Models;

public sealed class StockLedgerRecord
{
    public long LedgerId { get; set; }
    public long ProductId { get; set; }
    public long? LocationId { get; set; }
    public string DocType { get; set; } = string.Empty;
    public long DocId { get; set; }
    public decimal QtyChange { get; set; }
    public DateTime CreatedAt { get; set; }
}
