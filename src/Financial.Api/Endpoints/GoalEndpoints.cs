using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class GoalEndpoints
{
    public static void MapGoalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goals")
            .WithTags("Goals")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IGoalService service) =>
        {
            var userId = user.GetUserId();
            var goals = await service.GetAllAsync(userId);
            return Results.Ok(goals);
        })
        .WithName("GetGoals")
        .WithOpenApi();

        group.MapGet("/active", async (ClaimsPrincipal user, IGoalService service) =>
        {
            var userId = user.GetUserId();
            var goals = await service.GetActiveGoalsAsync(userId);
            return Results.Ok(goals);
        })
        .WithName("GetActiveGoals")
        .WithOpenApi();

        group.MapGet("/progress", async (ClaimsPrincipal user, IGoalService service) =>
        {
            var userId = user.GetUserId();
            var progress = await service.GetGoalsProgressAsync(userId);
            return Results.Ok(progress);
        })
        .WithName("GetGoalsProgress")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (ClaimsPrincipal user, Guid id, IGoalService service) =>
        {
            var userId = user.GetUserId();
            var goal = await service.GetByIdAsync(userId, id);
            return goal != null ? Results.Ok(goal) : Results.NotFound();
        })
        .WithName("GetGoalById")
        .WithOpenApi();

        group.MapPost("/", async (ClaimsPrincipal user, CreateGoalDto dto, IGoalService service) =>
        {
            var userId = user.GetUserId();
            var goal = await service.CreateAsync(userId, dto);
            return Results.Created($"/api/goals/{goal.Id}", goal);
        })
        .WithName("CreateGoal")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (ClaimsPrincipal user, Guid id, UpdateGoalDto dto, IGoalService service) =>
        {
            try
            {
                var userId = user.GetUserId();
                var goal = await service.UpdateAsync(userId, id, dto);
                return Results.Ok(goal);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithName("UpdateGoal")
        .WithOpenApi();

        group.MapPatch("/{id:guid}/progress", async (ClaimsPrincipal user, Guid id, UpdateGoalProgressDto dto, IGoalService service) =>
        {
            try
            {
                var userId = user.GetUserId();
                var goal = await service.UpdateProgressAsync(userId, id, dto);
                return Results.Ok(goal);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateGoalProgress")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (ClaimsPrincipal user, Guid id, IGoalService service) =>
        {
            try
            {
                var userId = user.GetUserId();
                await service.DeleteAsync(userId, id);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithName("DeleteGoal")
        .WithOpenApi();
    }
}
