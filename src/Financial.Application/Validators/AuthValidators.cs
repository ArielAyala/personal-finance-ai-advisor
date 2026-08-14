using Financial.Application.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Financial.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Must(ContainUppercase).WithMessage("Password must contain at least one uppercase letter.")
            .Must(ContainLowercase).WithMessage("Password must contain at least one lowercase letter.")
            .Must(ContainDigit).WithMessage("Password must contain at least one digit.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.PreferredCurrency)
            .MustBeValidCurrency();
    }

    private bool ContainUppercase(string password)
    {
        return password.Any(char.IsUpper);
    }

    private bool ContainLowercase(string password)
    {
        return password.Any(char.IsLower);
    }

    private bool ContainDigit(string password)
    {
        return password.Any(char.IsDigit);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
