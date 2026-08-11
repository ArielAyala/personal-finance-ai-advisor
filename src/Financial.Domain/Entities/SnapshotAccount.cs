namespace Financial.Domain.Entities;

public class SnapshotAccount
{
    public Guid Id { get; set; }
    public Guid SnapshotId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNumberMasked { get; set; }
    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = "PYG";
    public decimal? ExchangeRate { get; set; }
    public decimal ConvertedAmount { get; set; }
    public string BaseCurrency { get; set; } = "PYG";

    // Navigation properties
    public FinancialSnapshot Snapshot { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
