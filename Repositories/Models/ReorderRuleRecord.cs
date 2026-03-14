namespace Repositories.Models;

public sealed class ReorderRuleRecord
{
    public long ReorderRuleId { get; set; }
    public long ProductId { get; set; }
    public long? LocationId { get; set; }
    public decimal MinQty { get; set; }
    public decimal? MaxQty { get; set; }
    public bool IsActive { get; set; } = true;
}
