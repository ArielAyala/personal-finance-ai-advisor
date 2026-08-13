namespace Financial.Application.DTOs;

public record FinancialSnapshotDto(
    Guid Id,
    DateTime Date,
    string BaseCurrency,
    decimal TotalAmount,
    DateTime? ExchangeRateDate,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    IEnumerable<SnapshotAccountDto> Accounts
);

public record FinancialSnapshotSummaryDto(
    Guid Id,
    DateTime Date,
    string BaseCurrency,
    decimal TotalAmount,
    string Status,
    int AccountCount,
    DateTime CreatedAt
);

public record CreateFinancialSnapshotDto(
    DateTime Date,
    string BaseCurrency = "PYG",
    DateTime? ExchangeRateDate = null,
    string? Notes = null
);

public record UpdateFinancialSnapshotDto(
    DateTime? Date,
    string? BaseCurrency,
    DateTime? ExchangeRateDate,
    string? Notes
);

public record CloseSnapshotDto(
    DateTime? ClosedAt
);

public record SnapshotGrowthDto(
    Guid CurrentSnapshotId,
    DateTime CurrentDate,
    decimal CurrentAmount,
    Guid? PreviousSnapshotId,
    DateTime? PreviousDate,
    decimal? PreviousAmount,
    decimal? AbsoluteGrowth,
    decimal? PercentageGrowth
);
