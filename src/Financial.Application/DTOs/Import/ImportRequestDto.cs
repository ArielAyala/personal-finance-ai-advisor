namespace Financial.Application.DTOs.Import;

public class ImportRequestDto
{
    public Guid BatchId { get; set; }
    public bool SkipDuplicates { get; set; } = true;
    public bool AutoCreateCategories { get; set; } = true;
    public bool AutoCreateAccounts { get; set; } = false;
}

public class ImportResultDto
{
    public Guid BatchId { get; set; }
    public bool Success { get; set; }
    public int ImportedRecords { get; set; }
    public int SkippedRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}
