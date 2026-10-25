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

        if (preview.Transactions.Any())
        {
            foreach (var transaction in preview.Transactions.Where(t => string.IsNullOrWhiteSpace(t.Account)))
            {
                transaction.IsValid = false;
                transaction.ValidationMessages.Add("Missing account");
            }

            preview.Transactions = await _duplicateDetection.DetectDuplicatesAsync(
                preview.Transactions, 
                userId, 
                cancellationToken);

            var accounts = (await _accountRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();
            var categories = (await _categoryRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();

            preview.NewAccounts = BuildProposedAccounts(preview.Transactions, accounts);
            preview.NewCategories = BuildProposedCategories(preview.Transactions, categories);
            preview.DuplicateRecords = preview.Transactions.Count(t => t.IsDuplicate);
            preview.ValidRecords = preview.Transactions.Count(t => t.IsValid);
            preview.InvalidRecords = preview.Transactions.Count(t => !t.IsValid);
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
            var rows = request.Transactions;
            if (rows.Count == 0)
            {
                result.Errors.Add("No transactions to import");
                return result;
            }

            if (request.SkipDuplicates)
            {
                await _duplicateDetection.DetectDuplicatesAsync(rows, userId, cancellationToken);
            }

            var accounts = (await _accountRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();
            var categories = (await _categoryRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();

            var proposedAccounts = BuildProposedAccounts(rows, accounts)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            var proposedCategories = BuildProposedCategories(rows, categories)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            var approvedAccounts = new HashSet<string>(request.ApprovedAccounts, StringComparer.OrdinalIgnoreCase);
            var approvedCategories = new HashSet<string>(request.ApprovedCategories, StringComparer.OrdinalIgnoreCase);

            var accountsByName = new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase);
            foreach (var account in accounts)
                accountsByName.TryAdd(account.Name, account);

            var categoriesByName = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            foreach (var category in categories)
                categoriesByName.TryAdd(category.Name, category);

            foreach (var row in rows)
            {
                try
                {
                    if (!row.IsValid || string.IsNullOrWhiteSpace(row.Account))
                    {
                        result.SkippedRecords++;
                        result.Errors.Add($"Row {row.RowNumber}: invalid record");
                        continue;
                    }

                    if (request.SkipDuplicates && row.IsDuplicate)
                    {
                        result.SkippedRecords++;
                        continue;
                    }

                    var accountName = row.Account.Trim();
                    if (!accountsByName.TryGetValue(accountName, out var account))
                    {
                        if (!approvedAccounts.Contains(accountName))
                        {
                            result.SkippedRecords++;
                            result.Errors.Add($"Row {row.RowNumber}: account '{accountName}' does not exist and was not approved");
                            continue;
                        }

                        var proposed = proposedAccounts[accountName];
                        account = await _accountRepository.CreateAsync(new Account
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Name = accountName,
                            Type = Enum.Parse<AccountType>(proposed.Type),
                            Currency = proposed.Currency,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }, cancellationToken);
                        accountsByName[accountName] = account;
                    }

                    Category? category = null;
                    var categoryName = row.Category?.Trim();
                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        if (!categoriesByName.TryGetValue(categoryName, out category)
                            && approvedCategories.Contains(categoryName))
                        {
                            var proposed = proposedCategories[categoryName];
                            category = await _categoryRepository.CreateAsync(new Category
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                Name = categoryName,
                                Type = Enum.Parse<CategoryType>(proposed.Type),
                                CreatedAt = DateTime.UtcNow
                            }, cancellationToken);
                            categoriesByName[categoryName] = category;
                        }
                    }

                    await _transactionRepository.CreateAsync(new Transaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        AccountId = account.Id,
                        CategoryId = category?.Id,
                        Date = row.Date,
                        Amount = Math.Abs(row.Amount),
                        Currency = row.Currency.Trim().ToUpperInvariant(),
                        Merchant = row.Merchant,
                        Notes = row.Notes,
                        Type = row.Amount < 0 ? TransactionType.Expense : TransactionType.Income,
                        CreatedAt = DateTime.UtcNow
                    }, cancellationToken);

                    result.ImportedRecords++;
                }
                catch (Exception ex)
                {
                    result.FailedRecords++;
                    result.Errors.Add($"Row {row.RowNumber}: {ex.Message}");
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
            fileStream.Seek(0, SeekOrigin.Begin);
            var preview = await parser.ParseFileAsync(fileStream, fileName, Guid.Empty, cancellationToken);
            return preview.Errors.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    private static List<ProposedAccountDto> BuildProposedAccounts(
        IEnumerable<TransactionImportDto> rows,
        IEnumerable<Account> existingAccounts)
    {
        var existingNames = existingAccounts.Select(a => a.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Account) && !existingNames.Contains(r.Account.Trim()))
            .GroupBy(r => r.Account!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var currency = g.First().Currency.Trim().ToUpperInvariant();
                return new ProposedAccountDto
                {
                    Name = g.Key,
                    Currency = currency,
                    Type = currency == "USD" ? nameof(AccountType.UsdAccount) : nameof(AccountType.Other),
                    RowCount = g.Count()
                };
            })
            .ToList();
    }

    private static List<ProposedCategoryDto> BuildProposedCategories(
        IEnumerable<TransactionImportDto> rows,
        IEnumerable<Category> existingCategories)
    {
        var existingNames = existingCategories.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Category) && !existingNames.Contains(r.Category.Trim()))
            .GroupBy(r => r.Category!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new ProposedCategoryDto
            {
                Name = g.Key,
                Type = g.All(r => r.Amount >= 0) ? nameof(CategoryType.Income) : nameof(CategoryType.Expense),
                RowCount = g.Count()
            })
            .ToList();
    }
}
