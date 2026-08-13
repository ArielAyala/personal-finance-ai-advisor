using Financial.Application.DTOs;
using Financial.Application.Interfaces;

namespace Financial.Api.Endpoints;

public static class GoalEndpoints
{
    public static void MapGoalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goals")
            .WithTags("Goals");

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        group.MapGet("/", async (IGoalService service) =>
        {
            var goals = await service.GetAllAsync(devUserId);
            return Results.Ok(goals);
        })
        .WithName("GetGoals")
        .WithOpenApi();

        group.MapGet("/active", async (IGoalService service) =>
        {
            var goals = await service.GetActiveGoalsAsync(devUserId);
            return Results.Ok(goals);
        })
        .WithName("GetActiveGoals")
        .WithOpenApi();

        group.MapGet("/progress", async (IGoalService service) =>
        {
            var progress = await service.GetGoalsProgressAsync(devUserId);
            return Results.Ok(progress);
        })
        .WithName("GetGoalsProgress")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, IGoalService service) =>
        {
            var goal = await service.GetByIdAsync(devUserId, id);
            return goal != null ? Results.Ok(goal) : Results.NotFound();
        })
        .WithName("GetGoalById")
        .WithOpenApi();

        group.MapPost("/", async (CreateGoalDto dto, IGoalService service) =>
        {
            var goal = await service.CreateAsync(devUserId, dto);
            return Results.Created($"/api/goals/{goal.Id}", goal);
        })
        .WithName("CreateGoal")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateGoalDto dto, IGoalService service) =>
        {
            try
            {
                var goal = await service.UpdateAsync(devUserId, id, dto);
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

        group.MapPatch("/{id:guid}/progress", async (Guid id, UpdateGoalProgressDto dto, IGoalService service) =>
        {
            try
            {
                var goal = await service.UpdateProgressAsync(devUserId, id, dto);
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

        group.MapDelete("/{id:guid}", async (Guid id, IGoalService service) =>
        {
            try
            {
                await service.DeleteAsync(devUserId, id);
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
