using System.ComponentModel;
using System.Text.Json;
using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.SemanticKernel;

namespace Financial.Application.AI.Plugins;

/// <summary>
/// Financial data tools for the AI agent to query transactions, accounts, and analytics
/// </summary>
public class FinancialDataPlugin
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAnalyticsService _analyticsService;
    private readonly IFinancialSnapshotRepository _snapshotRepository;
    private readonly IBillRepository _billRepository;

    public FinancialDataPlugin(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAnalyticsService analyticsService,
        IFinancialSnapshotRepository snapshotRepository,
        IBillRepository billRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _analyticsService = analyticsService;
        _snapshotRepository = snapshotRepository;
        _billRepository = billRepository;
    }

    [KernelFunction("get_monthly_summary")]
    [Description("Get income, expenses, and savings summary for a specific month")]
    public async Task<string> GetMonthlySummaryAsync(
        [Description("User ID")] Guid userId,
        [Description("Year (e.g., 2026)")] int year,
        [Description("Month (1-12)")] int month,
        [Description("Currency code (e.g., PYG, USD)")] string currency = "PYG")
    {
        try
        {
            var summary = await _analyticsService.GetMonthlySummaryAsync(userId, year, month, currency);
            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting monthly summary: {ex.Message}";
        }
    }

    [KernelFunction("get_recent_transactions")]
    [Description("Get the most recent transactions for the user")]
    public async Task<string> GetRecentTransactionsAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of transactions to retrieve (default 10)")] int limit = 10)
    {
        try
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            var recent = transactions
                .OrderByDescending(t => t.Date)
                .Take(limit)
                .Select(t => new
                {
                    t.Date,
                    t.Amount,
                    t.Currency,
                    t.Merchant,
                    Category = t.Category?.Name,
                    Account = t.Account?.Name
                })
                .ToList();

            return JsonSerializer.Serialize(recent, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting recent transactions: {ex.Message}";
        }
    }

    [KernelFunction("get_expense_trends")]
    [Description("Get expense trends over a period of time")]
    public async Task<string> GetExpenseTrendsAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of months to analyze")] int months = 3,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            var range = new MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);
            var trends = await _analyticsService.GetExpenseTrendsAsync(userId, range, currency);
            return JsonSerializer.Serialize(trends, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting expense trends: {ex.Message}";
        }
    }

    [KernelFunction("get_category_breakdown")]
    [Description("Get spending breakdown by category over a period")]
    public async Task<string> GetCategoryBreakdownAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of months to analyze")] int months = 1,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            var range = new MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);
            var breakdown = await _analyticsService.GetCategoryTrendsAsync(userId, range, currency);
            return JsonSerializer.Serialize(breakdown, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting category breakdown: {ex.Message}";
        }
    }

    [KernelFunction("get_net_worth")]
    [Description("Get current net worth and historical evolution")]
    public async Task<string> GetNetWorthAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of months to look back (0 for all)")] int months = 0)
    {
        try
        {
            DateTime? startDate = months > 0 ? DateTime.Today.AddMonths(-months) : null;
            var netWorth = await _analyticsService.GetNetWorthEvolutionAsync(userId, startDate, null);
            return JsonSerializer.Serialize(netWorth, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting net worth: {ex.Message}";
        }
    }

    [KernelFunction("get_active_bills")]
    [Description("Get all active bills and upcoming obligations")]
    public async Task<string> GetActiveBillsAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of days to look ahead (default 30)")] int daysAhead = 30)
    {
        try
        {
            var endDate = DateTime.Today.AddDays(daysAhead);
            var obligations = await _analyticsService.GetUpcomingObligationsAsync(userId, DateTime.Today, endDate);
            return JsonSerializer.Serialize(obligations, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting active bills: {ex.Message}";
        }
    }

    [KernelFunction("get_debt_analysis")]
    [Description("Get comprehensive debt and installment analysis")]
    public async Task<string> GetDebtAnalysisAsync(
        [Description("User ID")] Guid userId,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var debtAnalysis = await _analyticsService.GetDebtAnalysisAsync(userId, currency);
            return JsonSerializer.Serialize(debtAnalysis, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting debt analysis: {ex.Message}";
        }
    }

    [KernelFunction("get_savings_rate")]
    [Description("Get savings rate over time")]
    public async Task<string> GetSavingsRateAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of months to analyze")] int months = 6,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            var range = new MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);
            var savingsRate = await _analyticsService.GetSavingsRateAsync(userId, range, currency);
            return JsonSerializer.Serialize(savingsRate, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting savings rate: {ex.Message}";
        }
    }

    [KernelFunction("get_accounts_summary")]
    [Description("Get summary of all user accounts with current balances")]
    public async Task<string> GetAccountsSummaryAsync(
        [Description("User ID")] Guid userId)
    {
        try
        {
            var accounts = await _accountRepository.GetByUserIdAsync(userId);
            var summary = accounts.Select(a => new
            {
                a.Name,
                a.Type,
                a.Currency,
                Balance = a.CurrentBalance,
                a.IsActive,
                MaskedNumber = a.AccountNumber
            }).ToList();

            return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting accounts summary: {ex.Message}";
        }
    }
}
