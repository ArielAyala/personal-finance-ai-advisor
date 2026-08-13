using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using FluentValidation;

namespace Financial.Application.Validators;

/// <summary>
/// Validator for CreateTransactionDto.
/// Validates transaction creation with required fields and business rules.
/// </summary>
public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator(ICategoryRepository categoryRepository, IAccountRepository accountRepository)
    {
        // AccountId validation: required
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required.")
            .MustAsync(async (accountId, cancellation) =>
            {
                var account = await accountRepository.GetByIdAsync(accountId, cancellation);
                return account != null;
            })
            .WithMessage("Account does not exist.");

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

        // Amount validation: must not be 0
        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount must not be zero.");

        // Currency validation: must be PYG or USD
        RuleFor(x => x.Currency)
            .MustBeValidCurrency();

        // Date validation: must not be in the future
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future.");

        // Merchant validation: required if Amount < 0 (expense)
        RuleFor(x => x.Merchant)
            .NotEmpty().WithMessage("Merchant is required for expenses.")
            .When(x => x.Amount < 0);

        // Merchant validation: max 200 characters
        RuleFor(x => x.Merchant)
            .MaximumLength(200).WithMessage("Merchant must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Merchant));

        // Description validation: max 500 characters
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // Notes validation: max 1000 characters
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        // Type validation: must be valid transaction type
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => new[] { "Income", "Expense", "Transfer" }.Contains(type))
            .WithMessage("Type must be one of: Income, Expense, Transfer.");
    }
}

/// <summary>
/// Validator for UpdateTransactionDto.
/// Validates transaction updates with optional fields and business rules.
/// </summary>
public class UpdateTransactionDtoValidator : AbstractValidator<UpdateTransactionDto>
{
    public UpdateTransactionDtoValidator(ICategoryRepository categoryRepository)
    {
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

        // Amount validation: if provided, must not be 0
        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount must not be zero.")
            .When(x => x.Amount.HasValue);

        // Date validation: if provided, must not be in the future
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future.")
            .When(x => x.Date.HasValue);

        // Merchant validation: max 200 characters
        RuleFor(x => x.Merchant)
            .MaximumLength(200).WithMessage("Merchant must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Merchant));

        // Description validation: max 500 characters
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // Notes validation: max 1000 characters
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
