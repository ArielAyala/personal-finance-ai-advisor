using Pgvector;

namespace Financial.Domain.Entities;

// This entity will be used in Phase 7 - RAG Integration
public class DocumentEmbedding
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    // Vector will be stored using pgvector's Vector type
    public Vector? Embedding { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Document Document { get; set; } = null!;
}
