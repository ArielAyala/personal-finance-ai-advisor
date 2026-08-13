namespace Financial.Application.DTOs;

public record GoalDto(
    Guid Id,
    string Title,
    string? Description,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Currency,
    DateTime? TargetDate,
    string Status,
    decimal ProgressPercentage,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CompletedAt
);

public record CreateGoalDto(
    string Title,
    string? Description,
    decimal TargetAmount,
    string Currency = "PYG",
    DateTime? TargetDate = null
);

public record UpdateGoalDto(
    string? Title,
    string? Description,
    decimal? TargetAmount,
    decimal? CurrentAmount,
    string? Currency,
    DateTime? TargetDate,
    string? Status
);

public record UpdateGoalProgressDto(
    decimal Amount,
    string Operation // "add" or "set"
);

public record GoalProgressDto(
    Guid GoalId,
    string Title,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    decimal ProgressPercentage,
    int? DaysUntilTarget,
    bool IsCompleted,
    bool IsOverdue
);
