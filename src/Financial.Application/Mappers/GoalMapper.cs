using Financial.Application.DTOs;
using Financial.Domain.Entities;

namespace Financial.Application.Mappers;

public static class GoalMapper
{
    public static GoalDto ToDto(Goal goal)
    {
        var progressPercentage = goal.TargetAmount > 0 
            ? Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2) 
            : 0;

        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.Currency,
            goal.TargetDate,
            goal.Status.ToString(),
            progressPercentage,
            goal.CreatedAt,
            goal.UpdatedAt,
            goal.CompletedAt
        );
    }

    public static GoalProgressDto ToProgressDto(Goal goal)
    {
        var progressPercentage = goal.TargetAmount > 0 
            ? Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2) 
            : 0;
        
        var remainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount);
        var isCompleted = goal.CurrentAmount >= goal.TargetAmount;
        var isOverdue = goal.TargetDate.HasValue && goal.TargetDate.Value < DateTime.UtcNow && !isCompleted;
        var daysUntilTarget = goal.TargetDate.HasValue 
            ? (int)(goal.TargetDate.Value - DateTime.UtcNow).TotalDays 
            : (int?)null;

        return new GoalProgressDto(
            goal.Id,
            goal.Title,
            goal.TargetAmount,
            goal.CurrentAmount,
            remainingAmount,
            progressPercentage,
            daysUntilTarget,
            isCompleted,
            isOverdue
        );
    }
}
