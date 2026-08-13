using Financial.Domain.Entities;

namespace Financial.Application.Interfaces;

public interface IFinancialSnapshotRepository
{
    Task<FinancialSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FinancialSnapshot?> GetByIdWithAccountsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinancialSnapshot>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinancialSnapshot>> GetByUserIdWithAccountsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FinancialSnapshot?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FinancialSnapshot?> GetLatestClosedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinancialSnapshot>> GetClosedSnapshotsByUserIdAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<FinancialSnapshot> CreateAsync(FinancialSnapshot snapshot, CancellationToken cancellationToken = default);
    Task UpdateAsync(FinancialSnapshot snapshot, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
