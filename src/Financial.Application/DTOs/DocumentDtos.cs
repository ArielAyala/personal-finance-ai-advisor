namespace Financial.Application.DTOs;

public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    string FileName,
    string FileType,
    long FileSize,
    string Type,
    bool IsProcessed,
    DateTime CreatedAt
);

public record CreateDocumentDto(
    string Title,
    string? Description,
    string Type
);

public record UpdateDocumentDto(
    string? Title,
    string? Description
);

public record DocumentSearchResultDto(
    Guid DocumentId,
    string Title,
    string Content,
    int ChunkIndex,
    double Similarity,
    DateTime CreatedAt
);

public record DocumentUploadResultDto(
    Guid DocumentId,
    string Message,
    bool Success
);

public record SearchDocumentsRequestDto(
    string Query,
    int Limit = 5
);

public record SearchDocumentsResponseDto(
    string Query,
    List<DocumentSearchResultDto> Results
);
