using Financial.Application.DTOs.Import;
using Financial.Application.Interfaces;
using Financial.Domain.Entities;

namespace Financial.Application.Services.Import;

public class ImportService : IImportService
{
    private readonly IEnumerable<IFileParserService> _parsers;
    private readonly IDuplicateDetectionService _duplicateDetection;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ImportService(
        IEnumerable<IFileParserService> parsers,
        IDuplicateDetectionService duplicateDetection,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _parsers = parsers;
        _duplicateDetection = duplicateDetection;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ImportPreviewDto> PreviewImportAsync(
        Stream fileStream, 
        string fileName, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName));
        
        if (parser == null)
        {
            return new ImportPreviewDto
            {
                FileName = fileName,
                Errors = new List<ImportValidationError>
                {
                    new ImportValidationError
                    {
                        RowNumber = 0,
                        Field = "File",
                        Message = "Unsupported file format. Supported formats: .csv, .xlsx, .xls"
                    }
                }
            };
        }

        var preview = await parser.ParseFileAsync(fileStream, fileName, userId, cancellationToken);

        // Detect duplicates
        if (preview.Transactions.Any())
        {
            preview.Transactions = await _duplicateDetection.DetectDuplicatesAsync(
                preview.Transactions, 
                userId, 
                cancellationToken);
            
            preview.DuplicateRecords = preview.Transactions.Count(t => t.IsDuplicate);
        }

        return preview;
    }

    public async Task<ImportResultDto> ExecuteImportAsync(
        ImportRequestDto request, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto
        {
            BatchId = request.BatchId,
            Success = false,
            ImportedRecords = 0,
            SkippedRecords = 0,
            FailedRecords = 0,
            Errors = new List<string>(),
            CompletedAt = DateTime.UtcNow
        };

        try
        {
            // Note: In a real implementation, we would retrieve the preview data from cache or temporary storage
            // For now, we'll assume the preview data is passed in the request or needs to be re-parsed
            
            // Get user's accounts and categories
            var accounts = await _accountRepository.GetByUserIdAsync(userId, cancellationToken);
            var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);

            // This is a placeholder - in production, retrieve the actual transactions from the preview
            var transactionsToImport = new List<Transaction>();

            // Import each transaction
            foreach (var transaction in transactionsToImport)
            {
                try
                {
                    if (transaction == null)
                    {
                        result.SkippedRecords++;
                        continue;
                    }

                    await _transactionRepository.CreateAsync(transaction, cancellationToken);
                    result.ImportedRecords++;
                }
                catch (Exception ex)
                {
                    result.FailedRecords++;
                    result.Errors.Add($"Failed to import transaction: {ex.Message}");
                }
            }

            result.Success = result.FailedRecords == 0;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> ValidateFileAsync(
        Stream fileStream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName));
        
        if (parser == null)
            return false;

        try
        {
            // Basic validation - check if file can be opened and parsed
            fileStream.Seek(0, SeekOrigin.Begin);
            var preview = await parser.ParseFileAsync(fileStream, fileName, Guid.Empty, cancellationToken);
            return preview.Errors.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Account?> FindOrCreateAccountAsync(
        string? accountName, 
        Guid userId, 
        bool autoCreate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            return null;

        var accounts = await _accountRepository.GetByUserIdAsync(userId, cancellationToken);
        var account = accounts.FirstOrDefault(a => 
            a.Name.Equals(accountName, StringComparison.OrdinalIgnoreCase));

        if (account == null && autoCreate)
        {
            account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = accountName,
                Type = AccountType.Other,
                Currency = "PYG",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            account = await _accountRepository.CreateAsync(account, cancellationToken);
        }

        return account;
    }

    private async Task<Category?> FindOrCreateCategoryAsync(
        string? categoryName, 
        Guid userId, 
        bool autoCreate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);
        var category = categories.FirstOrDefault(c => 
            c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (category == null && autoCreate)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = categoryName,
                Type = CategoryType.Expense,
                CreatedAt = DateTime.UtcNow
            };
            
            category = await _categoryRepository.CreateAsync(category, cancellationToken);
        }

        return category;
    }
}
