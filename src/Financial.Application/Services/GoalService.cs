using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Application.Mappers;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;

    public GoalService(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto?> GetByIdAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId, cancellationToken);
        
        if (goal == null || goal.UserId != userId)
            return null;

        return GoalMapper.ToDto(goal);
    }

    public async Task<IEnumerable<GoalDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId, cancellationToken);
        return goals.Select(GoalMapper.ToDto);
    }

    public async Task<IEnumerable<GoalDto>> GetActiveGoalsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var goals = await _goalRepository.GetActiveGoalsByUserIdAsync(userId, cancellationToken);
        return goals.Select(GoalMapper.ToDto);
    }

    public async Task<IEnumerable<GoalProgressDto>> GetGoalsProgressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId, cancellationToken);
        return goals.Select(GoalMapper.ToProgressDto);
    }

    public async Task<GoalDto> CreateAsync(Guid userId, CreateGoalDto dto, CancellationToken cancellationToken = default)
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            TargetAmount = dto.TargetAmount,
            CurrentAmount = 0,
            Currency = dto.Currency,
            TargetDate = dto.TargetDate,
            Status = GoalStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _goalRepository.AddAsync(goal, cancellationToken);
        return GoalMapper.ToDto(created);
    }

    public async Task<GoalDto> UpdateAsync(Guid userId, Guid goalId, UpdateGoalDto dto, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Goal with ID {goalId} not found");

        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("Goal does not belong to user");

        // Update fields
        if (dto.Title != null) goal.Title = dto.Title;
        if (dto.Description != null) goal.Description = dto.Description;
        if (dto.TargetAmount.HasValue) goal.TargetAmount = dto.TargetAmount.Value;
        if (dto.CurrentAmount.HasValue) goal.CurrentAmount = dto.CurrentAmount.Value;
        if (dto.Currency != null) goal.Currency = dto.Currency;
        if (dto.TargetDate.HasValue) goal.TargetDate = dto.TargetDate;
        if (dto.Status != null) 
        {
            goal.Status = Enum.Parse<GoalStatus>(dto.Status);
            if (goal.Status == GoalStatus.Completed && !goal.CompletedAt.HasValue)
                goal.CompletedAt = DateTime.UtcNow;
        }

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        return GoalMapper.ToDto(goal);
    }

    public async Task<GoalDto> UpdateProgressAsync(Guid userId, Guid goalId, UpdateGoalProgressDto dto, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Goal with ID {goalId} not found");

        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("Goal does not belong to user");

        // Update current amount based on operation
        if (dto.Operation.Equals("add", StringComparison.OrdinalIgnoreCase))
        {
            goal.CurrentAmount += dto.Amount;
        }
        else if (dto.Operation.Equals("set", StringComparison.OrdinalIgnoreCase))
        {
            goal.CurrentAmount = dto.Amount;
        }
        else
        {
            throw new ArgumentException($"Invalid operation: {dto.Operation}. Must be 'add' or 'set'");
        }

        // Ensure current amount doesn't go negative
        goal.CurrentAmount = Math.Max(0, goal.CurrentAmount);

        await _goalRepository.UpdateAsync(goal, cancellationToken);
        return GoalMapper.ToDto(goal);
    }

    public async Task DeleteAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Goal with ID {goalId} not found");

        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("Goal does not belong to user");

        await _goalRepository.DeleteAsync(goalId, cancellationToken);
    }
}
