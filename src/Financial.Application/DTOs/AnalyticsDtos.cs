using Financial.Domain.Entities;

namespace Financial.Application.DTOs;

// Monthly Summary DTOs
public record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal SavingsRate,
    string Currency,
    int TransactionCount,
    Dictionary<string, decimal> ExpensesByCategory,
    Dictionary<string, decimal> IncomeByCategory
);

// Expense Trends DTOs
public record ExpenseTrendDto(
    int Year,
    int Month,
    decimal TotalAmount,
    decimal AverageDaily,
    decimal PercentageChange,
    string Currency
);

public record ExpenseTrendsResponseDto(
    List<ExpenseTrendDto> MonthlyTrends,
    decimal AverageMonthlyExpense,
    string TrendDirection, // "Increasing", "Decreasing", "Stable"
    decimal TrendPercentage,
    string Currency
);

public record CategoryTrendDto(
    string CategoryName,
    List<MonthlyAmountDto> MonthlyAmounts,
    decimal AverageMonthly,
    decimal TotalAmount,
    string Currency
);

public record MonthlyAmountDto(
    int Year,
    int Month,
    decimal Amount
);

// Savings Rate DTOs
public record SavingsRateDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal SavingsRate,
    string Currency
);

public record SavingsRateResponseDto(
    List<SavingsRateDto> MonthlySavingsRates,
    decimal AverageSavingsRate,
    decimal BestMonth,
    decimal WorstMonth,
    string Currency
);

// Net Worth Evolution DTOs
public record NetWorthPointDto(
    DateTime Date,
    decimal TotalAmount,
    decimal ChangeAmount,
    decimal ChangePercentage,
    string Currency,
    SnapshotStatus Status
);

public record NetWorthEvolutionDto(
    List<NetWorthPointDto> DataPoints,
    decimal CurrentNetWorth,
    decimal InitialNetWorth,
    decimal TotalGrowth,
    decimal TotalGrowthPercentage,
    decimal AverageMonthlyGrowth,
    string Currency
);

// Debt Analysis DTOs
public record DebtSummaryDto(
    Guid BillId,
    string Title,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    int? TotalInstallments,
    int? PaidInstallments,
    int? RemainingInstallments,
    DateTime DueDate,
    string Periodicity,
    string Status,
    string Currency,
    string? CategoryName
);

public record DebtAnalysisDto(
    List<DebtSummaryDto> ActiveDebts,
    decimal TotalDebt,
    decimal TotalPaid,
    decimal TotalRemaining,
    decimal MonthlyDebtPayment,
    int ActiveDebtCount,
    string Currency
);

// Upcoming Obligations DTOs
public record UpcomingObligationDto(
    Guid BillId,
    string Title,
    decimal Amount,
    DateTime DueDate,
    int DaysUntilDue,
    string Periodicity,
    string Status,
    string Currency,
    string? CategoryName
);

public record UpcomingObligationsDto(
    List<UpcomingObligationDto> Obligations,
    decimal TotalAmount,
    int Count,
    string Currency
);

// Analytics Filter DTOs
public record AnalyticsFilterDto(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Currency,
    Guid? AccountId,
    Guid? CategoryId
);

public record MonthRangeDto(
    int StartYear,
    int StartMonth,
    int EndYear,
    int EndMonth
);
