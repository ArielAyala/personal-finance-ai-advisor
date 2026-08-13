using Financial.Domain.Entities;

namespace Financial.Application.Interfaces;

/// <summary>
/// Repository interface for Goal entities
/// </summary>
public interface IGoalRepository
{
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Goal>> GetActiveGoalsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Goal> AddAsync(Goal goal, CancellationToken cancellationToken = default);
    Task UpdateAsync(Goal goal, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
