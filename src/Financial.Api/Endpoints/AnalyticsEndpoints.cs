using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Financial.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics");

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Monthly Summary
        group.MapGet("/monthly-summary", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var summary = await service.GetMonthlySummaryAsync(devUserId, year, month, currency);
                return Results.Ok(summary);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetMonthlySummary")
        .WithOpenApi()
        .WithDescription("Get monthly income, expenses, and savings summary with category breakdown");

        // Expense Trends
        group.MapGet("/expense-trends", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var range = new MonthRangeDto(startYear, startMonth, endYear, endMonth);
                var trends = await service.GetExpenseTrendsAsync(devUserId, range, currency);
                return Results.Ok(trends);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetExpenseTrends")
        .WithOpenApi()
        .WithDescription("Get expense trends over a period with monthly breakdown and trend analysis");

        // Category Trends
        group.MapGet("/category-trends", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var range = new MonthRangeDto(startYear, startMonth, endYear, endMonth);
                var trends = await service.GetCategoryTrendsAsync(devUserId, range, currency);
                return Results.Ok(trends);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetCategoryTrends")
        .WithOpenApi()
        .WithDescription("Get expense trends broken down by category");

        // Savings Rate
        group.MapGet("/savings-rate", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var range = new MonthRangeDto(startYear, startMonth, endYear, endMonth);
                var savingsRate = await service.GetSavingsRateAsync(devUserId, range, currency);
                return Results.Ok(savingsRate);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetSavingsRate")
        .WithOpenApi()
        .WithDescription("Get savings rate analysis over time");

        // Net Worth Evolution
        group.MapGet("/net-worth-evolution", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var evolution = await service.GetNetWorthEvolutionAsync(
                    devUserId, 
                    startDate, 
                    endDate, 
                    currency);
                return Results.Ok(evolution);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetNetWorthEvolution")
        .WithOpenApi()
        .WithDescription("Get net worth evolution from financial snapshots");

        // Debt Analysis
        group.MapGet("/debt-analysis", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var debtAnalysis = await service.GetDebtAnalysisAsync(devUserId, currency);
                return Results.Ok(debtAnalysis);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetDebtAnalysis")
        .WithOpenApi()
        .WithDescription("Get comprehensive debt analysis from active bills with installments");

        // Upcoming Obligations
        group.MapGet("/upcoming-obligations", async (
            [FromServices] IAnalyticsService service,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var obligations = await service.GetUpcomingObligationsAsync(
                    devUserId,
                    startDate,
                    endDate,
                    currency);
                return Results.Ok(obligations);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetUpcomingObligations")
        .WithOpenApi()
        .WithDescription("Get upcoming financial obligations within a date range");
    }
}
