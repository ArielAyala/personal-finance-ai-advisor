using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Services;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts")
            .WithTags("Accounts")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, AccountService service) =>
        {
            var userId = user.GetUserId();
            var accounts = await service.GetUserAccountsAsync(userId);
            return Results.Ok(accounts);
        })
        .WithName("GetAccounts")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, AccountService service) =>
        {
            var account = await service.GetAccountByIdAsync(id);
            return account != null ? Results.Ok(account) : Results.NotFound();
        })
        .WithName("GetAccountById")
        .WithOpenApi();

        group.MapPost("/", async (ClaimsPrincipal user, CreateAccountDto dto, AccountService service) =>
        {
            var userId = user.GetUserId();
            var account = await service.CreateAccountAsync(userId, dto);
            return Results.Created($"/api/accounts/{account.Id}", account);
        })
        .WithName("CreateAccount")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountDto dto, AccountService service) =>
        {
            try
            {
                var account = await service.UpdateAccountAsync(id, dto);
                return Results.Ok(account);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateAccount")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, AccountService service) =>
        {
            await service.DeleteAccountAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteAccount")
        .WithOpenApi();
    }
}
