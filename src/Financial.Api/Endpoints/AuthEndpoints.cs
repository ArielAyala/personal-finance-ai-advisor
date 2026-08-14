using Financial.Application.DTOs;
using Financial.Application.Interfaces;

namespace Financial.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", async (RegisterRequestDto request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("Register")
        .WithOpenApi()
        .AllowAnonymous();

        group.MapPost("/login", async (LoginRequestDto request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("Login")
        .WithOpenApi()
        .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshTokenRequestDto request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.RefreshTokenAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("RefreshToken")
        .WithOpenApi()
        .AllowAnonymous();

        group.MapPost("/revoke", async (RefreshTokenRequestDto request, IAuthService authService) =>
        {
            try
            {
                await authService.RevokeTokenAsync(request.RefreshToken);
                return Results.Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("RevokeToken")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
