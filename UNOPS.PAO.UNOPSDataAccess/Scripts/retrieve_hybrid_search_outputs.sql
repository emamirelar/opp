-- Hybrid search function for Output entities
-- Combines: Semantic search (embeddings) + Keyword matching (full-text) + Similarity (pg_trgm)

-- Drop existing function if it exists
DROP FUNCTION IF EXISTS public.retrieve_hybrid_search_outputs(bytea, TEXT, FLOAT, FLOAT, FLOAT, INT);
DROP FUNCTION IF EXISTS public.retrieve_hybrid_search_outputs(vector, TEXT, FLOAT, FLOAT, FLOAT, INT);
DROP FUNCTION IF EXISTS public.retrieve_hybrid_search_outputs(TEXT, TEXT, FLOAT, FLOAT, FLOAT, INT);
DROP FUNCTION IF EXISTS public.retrieve_hybrid_search_outputs(vector(768), TEXT, FLOAT, FLOAT, FLOAT, INT);

CREATE OR REPLACE FUNCTION public.retrieve_hybrid_search_outputs(
    search_embedding TEXT,                     -- Embedding as TEXT string "[val1,val2,...]" - will be cast to vector internally
    search_text TEXT,                          -- Original text for keyword and similarity matching
    semantic_threshold FLOAT DEFAULT 0.5,      -- Minimum cosine similarity for semantic match
    keyword_boost FLOAT DEFAULT 0.1,           -- Boost score for keyword matches
    similarity_boost FLOAT DEFAULT 0.05,       -- Boost score for text similarity
    max_results INT DEFAULT 5                  -- Maximum number of results to return
)
RETURNS TABLE (
    output_id INT,
    entity_embedding_id INT,
    level_name TEXT,
    output_text TEXT,
    output_hierarchy TEXT,
    keywords TEXT,
    semantic_score REAL,
    keyword_score REAL,
    similarity_score REAL,
    combined_score REAL
) AS $$
BEGIN
    RETURN QUERY
    WITH semantic_matches AS (
        -- Semantic search using cosine similarity on embeddings
        SELECT 
            ee."Id" as embedding_id,
            ee."EntityId" as output_id,
            ee."EntityData" as entity_data,
            ee."Metadata" as metadata,
            ee."Keywords" as keywords,
            -- Cosine similarity: 1 - cosine_distance (higher = more similar)
            -- Cast TEXT to vector(768) for comparison
            (1.0 - (ee."FullEmbedding" <-> search_embedding::vector(768))) as semantic_sim
        FROM public."EntityEmbeddings" ee
        WHERE ee."EntityName" = 'Output'
          AND ee."FullEmbedding" IS NOT NULL
          AND (1.0 - (ee."FullEmbedding" <-> search_embedding::vector(768))) >= semantic_threshold
    ),
    keyword_matches AS (
        -- Keyword matching using full-text search
        SELECT 
            sm.embedding_id,
            sm.output_id,
            sm.entity_data,
            sm.metadata,
            sm.keywords,
            sm.semantic_sim,
            -- Keyword match score using ts_rank
            CASE 
                WHEN sm.keywords IS NOT NULL AND sm.keywords != '' THEN
                    ts_rank(to_tsvector('english', sm.keywords), plainto_tsquery('english', search_text))
                ELSE 0.0
            END as keyword_match
        FROM semantic_matches sm
    ),
    similarity_matches AS (
        -- Text similarity using pg_trgm
        SELECT 
            km.embedding_id,
            km.output_id,
            km.entity_data,
            km.metadata,
            km.keywords,
            km.semantic_sim,
            km.keyword_match,
            -- Trigram similarity (0-1, higher = more similar)
            GREATEST(
                similarity(km.entity_data, search_text),
                CASE WHEN km.keywords IS NOT NULL THEN similarity(km.keywords, search_text) ELSE 0.0 END
            ) as text_sim
        FROM keyword_matches km
    )
    SELECT 
        CAST(sm.output_id AS INTEGER) as output_id,
        CAST(sm.embedding_id AS INTEGER) as entity_embedding_id,
        CAST((sm.metadata::jsonb->>'Level') AS TEXT) as level_name,
        CAST((sm.metadata::jsonb->>'Text') AS TEXT) as output_text,
        CAST((sm.metadata::jsonb->>'Hierarchy') AS TEXT) as output_hierarchy,
        CAST(COALESCE(sm.keywords, '') AS TEXT) as keywords,
        CAST(sm.semantic_sim AS REAL) as semantic_score,
        CAST((sm.keyword_match * keyword_boost) AS REAL) as keyword_score,
        CAST((sm.text_sim * similarity_boost) AS REAL) as similarity_score,
        -- Combined score: semantic (primary) + keyword boost + similarity boost
        CAST((sm.semantic_sim + (sm.keyword_match * keyword_boost) + (sm.text_sim * similarity_boost)) AS REAL) as combined_score
    FROM similarity_matches sm
    ORDER BY 
        -- Sort by combined score (descending)
        combined_score DESC,
        -- Then by semantic similarity (descending)
        semantic_score DESC
    LIMIT max_results;
END;
$$ LANGUAGE plpgsql;

-- Usage example:
-- SELECT * FROM retrieve_hybrid_search_outputs(
--     search_embedding := '[0.1,0.2,0.3,...]',  -- Embedding as TEXT string (no spaces)
--     search_text := 'Handover of upgraded MIS and operational transition',
--     semantic_threshold := 0.5,   -- Lowered threshold for broader matches
--     keyword_boost := 0.1,
--     similarity_boost := 0.05,
--     max_results := 5
-- );

COMMENT ON FUNCTION public.retrieve_hybrid_search_outputs IS 
'Hybrid search for Output entities combining semantic search (embeddings), keyword matching (full-text), and text similarity (pg_trgm). 
Returns top matches with individual and combined scores for transparency. Embedding passed as TEXT and cast to vector(768) internally.';

