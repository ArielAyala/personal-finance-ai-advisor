using Financial.Application.AI;
using Financial.Application.Configuration;
using Financial.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0001 // ITextEmbeddingGenerationService is experimental

namespace Financial.Application.Services;

/// <summary>
/// Service for generating text embeddings.
/// The embedding provider is selected at runtime via the AI:Provider configuration key.
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

        var aiSettings = new AiSettings
        {
            Provider = configuration["AI:Provider"] ?? "OpenAI",
            OpenAI = new OpenAiProviderSettings
            {
                ApiKey = configuration["AI:OpenAI:ApiKey"] ?? string.Empty,
                ChatModelId = configuration["AI:OpenAI:ChatModelId"] ?? "gpt-4o-mini",
                EmbeddingModelId = configuration["AI:OpenAI:EmbeddingModelId"] ?? "text-embedding-3-small"
            },
            AzureOpenAI = new AzureOpenAiProviderSettings
            {
                Endpoint = configuration["AI:AzureOpenAI:Endpoint"] ?? string.Empty,
                ApiKey = configuration["AI:AzureOpenAI:ApiKey"] ?? string.Empty,
                ChatDeploymentName = configuration["AI:AzureOpenAI:ChatDeploymentName"] ?? string.Empty,
                EmbeddingDeploymentName = configuration["AI:AzureOpenAI:EmbeddingDeploymentName"] ?? string.Empty
            }
        };

        var kernelBuilder = Kernel.CreateBuilder()
            .AddEmbeddingProvider(aiSettings);

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
