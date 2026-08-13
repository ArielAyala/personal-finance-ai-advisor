using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class FinancialSnapshotRepository : IFinancialSnapshotRepository
{
    private readonly FinancialDbContext _context;

    public FinancialSnapshotRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<FinancialSnapshot?> GetByIdWithAccountsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .Include(s => s.SnapshotAccounts)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<FinancialSnapshot>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FinancialSnapshot>> GetByUserIdWithAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .Include(s => s.SnapshotAccounts)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<FinancialSnapshot?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FinancialSnapshot?> GetLatestClosedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .Where(s => s.UserId == userId && s.Status == SnapshotStatus.Closed)
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<FinancialSnapshot>> GetClosedSnapshotsByUserIdAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialSnapshots
            .Include(s => s.SnapshotAccounts)
            .Where(s => s.UserId == userId && s.Status == SnapshotStatus.Closed)
            .OrderByDescending(s => s.Date)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<FinancialSnapshot> CreateAsync(FinancialSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        snapshot.CreatedAt = DateTime.UtcNow;
        _context.FinancialSnapshots.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    public async Task UpdateAsync(FinancialSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        snapshot.UpdatedAt = DateTime.UtcNow;
        _context.FinancialSnapshots.Update(snapshot);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshot = await GetByIdAsync(id, cancellationToken);
        if (snapshot != null)
        {
            _context.FinancialSnapshots.Remove(snapshot);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
