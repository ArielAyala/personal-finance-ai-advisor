using Financial.Application.DTOs;

namespace Financial.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DocumentUploadResultDto> UploadDocumentAsync(Guid userId, CreateDocumentDto dto, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, Guid userId, UpdateDocumentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<SearchDocumentsResponseDto> SearchDocumentsAsync(Guid userId, SearchDocumentsRequestDto request, CancellationToken cancellationToken = default);
}
