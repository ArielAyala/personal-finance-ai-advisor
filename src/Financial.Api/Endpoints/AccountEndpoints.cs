using Financial.Application.DTOs;
using Financial.Application.Services;

namespace Financial.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts")
            .WithTags("Accounts");

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        group.MapGet("/", async (AccountService service) =>
        {
            var accounts = await service.GetUserAccountsAsync(devUserId);
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

        group.MapPost("/", async (CreateAccountDto dto, AccountService service) =>
        {
            var account = await service.CreateAccountAsync(devUserId, dto);
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
