using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Application.Mappers;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class FinancialSnapshotService
{
    private readonly IFinancialSnapshotRepository _snapshotRepository;
    private readonly ISnapshotAccountRepository _snapshotAccountRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ExchangeRateService _exchangeRateService;

    public FinancialSnapshotService(
        IFinancialSnapshotRepository snapshotRepository,
        ISnapshotAccountRepository snapshotAccountRepository,
        IAccountRepository accountRepository,
        ExchangeRateService exchangeRateService)
    {
        _snapshotRepository = snapshotRepository;
        _snapshotAccountRepository = snapshotAccountRepository;
        _accountRepository = accountRepository;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<IEnumerable<FinancialSnapshotSummaryDto>> GetUserSnapshotsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var snapshots = await _snapshotRepository.GetByUserIdAsync(userId, cancellationToken);
        return FinancialSnapshotMapper.ToSummaryDtoList(snapshots);
    }

    public async Task<FinancialSnapshotDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepository.GetByIdWithAccountsAsync(id, cancellationToken);
        return snapshot != null ? FinancialSnapshotMapper.ToDto(snapshot) : null;
    }

    public async Task<FinancialSnapshotDto?> GetLatestAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepository.GetLatestByUserIdAsync(userId, cancellationToken);
        if (snapshot == null)
            return null;

        // Load accounts
        var snapshotWithAccounts = await _snapshotRepository.GetByIdWithAccountsAsync(snapshot.Id, cancellationToken);
        return snapshotWithAccounts != null ? FinancialSnapshotMapper.ToDto(snapshotWithAccounts) : null;
    }

    public async Task<SnapshotGrowthDto?> GetNetWorthGrowthAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var closedSnapshots = await _snapshotRepository.GetClosedSnapshotsByUserIdAsync(userId, 2, cancellationToken);
        var snapshotList = closedSnapshots.ToList();

        if (snapshotList.Count == 0)
            return null;

        var current = snapshotList[0];
        var previous = snapshotList.Count > 1 ? snapshotList[1] : null;

        decimal? absoluteGrowth = null;
        decimal? percentageGrowth = null;

        if (previous != null)
        {
            absoluteGrowth = current.TotalAmount - previous.TotalAmount;
            percentageGrowth = previous.TotalAmount != 0 
                ? (absoluteGrowth.Value / previous.TotalAmount) * 100 
                : null;
        }

        return new SnapshotGrowthDto(
            CurrentSnapshotId: current.Id,
            CurrentDate: current.Date,
            CurrentAmount: current.TotalAmount,
            PreviousSnapshotId: previous?.Id,
            PreviousDate: previous?.Date,
            PreviousAmount: previous?.TotalAmount,
            AbsoluteGrowth: absoluteGrowth,
            PercentageGrowth: percentageGrowth
        );
    }

    public async Task<FinancialSnapshotDto> CreateAsync(Guid userId, CreateFinancialSnapshotDto dto, CancellationToken cancellationToken = default)
    {
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = dto.Date.Date,
            BaseCurrency = dto.BaseCurrency,
            TotalAmount = 0, // Will be calculated when accounts are added
            ExchangeRateDate = dto.ExchangeRateDate,
            Status = SnapshotStatus.Draft,
            Notes = dto.Notes
        };

        var created = await _snapshotRepository.CreateAsync(snapshot, cancellationToken);
        return FinancialSnapshotMapper.ToDto(created);
    }

    public async Task<FinancialSnapshotDto> UpdateAsync(Guid id, UpdateFinancialSnapshotDto dto, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepository.GetByIdWithAccountsAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Financial snapshot with ID {id} not found");

        if (snapshot.Status == SnapshotStatus.Closed)
        {
            throw new InvalidOperationException("Cannot update a closed snapshot");
        }

        if (dto.Date.HasValue) snapshot.Date = dto.Date.Value.Date;
        if (dto.BaseCurrency != null) snapshot.BaseCurrency = dto.BaseCurrency;
        if (dto.ExchangeRateDate.HasValue) snapshot.ExchangeRateDate = dto.ExchangeRateDate.Value;
        if (dto.Notes != null) snapshot.Notes = dto.Notes;

        await _snapshotRepository.UpdateAsync(snapshot, cancellationToken);
        
        var updated = await _snapshotRepository.GetByIdWithAccountsAsync(id, cancellationToken);
        return FinancialSnapshotMapper.ToDto(updated!);
    }

    public async Task<FinancialSnapshotDto> CloseSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepository.GetByIdWithAccountsAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Financial snapshot with ID {id} not found");

        if (snapshot.Status == SnapshotStatus.Closed)
        {
            throw new InvalidOperationException("Snapshot is already closed");
        }

        snapshot.Status = SnapshotStatus.Closed;
        snapshot.ClosedAt = DateTime.UtcNow;

        await _snapshotRepository.UpdateAsync(snapshot, cancellationToken);
        
        var updated = await _snapshotRepository.GetByIdWithAccountsAsync(id, cancellationToken);
        return FinancialSnapshotMapper.ToDto(updated!);
    }

    public async Task<SnapshotAccountDto> AddAccountToSnapshotAsync(Guid snapshotId, CreateSnapshotAccountDto dto, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepository.GetByIdWithAccountsAsync(snapshotId, cancellationToken)
            ?? throw new KeyNotFoundException($"Financial snapshot with ID {snapshotId} not found");

        if (snapshot.Status == SnapshotStatus.Closed)
        {
            throw new InvalidOperationException("Cannot add accounts to a closed snapshot");
        }

        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account with ID {dto.AccountId} not found");

        // Check if account belongs to the same user as the snapshot
        if (account.UserId != snapshot.UserId)
        {
            throw new InvalidOperationException("Account does not belong to the snapshot owner");
        }

        // Calculate converted amount
        decimal convertedAmount = dto.OriginalAmount;
        decimal? exchangeRate = dto.ExchangeRate;

        if (dto.OriginalCurrency != snapshot.BaseCurrency)
        {
            if (exchangeRate.HasValue)
            {
                convertedAmount = dto.OriginalAmount * exchangeRate.Value;
            }
            else
            {
                var rateDate = snapshot.ExchangeRateDate ?? snapshot.Date;
                var converted = await _exchangeRateService.ConvertCurrencyAsync(
                    dto.OriginalAmount, 
                    dto.OriginalCurrency, 
                    snapshot.BaseCurrency, 
                    rateDate, 
                    cancellationToken);

                if (!converted.HasValue)
                {
                    throw new InvalidOperationException($"Exchange rate from {dto.OriginalCurrency} to {snapshot.BaseCurrency} not found for date {rateDate:yyyy-MM-dd}");
                }

                convertedAmount = converted.Value;
                
                // Get the actual rate for storage
                var rateDto = await _exchangeRateService.GetRateAsync(dto.OriginalCurrency, snapshot.BaseCurrency, rateDate, cancellationToken);
                exchangeRate = rateDto?.Rate;
            }
        }

        var snapshotAccount = new SnapshotAccount
        {
            Id = Guid.NewGuid(),
            SnapshotId = snapshotId,
            AccountId = dto.AccountId,
            AccountName = account.Name,
            AccountNumberMasked = account.AccountNumber != null ? MaskAccountNumber(account.AccountNumber) : null,
            OriginalAmount = dto.OriginalAmount,
            OriginalCurrency = dto.OriginalCurrency,
            ExchangeRate = exchangeRate,
            ConvertedAmount = convertedAmount,
            BaseCurrency = snapshot.BaseCurrency
        };

        var created = await _snapshotAccountRepository.CreateAsync(snapshotAccount, cancellationToken);

        // Recalculate snapshot total
        await RecalculateSnapshotTotalAsync(snapshotId, cancellationToken);

        return SnapshotAccountMapper.ToDto(created);
    }

    public async Task<SnapshotAccountDto> UpdateSnapshotAccountAsync(Guid snapshotAccountId, UpdateSnapshotAccountDto dto, CancellationToken cancellationToken = default)
    {
        var snapshotAccount = await _snapshotAccountRepository.GetByIdAsync(snapshotAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Snapshot account with ID {snapshotAccountId} not found");

        var snapshot = await _snapshotRepository.GetByIdAsync(snapshotAccount.SnapshotId, cancellationToken);
        if (snapshot!.Status == SnapshotStatus.Closed)
        {
            throw new InvalidOperationException("Cannot update accounts in a closed snapshot");
        }

        if (dto.OriginalAmount.HasValue) snapshotAccount.OriginalAmount = dto.OriginalAmount.Value;
        if (dto.OriginalCurrency != null) snapshotAccount.OriginalCurrency = dto.OriginalCurrency;
        if (dto.ExchangeRate.HasValue) snapshotAccount.ExchangeRate = dto.ExchangeRate.Value;

        // Recalculate converted amount
        if (snapshotAccount.OriginalCurrency == snapshot.BaseCurrency)
        {
            snapshotAccount.ConvertedAmount = snapshotAccount.OriginalAmount;
            snapshotAccount.ExchangeRate = null;
        }
        else if (snapshotAccount.ExchangeRate.HasValue)
        {
            snapshotAccount.ConvertedAmount = snapshotAccount.OriginalAmount * snapshotAccount.ExchangeRate.Value;
        }

        await _snapshotAccountRepository.UpdateAsync(snapshotAccount, cancellationToken);
        await RecalculateSnapshotTotalAsync(snapshotAccount.SnapshotId, cancellationToken);

        return SnapshotAccountMapper.ToDto(snapshotAccount);
    }

    public async Task RemoveAccountFromSnapshotAsync(Guid snapshotAccountId, CancellationToken cancellationToken = default)
    {
        var snapshotAccount = await _snapshotAccountRepository.GetByIdAsync(snapshotAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Snapshot account with ID {snapshotAccountId} not found");

        var snapshot = await _snapshotRepository.GetByIdAsync(snapshotAccount.SnapshotId, cancellationToken);
        if (snapshot!.Status == SnapshotStatus.Closed)
        {
            throw new InvalidOperationException("Cannot remove accounts from a closed snapshot");
        }

        var snapshotId = snapshotAccount.SnapshotId;
        await _snapshotAccountRepository.DeleteAsync(snapshotAccountId, cancellationToken);
        await RecalculateSnapshotTotalAsync(snapshotId, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Financial snapshot with ID {id} not found");

        if (snapshot.Status == SnapshotStatus.Closed)
        {
            throw new InvalidOperationException("Cannot delete a closed snapshot. Consider keeping it for historical records.");
        }

        await _snapshotRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task RecalculateSnapshotTotalAsync(Guid snapshotId, CancellationToken cancellationToken)
    {
        var snapshot = await _snapshotRepository.GetByIdWithAccountsAsync(snapshotId, cancellationToken);
        if (snapshot == null) return;

        snapshot.TotalAmount = snapshot.SnapshotAccounts.Sum(sa => sa.ConvertedAmount);
        await _snapshotRepository.UpdateAsync(snapshot, cancellationToken);
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber)) return string.Empty;
        if (accountNumber.Length <= 4) return new string('*', accountNumber.Length);
        
        var lastFour = accountNumber[^4..];
        var masked = new string('*', accountNumber.Length - 4);
        return masked + lastFour;
    }
}
