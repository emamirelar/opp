-- =============================================================================
-- 01-extensions.sql
-- Create required PostgreSQL extensions
-- =============================================================================

-- pgvector for semantic search / embeddings
CREATE EXTENSION IF NOT EXISTS vector;

-- pg_trgm for trigram text similarity search
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Verify extensions
SELECT extname, extversion FROM pg_extension WHERE extname IN ('vector', 'pg_trgm');
