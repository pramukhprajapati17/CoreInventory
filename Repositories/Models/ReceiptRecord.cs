namespace Repositories.Models;

public sealed class ReceiptRecord
{
    public long ReceiptId { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public long? SupplierId { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? ExpectedDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReceiptLineRecord> Lines { get; set; } = new();
}

public sealed class ReceiptLineRecord
{
    public long ReceiptLineId { get; set; }
    public long ReceiptId { get; set; }
    public long ProductId { get; set; }
    public long? LocationId { get; set; }
    public decimal Quantity { get; set; }
}
