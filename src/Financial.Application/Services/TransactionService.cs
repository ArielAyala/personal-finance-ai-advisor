using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(
        Guid userId, 
        TransactionFilterDto? filter = null, 
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(userId, filter, cancellationToken);
        return transactions.Select(MapToDto);
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        return transaction != null ? MapToDto(transaction) : null;
    }

    public async Task<TransactionDto> CreateTransactionAsync(
        Guid userId, 
        CreateTransactionDto dto, 
        CancellationToken cancellationToken = default)
    {
        // Verify account belongs to user
        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account with ID {dto.AccountId} not found");

        if (account.UserId != userId)
            throw new UnauthorizedAccessException("Account does not belong to user");

        // Verify category if provided
        if (dto.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Category with ID {dto.CategoryId} not found");

            if (category.UserId != userId)
                throw new UnauthorizedAccessException("Category does not belong to user");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Date = dto.Date,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Merchant = dto.Merchant,
            Description = dto.Description,
            Notes = dto.Notes,
            Type = Enum.Parse<TransactionType>(dto.Type)
        };

        var created = await _transactionRepository.CreateAsync(transaction, cancellationToken);
        
        // Reload with navigation properties
        var loaded = await _transactionRepository.GetByIdAsync(created.Id, cancellationToken);
        return MapToDto(loaded!);
    }

    public async Task<TransactionDto> UpdateTransactionAsync(
        Guid id, 
        UpdateTransactionDto dto, 
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction with ID {id} not found");

        if (dto.CategoryId.HasValue) transaction.CategoryId = dto.CategoryId;
        if (dto.Date.HasValue) transaction.Date = dto.Date.Value;
        if (dto.Amount.HasValue) transaction.Amount = dto.Amount.Value;
        if (dto.Merchant != null) transaction.Merchant = dto.Merchant;
        if (dto.Description != null) transaction.Description = dto.Description;
        if (dto.Notes != null) transaction.Notes = dto.Notes;

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        
        // Reload with navigation properties
        var loaded = await _transactionRepository.GetByIdAsync(transaction.Id, cancellationToken);
        return MapToDto(loaded!);
    }

    public async Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _transactionRepository.DeleteAsync(id, cancellationToken);
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            transaction.Account?.Name ?? "",
            transaction.CategoryId,
            transaction.Category?.Name,
            transaction.Date,
            transaction.Amount,
            transaction.Currency,
            transaction.Merchant,
            transaction.Description,
            transaction.Notes,
            transaction.Type.ToString(),
            transaction.Reference,
            transaction.CreatedAt
        );
    }
}
