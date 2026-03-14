namespace Repositories.Models;

public sealed class UomRecord
{
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;
}
