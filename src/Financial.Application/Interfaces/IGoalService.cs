using Financial.Application.DTOs;

namespace Financial.Application.Interfaces;

/// <summary>
/// Service interface for managing financial goals
/// </summary>
public interface IGoalService
{
    Task<GoalDto?> GetByIdAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GoalDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GoalDto>> GetActiveGoalsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GoalProgressDto>> GetGoalsProgressAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GoalDto> CreateAsync(Guid userId, CreateGoalDto dto, CancellationToken cancellationToken = default);
    Task<GoalDto> UpdateAsync(Guid userId, Guid goalId, UpdateGoalDto dto, CancellationToken cancellationToken = default);
    Task<GoalDto> UpdateProgressAsync(Guid userId, Guid goalId, UpdateGoalProgressDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default);
}
