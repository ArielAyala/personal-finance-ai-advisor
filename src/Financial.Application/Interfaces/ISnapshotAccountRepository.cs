using Financial.Domain.Entities;

namespace Financial.Application.Interfaces;

public interface ISnapshotAccountRepository
{
    Task<SnapshotAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SnapshotAccount>> GetBySnapshotIdAsync(Guid snapshotId, CancellationToken cancellationToken = default);
    Task<SnapshotAccount> CreateAsync(SnapshotAccount snapshotAccount, CancellationToken cancellationToken = default);
    Task UpdateAsync(SnapshotAccount snapshotAccount, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
