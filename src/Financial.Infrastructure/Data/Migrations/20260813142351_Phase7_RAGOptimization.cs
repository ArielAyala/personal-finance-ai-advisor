using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Financial.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase7_RAGOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create HNSW index on embedding vector for fast similarity search
            // HNSW (Hierarchical Navigable Small World) is a graph-based algorithm
            // optimized for approximate nearest neighbor search in high-dimensional spaces
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_document_embeddings_embedding_hnsw 
                ON document_embeddings 
                USING hnsw (embedding vector_cosine_ops);
            ");
            
            // Optional: Create extension if not exists (should already be enabled)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_document_embeddings_embedding_hnsw;");
        }
    }
}
