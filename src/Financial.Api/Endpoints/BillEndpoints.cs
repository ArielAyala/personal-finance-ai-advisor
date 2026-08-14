using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Services;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class BillEndpoints
{
    public static void MapBillEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bills")
            .WithTags("Bills")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, BillService service) =>
        {
            var userId = user.GetUserId();
            var bills = await service.GetUserBillsAsync(userId);
            return Results.Ok(bills);
        })
        .WithName("GetBills")
        .WithOpenApi();

        group.MapGet("/active", async (ClaimsPrincipal user, BillService service) =>
        {
            var userId = user.GetUserId();
            var bills = await service.GetActiveBillsAsync(userId);
            return Results.Ok(bills);
        })
        .WithName("GetActiveBills")
        .WithOpenApi();

        group.MapGet("/overdue", async (ClaimsPrincipal user, BillService service) =>
        {
            var userId = user.GetUserId();
            var bills = await service.GetOverdueBillsAsync(userId);
            return Results.Ok(bills);
        })
        .WithName("GetOverdueBills")
        .WithOpenApi();

        group.MapGet("/upcoming", async (ClaimsPrincipal user, DateTime? startDate, DateTime? endDate, BillService service) =>
        {
            var userId = user.GetUserId();
            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? start.AddMonths(1);
            var bills = await service.GetUpcomingBillsAsync(userId, start, end);
            return Results.Ok(bills);
        })
        .WithName("GetUpcomingBills")
        .WithOpenApi();

        group.MapGet("/monthly-obligations/{year:int}/{month:int}", async (ClaimsPrincipal user, int year, int month, BillService service) =>
        {
            var userId = user.GetUserId();
            var total = await service.GetMonthlyObligationsAsync(userId, year, month);
            return Results.Ok(new { Year = year, Month = month, TotalAmount = total });
        })
        .WithName("GetMonthlyObligations")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, BillService service) =>
        {
            var bill = await service.GetBillByIdAsync(id);
            return bill != null ? Results.Ok(bill) : Results.NotFound();
        })
        .WithName("GetBillById")
        .WithOpenApi();

        group.MapPost("/", async (ClaimsPrincipal user, CreateBillDto dto, BillService service) =>
        {
            var userId = user.GetUserId();
            var bill = await service.CreateBillAsync(userId, dto);
            return Results.Created($"/api/bills/{bill.Id}", bill);
        })
        .WithName("CreateBill")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateBillDto dto, BillService service) =>
        {
            try
            {
                var bill = await service.UpdateBillAsync(id, dto);
                return Results.Ok(bill);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateBill")
        .WithOpenApi();

        group.MapPost("/{id:guid}/pay", async (Guid id, PayBillDto dto, BillService service) =>
        {
            try
            {
                var bill = await service.PayBillAsync(id, dto);
                return Results.Ok(bill);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("PayBill")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, BillService service) =>
        {
            await service.DeleteBillAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteBill")
        .WithOpenApi();
    }
}
