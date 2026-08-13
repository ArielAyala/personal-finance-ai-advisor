using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using FluentValidation;

namespace Financial.Application.Validators;

/// <summary>
/// Validator for CreateBillDto.
/// Validates bill creation with required fields and business rules.
/// </summary>
public class CreateBillDtoValidator : AbstractValidator<CreateBillDto>
{
    public CreateBillDtoValidator(ICategoryRepository categoryRepository, IAccountRepository accountRepository)
    {
        // Title validation: required, 3-200 characters
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters.");

        // Amount validation: must be greater than 0
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        // Currency validation: must be PYG or USD
        RuleFor(x => x.Currency)
            .MustBeValidCurrency();

        // DueDate validation: must not be in the past
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Due date cannot be in the past.");

        // Periodicity validation: must be valid enum value
        RuleFor(x => x.Periodicity)
            .NotEmpty().WithMessage("Periodicity is required.")
            .Must(periodicity => new[] { "OneTime", "Daily", "Weekly", "Monthly", "Yearly", "Custom" }.Contains(periodicity))
            .WithMessage("Periodicity must be one of: OneTime, Daily, Weekly, Monthly, Yearly, Custom.");

        // Installments validation: if provided, must be greater than 0
        RuleFor(x => x.Installments)
            .GreaterThan(0).WithMessage("Installments must be greater than 0.")
            .When(x => x.Installments.HasValue);

        // CategoryId validation: if provided, must exist
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellation) =>
            {
                if (categoryId == null) return true;
                var category = await categoryRepository.GetByIdAsync(categoryId.Value, cancellation);
                return category != null;
            })
            .WithMessage("Category does not exist.")
            .When(x => x.CategoryId.HasValue);

        // AccountId validation: if provided, must exist
        RuleFor(x => x.AccountId)
            .MustAsync(async (accountId, cancellation) =>
            {
                if (accountId == null) return true;
                var account = await accountRepository.GetByIdAsync(accountId.Value, cancellation);
                return account != null;
            })
            .WithMessage("Account does not exist.")
            .When(x => x.AccountId.HasValue);

        // Notes validation: max 1000 characters
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateBillDto.
/// Validates bill updates with optional fields and business rules.
/// </summary>
public class UpdateBillDtoValidator : AbstractValidator<UpdateBillDto>
{
    public UpdateBillDtoValidator(ICategoryRepository categoryRepository, IAccountRepository accountRepository)
    {
        // Title validation: if provided, must be 3-200 characters
        RuleFor(x => x.Title)
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        // Amount validation: if provided, must be greater than 0
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.")
            .When(x => x.Amount.HasValue);

        // Currency validation: if provided, must be PYG or USD
        RuleFor(x => x.Currency)
            .MustBeValidCurrencyWhenProvided()
            .When(x => x.Currency != null);

        // PaidAmount validation: if provided, must be >= 0 and <= Amount
        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount must be greater than or equal to 0.")
            .When(x => x.PaidAmount.HasValue);

        // Cross-field validation: PaidAmount <= Amount
        RuleFor(x => x.PaidAmount)
            .LessThanOrEqualTo(x => x.Amount ?? decimal.MaxValue)
            .WithMessage("Paid amount cannot exceed bill amount.")
            .When(x => x.PaidAmount.HasValue && x.Amount.HasValue);

        // DueDate validation: if provided, must not be in the past
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Due date cannot be in the past.")
            .When(x => x.DueDate.HasValue);

        // Periodicity validation: if provided, must be valid enum value
        RuleFor(x => x.Periodicity)
            .Must(periodicity => periodicity == null || new[] { "OneTime", "Daily", "Weekly", "Monthly", "Yearly", "Custom" }.Contains(periodicity))
            .WithMessage("Periodicity must be one of: OneTime, Daily, Weekly, Monthly, Yearly, Custom.")
            .When(x => x.Periodicity != null);

        // Installments validation: if provided, must be greater than 0 and >= PaidInstallments
        RuleFor(x => x.Installments)
            .GreaterThan(0).WithMessage("Installments must be greater than 0.")
            .When(x => x.Installments.HasValue);

        // Cross-field validation: Installments >= PaidInstallments
        RuleFor(x => x.Installments)
            .GreaterThanOrEqualTo(x => x.PaidInstallments ?? 0)
            .WithMessage("Total installments cannot be less than paid installments.")
            .When(x => x.Installments.HasValue && x.PaidInstallments.HasValue);

        // PaidInstallments validation: if provided, must be >= 0
        RuleFor(x => x.PaidInstallments)
            .GreaterThanOrEqualTo(0).WithMessage("Paid installments must be greater than or equal to 0.")
            .When(x => x.PaidInstallments.HasValue);

        // Status validation: if provided, must be valid bill status
        RuleFor(x => x.Status)
            .Must(status => status == null || new[] { "Active", "Paid", "Overdue", "Cancelled" }.Contains(status))
            .WithMessage("Status must be one of: Active, Paid, Overdue, Cancelled.")
            .When(x => x.Status != null);

        // CategoryId validation: if provided, must exist
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellation) =>
            {
                if (categoryId == null) return true;
                var category = await categoryRepository.GetByIdAsync(categoryId.Value, cancellation);
                return category != null;
            })
            .WithMessage("Category does not exist.")
            .When(x => x.CategoryId.HasValue);

        // AccountId validation: if provided, must exist
        RuleFor(x => x.AccountId)
            .MustAsync(async (accountId, cancellation) =>
            {
                if (accountId == null) return true;
                var account = await accountRepository.GetByIdAsync(accountId.Value, cancellation);
                return account != null;
            })
            .WithMessage("Account does not exist.")
            .When(x => x.AccountId.HasValue);

        // Notes validation: max 1000 characters
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Validator for PayBillDto.
/// Validates bill payment with amount and date.
/// </summary>
public class PayBillDtoValidator : AbstractValidator<PayBillDto>
{
    public PayBillDtoValidator()
    {
        // PaymentAmount validation: must be greater than 0
        RuleFor(x => x.PaymentAmount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        // PaymentDate validation: must not be in the future
        RuleFor(x => x.PaymentDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future.");
    }
}
