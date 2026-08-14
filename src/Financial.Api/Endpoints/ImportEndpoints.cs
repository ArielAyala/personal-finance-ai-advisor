using Financial.Api.Extensions;
using Financial.Application.DTOs.Import;
using Financial.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Financial.Api.Endpoints;

public static class ImportEndpoints
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".csv", ".xlsx", ".xls" };

    public static void MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import")
            .WithTags("Import")
            .DisableAntiforgery() // Required for file uploads in minimal API
            .RequireAuthorization();

        group.MapPost("/preview", async (
            IFormFile file,
            ClaimsPrincipal user,
            IImportService importService) =>
        {
            var userId = user.GetUserId();
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "No file uploaded" });
                }

                if (file.Length > MaxFileSize)
                {
                    return Results.BadRequest(new { error = $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024} MB" });
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    return Results.BadRequest(new { error = $"File type not supported. Allowed types: {string.Join(", ", AllowedExtensions)}" });
                }

                // Parse and preview
                using var stream = file.OpenReadStream();
                var preview = await importService.PreviewImportAsync(stream, file.FileName, userId);

                return Results.Ok(preview);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error processing file");
            }
        })
        .WithName("PreviewImport")
        .WithOpenApi()
        .Accepts<IFormFile>("multipart/form-data");

        group.MapPost("/execute", async (
            [FromBody] ImportRequestDto request,
            ClaimsPrincipal user,
            IImportService importService) =>
        {
            var userId = user.GetUserId();
            try
            {
                var result = await importService.ExecuteImportAsync(request, userId);
                
                if (result.Success)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error executing import");
            }
        })
        .WithName("ExecuteImport")
        .WithOpenApi();

        group.MapPost("/validate", async (
            IFormFile file,
            IImportService importService) =>
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "No file uploaded" });
                }

                using var stream = file.OpenReadStream();
                var isValid = await importService.ValidateFileAsync(stream, file.FileName);

                return Results.Ok(new { isValid, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error validating file");
            }
        })
        .WithName("ValidateImportFile")
        .WithOpenApi()
        .Accepts<IFormFile>("multipart/form-data");
    }
}
