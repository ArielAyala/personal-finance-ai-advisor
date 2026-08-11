namespace Financial.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "PYG"; // PYG or USD
    public string? AccountNumber { get; set; } // Masked or encrypted
    public string? Institution { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<SnapshotAccount> SnapshotAccounts { get; set; } = new List<SnapshotAccount>();
}

public enum AccountType
{
    BankAccount,
    CreditCard,
    Cash,
    Investment,
    Savings,
    UsdAccount,
    Other
}
