namespace Financial.Domain.Entities;

public class Bill
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PYG";
    public decimal PaidAmount { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public BillPeriodicity Periodicity { get; set; }
    public int? Installments { get; set; } // Total installments for payment plans
    public int? PaidInstallments { get; set; } = 0;
    public BillStatus Status { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AccountId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Category? Category { get; set; }
    public Account? Account { get; set; }
}

public enum BillPeriodicity
{
    OneTime,
    Daily,
    Weekly,
    Monthly,
    Yearly,
    Custom
}

public enum BillStatus
{
    Active,
    Paid,
    Overdue,
    Cancelled
}
