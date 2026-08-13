using FluentValidation;

namespace Financial.Application.Validators;

/// <summary>
/// Base validator with common validation rules for currency validation.
/// </summary>
public static class CommonValidationRules
{
    /// <summary>
    /// Supported currencies: PYG (Paraguayan Guaraní) and USD (US Dollar).
    /// </summary>
    public static readonly string[] ValidCurrencies = { "PYG", "USD" };
    
    /// <summary>
    /// Validates that currency is either PYG or USD.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidCurrency<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Currency is required.")
            .Must(currency => ValidCurrencies.Contains(currency))
            .WithMessage($"Currency must be one of: {string.Join(", ", ValidCurrencies)}.");
    }
    
    /// <summary>
    /// Validates that an optional currency is either PYG or USD when provided.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidCurrencyWhenProvided<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(currency => currency == null || ValidCurrencies.Contains(currency))
            .WithMessage($"Currency must be one of: {string.Join(", ", ValidCurrencies)}.");
    }
}
