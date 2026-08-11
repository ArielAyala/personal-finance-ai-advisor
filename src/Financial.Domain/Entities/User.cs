namespace Financial.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PreferredCurrency { get; set; } = "PYG"; // Default to PYG
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<FinancialSnapshot> FinancialSnapshots { get; set; } = new List<FinancialSnapshot>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
