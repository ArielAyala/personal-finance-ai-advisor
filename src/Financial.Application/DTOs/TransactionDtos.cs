namespace Financial.Application.DTOs;

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    Guid? CategoryId,
    string? CategoryName,
    DateTime Date,
    decimal Amount,
    string Currency,
    string? Merchant,
    string? Description,
    string? Notes,
    string Type,
    string? Reference,
    DateTime CreatedAt
);

public record CreateTransactionDto(
    Guid AccountId,
    Guid? CategoryId,
    DateTime Date,
    decimal Amount,
    string Currency,
    string? Merchant,
    string? Description,
    string? Notes,
    string Type
);

public record UpdateTransactionDto(
    Guid? CategoryId,
    DateTime? Date,
    decimal? Amount,
    string? Merchant,
    string? Description,
    string? Notes
);

public record TransactionFilterDto(
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? AccountId,
    Guid? CategoryId,
    string? Type,
    int PageNumber = 1,
    int PageSize = 50
);
