using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class DocumentEmbeddingRepository : IDocumentEmbeddingRepository
{
    private readonly FinancialDbContext _context;

    public DocumentEmbeddingRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentEmbedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentEmbeddings
            .Include(de => de.Document)
            .FirstOrDefaultAsync(de => de.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DocumentEmbedding>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentEmbeddings
            .Where(de => de.DocumentId == documentId)
            .OrderBy(de => de.ChunkIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DocumentEmbedding>> SearchSimilarAsync(float[] queryEmbedding, int limit = 5, CancellationToken cancellationToken = default)
    {
        var queryVector = new Vector(queryEmbedding);
        
        return await _context.DocumentEmbeddings
            .Include(de => de.Document)
            .OrderBy(de => de.Embedding.CosineDistance(queryVector))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DocumentEmbedding>> SearchSimilarByUserAsync(Guid userId, float[] queryEmbedding, int limit = 5, CancellationToken cancellationToken = default)
    {
        var queryVector = new Vector(queryEmbedding);
        
        return await _context.DocumentEmbeddings
            .Include(de => de.Document)
            .Where(de => de.Document.UserId == userId)
            .OrderBy(de => de.Embedding.CosineDistance(queryVector))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentEmbedding> CreateAsync(DocumentEmbedding embedding, CancellationToken cancellationToken = default)
    {
        embedding.CreatedAt = DateTime.UtcNow;
        _context.DocumentEmbeddings.Add(embedding);
        await _context.SaveChangesAsync(cancellationToken);
        return embedding;
    }

    public async Task CreateBatchAsync(IEnumerable<DocumentEmbedding> embeddings, CancellationToken cancellationToken = default)
    {
        var embeddingsList = embeddings.ToList();
        var now = DateTime.UtcNow;
        
        foreach (var embedding in embeddingsList)
        {
            embedding.CreatedAt = now;
        }
        
        _context.DocumentEmbeddings.AddRange(embeddingsList);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var embeddings = await GetByDocumentIdAsync(documentId, cancellationToken);
        _context.DocumentEmbeddings.RemoveRange(embeddings);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
