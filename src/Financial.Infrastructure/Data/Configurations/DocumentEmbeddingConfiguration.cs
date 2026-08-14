using Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Infrastructure.Data.Configurations;

public class DocumentEmbeddingConfiguration : IEntityTypeConfiguration<DocumentEmbedding>
{
    public void Configure(EntityTypeBuilder<DocumentEmbedding> builder)
    {
        builder.ToTable("document_embeddings");

        builder.HasKey(de => de.Id);

        builder.Property(de => de.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(de => de.ChunkIndex)
            .IsRequired();

        // pgvector configuration - will be used in Phase 7
        builder.Property(de => de.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(1536)"); // OpenAI embeddings dimension

        builder.Property(de => de.CreatedAt)
            .IsRequired();

        builder.HasIndex(de => de.DocumentId);
        
        // For vector similarity search - will be used in Phase 7
        // CreateIndex for HNSW will be added in migration manually
    }
}
