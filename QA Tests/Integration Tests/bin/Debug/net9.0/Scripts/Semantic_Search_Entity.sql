-- ============================================================================
-- GENERIC SEMANTIC ENTITY SEARCH FUNCTION
-- ============================================================================
-- This PostgreSQL script provides semantic search capabilities for any entity type
-- using vector embeddings stored in the EntityEmbeddings table.
--
-- FEATURES:
-- - Semantic search using vector similarity (cosine distance)
-- - Generic function that works with any entity type (Opportunities, Partners, Contacts, etc.)
-- - Returns similar entities with relevance scores
-- - Excludes the current entity from results
-- - Configurable result limits and similarity thresholds
-- ============================================================================

-- Enable required extensions
--CREATE EXTENSION IF NOT EXISTS vector;

-- Drop existing functions to ensure clean recreation
DROP FUNCTION IF EXISTS public.search_opportunities_semantic(INTEGER, INTEGER, REAL);
DROP FUNCTION IF EXISTS public.search_entity_semantic(TEXT, INTEGER, INTEGER, REAL);
DROP FUNCTION IF EXISTS public.semantic_search_entity(TEXT, INTEGER, INTEGER, REAL);

-- ============================================================================
-- GENERIC SEMANTIC ENTITY SEARCH FUNCTION
-- ============================================================================
-- Searches for similar entities of any type using vector embeddings
-- This is the primary function to use for semantic similarity search
-- Parameters:
--   - entity_name: The entity type to search (e.g., 'Opportunities', 'Partners', 'Contacts', 'Risks', etc.)
--                  NOTE: Entity names are pluralized in EntityEmbeddings table
--   - current_entity_id: The ID of the current entity (to exclude from results)
--   - max_results: Maximum number of similar entities to return (default: 6)
--   - similarity_threshold: Minimum similarity score (0-1) to include (default: 0.15)
-- Returns:
--   JSON object with similar entities and metadata
-- ============================================================================
CREATE OR REPLACE FUNCTION public.semantic_search_entity(
    entity_name TEXT,
    current_entity_id INTEGER,
    max_results INTEGER DEFAULT 6,
    similarity_threshold REAL DEFAULT 0.15
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    result_json JSON;
    current_embedding vector(768);
    start_time TIMESTAMP;
    execution_time REAL;
BEGIN
    start_time := clock_timestamp();
    
    -- Get the embedding for the current entity
    SELECT "FullEmbedding"
    INTO current_embedding
    FROM public."EntityEmbeddings"
    WHERE "EntityName" = entity_name
    AND "EntityId" = current_entity_id
    AND "FullEmbedding" IS NOT NULL
    LIMIT 1;
    
    -- If no embedding found for the current entity, return empty result
    IF current_embedding IS NULL THEN
        RETURN json_build_object(
            'entityName', entity_name,
            'currentEntityId', current_entity_id,
            'hasEmbedding', false,
            'similarEntities', '[]'::json,
            'totalFound', 0,
            'message', 'No embedding found for the current entity'
        );
    END IF;
    
    -- Find similar entities using vector similarity
    WITH similar_entities AS (
        SELECT 
            ee."EntityId" as entity_id,
            -- Calculate similarity score (1 - cosine distance)
            round((1 - (ee."FullEmbedding" <=> current_embedding))::numeric, 4) as similarity_score,
            left(COALESCE(ee."EntityData", ''), 500) as snippet
        FROM public."EntityEmbeddings" ee
        WHERE ee."EntityName" = entity_name
        AND ee."EntityId" != current_entity_id  -- Exclude the current entity
        AND ee."FullEmbedding" IS NOT NULL
        AND (1 - (ee."FullEmbedding" <=> current_embedding)) > similarity_threshold
        ORDER BY ee."FullEmbedding" <=> current_embedding  -- Order by distance (ascending)
        LIMIT max_results
    )
    SELECT json_build_object(
        'entityName', entity_name,
        'currentEntityId', current_entity_id,
        'hasEmbedding', true,
        'similarEntities', COALESCE(
            (SELECT json_agg(
                json_build_object(
                    'entityId', entity_id,
                    'similarityScore', similarity_score,
                    'relevancePercentage', round((similarity_score * 100)::numeric, 1),
                    'snippet', snippet
                )
                ORDER BY similarity_score DESC
            ) FROM similar_entities),
            '[]'::json
        ),
        'totalFound', (SELECT COUNT(*) FROM similar_entities),
        'similarityThreshold', similarity_threshold,
        'maxResults', max_results,
        'executionTimeMs', round((EXTRACT(EPOCH FROM (clock_timestamp() - start_time)) * 1000)::numeric, 2)
    )
    INTO result_json;
    
    RETURN result_json;
END
$$;

-- ============================================================================
-- EXAMPLE USAGE
-- ============================================================================
-- Search for opportunities similar to opportunity ID 2:
-- SELECT public.semantic_search_entity('Opportunities', 2);
--
-- Search for opportunities similar to opportunity ID 2 with custom parameters:
-- SELECT public.semantic_search_entity('Opportunities', 2, 10, 0.20);
--
-- Search for similar partners:
-- SELECT public.semantic_search_entity('Partners', 5, 6, 0.15);
--
-- Search for similar contacts:
-- SELECT public.semantic_search_entity('Contacts', 10, 6, 0.15);
--
-- Search for similar risks:
-- SELECT public.semantic_search_entity('Risks', 3, 6, 0.15);
-- ============================================================================

-- Grant execute permissions to the appropriate roles
-- GRANT EXECUTE ON FUNCTION public.semantic_search_entity(TEXT, INTEGER, INTEGER, REAL) TO <your_role>;

