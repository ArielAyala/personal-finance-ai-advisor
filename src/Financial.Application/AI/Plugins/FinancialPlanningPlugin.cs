using System.ComponentModel;
using System.Text.Json;
using Financial.Application.Interfaces;
using Microsoft.SemanticKernel;

namespace Financial.Application.AI.Plugins;

/// <summary>
/// Financial planning and goal tracking tools for the AI agent
/// </summary>
public class FinancialPlanningPlugin
{
    private readonly IGoalRepository _goalRepository;
    private readonly IAnalyticsService _analyticsService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBillRepository _billRepository;

    public FinancialPlanningPlugin(
        IGoalRepository goalRepository,
        IAnalyticsService analyticsService,
        ITransactionRepository transactionRepository,
        IBillRepository billRepository)
    {
        _goalRepository = goalRepository;
        _analyticsService = analyticsService;
        _transactionRepository = transactionRepository;
        _billRepository = billRepository;
    }

    [KernelFunction("get_financial_goals")]
    [Description("Get user's financial goals with progress tracking")]
    public async Task<string> GetFinancialGoalsAsync(
        [Description("User ID")] Guid userId,
        [Description("Filter by status: Active, Completed, Cancelled, OnHold, or null for all")] string? status = null)
    {
        try
        {
            IEnumerable<Domain.Entities.Goal> goals;
            
            if (string.IsNullOrEmpty(status))
            {
                goals = await _goalRepository.GetByUserIdAsync(userId);
            }
            else if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                goals = await _goalRepository.GetActiveGoalsByUserIdAsync(userId);
            }
            else
            {
                goals = await _goalRepository.GetByUserIdAsync(userId);
                var statusEnum = Enum.Parse<Domain.Entities.GoalStatus>(status, true);
                goals = goals.Where(g => g.Status == statusEnum);
            }

            var goalsWithProgress = goals.Select(g => new
            {
                g.Id,
                g.Title,
                g.Description,
                g.TargetAmount,
                g.CurrentAmount,
                g.Currency,
                RemainingAmount = Math.Max(0, g.TargetAmount - g.CurrentAmount),
                ProgressPercentage = g.TargetAmount > 0 
                    ? Math.Round((g.CurrentAmount / g.TargetAmount) * 100, 2) 
                    : 0,
                g.TargetDate,
                DaysRemaining = g.TargetDate.HasValue 
                    ? (int)(g.TargetDate.Value - DateTime.UtcNow).TotalDays 
                    : (int?)null,
                Status = g.Status.ToString(),
                g.CreatedAt,
                g.CompletedAt
            }).ToList();

            return JsonSerializer.Serialize(goalsWithProgress, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error getting financial goals: {ex.Message}";
        }
    }

    [KernelFunction("calculate_affordability")]
    [Description("Calculate if user can afford a purchase based on income, expenses, bills, and goals")]
    public async Task<string> CalculateAffordabilityAsync(
        [Description("User ID")] Guid userId,
        [Description("Purchase amount")] decimal amount,
        [Description("Currency code")] string currency = "PYG",
        [Description("Number of months to analyze")] int months = 3)
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            
            // Get average monthly income and expenses
            var monthlyData = new List<decimal>();
            for (int i = 0; i < months; i++)
            {
                var date = endDate.AddMonths(-i);
                var summary = await _analyticsService.GetMonthlySummaryAsync(userId, date.Year, date.Month, currency);
                monthlyData.Add(summary.NetSavings);
            }

            var avgMonthlySavings = monthlyData.Average();

            // Get upcoming bills for next month
            var nextMonth = DateTime.Today.AddMonths(1);
            var bills = await _billRepository.GetUpcomingByUserIdAsync(userId, DateTime.Today, nextMonth);
            var upcomingBills = bills.Where(b => b.Currency == currency).Sum(b => b.Amount - b.PaidAmount);

            // Get active goals
            var goals = await _goalRepository.GetActiveGoalsByUserIdAsync(userId);
            var goalCommitment = goals.Where(g => g.Currency == currency)
                .Sum(g => Math.Max(0, g.TargetAmount - g.CurrentAmount));

            var result = new
            {
                PurchaseAmount = amount,
                Currency = currency,
                Analysis = new
                {
                    AverageMonthlySavings = Math.Round(avgMonthlySavings, 2),
                    UpcomingBills = Math.Round(upcomingBills, 2),
                    ActiveGoalsRemaining = Math.Round(goalCommitment, 2),
                    AvailableCapacity = Math.Round(avgMonthlySavings - upcomingBills, 2),
                    PurchaseToSavingsRatio = avgMonthlySavings > 0 
                        ? Math.Round((amount / avgMonthlySavings) * 100, 2) 
                        : 0
                },
                Recommendation = new
                {
                    CanAfford = avgMonthlySavings > upcomingBills && amount <= (avgMonthlySavings - upcomingBills),
                    ImpactOnGoals = goalCommitment > 0 && amount > (avgMonthlySavings - upcomingBills) * 0.5m,
                    MonthsToSave = avgMonthlySavings > upcomingBills 
                        ? Math.Ceiling(amount / (avgMonthlySavings - upcomingBills))
                        : (decimal?)null
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error calculating affordability: {ex.Message}";
        }
    }

    [KernelFunction("recommend_savings_allocation")]
    [Description("Recommend how to allocate savings across goals based on priorities and deadlines")]
    public async Task<string> RecommendSavingsAllocationAsync(
        [Description("User ID")] Guid userId,
        [Description("Available amount to allocate")] decimal availableAmount,
        [Description("Currency code")] string currency = "PYG")
    {
        try
        {
            var goals = await _goalRepository.GetActiveGoalsByUserIdAsync(userId);
            var relevantGoals = goals.Where(g => g.Currency == currency).ToList();

            if (!relevantGoals.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    Message = "No active goals found to allocate savings",
                    AvailableAmount = availableAmount,
                    Currency = currency
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            // Calculate priority scores based on:
            // 1. How close they are to target date
            // 2. How close they are to completion
            // 3. Emergency goals (no target date) get lower priority
            var allocations = relevantGoals.Select(g =>
            {
                var remaining = Math.Max(0, g.TargetAmount - g.CurrentAmount);
                var progress = g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) : 0;
                
                // Calculate urgency score
                var urgencyScore = 1.0;
                if (g.TargetDate.HasValue)
                {
                    var daysRemaining = (g.TargetDate.Value - DateTime.UtcNow).TotalDays;
                    if (daysRemaining <= 0)
                        urgencyScore = 10.0; // Overdue
                    else if (daysRemaining <= 30)
                        urgencyScore = 5.0; // Very urgent
                    else if (daysRemaining <= 90)
                        urgencyScore = 3.0; // Urgent
                    else if (daysRemaining <= 180)
                        urgencyScore = 2.0; // Moderate
                }

                var priorityScore = urgencyScore * (1.0 + (double)progress);

                return new
                {
                    Goal = g.Title,
                    GoalId = g.Id,
                    TargetAmount = g.TargetAmount,
                    CurrentAmount = g.CurrentAmount,
                    Remaining = remaining,
                    Progress = Math.Round(progress * 100, 2),
                    TargetDate = g.TargetDate,
                    PriorityScore = Math.Round(priorityScore, 2)
                };
            }).OrderByDescending(x => x.PriorityScore).ToList();

            // Allocate based on priority
            var recommendations = new List<object>();
            var remainingAmount = availableAmount;

            foreach (var allocation in allocations)
            {
                if (remainingAmount <= 0) break;

                var allocatedAmount = Math.Min(remainingAmount, allocation.Remaining);
                remainingAmount -= allocatedAmount;

                recommendations.Add(new
                {
                    allocation.Goal,
                    allocation.GoalId,
                    AllocatedAmount = Math.Round(allocatedAmount, 2),
                    allocation.Remaining,
                    WillComplete = allocatedAmount >= allocation.Remaining,
                    allocation.PriorityScore
                });
            }

            var result = new
            {
                TotalAvailable = availableAmount,
                Currency = currency,
                TotalAllocated = Math.Round(availableAmount - remainingAmount, 2),
                Unallocated = Math.Round(remainingAmount, 2),
                Recommendations = recommendations
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error recommending savings allocation: {ex.Message}";
        }
    }

    [KernelFunction("analyze_goal_feasibility")]
    [Description("Analyze if a goal is feasible given current savings rate")]
    public async Task<string> AnalyzeGoalFeasibilityAsync(
        [Description("User ID")] Guid userId,
        [Description("Goal ID to analyze")] Guid goalId,
        [Description("Number of months to use for savings rate calculation")] int months = 6)
    {
        try
        {
            var goal = await _goalRepository.GetByIdAsync(goalId);
            
            if (goal == null || goal.UserId != userId)
            {
                return JsonSerializer.Serialize(new { Error = "Goal not found or access denied" });
            }

            // Calculate average savings rate
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);
            var range = new DTOs.MonthRangeDto(startDate.Year, startDate.Month, endDate.Year, endDate.Month);
            var savingsRate = await _analyticsService.GetSavingsRateAsync(userId, range, goal.Currency);

            var avgMonthlySavings = savingsRate.MonthlySavingsRates.Any() 
                ? savingsRate.MonthlySavingsRates.Average(m => m.NetSavings) 
                : 0;

            var remaining = Math.Max(0, goal.TargetAmount - goal.CurrentAmount);
            var monthsNeeded = avgMonthlySavings > 0 
                ? Math.Ceiling(remaining / avgMonthlySavings) 
                : (decimal?)null;

            var projectedCompletionDate = monthsNeeded.HasValue 
                ? DateTime.Today.AddMonths((int)monthsNeeded.Value) 
                : (DateTime?)null;

            var isFeasible = goal.TargetDate.HasValue && projectedCompletionDate.HasValue
                ? projectedCompletionDate <= goal.TargetDate.Value
                : avgMonthlySavings > 0;

            var result = new
            {
                Goal = new
                {
                    goal.Title,
                    goal.TargetAmount,
                    goal.CurrentAmount,
                    Remaining = remaining,
                    goal.Currency,
                    goal.TargetDate
                },
                Analysis = new
                {
                    AverageMonthlySavings = Math.Round(avgMonthlySavings, 2),
                    MonthsNeeded = monthsNeeded,
                    ProjectedCompletionDate = projectedCompletionDate,
                    IsFeasible = isFeasible,
                    RequiredMonthlySaving = goal.TargetDate.HasValue
                        ? Math.Round(remaining / Math.Max(1, (decimal)(goal.TargetDate.Value - DateTime.Today).TotalDays / 30), 2)
                        : (decimal?)null
                },
                Recommendations = isFeasible 
                    ? "Goal is feasible at current savings rate" 
                    : "Consider adjusting target date or increasing savings rate"
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error analyzing goal feasibility: {ex.Message}";
        }
    }
}
