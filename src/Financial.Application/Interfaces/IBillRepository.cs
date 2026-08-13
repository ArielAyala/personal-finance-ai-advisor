using Financial.Domain.Entities;

namespace Financial.Application.Interfaces;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bill>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bill>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bill>> GetOverdueByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bill>> GetUpcomingByUserIdAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Bill> CreateAsync(Bill bill, CancellationToken cancellationToken = default);
    Task UpdateAsync(Bill bill, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
