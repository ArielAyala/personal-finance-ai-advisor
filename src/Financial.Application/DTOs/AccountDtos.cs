namespace Financial.Application.DTOs;

public record AccountDto(
    Guid Id,
    string Name,
    string Type,
    string Currency,
    string? AccountNumber,
    string? Institution,
    decimal CurrentBalance,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateAccountDto(
    string Name,
    string Type,
    string Currency,
    string? AccountNumber,
    string? Institution,
    decimal InitialBalance = 0
);

public record UpdateAccountDto(
    string? Name,
    string? AccountNumber,
    string? Institution,
    decimal? CurrentBalance,
    bool? IsActive
);
