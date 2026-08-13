namespace Financial.Application.DTOs;

public record ExchangeRateDto(
    Guid Id,
    string SourceCurrency,
    string TargetCurrency,
    decimal Rate,
    DateTime Date,
    string? Source,
    string? Notes,
    DateTime CreatedAt
);

public record CreateExchangeRateDto(
    string SourceCurrency,
    string TargetCurrency,
    decimal Rate,
    DateTime Date,
    string? Source = null,
    string? Notes = null
);

public record UpdateExchangeRateDto(
    decimal? Rate,
    string? Source,
    string? Notes
);

public record ExchangeRateQueryDto(
    string SourceCurrency,
    string TargetCurrency,
    DateTime Date
);
