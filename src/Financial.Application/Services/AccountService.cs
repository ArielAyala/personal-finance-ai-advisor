using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(userId, cancellationToken);
        return accounts.Select(MapToDto);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        return account != null ? MapToDto(account) : null;
    }

    public async Task<AccountDto> CreateAccountAsync(Guid userId, CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            Type = Enum.Parse<AccountType>(dto.Type),
            Currency = dto.Currency,
            AccountNumber = dto.AccountNumber,
            Institution = dto.Institution,
            CurrentBalance = dto.InitialBalance,
            IsActive = true
        };

        var created = await _accountRepository.CreateAsync(account, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Account with ID {id} not found");

        if (dto.Name != null) account.Name = dto.Name;
        if (dto.AccountNumber != null) account.AccountNumber = dto.AccountNumber;
        if (dto.Institution != null) account.Institution = dto.Institution;
        if (dto.CurrentBalance.HasValue) account.CurrentBalance = dto.CurrentBalance.Value;
        if (dto.IsActive.HasValue) account.IsActive = dto.IsActive.Value;

        await _accountRepository.UpdateAsync(account, cancellationToken);
        return MapToDto(account);
    }

    public async Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _accountRepository.DeleteAsync(id, cancellationToken);
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto(
            account.Id,
            account.Name,
            account.Type.ToString(),
            account.Currency,
            account.AccountNumber,
            account.Institution,
            account.CurrentBalance,
            account.IsActive,
            account.CreatedAt
        );
    }
}
