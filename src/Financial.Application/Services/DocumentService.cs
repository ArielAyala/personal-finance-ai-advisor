using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Application.Mappers;
using Financial.Domain.Entities;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace Financial.Application.Services;

/// <summary>
/// Service for managing documents and their ingestion pipeline
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentEmbeddingRepository _embeddingRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITextExtractionService _textExtractionService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IDocumentEmbeddingRepository embeddingRepository,
        IFileStorageService fileStorageService,
        ITextExtractionService textExtractionService,
        IEmbeddingService embeddingService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _embeddingRepository = embeddingRepository;
        _fileStorageService = fileStorageService;
        _textExtractionService = textExtractionService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        
        if (document == null || document.UserId != userId)
        {
            return null;
        }

        return MapToDto(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId, cancellationToken);
        return documents.Select(MapToDto);
    }

    public async Task<DocumentUploadResultDto> UploadDocumentAsync(
        Guid userId,
        CreateDocumentDto dto,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file type
            if (!_textExtractionService.SupportsFileType(contentType, fileName))
            {
                return new DocumentUploadResultDto(
                    Guid.Empty,
                    $"File type '{contentType}' is not supported. Supported types: PDF, Excel, Text",
                    false
                );
            }

            // Save file to storage
            var filePath = await _fileStorageService.SaveFileAsync(fileStream, fileName, cancellationToken);
            
            // Reset stream for text extraction
            fileStream.Position = 0;

            // Create document entity
            var document = new Document
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                FilePath = filePath,
                FileName = fileName,
                FileType = contentType,
                FileSize = fileStream.Length,
                Type = Enum.Parse<DocumentType>(dto.Type),
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save document to database
            await _documentRepository.CreateAsync(document, cancellationToken);

            // Process document asynchronously (in background)
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessDocumentAsync(document.Id, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document {DocumentId} in background", document.Id);
                }
            }, cancellationToken);

            _logger.LogInformation("Document uploaded successfully: {DocumentId}", document.Id);

            return new DocumentUploadResultDto(
                document.Id,
                "Document uploaded successfully. Processing in background.",
                true
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return new DocumentUploadResultDto(
                Guid.Empty,
                $"Error uploading document: {ex.Message}",
                false
            );
        }
    }

    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting document processing for {DocumentId}", documentId);

            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found");
            }

            if (document.IsProcessed)
            {
                _logger.LogInformation("Document {DocumentId} already processed", documentId);
                return;
            }

            // Step 1: Extract text from file
            using var fileStream = await _fileStorageService.GetFileAsync(document.FilePath, cancellationToken);
            var extractedText = await _textExtractionService.ExtractTextAsync(
                fileStream,
                document.FileName,
                document.FileType,
                cancellationToken
            );

            document.ExtractedText = extractedText;

            // Step 2: Chunk text into smaller pieces
            var chunks = ChunkText(extractedText);

            _logger.LogInformation("Document {DocumentId}: Extracted {TextLength} chars, created {ChunkCount} chunks",
                documentId, extractedText.Length, chunks.Count);

            // Step 3: Generate embeddings for each chunk
            var embeddings = new List<DocumentEmbedding>();
            
            for (int i = 0; i < chunks.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chunk = chunks[i];
                var embeddingVector = await _embeddingService.GenerateEmbeddingAsync(chunk, cancellationToken);

                var embedding = new DocumentEmbedding
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    Content = chunk,
                    ChunkIndex = i,
                    Embedding = new Vector(embeddingVector),
                    CreatedAt = DateTime.UtcNow
                };

                embeddings.Add(embedding);
            }

            // Step 4: Save embeddings to database
            await _embeddingRepository.CreateBatchAsync(embeddings, cancellationToken);

            // Step 5: Mark document as processed
            document.IsProcessed = true;
            await _documentRepository.UpdateAsync(document, cancellationToken);

            _logger.LogInformation("Document {DocumentId} processed successfully with {EmbeddingCount} embeddings",
                documentId, embeddings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, Guid userId, UpdateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        
        if (document == null || document.UserId != userId)
        {
            throw new InvalidOperationException($"Document {id} not found or access denied");
        }

        if (dto.Title != null) document.Title = dto.Title;
        if (dto.Description != null) document.Description = dto.Description;

        await _documentRepository.UpdateAsync(document, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        
        if (document == null || document.UserId != userId)
        {
            throw new InvalidOperationException($"Document {id} not found or access denied");
        }

        // Delete embeddings
        await _embeddingRepository.DeleteByDocumentIdAsync(id, cancellationToken);

        // Delete file from storage
        await _fileStorageService.DeleteFileAsync(document.FilePath, cancellationToken);

        // Delete document from database
        await _documentRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Document {DocumentId} deleted successfully", id);
    }

    public async Task<SearchDocumentsResponseDto> SearchDocumentsAsync(
        Guid userId,
        SearchDocumentsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Query, cancellationToken);

        // Search for similar embeddings
        var results = await _embeddingRepository.SearchSimilarByUserAsync(
            userId,
            queryEmbedding,
            request.Limit,
            cancellationToken
        );

        var searchResults = results.Select(r => new DocumentSearchResultDto(
            r.Document.Id,
            r.Document.Title,
            r.Content,
            r.ChunkIndex,
            CalculateSimilarity(queryEmbedding, r.Embedding.ToArray()),
            r.Document.CreatedAt
        )).ToList();

        return new SearchDocumentsResponseDto(request.Query, searchResults);
    }

    // Helper methods

    private DocumentDto MapToDto(Document document)
    {
        return new DocumentDto(
            document.Id,
            document.Title,
            document.Description,
            document.FileName,
            document.FileType,
            document.FileSize,
            document.Type.ToString(),
            document.IsProcessed,
            document.CreatedAt
        );
    }

    private List<string> ChunkText(string text, int maxChunkSize = 1000, int overlap = 200)
    {
        var chunks = new List<string>();
        
        if (string.IsNullOrWhiteSpace(text))
        {
            return chunks;
        }

        // Split by paragraphs first
        var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        var currentChunk = new List<string>();
        var currentLength = 0;

        foreach (var paragraph in paragraphs)
        {
            var paragraphLength = paragraph.Length;

            if (currentLength + paragraphLength > maxChunkSize && currentChunk.Any())
            {
                // Save current chunk
                chunks.Add(string.Join("\n\n", currentChunk));
                
                // Start new chunk with overlap (keep last paragraph)
                if (currentChunk.Count > 0)
                {
                    currentChunk = new List<string> { currentChunk.Last() };
                    currentLength = currentChunk.Last().Length;
                }
                else
                {
                    currentChunk = new List<string>();
                    currentLength = 0;
                }
            }

            currentChunk.Add(paragraph);
            currentLength += paragraphLength;
        }

        // Add remaining chunk
        if (currentChunk.Any())
        {
            chunks.Add(string.Join("\n\n", currentChunk));
        }

        return chunks;
    }

    private double CalculateSimilarity(float[] embedding1, float[] embedding2)
    {
        // Cosine similarity: closer to 1 means more similar
        var dotProduct = embedding1.Zip(embedding2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(embedding1.Sum(x => x * x));
        var magnitude2 = Math.Sqrt(embedding2.Sum(x => x * x));
        
        return dotProduct / (magnitude1 * magnitude2);
    }
}
