namespace Financial.Application.DTOs;

public record SnapshotAccountDto(
    Guid Id,
    Guid SnapshotId,
    Guid AccountId,
    string AccountName,
    string? AccountNumberMasked,
    decimal OriginalAmount,
    string OriginalCurrency,
    decimal? ExchangeRate,
    decimal ConvertedAmount,
    string BaseCurrency
);

public record CreateSnapshotAccountDto(
    Guid AccountId,
    decimal OriginalAmount,
    string OriginalCurrency,
    decimal? ExchangeRate = null
);

public record UpdateSnapshotAccountDto(
    decimal? OriginalAmount,
    string? OriginalCurrency,
    decimal? ExchangeRate
);
