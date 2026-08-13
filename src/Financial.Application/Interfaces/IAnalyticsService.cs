using Financial.Application.DTOs;

namespace Financial.Application.Interfaces;

public interface IAnalyticsService
{
    /// <summary>
    /// Gets a monthly summary of income, expenses, and savings
    /// </summary>
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        Guid userId, 
        int year, 
        int month, 
        string currency = "PYG",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expense trends over a period of months
    /// </summary>
    Task<ExpenseTrendsResponseDto> GetExpenseTrendsAsync(
        Guid userId,
        MonthRangeDto range,
        string currency = "PYG",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expense trends by category
    /// </summary>
    Task<List<CategoryTrendDto>> GetCategoryTrendsAsync(
        Guid userId,
        MonthRangeDto range,
        string currency = "PYG",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets savings rate over time
    /// </summary>
    Task<SavingsRateResponseDto> GetSavingsRateAsync(
        Guid userId,
        MonthRangeDto range,
        string currency = "PYG",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets net worth evolution from financial snapshots
    /// </summary>
    Task<NetWorthEvolutionDto> GetNetWorthEvolutionAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string currency = "PYG",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets debt analysis from active bills
    /// </summary>
    Task<DebtAnalysisDto> GetDebtAnalysisAsync(
        Guid userId,
        string currency = "PYG",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming financial obligations
    /// </summary>
    Task<UpcomingObligationsDto> GetUpcomingObligationsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string currency = "PYG",
        CancellationToken cancellationToken = default);
}
