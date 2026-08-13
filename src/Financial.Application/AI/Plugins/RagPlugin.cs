using System.ComponentModel;
using System.Text.Json;
using Financial.Application.Interfaces;
using Microsoft.SemanticKernel;

namespace Financial.Application.AI.Plugins;

/// <summary>
/// RAG (Retrieval-Augmented Generation) plugin for AI agent to search user documents
/// </summary>
public class RagPlugin
{
    private readonly IRagService _ragService;

    public RagPlugin(IRagService ragService)
    {
        _ragService = ragService;
    }

    [KernelFunction("search_documents")]
    [Description("Search through user's uploaded documents for relevant information. Use this when the user asks about documents, files, or information that might be in their uploaded content.")]
    public async Task<string> SearchDocumentsAsync(
        [Description("User ID")] Guid userId,
        [Description("Search query - what to look for in documents")] string query,
        [Description("Maximum number of results to return (default 5)")] int limit = 5)
    {
        try
        {
            var results = await _ragService.SearchAsync(userId, query, limit);

            if (!results.Results.Any())
            {
                return "No relevant documents found.";
            }

            var formattedResults = results.Results.Select((r, index) => new
            {
                Rank = index + 1,
                Document = r.Title,
                Similarity = $"{r.Similarity:P2}",
                Content = r.Content.Length > 300 ? r.Content.Substring(0, 297) + "..." : r.Content,
                ChunkIndex = r.ChunkIndex
            }).ToList();

            return JsonSerializer.Serialize(formattedResults, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error searching documents: {ex.Message}";
        }
    }

    [KernelFunction("get_document_context")]
    [Description("Get relevant context from user's documents for answering a specific question. Returns the most relevant text excerpts.")]
    public async Task<string> GetDocumentContextAsync(
        [Description("User ID")] Guid userId,
        [Description("Question or topic to find context for")] string query,
        [Description("Maximum number of text chunks to retrieve (default 3)")] int maxChunks = 3)
    {
        try
        {
            var context = await _ragService.GetRelevantContextAsync(userId, query, maxChunks);

            if (string.IsNullOrWhiteSpace(context))
            {
                return "No relevant context found in user documents.";
            }

            return $"Relevant context from user documents:\n\n{context}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving document context: {ex.Message}";
        }
    }
}
