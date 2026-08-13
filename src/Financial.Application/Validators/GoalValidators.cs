using Financial.Application.DTOs;
using FluentValidation;

namespace Financial.Application.Validators;

/// <summary>
/// Validator for CreateGoalDto.
/// Validates goal creation with required fields and business rules.
/// </summary>
public class CreateGoalDtoValidator : AbstractValidator<CreateGoalDto>
{
    public CreateGoalDtoValidator()
    {
        // Title validation: required, 3-200 characters
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters.");

        // Description validation: optional, max 1000 characters
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description != null);

        // TargetAmount validation: must be greater than 0
        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than 0.");

        // Currency validation: must be PYG or USD
        RuleFor(x => x.Currency)
            .MustBeValidCurrency();

        // TargetDate validation: must be in the future if provided
        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Target date must be in the future.")
            .When(x => x.TargetDate.HasValue);
    }
}

/// <summary>
/// Validator for UpdateGoalDto.
/// Validates goal updates with optional fields and business rules.
/// </summary>
public class UpdateGoalDtoValidator : AbstractValidator<UpdateGoalDto>
{
    public UpdateGoalDtoValidator()
    {
        // Title validation: if provided, must be 3-200 characters
        RuleFor(x => x.Title)
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        // Description validation: optional, max 1000 characters
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description != null);

        // TargetAmount validation: if provided, must be greater than 0
        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than 0.")
            .When(x => x.TargetAmount.HasValue);

        // CurrentAmount validation: if provided, must be >= 0 and <= TargetAmount
        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Current amount must be greater than or equal to 0.")
            .When(x => x.CurrentAmount.HasValue);

        // Cross-field validation: CurrentAmount <= TargetAmount
        RuleFor(x => x.CurrentAmount)
            .LessThanOrEqualTo(x => x.TargetAmount ?? decimal.MaxValue)
            .WithMessage("Current amount cannot exceed target amount.")
            .When(x => x.CurrentAmount.HasValue && x.TargetAmount.HasValue);

        // Currency validation: if provided, must be PYG or USD
        RuleFor(x => x.Currency)
            .MustBeValidCurrencyWhenProvided()
            .When(x => x.Currency != null);

        // TargetDate validation: must be in the future if provided
        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Target date must be in the future.")
            .When(x => x.TargetDate.HasValue);

        // Status validation: must be valid goal status
        RuleFor(x => x.Status)
            .Must(status => status == null || new[] { "Active", "Completed", "Cancelled" }.Contains(status))
            .WithMessage("Status must be one of: Active, Completed, Cancelled.")
            .When(x => x.Status != null);
    }
}

/// <summary>
/// Validator for UpdateGoalProgressDto.
/// Validates goal progress updates.
/// </summary>
public class UpdateGoalProgressDtoValidator : AbstractValidator<UpdateGoalProgressDto>
{
    public UpdateGoalProgressDtoValidator()
    {
        // Amount validation: must not be 0
        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount must not be zero.");

        // Operation validation: must be "add" or "set"
        RuleFor(x => x.Operation)
            .NotEmpty().WithMessage("Operation is required.")
            .Must(op => op == "add" || op == "set")
            .WithMessage("Operation must be 'add' or 'set'.");
    }
}
