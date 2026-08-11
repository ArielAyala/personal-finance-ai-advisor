namespace Financial.Domain.Entities;

/// <summary>
/// Represents a batch import operation
/// </summary>
public class ImportBatch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // CSV, Excel, PDF
    public ImportStatus Status { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public int DuplicateRecords { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum ImportStatus
{
    Pending,
    Processing,
    Preview,
    Completed,
    Failed,
    Cancelled
}
