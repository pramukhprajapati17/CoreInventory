namespace Repositories.Models;

public sealed class AdjustmentRecord
{
    public long AdjustmentId { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public long LocationId { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reason { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AdjustmentLineRecord> Lines { get; set; } = new();
}

public sealed class AdjustmentLineRecord
{
    public long AdjustmentLineId { get; set; }
    public long AdjustmentId { get; set; }
    public long ProductId { get; set; }
    public decimal CountedQty { get; set; }
    public decimal SystemQty { get; set; }
}
