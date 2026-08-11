using Financial.Application.DTOs.Import;
using Financial.Application.Interfaces;

namespace Financial.Application.Services.Import;

public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ITransactionRepository _transactionRepository;

    public DuplicateDetectionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<List<TransactionImportDto>> DetectDuplicatesAsync(
        List<TransactionImportDto> transactions, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        // Get existing transactions for the user
        var existingTransactions = await _transactionRepository.GetByUserIdAsync(userId, null, cancellationToken);
        
        foreach (var transaction in transactions)
        {
            // Check if a similar transaction already exists
            // Criteria: same date, amount, and merchant within a reasonable timeframe
            var isDuplicate = existingTransactions.Any(existing =>
                existing.Date.Date == transaction.Date.Date &&
                existing.Amount == transaction.Amount &&
                (string.IsNullOrEmpty(transaction.Merchant) || 
                 existing.Description?.Contains(transaction.Merchant, StringComparison.OrdinalIgnoreCase) == true)
            );

            transaction.IsDuplicate = isDuplicate;
            
            if (isDuplicate)
            {
                transaction.ValidationMessages.Add("Potential duplicate transaction found");
            }
        }

        return transactions;
    }
}
