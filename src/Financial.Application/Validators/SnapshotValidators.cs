using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using FluentValidation;

namespace Financial.Application.Validators;

/// <summary>
/// Validator for CreateFinancialSnapshotDto.
/// Validates financial snapshot creation with required fields and business rules.
/// </summary>
public class CreateFinancialSnapshotDtoValidator : AbstractValidator<CreateFinancialSnapshotDto>
{
    public CreateFinancialSnapshotDtoValidator()
    {
        // Date validation: required
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        // BaseCurrency validation: must be PYG or USD
        RuleFor(x => x.BaseCurrency)
            .MustBeValidCurrency();

        // ExchangeRateDate validation: must not be in the future if provided
        RuleFor(x => x.ExchangeRateDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Exchange rate date cannot be in the future.")
            .When(x => x.ExchangeRateDate.HasValue);

        // Notes validation: max 1000 characters
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateFinancialSnapshotDto.
/// Validates financial snapshot updates with optional fields and business rules.
/// </summary>
public class UpdateFinancialSnapshotDtoValidator : AbstractValidator<UpdateFinancialSnapshotDto>
{
    public UpdateFinancialSnapshotDtoValidator()
    {
        // BaseCurrency validation: if provided, must be PYG or USD
        RuleFor(x => x.BaseCurrency)
            .MustBeValidCurrencyWhenProvided()
            .When(x => x.BaseCurrency != null);

        // ExchangeRateDate validation: must not be in the future if provided
        RuleFor(x => x.ExchangeRateDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Exchange rate date cannot be in the future.")
            .When(x => x.ExchangeRateDate.HasValue);

        // Notes validation: max 1000 characters
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Validator for CreateSnapshotAccountDto.
/// Validates snapshot account creation with required fields and business rules.
/// </summary>
public class CreateSnapshotAccountDtoValidator : AbstractValidator<CreateSnapshotAccountDto>
{
    public CreateSnapshotAccountDtoValidator(IAccountRepository accountRepository)
    {
        // AccountId validation: required and must exist
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required.")
            .MustAsync(async (accountId, cancellation) =>
            {
                var account = await accountRepository.GetByIdAsync(accountId, cancellation);
                return account != null;
            })
            .WithMessage("Account does not exist.");

        // OriginalAmount validation: no specific constraint (can be negative for liabilities)
        // But we'll add a reasonable range check
        RuleFor(x => x.OriginalAmount)
            .NotNull().WithMessage("Original amount is required.");

        // OriginalCurrency validation: must be PYG or USD
        RuleFor(x => x.OriginalCurrency)
            .MustBeValidCurrency();

        // ExchangeRate validation: if provided, must be greater than 0
        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0.")
            .When(x => x.ExchangeRate.HasValue);
    }
}

/// <summary>
/// Validator for UpdateSnapshotAccountDto.
/// Validates snapshot account updates with optional fields and business rules.
/// </summary>
public class UpdateSnapshotAccountDtoValidator : AbstractValidator<UpdateSnapshotAccountDto>
{
    public UpdateSnapshotAccountDtoValidator()
    {
        // OriginalCurrency validation: if provided, must be PYG or USD
        RuleFor(x => x.OriginalCurrency)
            .MustBeValidCurrencyWhenProvided()
            .When(x => x.OriginalCurrency != null);

        // ExchangeRate validation: if provided, must be greater than 0
        RuleFor(x => x.ExchangeRate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than 0.")
            .When(x => x.ExchangeRate.HasValue);
    }
}
