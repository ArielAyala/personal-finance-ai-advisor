namespace Financial.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType Type { get; set; }
    public string? Color { get; set; } // For UI visualization
    public string? Icon { get; set; } // For UI visualization
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}

public enum CategoryType
{
    Income,
    Expense,
    Transfer
}
