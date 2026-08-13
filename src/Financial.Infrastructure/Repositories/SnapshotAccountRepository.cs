using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class SnapshotAccountRepository : ISnapshotAccountRepository
{
    private readonly FinancialDbContext _context;

    public SnapshotAccountRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<SnapshotAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SnapshotAccounts
            .FirstOrDefaultAsync(sa => sa.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SnapshotAccount>> GetBySnapshotIdAsync(Guid snapshotId, CancellationToken cancellationToken = default)
    {
        return await _context.SnapshotAccounts
            .Where(sa => sa.SnapshotId == snapshotId)
            .OrderBy(sa => sa.AccountName)
            .ToListAsync(cancellationToken);
    }

    public async Task<SnapshotAccount> CreateAsync(SnapshotAccount snapshotAccount, CancellationToken cancellationToken = default)
    {
        _context.SnapshotAccounts.Add(snapshotAccount);
        await _context.SaveChangesAsync(cancellationToken);
        return snapshotAccount;
    }

    public async Task UpdateAsync(SnapshotAccount snapshotAccount, CancellationToken cancellationToken = default)
    {
        _context.SnapshotAccounts.Update(snapshotAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshotAccount = await GetByIdAsync(id, cancellationToken);
        if (snapshotAccount != null)
        {
            _context.SnapshotAccounts.Remove(snapshotAccount);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
