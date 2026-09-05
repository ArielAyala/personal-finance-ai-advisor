using Financial.Application.Configuration;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0001 // ITextEmbeddingGenerationService is experimental

namespace Financial.Application.AI;

/// <summary>
/// Extension methods on <see cref="IKernelBuilder"/> that wire the appropriate
/// Semantic Kernel connector based on the active <see cref="AiSettings.Provider"/>.
///
/// Supported providers today: OpenAI (default), AzureOpenAI.
/// Adding a new provider only requires a new case here — no changes to services or plugins.
/// </summary>
internal static class AIProviderKernelExtensions
{
    /// <summary>Registers the chat-completion connector for the configured provider.</summary>
    internal static IKernelBuilder AddChatCompletionProvider(
        this IKernelBuilder builder,
        AiSettings settings)
    {
        switch (settings.Provider)
        {
            case "AzureOpenAI":
                var az = settings.AzureOpenAI;
                if (string.IsNullOrWhiteSpace(az.Endpoint) || string.IsNullOrWhiteSpace(az.ApiKey))
                    throw new InvalidOperationException(
                        "AI:AzureOpenAI:Endpoint and AI:AzureOpenAI:ApiKey must be configured when Provider is 'AzureOpenAI'.");
                if (string.IsNullOrWhiteSpace(az.ChatDeploymentName))
                    throw new InvalidOperationException(
                        "AI:AzureOpenAI:ChatDeploymentName must be configured when Provider is 'AzureOpenAI'.");
                builder.AddAzureOpenAIChatCompletion(az.ChatDeploymentName, az.Endpoint, az.ApiKey);
                break;

            default: // "OpenAI"
                var oa = settings.OpenAI;
                if (string.IsNullOrWhiteSpace(oa.ApiKey))
                    throw new InvalidOperationException(
                        "AI:OpenAI:ApiKey must be configured when Provider is 'OpenAI'.");
                builder.AddOpenAIChatCompletion(oa.ChatModelId, oa.ApiKey);
                break;
        }

        return builder;
    }

    /// <summary>Registers the text-embedding connector for the configured provider.</summary>
    internal static IKernelBuilder AddEmbeddingProvider(
        this IKernelBuilder builder,
        AiSettings settings)
    {
        switch (settings.Provider)
        {
            case "AzureOpenAI":
                var az = settings.AzureOpenAI;
                if (string.IsNullOrWhiteSpace(az.EmbeddingDeploymentName))
                    throw new InvalidOperationException(
                        "AI:AzureOpenAI:EmbeddingDeploymentName must be configured when Provider is 'AzureOpenAI'.");
                builder.AddAzureOpenAITextEmbeddingGeneration(az.EmbeddingDeploymentName, az.Endpoint, az.ApiKey);
                break;

            default: // "OpenAI"
                var oa = settings.OpenAI;
                builder.AddOpenAITextEmbeddingGeneration(oa.EmbeddingModelId, oa.ApiKey);
                break;
        }

        return builder;
    }
}
