namespace Financial.Application.DTOs;

// Request DTOs
public record RegisterRequestDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PreferredCurrency = "PYG"
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record RefreshTokenRequestDto(
    string RefreshToken
);

// Response DTOs
public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
