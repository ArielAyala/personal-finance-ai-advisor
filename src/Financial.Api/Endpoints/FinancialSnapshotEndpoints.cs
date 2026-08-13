using Financial.Application.DTOs;
using Financial.Application.Services;

namespace Financial.Api.Endpoints;

public static class FinancialSnapshotEndpoints
{
    public static void MapFinancialSnapshotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/snapshots")
            .WithTags("Financial Snapshots");

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        group.MapGet("/", async (FinancialSnapshotService service) =>
        {
            var snapshots = await service.GetUserSnapshotsAsync(devUserId);
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

        group.MapGet("/latest", async (FinancialSnapshotService service) =>
        {
            var snapshot = await service.GetLatestAsync(devUserId);
            return snapshot != null ? Results.Ok(snapshot) : Results.NotFound();
        })
        .WithName("GetLatestSnapshot")
        .WithOpenApi();

        group.MapGet("/growth", async (FinancialSnapshotService service) =>
        {
            var growth = await service.GetNetWorthGrowthAsync(devUserId);
            return growth != null ? Results.Ok(growth) : Results.NotFound();
        })
        .WithName("GetNetWorthGrowth")
        .WithOpenApi();

        group.MapPost("/", async (CreateFinancialSnapshotDto dto, FinancialSnapshotService service) =>
        {
            var snapshot = await service.CreateAsync(devUserId, dto);
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
