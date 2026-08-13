namespace Financial.Application.DTOs;

/// <summary>
/// Represents a chat message in the AI assistant conversation
/// </summary>
public record ChatMessageDto(
    string Role,
    string Content,
    DateTime Timestamp
);

/// <summary>
/// Request to send a message to the AI assistant
/// </summary>
public record ChatRequestDto(
    string Message,
    List<ChatMessageDto>? History = null
);

/// <summary>
/// Response from the AI assistant
/// </summary>
public record ChatResponseDto(
    string Message,
    string? ToolsUsed = null,
    DateTime Timestamp = default
)
{
    public DateTime Timestamp { get; init; } = Timestamp == default ? DateTime.UtcNow : Timestamp;
}

/// <summary>
/// Structured financial query result
/// </summary>
public record FinancialQueryResultDto(
    string Query,
    string Answer,
    Dictionary<string, object>? Data = null,
    List<string>? SourceTables = null
);
