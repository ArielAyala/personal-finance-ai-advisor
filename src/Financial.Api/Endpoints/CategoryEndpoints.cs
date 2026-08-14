using Financial.Api.Extensions;
using Financial.Application.DTOs;
using Financial.Application.Services;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var categories = await service.GetUserCategoriesAsync(userId);
            return Results.Ok(categories);
        })
        .WithName("GetCategories")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, CategoryService service) =>
        {
            var category = await service.GetCategoryByIdAsync(id);
            return category != null ? Results.Ok(category) : Results.NotFound();
        })
        .WithName("GetCategoryById")
        .WithOpenApi();

        group.MapPost("/", async (ClaimsPrincipal user, CreateCategoryDto dto, CategoryService service) =>
        {
            var userId = user.GetUserId();
            var category = await service.CreateCategoryAsync(userId, dto);
            return Results.Created($"/api/categories/{category.Id}", category);
        })
        .WithName("CreateCategory")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryDto dto, CategoryService service) =>
        {
            try
            {
                var category = await service.UpdateCategoryAsync(id, dto);
                return Results.Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateCategory")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, CategoryService service) =>
        {
            await service.DeleteCategoryAsync(id);
            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .WithOpenApi();
    }
}
