namespace Financial.Domain.Entities;

public class Goal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; } = 0;
    public string Currency { get; set; } = "PYG";
    public DateTime? TargetDate { get; set; }
    public GoalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

public enum GoalStatus
{
    Active,
    Completed,
    Cancelled,
    OnHold
}
