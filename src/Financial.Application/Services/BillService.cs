using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Application.Mappers;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class BillService
{
    private readonly IBillRepository _billRepository;

    public BillService(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<IEnumerable<BillDto>> GetUserBillsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetByUserIdAsync(userId, cancellationToken);
        return bills.Select(BillMapper.ToDto);
    }

    public async Task<IEnumerable<BillDto>> GetActiveBillsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        return bills.Select(BillMapper.ToDto);
    }

    public async Task<IEnumerable<BillDto>> GetOverdueBillsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetOverdueByUserIdAsync(userId, cancellationToken);
        return bills.Select(BillMapper.ToDto);
    }

    public async Task<IEnumerable<BillDto>> GetUpcomingBillsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetUpcomingByUserIdAsync(userId, startDate, endDate, cancellationToken);
        return bills.Select(BillMapper.ToDto);
    }

    public async Task<BillDto?> GetBillByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken);
        return bill != null ? BillMapper.ToDto(bill) : null;
    }

    public async Task<BillDto> CreateBillAsync(Guid userId, CreateBillDto dto, CancellationToken cancellationToken = default)
    {
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Amount = dto.Amount,
            Currency = dto.Currency,
            PaidAmount = 0,
            DueDate = dto.DueDate,
            Periodicity = Enum.Parse<BillPeriodicity>(dto.Periodicity),
            Installments = dto.Installments,
            PaidInstallments = 0,
            Status = BillStatus.Active,
            CategoryId = dto.CategoryId,
            AccountId = dto.AccountId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _billRepository.CreateAsync(bill, cancellationToken);
        return BillMapper.ToDto(created);
    }

    public async Task<BillDto> UpdateBillAsync(Guid id, UpdateBillDto dto, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill with ID {id} not found");

        if (dto.Title != null) bill.Title = dto.Title;
        if (dto.Amount.HasValue) bill.Amount = dto.Amount.Value;
        if (dto.Currency != null) bill.Currency = dto.Currency;
        if (dto.PaidAmount.HasValue) bill.PaidAmount = dto.PaidAmount.Value;
        if (dto.DueDate.HasValue) bill.DueDate = dto.DueDate.Value;
        if (dto.Periodicity != null) bill.Periodicity = Enum.Parse<BillPeriodicity>(dto.Periodicity);
        if (dto.Installments.HasValue) bill.Installments = dto.Installments;
        if (dto.PaidInstallments.HasValue) bill.PaidInstallments = dto.PaidInstallments;
        if (dto.Status != null) bill.Status = Enum.Parse<BillStatus>(dto.Status);
        if (dto.CategoryId.HasValue) bill.CategoryId = dto.CategoryId;
        if (dto.AccountId.HasValue) bill.AccountId = dto.AccountId;
        if (dto.Notes != null) bill.Notes = dto.Notes;

        bill.UpdatedAt = DateTime.UtcNow;

        await _billRepository.UpdateAsync(bill, cancellationToken);
        return BillMapper.ToDto(bill);
    }

    public async Task<BillDto> PayBillAsync(Guid id, PayBillDto dto, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill with ID {id} not found");

        // Update paid amount
        bill.PaidAmount += dto.PaymentAmount;

        // If bill has installments, increment paid installments
        if (bill.Installments.HasValue && bill.PaidInstallments.HasValue)
        {
            bill.PaidInstallments++;
            
            // Check if all installments are paid
            if (bill.PaidInstallments >= bill.Installments)
            {
                bill.Status = BillStatus.Paid;
            }
        }
        // For non-installment bills, check if fully paid
        else if (bill.PaidAmount >= bill.Amount)
        {
            bill.Status = BillStatus.Paid;
        }

        bill.UpdatedAt = DateTime.UtcNow;

        await _billRepository.UpdateAsync(bill, cancellationToken);
        return BillMapper.ToDto(bill);
    }

    public async Task DeleteBillAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _billRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<decimal> GetMonthlyObligationsAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var bills = await _billRepository.GetUpcomingByUserIdAsync(userId, startDate, endDate, cancellationToken);
        
        return bills.Sum(b => b.Amount - b.PaidAmount);
    }
}
