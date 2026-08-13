using Financial.Application.DTOs;

namespace Financial.Application.Interfaces;

public interface IRagService
{
    Task<SearchDocumentsResponseDto> SearchAsync(Guid userId, string query, int limit = 5, CancellationToken cancellationToken = default);
    Task<string> GetRelevantContextAsync(Guid userId, string query, int maxChunks = 3, CancellationToken cancellationToken = default);
}
