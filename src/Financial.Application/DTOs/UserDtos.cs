namespace Financial.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string PreferredCurrency,
    DateTime CreatedAt
);

public record CreateUserDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PreferredCurrency = "PYG"
);

public record LoginDto(
    string Email,
    string Password
);
