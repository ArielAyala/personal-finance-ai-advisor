namespace Financial.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // pdf, excel, etc.
    public long FileSize { get; set; }
    public DocumentType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // For RAG - Phase 7
    public string? ExtractedText { get; set; }
    public bool IsProcessed { get; set; } = false;

    // Navigation properties
    public User User { get; set; } = null!;
}

public enum DocumentType
{
    BankStatement,
    Invoice,
    Receipt,
    Contract,
    Report,
    Other
}
