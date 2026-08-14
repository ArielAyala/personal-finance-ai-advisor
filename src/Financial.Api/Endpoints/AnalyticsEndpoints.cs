using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .RequireAuthorization();

        // Monthly Summary
        group.MapGet("/monthly-summary", async (
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var summary = await service.GetMonthlySummaryAsync(userId, year, month, currency);
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
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var range = new MonthRangeDto(startYear, startMonth, endYear, endMonth);
                var trends = await service.GetExpenseTrendsAsync(userId, range, currency);
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
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var range = new MonthRangeDto(startYear, startMonth, endYear, endMonth);
                var trends = await service.GetCategoryTrendsAsync(userId, range, currency);
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
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var range = new MonthRangeDto(startYear, startMonth, endYear, endMonth);
                var savingsRate = await service.GetSavingsRateAsync(userId, range, currency);
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
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var evolution = await service.GetNetWorthEvolutionAsync(
                    userId, 
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
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var debtAnalysis = await service.GetDebtAnalysisAsync(userId, currency);
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
            ClaimsPrincipal user,
            [FromServices] IAnalyticsService service,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string currency = "PYG") =>
        {
            try
            {
                var userId = user.GetUserId();
                var obligations = await service.GetUpcomingObligationsAsync(
                    userId,
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
