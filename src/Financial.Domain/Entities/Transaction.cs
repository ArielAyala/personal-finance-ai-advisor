namespace Financial.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; } // Always positive; direction is given by Type
    public string Currency { get; set; } = "PYG";
    public string? Merchant { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public TransactionType Type { get; set; }
    public string? Reference { get; set; } // External reference from import
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public Category? Category { get; set; }
}

public enum TransactionType
{
    Income,
    Expense,
    Transfer
}
