using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class BillRepository : IBillRepository
{
    private readonly FinancialDbContext _context;

    public BillRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<Bill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .Include(b => b.Category)
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Bill>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bill>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.UserId == userId && b.Status == BillStatus.Active)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bill>> GetOverdueByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Bills
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.UserId == userId 
                && b.Status == BillStatus.Active 
                && b.DueDate < today)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bill>> GetUpcomingByUserIdAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Bills
            .Include(b => b.Category)
            .Include(b => b.Account)
            .Where(b => b.UserId == userId 
                && b.Status == BillStatus.Active 
                && b.DueDate >= startDate 
                && b.DueDate <= endDate)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Bill> CreateAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        bill.CreatedAt = DateTime.UtcNow;
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync(cancellationToken);
        return bill;
    }

    public async Task UpdateAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        bill.UpdatedAt = DateTime.UtcNow;
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bill = await GetByIdAsync(id, cancellationToken);
        if (bill != null)
        {
            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
