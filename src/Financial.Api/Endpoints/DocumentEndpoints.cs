using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Financial.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Documents")
            .DisableAntiforgery(); // For file uploads

        // TODO: Add authentication once auth is implemented
        // For now using hardcoded userId for development
        var devUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        group.MapGet("/", async (IDocumentService service) =>
        {
            var documents = await service.GetUserDocumentsAsync(devUserId);
            return Results.Ok(documents);
        })
        .WithName("GetDocuments")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, IDocumentService service) =>
        {
            var document = await service.GetByIdAsync(id, devUserId);
            return document != null ? Results.Ok(document) : Results.NotFound();
        })
        .WithName("GetDocumentById")
        .WithOpenApi();

        group.MapPost("/upload", async (
            [FromForm] string title,
            [FromForm] string? description,
            [FromForm] string type,
            [FromForm] IFormFile file,
            IDocumentService service) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "File is required" });
            }

            var dto = new CreateDocumentDto(title, description, type);

            using var stream = file.OpenReadStream();
            var result = await service.UploadDocumentAsync(
                devUserId,
                dto,
                stream,
                file.FileName,
                file.ContentType
            );

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.Message });
            }

            return Results.Created($"/api/documents/{result.DocumentId}", result);
        })
        .WithName("UploadDocument")
        .WithOpenApi()
        .Accepts<IFormFile>("multipart/form-data");

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateDocumentDto dto,
            IDocumentService service) =>
        {
            try
            {
                await service.UpdateAsync(id, devUserId, dto);
                var updated = await service.GetByIdAsync(id, devUserId);
                return Results.Ok(updated);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateDocument")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, IDocumentService service) =>
        {
            try
            {
                await service.DeleteAsync(id, devUserId);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteDocument")
        .WithOpenApi();

        group.MapPost("/search", async (
            SearchDocumentsRequestDto request,
            IDocumentService service) =>
        {
            var results = await service.SearchDocumentsAsync(devUserId, request);
            return Results.Ok(results);
        })
        .WithName("SearchDocuments")
        .WithOpenApi();

        group.MapPost("/{id:guid}/process", async (
            Guid id,
            IDocumentService service) =>
        {
            try
            {
                await service.ProcessDocumentAsync(id);
                return Results.Ok(new { message = "Document processing started" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ProcessDocument")
        .WithOpenApi();
    }
}
