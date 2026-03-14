namespace Repositories.Models;

public sealed class DeliveryRecord
{
    public long DeliveryId { get; set; }
    public string DeliveryNo { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? ExpectedDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DeliveryLineRecord> Lines { get; set; } = new();
}

public sealed class DeliveryLineRecord
{
    public long DeliveryLineId { get; set; }
    public long DeliveryId { get; set; }
    public long ProductId { get; set; }
    public long? LocationId { get; set; }
    public decimal Quantity { get; set; }
}
