namespace Financial.Domain.Entities;

public class ExchangeRate
{
    public Guid Id { get; set; }
    public string SourceCurrency { get; set; } = string.Empty; // e.g., "USD"
    public string TargetCurrency { get; set; } = string.Empty; // e.g., "PYG"
    public decimal Rate { get; set; }
    public DateTime Date { get; set; }
    public string? Source { get; set; } // e.g., "Manual input", "API", etc.
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
