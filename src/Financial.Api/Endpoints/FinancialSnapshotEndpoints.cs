using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Services;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class FinancialSnapshotEndpoints
{
    public static void MapFinancialSnapshotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/snapshots")
            .WithTags("Financial Snapshots")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, FinancialSnapshotService service) =>
        {
            var userId = user.GetUserId();
            var snapshots = await service.GetUserSnapshotsAsync(userId);
            return Results.Ok(snapshots);
        })
        .WithName("GetSnapshots")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, FinancialSnapshotService service) =>
        {
            var snapshot = await service.GetByIdAsync(id);
            return snapshot != null ? Results.Ok(snapshot) : Results.NotFound();
        })
        .WithName("GetSnapshotById")
        .WithOpenApi();

        group.MapGet("/latest", async (ClaimsPrincipal user, FinancialSnapshotService service) =>
        {
            var userId = user.GetUserId();
            var snapshot = await service.GetLatestAsync(userId);
            return snapshot != null ? Results.Ok(snapshot) : Results.NotFound();
        })
        .WithName("GetLatestSnapshot")
        .WithOpenApi();

        group.MapGet("/growth", async (ClaimsPrincipal user, FinancialSnapshotService service) =>
        {
            var userId = user.GetUserId();
            var growth = await service.GetNetWorthGrowthAsync(userId);
            return growth != null ? Results.Ok(growth) : Results.NotFound();
        })
        .WithName("GetNetWorthGrowth")
        .WithOpenApi();

        group.MapPost("/", async (ClaimsPrincipal user, CreateFinancialSnapshotDto dto, FinancialSnapshotService service) =>
        {
            var userId = user.GetUserId();
            var snapshot = await service.CreateAsync(userId, dto);
            return Results.Created($"/api/snapshots/{snapshot.Id}", snapshot);
        })
        .WithName("CreateSnapshot")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateFinancialSnapshotDto dto, FinancialSnapshotService service) =>
        {
            try
            {
                var snapshot = await service.UpdateAsync(id, dto);
                return Results.Ok(snapshot);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateSnapshot")
        .WithOpenApi();

        group.MapPost("/{id:guid}/close", async (Guid id, FinancialSnapshotService service) =>
        {
            try
            {
                var snapshot = await service.CloseSnapshotAsync(id);
                return Results.Ok(snapshot);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CloseSnapshot")
        .WithOpenApi();

        group.MapPost("/{id:guid}/accounts", async (Guid id, CreateSnapshotAccountDto dto, FinancialSnapshotService service) =>
        {
            try
            {
                var snapshotAccount = await service.AddAccountToSnapshotAsync(id, dto);
                return Results.Created($"/api/snapshots/{id}/accounts/{snapshotAccount.Id}", snapshotAccount);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("AddAccountToSnapshot")
        .WithOpenApi();

        group.MapPut("/accounts/{snapshotAccountId:guid}", async (Guid snapshotAccountId, UpdateSnapshotAccountDto dto, FinancialSnapshotService service) =>
        {
            try
            {
                var snapshotAccount = await service.UpdateSnapshotAccountAsync(snapshotAccountId, dto);
                return Results.Ok(snapshotAccount);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateSnapshotAccount")
        .WithOpenApi();

        group.MapDelete("/accounts/{snapshotAccountId:guid}", async (Guid snapshotAccountId, FinancialSnapshotService service) =>
        {
            try
            {
                await service.RemoveAccountFromSnapshotAsync(snapshotAccountId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("RemoveAccountFromSnapshot")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, FinancialSnapshotService service) =>
        {
            try
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("DeleteSnapshot")
        .WithOpenApi();
    }
}
