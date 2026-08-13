using Financial.Application.DTOs;
using FluentValidation;

namespace Financial.Application.Validators;

/// <summary>
/// Validator for CreateAccountDto.
/// Validates account creation with required fields and business rules.
/// </summary>
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        // Name validation: required, 3-200 characters
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 200).WithMessage("Name must be between 3 and 200 characters.");

        // Type validation: must be valid AccountType
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => new[] { "BankAccount", "CreditCard", "Cash", "Investment", "Other" }.Contains(type))
            .WithMessage("Type must be one of: BankAccount, CreditCard, Cash, Investment, Other.");

        // Currency validation: must be PYG or USD
        RuleFor(x => x.Currency)
            .MustBeValidCurrency();

        // AccountNumber validation: max 50 characters
        RuleFor(x => x.AccountNumber)
            .MaximumLength(50).WithMessage("Account number must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));

        // Institution validation: max 200 characters
        RuleFor(x => x.Institution)
            .MaximumLength(200).WithMessage("Institution must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Institution));

        // InitialBalance validation: must be >= 0
        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance must be greater than or equal to 0.");
    }
}

/// <summary>
/// Validator for UpdateAccountDto.
/// Validates account updates with optional fields and business rules.
/// </summary>
public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountDtoValidator()
    {
        // Name validation: if provided, must be 3-200 characters
        RuleFor(x => x.Name)
            .Length(3, 200).WithMessage("Name must be between 3 and 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        // AccountNumber validation: max 50 characters
        RuleFor(x => x.AccountNumber)
            .MaximumLength(50).WithMessage("Account number must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));

        // Institution validation: max 200 characters
        RuleFor(x => x.Institution)
            .MaximumLength(200).WithMessage("Institution must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Institution));

        // CurrentBalance validation: if provided, must be >= 0
        RuleFor(x => x.CurrentBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Current balance must be greater than or equal to 0.")
            .When(x => x.CurrentBalance.HasValue);
    }
}
