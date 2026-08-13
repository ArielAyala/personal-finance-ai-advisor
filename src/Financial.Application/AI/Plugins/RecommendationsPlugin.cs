using System.ComponentModel;
using System.Text.Json;
using Financial.Application.Interfaces;
using Microsoft.SemanticKernel;

namespace Financial.Application.AI.Plugins;

/// <summary>
/// Advanced AI recommendations and insights plugin
/// </summary>
public class RecommendationsPlugin
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBillRepository _billRepository;
    private readonly IGoalRepository _goalRepository;

    public RecommendationsPlugin(
        IAnalyticsService analyticsService,
        ITransactionRepository transactionRepository,
        IBillRepository billRepository,
        IGoalRepository goalRepository)
    {
        _analyticsService = analyticsService;
        _transactionRepository = transactionRepository;
        _billRepository = billRepository;
        _goalRepository = goalRepository;
    }

    [KernelFunction("analyze_spending_health")]
    [Description("Analyze overall financial health and identify areas of concern or improvement")]
    public async Task<string> AnalyzeSpendingHealthAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of months to analyze")] int months = 3,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            var range = new DTOs.MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);

            // Get analytics data
            var savingsRate = await _analyticsService.GetSavingsRateAsync(userId, range, currency);
            var expenseTrends = await _analyticsService.GetExpenseTrendsAsync(userId, range, currency);
            var debtAnalysis = await _analyticsService.GetDebtAnalysisAsync(userId, currency);

            // Calculate health metrics
            var avgSavingsRate = savingsRate.AverageSavingsRate;
            var isIncreasingExpenses = expenseTrends.TrendDirection == "Increasing";
            var hasHighDebt = debtAnalysis.TotalRemaining > 0;

            // Generate insights
            var insights = new List<string>();
            var concerns = new List<string>();
            var recommendations = new List<string>();

            // Analyze savings rate
            if (avgSavingsRate >= 20)
            {
                insights.Add($"Excellent savings rate of {avgSavingsRate:F2}%");
            }
            else if (avgSavingsRate >= 10)
            {
                insights.Add($"Good savings rate of {avgSavingsRate:F2}%");
                recommendations.Add("Try to increase savings to 20% or more");
            }
            else if (avgSavingsRate > 0)
            {
                concerns.Add($"Low savings rate of {avgSavingsRate:F2}%");
                recommendations.Add("Focus on increasing income or reducing expenses");
            }
            else
            {
                concerns.Add("Negative savings - spending more than earning");
                recommendations.Add("Urgent: Review and cut non-essential expenses");
            }

            // Analyze expense trends
            if (isIncreasingExpenses)
            {
                concerns.Add($"Expenses are increasing by {expenseTrends.TrendPercentage:F2}%");
                recommendations.Add("Identify and control expense categories with highest growth");
            }
            else if (expenseTrends.TrendDirection == "Decreasing")
            {
                insights.Add($"Expenses are decreasing by {Math.Abs(expenseTrends.TrendPercentage):F2}%");
            }

            // Analyze debt
            if (hasHighDebt)
            {
                var debtToIncomeRatio = savingsRate.MonthlySavingsRates.Any()
                    ? debtAnalysis.TotalRemaining / Math.Max(1, savingsRate.MonthlySavingsRates.Average(m => m.TotalIncome))
                    : 0;

                if (debtToIncomeRatio > 0.5m)
                {
                    concerns.Add($"High debt burden: {debtAnalysis.TotalRemaining:N0} {currency}");
                    recommendations.Add("Prioritize debt repayment to reduce financial stress");
                }
                else
                {
                    insights.Add($"Manageable debt level: {debtAnalysis.TotalRemaining:N0} {currency}");
                }
            }

            var result = new
            {
                HealthScore = CalculateHealthScore(avgSavingsRate, isIncreasingExpenses, hasHighDebt, debtAnalysis.TotalRemaining),
                Currency = currency,
                AnalysisPeriod = $"{months} months",
                Metrics = new
                {
                    AverageSavingsRate = Math.Round(avgSavingsRate, 2),
                    ExpenseTrend = expenseTrends.TrendDirection,
                    TotalDebt = Math.Round(debtAnalysis.TotalRemaining, 2)
                },
                Insights = insights,
                Concerns = concerns,
                Recommendations = recommendations
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error analyzing spending health: {ex.Message}";
        }
    }

    [KernelFunction("identify_savings_opportunities")]
    [Description("Identify specific opportunities to reduce expenses and increase savings")]
    public async Task<string> IdentifySavingsOpportunitiesAsync(
        [Description("User ID")] Guid userId,
        [Description("Number of months to analyze")] int months = 3,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            var range = new DTOs.MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);

            // Get category trends
            var categoryTrends = await _analyticsService.GetCategoryTrendsAsync(userId, range, currency);

            var opportunities = new List<object>();

            foreach (var category in categoryTrends)
            {
                var avgMonthly = category.AverageMonthly;
                
                // Identify high-spending categories
                if (avgMonthly > 0)
                {
                    // Calculate potential savings (conservative 10-20% reduction)
                    var potentialSavings = avgMonthly * 0.15m;

                    // Check if spending is increasing
                    var recentAmount = category.MonthlyAmounts.OrderByDescending(m => m.Year).ThenByDescending(m => m.Month).FirstOrDefault()?.Amount ?? 0;
                    var olderAmount = category.MonthlyAmounts.OrderBy(m => m.Year).ThenBy(m => m.Month).FirstOrDefault()?.Amount ?? 0;
                    var isIncreasing = recentAmount > olderAmount * 1.1m;

                    opportunities.Add(new
                    {
                        Category = category.CategoryName,
                        AverageMonthlySpending = Math.Round(avgMonthly, 2),
                        PotentialMonthlySavings = Math.Round(potentialSavings, 2),
                        IsIncreasing = isIncreasing,
                        Priority = isIncreasing ? "High" : avgMonthly > 1000000 ? "Medium" : "Low",
                        Suggestion = GenerateCategorySuggestion(category.CategoryName, avgMonthly, isIncreasing)
                    });
                }
            }

            // Sort by potential savings
            var sortedOpportunities = opportunities
                .OrderByDescending(o => ((dynamic)o).PotentialMonthlySavings)
                .Take(5)
                .ToList();

            var totalPotentialSavings = sortedOpportunities.Sum(o => ((dynamic)o).PotentialMonthlySavings);

            var result = new
            {
                Currency = currency,
                AnalysisPeriod = $"{months} months",
                TotalPotentialMonthlySavings = Math.Round((decimal)totalPotentialSavings, 2),
                TotalPotentialYearlySavings = Math.Round((decimal)totalPotentialSavings * 12m, 2),
                TopOpportunities = sortedOpportunities
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error identifying savings opportunities: {ex.Message}";
        }
    }

    [KernelFunction("generate_financial_action_plan")]
    [Description("Generate a personalized action plan based on financial situation and goals")]
    public async Task<string> GenerateFinancialActionPlanAsync(
        [Description("User ID")] Guid userId,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-3);
            var range = new DTOs.MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);

            // Gather data
            var savingsRate = await _analyticsService.GetSavingsRateAsync(userId, range, currency);
            var goals = await _goalRepository.GetActiveGoalsByUserIdAsync(userId);
            var relevantGoals = goals.Where(g => g.Currency == currency).ToList();
            var bills = await _billRepository.GetActiveByUserIdAsync(userId);

            var avgMonthlySavings = savingsRate.MonthlySavingsRates.Any()
                ? savingsRate.MonthlySavingsRates.Average(m => m.NetSavings)
                : 0;

            // Build action plan
            var actionPlan = new List<object>();
            var priority = 1;

            // 1. Emergency fund check
            var hasEmergencyGoal = relevantGoals.Any(g => 
                g.Title.Contains("emergency", StringComparison.OrdinalIgnoreCase) ||
                g.Title.Contains("fondo", StringComparison.OrdinalIgnoreCase));

            if (!hasEmergencyGoal)
            {
                actionPlan.Add(new
                {
                    Priority = priority++,
                    Action = "Create Emergency Fund",
                    Description = "Set up an emergency fund goal for 3-6 months of expenses",
                    Timeline = "Immediate",
                    Impact = "High"
                });
            }

            // 2. Debt management
            if (bills.Any())
            {
                var highInterestBills = bills.Where(b => b.Installments.HasValue && b.Installments > 6).ToList();
                if (highInterestBills.Any())
                {
                    actionPlan.Add(new
                    {
                        Priority = priority++,
                        Action = "Accelerate Debt Repayment",
                        Description = $"Focus on paying off {highInterestBills.Count} installment-based bills",
                        Timeline = "3-6 months",
                        Impact = "High"
                    });
                }
            }

            // 3. Savings improvement
            if (savingsRate.AverageSavingsRate < 15)
            {
                actionPlan.Add(new
                {
                    Priority = priority++,
                    Action = "Increase Savings Rate",
                    Description = $"Work towards 15-20% savings rate (currently {savingsRate.AverageSavingsRate:F2}%)",
                    Timeline = "3-6 months",
                    Impact = "High"
                });
            }

            // 4. Goal progress
            foreach (var goal in relevantGoals.Take(3))
            {
                var progress = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
                if (progress < 50 && goal.TargetDate.HasValue && goal.TargetDate.Value > DateTime.Today)
                {
                    var monthsRemaining = Math.Max(1, (goal.TargetDate.Value - DateTime.Today).Days / 30);
                    var requiredMonthly = (goal.TargetAmount - goal.CurrentAmount) / monthsRemaining;

                    actionPlan.Add(new
                    {
                        Priority = priority++,
                        Action = $"Progress on: {goal.Title}",
                        Description = $"Allocate {requiredMonthly:N0} {currency}/month to stay on track",
                        Timeline = $"{monthsRemaining} months remaining",
                        Impact = "Medium"
                    });
                }
            }

            var result = new
            {
                Currency = currency,
                CurrentSituation = new
                {
                    AverageMonthlySavings = Math.Round(avgMonthlySavings, 2),
                    SavingsRate = Math.Round(savingsRate.AverageSavingsRate, 2),
                    ActiveGoals = relevantGoals.Count,
                    ActiveBills = bills.Count()
                },
                ActionPlan = actionPlan,
                EstimatedTimeframe = "3-12 months",
                ReviewRecommendation = "Review progress monthly and adjust plan as needed"
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error generating action plan: {ex.Message}";
        }
    }

    private static int CalculateHealthScore(decimal savingsRate, bool increasingExpenses, bool hasDebt, decimal debtAmount)
    {
        var score = 50; // Base score

        // Savings rate contribution (0-30 points)
        if (savingsRate >= 20) score += 30;
        else if (savingsRate >= 15) score += 25;
        else if (savingsRate >= 10) score += 15;
        else if (savingsRate >= 5) score += 5;
        else if (savingsRate < 0) score -= 20;

        // Expense trend contribution (0-20 points)
        if (increasingExpenses) score -= 15;
        else score += 10;

        // Debt contribution (0-20 points)
        if (!hasDebt)
            score += 20;
        else if (debtAmount < 1000000)
            score += 10;
        else if (debtAmount > 5000000)
            score -= 10;

        return Math.Clamp(score, 0, 100);
    }

    private static string GenerateCategorySuggestion(string categoryName, decimal avgMonthly, bool isIncreasing)
    {
        var category = categoryName.ToLower();
        
        if (category.Contains("comida") || category.Contains("food") || category.Contains("restaurant"))
            return "Consider meal planning and cooking at home more often";
        
        if (category.Contains("transporte") || category.Contains("transport") || category.Contains("uber"))
            return "Explore public transportation or carpooling options";
        
        if (category.Contains("entretenimiento") || category.Contains("entertainment") || category.Contains("streaming"))
            return "Review subscription services and keep only essential ones";
        
        if (category.Contains("compras") || category.Contains("shopping"))
            return "Implement a 24-hour rule before non-essential purchases";
        
        if (isIncreasing)
            return $"This category is increasing - review recent transactions for patterns";
        
        return "Look for ways to reduce spending in this category by 10-15%";
    }
}
