namespace Financial.Domain.Entities;

public class FinancialSnapshot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public string BaseCurrency { get; set; } = "PYG";
    public decimal TotalAmount { get; set; }
    public DateTime? ExchangeRateDate { get; set; }
    public SnapshotStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<SnapshotAccount> SnapshotAccounts { get; set; } = new List<SnapshotAccount>();
}

public enum SnapshotStatus
{
    Draft,
    Closed
}
