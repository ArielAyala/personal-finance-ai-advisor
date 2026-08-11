using Financial.Application.DTOs;
using Financial.Application.Services;

namespace Financial.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        group.MapGet("/", async (CategoryService service) =>
        {
            var categories = await service.GetUserCategoriesAsync(devUserId);
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

        group.MapPost("/", async (CreateCategoryDto dto, CategoryService service) =>
        {
            var category = await service.CreateCategoryAsync(devUserId, dto);
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
