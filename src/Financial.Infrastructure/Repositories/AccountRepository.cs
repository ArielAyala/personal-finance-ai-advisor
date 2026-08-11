using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly FinancialDbContext _context;

    public AccountRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        account.CreatedAt = DateTime.UtcNow;
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        account.UpdatedAt = DateTime.UtcNow;
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await GetByIdAsync(id, cancellationToken);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
