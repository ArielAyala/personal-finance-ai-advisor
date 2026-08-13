using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFinancialSnapshotRepository _snapshotRepository;
    private readonly IBillRepository _billRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;

    public AnalyticsService(
        ITransactionRepository transactionRepository,
        IFinancialSnapshotRepository snapshotRepository,
        IBillRepository billRepository,
        IExchangeRateRepository exchangeRateRepository)
    {
        _transactionRepository = transactionRepository;
        _snapshotRepository = snapshotRepository;
        _billRepository = billRepository;
        _exchangeRateRepository = exchangeRateRepository;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        Guid userId,
        int year,
        int month,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var filter = new TransactionFilterDto(
            startDate,
            endDate,
            null, // accountId
            null, // categoryId
            null, // type
            1,    // pageNumber
            10000 // pageSize - get all
        );

        var transactions = await _transactionRepository.GetByUserIdAsync(userId, filter, cancellationToken);
        var transactionsList = transactions.ToList();

        // TODO: In future, implement currency conversion using ExchangeRateRepository
        // For now, filter by currency
        var currencyTransactions = transactionsList
            .Where(t => t.Currency == currency)
            .ToList();

        var income = currencyTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var expenses = Math.Abs(currencyTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount));

        var netSavings = income - expenses;
        var savingsRate = income > 0 ? (netSavings / income) * 100 : 0;

        // Group expenses by category
        var expensesByCategory = currencyTransactions
            .Where(t => t.Type == TransactionType.Expense && t.Category != null)
            .GroupBy(t => t.Category!.Name)
            .ToDictionary(
                g => g.Key,
                g => Math.Abs(g.Sum(t => t.Amount))
            );

        // Group income by category
        var incomeByCategory = currencyTransactions
            .Where(t => t.Type == TransactionType.Income && t.Category != null)
            .GroupBy(t => t.Category!.Name)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(t => t.Amount)
            );

        return new MonthlySummaryDto(
            year,
            month,
            income,
            expenses,
            netSavings,
            savingsRate,
            currency,
            currencyTransactions.Count,
            expensesByCategory,
            incomeByCategory
        );
    }

    public async Task<ExpenseTrendsResponseDto> GetExpenseTrendsAsync(
        Guid userId,
        MonthRangeDto range,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var monthlyTrends = new List<ExpenseTrendDto>();
        
        var currentDate = new DateTime(range.StartYear, range.StartMonth, 1);
        var endDate = new DateTime(range.EndYear, range.EndMonth, 1);

        while (currentDate <= endDate)
        {
            var monthStart = currentDate;
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            var filter = new TransactionFilterDto(
                monthStart,
                monthEnd,
                null, null, "Expense",
                1, 10000
            );

            var transactions = await _transactionRepository.GetByUserIdAsync(userId, filter, cancellationToken);
            var monthExpenses = transactions
                .Where(t => t.Currency == currency)
                .Sum(t => Math.Abs(t.Amount));

            var averageDaily = monthExpenses / daysInMonth;

            // Calculate percentage change from previous month
            var previousMonthExpenses = monthlyTrends.LastOrDefault()?.TotalAmount ?? 0;
            var percentageChange = previousMonthExpenses > 0
                ? ((monthExpenses - previousMonthExpenses) / previousMonthExpenses) * 100
                : 0;

            monthlyTrends.Add(new ExpenseTrendDto(
                currentDate.Year,
                currentDate.Month,
                monthExpenses,
                averageDaily,
                percentageChange,
                currency
            ));

            currentDate = currentDate.AddMonths(1);
        }

        var averageMonthly = monthlyTrends.Count > 0
            ? monthlyTrends.Average(t => t.TotalAmount)
            : 0;

        // Determine trend direction
        string trendDirection = "Stable";
        decimal trendPercentage = 0;

        if (monthlyTrends.Count >= 2)
        {
            var firstHalf = monthlyTrends.Take(monthlyTrends.Count / 2).Average(t => t.TotalAmount);
            var secondHalf = monthlyTrends.Skip(monthlyTrends.Count / 2).Average(t => t.TotalAmount);

            if (firstHalf > 0)
            {
                trendPercentage = ((secondHalf - firstHalf) / firstHalf) * 100;
                trendDirection = trendPercentage > 5 ? "Increasing" :
                                trendPercentage < -5 ? "Decreasing" : "Stable";
            }
        }

        return new ExpenseTrendsResponseDto(
            monthlyTrends,
            averageMonthly,
            trendDirection,
            trendPercentage,
            currency
        );
    }

    public async Task<List<CategoryTrendDto>> GetCategoryTrendsAsync(
        Guid userId,
        MonthRangeDto range,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(range.StartYear, range.StartMonth, 1);
        var endDate = new DateTime(range.EndYear, range.EndMonth, 1).AddMonths(1).AddDays(-1);

        var filter = new TransactionFilterDto(
            startDate,
            endDate,
            null, null, "Expense",
            1, 10000
        );

        var transactions = await _transactionRepository.GetByUserIdAsync(userId, filter, cancellationToken);
        var expenseTransactions = transactions
            .Where(t => t.Currency == currency && t.Category != null)
            .ToList();

        // Group by category
        var categoryGroups = expenseTransactions
            .GroupBy(t => t.Category!.Name)
            .ToList();

        var categoryTrends = new List<CategoryTrendDto>();

        foreach (var categoryGroup in categoryGroups)
        {
            var monthlyAmounts = new List<MonthlyAmountDto>();
            var currentDate = new DateTime(range.StartYear, range.StartMonth, 1);
            var rangeEndDate = new DateTime(range.EndYear, range.EndMonth, 1);

            while (currentDate <= rangeEndDate)
            {
                var monthTransactions = categoryGroup
                    .Where(t => t.Date.Year == currentDate.Year && t.Date.Month == currentDate.Month)
                    .Sum(t => Math.Abs(t.Amount));

                monthlyAmounts.Add(new MonthlyAmountDto(
                    currentDate.Year,
                    currentDate.Month,
                    monthTransactions
                ));

                currentDate = currentDate.AddMonths(1);
            }

            var totalAmount = monthlyAmounts.Sum(m => m.Amount);
            var averageMonthly = monthlyAmounts.Count > 0 ? totalAmount / monthlyAmounts.Count : 0;

            categoryTrends.Add(new CategoryTrendDto(
                categoryGroup.Key,
                monthlyAmounts,
                averageMonthly,
                totalAmount,
                currency
            ));
        }

        return categoryTrends.OrderByDescending(c => c.TotalAmount).ToList();
    }

    public async Task<SavingsRateResponseDto> GetSavingsRateAsync(
        Guid userId,
        MonthRangeDto range,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var monthlySavingsRates = new List<SavingsRateDto>();
        
        var currentDate = new DateTime(range.StartYear, range.StartMonth, 1);
        var endDate = new DateTime(range.EndYear, range.EndMonth, 1);

        while (currentDate <= endDate)
        {
            var monthStart = currentDate;
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var filter = new TransactionFilterDto(
                monthStart,
                monthEnd,
                null, null, null,
                1, 10000
            );

            var transactions = await _transactionRepository.GetByUserIdAsync(userId, filter, cancellationToken);
            var currencyTransactions = transactions.Where(t => t.Currency == currency).ToList();

            var income = currencyTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expenses = Math.Abs(currencyTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount));

            var netSavings = income - expenses;
            var savingsRate = income > 0 ? (netSavings / income) * 100 : 0;

            monthlySavingsRates.Add(new SavingsRateDto(
                currentDate.Year,
                currentDate.Month,
                income,
                expenses,
                netSavings,
                savingsRate,
                currency
            ));

            currentDate = currentDate.AddMonths(1);
        }

        var averageSavingsRate = monthlySavingsRates.Count > 0
            ? monthlySavingsRates.Average(s => s.SavingsRate)
            : 0;

        var bestMonth = monthlySavingsRates.Count > 0
            ? monthlySavingsRates.Max(s => s.SavingsRate)
            : 0;

        var worstMonth = monthlySavingsRates.Count > 0
            ? monthlySavingsRates.Min(s => s.SavingsRate)
            : 0;

        return new SavingsRateResponseDto(
            monthlySavingsRates,
            averageSavingsRate,
            bestMonth,
            worstMonth,
            currency
        );
    }

    public async Task<NetWorthEvolutionDto> GetNetWorthEvolutionAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var snapshots = await _snapshotRepository.GetByUserIdAsync(userId, cancellationToken);

        // Filter by date range and currency
        var filteredSnapshots = snapshots
            .Where(s => s.BaseCurrency == currency)
            .Where(s => !startDate.HasValue || s.Date >= startDate.Value)
            .Where(s => !endDate.HasValue || s.Date <= endDate.Value)
            .OrderBy(s => s.Date)
            .ToList();

        var dataPoints = new List<NetWorthPointDto>();
        decimal previousAmount = 0;

        foreach (var snapshot in filteredSnapshots)
        {
            var changeAmount = previousAmount > 0 ? snapshot.TotalAmount - previousAmount : 0;
            var changePercentage = previousAmount > 0 ? (changeAmount / previousAmount) * 100 : 0;

            dataPoints.Add(new NetWorthPointDto(
                snapshot.Date,
                snapshot.TotalAmount,
                changeAmount,
                changePercentage,
                snapshot.BaseCurrency,
                snapshot.Status
            ));

            previousAmount = snapshot.TotalAmount;
        }

        var currentNetWorth = dataPoints.LastOrDefault()?.TotalAmount ?? 0;
        var initialNetWorth = dataPoints.FirstOrDefault()?.TotalAmount ?? 0;
        var totalGrowth = currentNetWorth - initialNetWorth;
        var totalGrowthPercentage = initialNetWorth > 0 ? (totalGrowth / initialNetWorth) * 100 : 0;

        // Calculate average monthly growth
        var monthsCount = dataPoints.Count > 1 ? dataPoints.Count - 1 : 1;
        var averageMonthlyGrowth = totalGrowth / monthsCount;

        return new NetWorthEvolutionDto(
            dataPoints,
            currentNetWorth,
            initialNetWorth,
            totalGrowth,
            totalGrowthPercentage,
            averageMonthlyGrowth,
            currency
        );
    }

    public async Task<DebtAnalysisDto> GetDebtAnalysisAsync(
        Guid userId,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetByUserIdAsync(userId, cancellationToken);

        // Filter active bills with installments (debts) in the specified currency
        var debts = bills
            .Where(b => b.Status == BillStatus.Active && b.Currency == currency)
            .Where(b => b.Installments.HasValue && b.Installments.Value > 1)
            .ToList();

        var debtSummaries = new List<DebtSummaryDto>();
        decimal monthlyPayment = 0;

        foreach (var debt in debts)
        {
            var totalAmount = debt.Amount * (debt.Installments ?? 1);
            var paidAmount = debt.PaidAmount;
            var remainingAmount = totalAmount - paidAmount;
            var remainingInstallments = (debt.Installments ?? 0) - (debt.PaidInstallments ?? 0);

            debtSummaries.Add(new DebtSummaryDto(
                debt.Id,
                debt.Title,
                totalAmount,
                paidAmount,
                remainingAmount,
                debt.Installments,
                debt.PaidInstallments,
                remainingInstallments,
                debt.DueDate,
                debt.Periodicity.ToString(),
                debt.Status.ToString(),
                debt.Currency,
                debt.Category?.Name
            ));

            // Calculate monthly payment based on periodicity
            if (debt.Periodicity == BillPeriodicity.Monthly)
            {
                monthlyPayment += debt.Amount;
            }
            else if (debt.Periodicity == BillPeriodicity.Weekly)
            {
                monthlyPayment += debt.Amount * 4.33m; // Average weeks per month
            }
        }

        var totalDebt = debtSummaries.Sum(d => d.TotalAmount);
        var totalPaid = debtSummaries.Sum(d => d.PaidAmount);
        var totalRemaining = debtSummaries.Sum(d => d.RemainingAmount);

        return new DebtAnalysisDto(
            debtSummaries.OrderBy(d => d.DueDate).ToList(),
            totalDebt,
            totalPaid,
            totalRemaining,
            monthlyPayment,
            debtSummaries.Count,
            currency
        );
    }

    public async Task<UpcomingObligationsDto> GetUpcomingObligationsAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string currency = "PYG",
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow;
        var end = endDate ?? start.AddMonths(1);

        var bills = await _billRepository.GetByUserIdAsync(userId, cancellationToken);

        var upcomingBills = bills
            .Where(b => b.Status == BillStatus.Active && b.Currency == currency)
            .Where(b => b.DueDate >= start && b.DueDate <= end)
            .OrderBy(b => b.DueDate)
            .ToList();

        var obligations = upcomingBills.Select(b => new UpcomingObligationDto(
            b.Id,
            b.Title,
            b.Amount,
            b.DueDate,
            (b.DueDate - DateTime.UtcNow).Days,
            b.Periodicity.ToString(),
            b.Status.ToString(),
            b.Currency,
            b.Category?.Name
        )).ToList();

        var totalAmount = obligations.Sum(o => o.Amount);

        return new UpcomingObligationsDto(
            obligations,
            totalAmount,
            obligations.Count,
            currency
        );
    }
}
