using Financial.Application.DTOs;
using Financial.Application.Services;

namespace Financial.Api.Endpoints;

public static class BillEndpoints
{
    public static void MapBillEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bills")
            .WithTags("Bills");

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        group.MapGet("/", async (BillService service) =>
        {
            var bills = await service.GetUserBillsAsync(devUserId);
            return Results.Ok(bills);
        })
        .WithName("GetBills")
        .WithOpenApi();

        group.MapGet("/active", async (BillService service) =>
        {
            var bills = await service.GetActiveBillsAsync(devUserId);
            return Results.Ok(bills);
        })
        .WithName("GetActiveBills")
        .WithOpenApi();

        group.MapGet("/overdue", async (BillService service) =>
        {
            var bills = await service.GetOverdueBillsAsync(devUserId);
            return Results.Ok(bills);
        })
        .WithName("GetOverdueBills")
        .WithOpenApi();

        group.MapGet("/upcoming", async (DateTime? startDate, DateTime? endDate, BillService service) =>
        {
            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? start.AddMonths(1);
            var bills = await service.GetUpcomingBillsAsync(devUserId, start, end);
            return Results.Ok(bills);
        })
        .WithName("GetUpcomingBills")
        .WithOpenApi();

        group.MapGet("/monthly-obligations/{year:int}/{month:int}", async (int year, int month, BillService service) =>
        {
            var total = await service.GetMonthlyObligationsAsync(devUserId, year, month);
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

        group.MapPost("/", async (CreateBillDto dto, BillService service) =>
        {
            var bill = await service.CreateBillAsync(devUserId, dto);
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
