using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transactions")
            .WithTags("Transactions")
            .RequireAuthorization();

        group.MapGet("/", async (
            ClaimsPrincipal user,
            TransactionService service,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? accountId = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? type = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50) =>
        {
            var userId = user.GetUserId();
            var filter = new TransactionFilterDto(
                startDate,
                endDate,
                accountId,
                categoryId,
                type,
                pageNumber,
                pageSize
            );

            var transactions = await service.GetUserTransactionsAsync(userId, filter);
            return Results.Ok(transactions);
        })
        .WithName("GetTransactions")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, TransactionService service) =>
        {
            var transaction = await service.GetTransactionByIdAsync(id);
            return transaction != null ? Results.Ok(transaction) : Results.NotFound();
        })
        .WithName("GetTransactionById")
        .WithOpenApi();

        group.MapPost("/", async (ClaimsPrincipal user, CreateTransactionDto dto, TransactionService service) =>
        {
            try
            {
                var userId = user.GetUserId();
                var transaction = await service.CreateTransactionAsync(userId, dto);
                return Results.Created($"/api/transactions/{transaction.Id}", transaction);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateTransaction")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateTransactionDto dto, TransactionService service) =>
        {
            try
            {
                var transaction = await service.UpdateTransactionAsync(id, dto);
                return Results.Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateTransaction")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, TransactionService service) =>
        {
            await service.DeleteTransactionAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteTransaction")
        .WithOpenApi();
    }
}
