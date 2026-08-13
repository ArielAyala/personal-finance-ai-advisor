using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Financial.Application.Services;

/// <summary>
/// Retrieval-Augmented Generation (RAG) service for semantic document search
/// </summary>
public class RagService : IRagService
{
    private readonly IDocumentEmbeddingRepository _embeddingRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IDocumentEmbeddingRepository embeddingRepository,
        IEmbeddingService embeddingService,
        ILogger<RagService> logger)
    {
        _embeddingRepository = embeddingRepository;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<SearchDocumentsResponseDto> SearchAsync(
        Guid userId,
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("RAG search for user {UserId}: {Query}", userId, query);

            // Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            // Search for similar embeddings in user's documents
            var results = await _embeddingRepository.SearchSimilarByUserAsync(
                userId,
                queryEmbedding,
                limit,
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

            _logger.LogInformation("RAG search returned {Count} results", searchResults.Count);

            return new SearchDocumentsResponseDto(query, searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RAG search for query: {Query}", query);
            throw;
        }
    }

    public async Task<string> GetRelevantContextAsync(
        Guid userId,
        string query,
        int maxChunks = 3,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchResponse = await SearchAsync(userId, query, maxChunks, cancellationToken);

            if (!searchResponse.Results.Any())
            {
                return string.Empty;
            }

            // Combine top results into a single context string
            var contextParts = searchResponse.Results
                .Select((r, index) => $"[Document {index + 1}: {r.Title}]\n{r.Content}")
                .ToList();

            var context = string.Join("\n\n---\n\n", contextParts);

            _logger.LogInformation("Retrieved {Length} characters of context from {Count} chunks",
                context.Length, searchResponse.Results.Count);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relevant context for query: {Query}", query);
            throw;
        }
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
