namespace Financial.Application.Configuration;

/// <summary>
/// Top-level AI configuration. Set AI:Provider to select the active backend.
/// </summary>
public class AiSettings
{
    /// <summary>Active provider. Supported values: "OpenAI" (default), "AzureOpenAI".</summary>
    public string Provider { get; init; } = "OpenAI";

    public OpenAiProviderSettings OpenAI { get; init; } = new();
    public AzureOpenAiProviderSettings AzureOpenAI { get; init; } = new();
}

public class OpenAiProviderSettings
{
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Chat completion model (e.g. "gpt-4o-mini", "gpt-4o").</summary>
    public string ChatModelId { get; init; } = "gpt-4o-mini";

    /// <summary>Text embedding model (e.g. "text-embedding-3-small").</summary>
    public string EmbeddingModelId { get; init; } = "text-embedding-3-small";
}

public class AzureOpenAiProviderSettings
{
    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Azure deployment name for chat completion.</summary>
    public string ChatDeploymentName { get; init; } = string.Empty;

    /// <summary>Azure deployment name for text embedding generation.</summary>
    public string EmbeddingDeploymentName { get; init; } = string.Empty;
}
