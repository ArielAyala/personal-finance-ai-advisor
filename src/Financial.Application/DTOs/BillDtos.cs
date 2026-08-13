namespace Financial.Application.DTOs;

public record BillDto(
    Guid Id,
    string Title,
    decimal Amount,
    string Currency,
    decimal PaidAmount,
    DateTime DueDate,
    string Periodicity,
    int? Installments,
    int? PaidInstallments,
    string Status,
    Guid? CategoryId,
    Guid? AccountId,
    string? Notes,
    DateTime CreatedAt
);

public record CreateBillDto(
    string Title,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    string Periodicity,
    int? Installments = null,
    Guid? CategoryId = null,
    Guid? AccountId = null,
    string? Notes = null
);

public record UpdateBillDto(
    string? Title,
    decimal? Amount,
    string? Currency,
    decimal? PaidAmount,
    DateTime? DueDate,
    string? Periodicity,
    int? Installments,
    int? PaidInstallments,
    string? Status,
    Guid? CategoryId,
    Guid? AccountId,
    string? Notes
);

public record PayBillDto(
    decimal PaymentAmount,
    DateTime PaymentDate
);
