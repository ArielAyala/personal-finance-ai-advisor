using Financial.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0001

namespace Financial.Application.Services;

/// <summary>
/// Service for generating embeddings using OpenAI
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        IConfiguration configuration,
        ILogger<EmbeddingService> logger)
    {
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"] ?? 
                     throw new InvalidOperationException("OpenAI API key not configured");

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", apiKey);
        var kernel = kernelBuilder.Build();

        _embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            return embedding.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            throw;
        }
    }

    public async Task<IEnumerable<float[]>> GenerateEmbeddingsBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        try
        {
            var textList = texts.ToList();
            var embeddings = new List<float[]>();

            // Generate embeddings one by one (can be optimized with batch API if available)
            foreach (var text in textList)
            {
                var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
                embeddings.Add(embedding);
            }

            return embeddings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings");
            throw;
        }
    }
}
