namespace Financial.Application.DTOs.Import;

public class ImportPreviewDto
{
    public Guid BatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int InvalidRecords { get; set; }
    public int DuplicateRecords { get; set; }
    public List<TransactionImportDto> Transactions { get; set; } = new();
    public List<ProposedAccountDto> NewAccounts { get; set; } = new();
    public List<ProposedCategoryDto> NewCategories { get; set; } = new();
    public List<ImportValidationError> Errors { get; set; } = new();
}

public class TransactionImportDto
{
    public int RowNumber { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Merchant { get; set; }
    public string? Category { get; set; }
    public string? Account { get; set; }
    public string? Notes { get; set; }
    public bool IsValid { get; set; }
    public bool IsDuplicate { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
}

public class ProposedAccountDto
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RowCount { get; set; }
}

public class ProposedCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RowCount { get; set; }
}

public class ImportValidationError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
