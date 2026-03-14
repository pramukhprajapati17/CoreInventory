namespace Repositories.Models;

public sealed class TransferRecord
{
    public long TransferId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public long FromLocationId { get; set; }
    public long ToLocationId { get; set; }
    public string Status { get; set; } = "Draft";
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TransferLineRecord> Lines { get; set; } = new();
}

public sealed class TransferLineRecord
{
    public long TransferLineId { get; set; }
    public long TransferId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
}
