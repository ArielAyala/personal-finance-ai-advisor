using Financial.Domain.Entities;

namespace Financial.Application.Interfaces;

public interface IDocumentEmbeddingRepository
{
    Task<DocumentEmbedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentEmbedding>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentEmbedding>> SearchSimilarAsync(float[] queryEmbedding, int limit = 5, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentEmbedding>> SearchSimilarByUserAsync(Guid userId, float[] queryEmbedding, int limit = 5, CancellationToken cancellationToken = default);
    Task<DocumentEmbedding> CreateAsync(DocumentEmbedding embedding, CancellationToken cancellationToken = default);
    Task CreateBatchAsync(IEnumerable<DocumentEmbedding> embeddings, CancellationToken cancellationToken = default);
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
