using Financial.Application.DTOs;

namespace Financial.Application.Interfaces;

/// <summary>
/// AI Assistant service for financial queries and recommendations
/// </summary>
public interface IAIAssistantService
{
    /// <summary>
    /// Sends a chat message to the AI assistant
    /// </summary>
    Task<ChatResponseDto> ChatAsync(Guid userId, ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a structured financial query using AI
    /// </summary>
    Task<FinancialQueryResultDto> QueryFinancialDataAsync(Guid userId, string query, CancellationToken cancellationToken = default);
}
