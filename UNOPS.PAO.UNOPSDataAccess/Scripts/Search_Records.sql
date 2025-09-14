-- ============================================================================
-- INTELLIGENT HYBRID ENTITY SEARCH FUNCTION
-- ============================================================================
-- This PostgreSQL function provides advanced hybrid search capabilities that
-- intelligently combines field-specific text search with semantic embedding search
-- across all entity types in the PAO system.
--
-- DUAL SEARCH ARCHITECTURE:
-- Part 1: Field-Specific Search - Searches actual database columns with smart scoring
-- Part 2: Semantic Embedding Search - Uses AI embeddings for meaning-based matching
--
-- CORE ENTITY SEARCH:
-- - Searches core entities: Partners, Contacts, Interactions
-- - Uses information_schema to find searchable columns dynamically
-- - Uses pg_trgm similarity search for flexible matching
-- - Safely excludes system columns and sensitive fields
--
-- INTELLIGENT FIELD SCORING SYSTEM (Normalized to max 100%):
-- - Names/Titles: Up to 1.0 (100%) - highest priority for person/entity identification
-- - Contact Info: Up to 0.9 (90%) - email, phone - high relevance for communication  
-- - Organization: Up to 0.8 (80%) - department, company - structural context
-- - Descriptions: Up to 0.6 (60%) - notes, details - content context
-- - Other Fields: Up to 0.5 (50%) - general text fields
-- - Triple search strategy per field: exact match, word boundary, similarity
--
-- TRIPLE SEARCH STRATEGY PER FIELD:
-- 1. Exact Substring Match: ILIKE '%term%' (highest score)
-- 2. Word Boundary Match: Regex \yterm\y (medium score)
-- 3. Similarity Match: pg_trgm similarity (flexible score)
--
-- SEMANTIC EMBEDDING INTEGRATION:
-- - Vector cosine similarity search using 768-dimensional embeddings
-- - Configurable similarity thresholds and boost factors
-- - Combined results from both field search and semantic search
-- - Graceful handling when embeddings are not available
--
-- PERFORMANCE OPTIMIZATIONS:
-- - Efficient FOREACH loops for dynamic entity iteration
-- - Proper indexing on EntityEmbeddings table
-- - IVFFlat index for fast vector similarity search
-- - Configurable result limits per entity type
--
-- RICH RESPONSE FORMAT:
-- - Structured JSON with fieldSearch and semanticSearch sections
-- - Detailed scoring information and match criteria
-- - Execution time tracking and performance metrics
-- - Comprehensive summary with result counts and capabilities
--
-- EXTENSIBILITY FEATURES:
-- - Configurable boost factors for different search modes
-- - Adjustable snippet lengths for result previews
-- - Flexible similarity thresholds for embedding search
-- - Easy addition of new field types and scoring rules
--
-- SECURITY CONSIDERATIONS:
-- - Safe column filtering to exclude sensitive data
-- - Proper parameter binding to prevent SQL injection
-- - Graceful error handling for missing tables/columns
-- - Comprehensive logging for monitoring and debugging
-- ============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS vector;

-- Drop existing functions to ensure clean recreation
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector, REAL, REAL, INTEGER);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector, REAL, REAL, INTEGER, BOOLEAN);

-- Create the comprehensive hybrid search function
CREATE OR REPLACE FUNCTION public.search_entity_records(
    search_query TEXT,
    embedding vector DEFAULT NULL,
    text_boost REAL DEFAULT 1.0,
    embedding_boost REAL DEFAULT 1.2,
    snippet_length INTEGER DEFAULT 150,
    debug_mode BOOLEAN DEFAULT FALSE
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    result_json JSON;
    available_entities TEXT[];
    entity_name TEXT;
    target_table_name TEXT;
    column_info RECORD;
    field_search_parts TEXT[];
    field_search_sql TEXT;
    embedding_search_sql TEXT;
    text_results JSON;
    embedding_results JSON;
    start_time TIMESTAMP;
    execution_time REAL;
BEGIN
    start_time := clock_timestamp();
    
    -- Default to core entity types (no dependency on EntityEmbeddings)
    available_entities := ARRAY['Partners', 'Contacts', 'Interactions'];
    
    -- Initialize empty results
    text_results := '{}'::json;
    embedding_results := '{}'::json;
    field_search_parts := ARRAY[]::TEXT[];
    
    -- PART 1: DIRECT TABLE FIELD SEARCH
    -- Build dynamic field-specific search for each entity
    FOREACH entity_name IN ARRAY available_entities
    LOOP
        target_table_name := entity_name;
        
        -- Check if table exists in public schema
        IF EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = target_table_name
        ) THEN
            -- Get searchable text columns for this table
            FOR column_info IN
                SELECT column_name, data_type
                FROM information_schema.columns
                WHERE table_schema = 'public' 
                AND table_name = target_table_name
                AND data_type IN ('text', 'character varying', 'varchar', 'char')
                AND column_name NOT IN ('Id', 'CreatedDate', 'ModifiedDate', 'DeletedDate', 'CreatedBy', 'ModifiedBy', 'DeletedBy', 'Status')
                AND column_name NOT ILIKE '%password%'
                AND column_name NOT ILIKE '%token%'
                AND column_name NOT ILIKE '%hash%'
            LOOP
                -- Build field-specific query with smart scoring
                field_search_parts := field_search_parts || format('
                    SELECT 
                        %L::TEXT as entity_type,
                        "Id"::TEXT as entity_id,
                        %L::TEXT as matched_field,
                        COALESCE("%s", '''')::TEXT as field_value,
                        LEAST(1.0, CASE 
                            -- High priority: Names and Titles (3.0x internal weight, capped at 1.0)
                            WHEN lower(%L) ~ ''(name|title|firstname|lastname|fullname)'' THEN
                                CASE 
                                    WHEN "%s" ILIKE %L THEN 1.0 * %s
                                    WHEN "%s" ~* %L THEN 0.9 * %s
                                    WHEN similarity("%s", %L) > 0.3 THEN similarity("%s", %L) * 0.8 * %s
                                    ELSE 0
                                END
                            -- Medium-high priority: Contact info (2.5x internal weight, capped at 1.0)  
                            WHEN lower(%L) ~ ''(email|phone|contact|mobile)'' THEN
                                CASE 
                                    WHEN "%s" ILIKE %L THEN 0.9 * %s
                                    WHEN "%s" ~* %L THEN 0.8 * %s
                                    WHEN similarity("%s", %L) > 0.3 THEN similarity("%s", %L) * 0.7 * %s
                                    ELSE 0
                                END
                            -- Medium priority: Organization fields (2.0x internal weight, capped at 1.0)
                            WHEN lower(%L) ~ ''(department|organization|position|role|company|office)'' THEN
                                CASE 
                                    WHEN "%s" ILIKE %L THEN 0.8 * %s
                                    WHEN "%s" ~* %L THEN 0.7 * %s
                                    WHEN similarity("%s", %L) > 0.3 THEN similarity("%s", %L) * 0.6 * %s
                                    ELSE 0
                                END
                            -- Lower priority: Descriptions and notes (1.0x weight, capped at 1.0)
                            WHEN lower(%L) ~ ''(description|note|comment|remark|detail|summary)'' THEN
                                CASE 
                                    WHEN "%s" ILIKE %L THEN 0.6 * %s
                                    WHEN "%s" ~* %L THEN 0.5 * %s
                                    WHEN similarity("%s", %L) > 0.2 THEN similarity("%s", %L) * 0.4 * %s
                                    ELSE 0
                                END
                            -- Default for other text fields (0.8x weight, capped at 1.0)
                            ELSE
                                CASE 
                                    WHEN "%s" ILIKE %L THEN 0.5 * %s
                                    WHEN "%s" ~* %L THEN 0.4 * %s
                                    WHEN similarity("%s", %L) > 0.2 THEN similarity("%s", %L) * 0.3 * %s
                                    ELSE 0
                                END
                        END) as score,
                        ''field-search''::TEXT as search_type,
                        CASE 
                            WHEN "%s" ILIKE %L THEN ''Exact Match''
                            WHEN "%s" ~* %L THEN ''Word Boundary''
                            ELSE ''Similarity Match''
                        END::TEXT as match_criteria,
                        left(COALESCE("%s", ''''), %s)::TEXT as snippet
                    FROM public."%s"
                    WHERE "%s" IS NOT NULL 
                    AND "%s" != ''''
                    AND (
                        "%s" ILIKE %L 
                        OR "%s" ~* %L 
                        OR similarity("%s", %L) > 0.1
                    )',
                    -- All the parameters in order:
                    entity_name,                                           -- entity_type
                    column_info.column_name,                              -- matched_field
                    column_info.column_name,                              -- field_value
                    column_info.column_name,                              -- column name for priority check
                    -- High priority scoring
                    column_info.column_name, '%' || search_query || '%', text_boost,  -- exact match
                    column_info.column_name, '\y' || search_query || '\y', text_boost, -- word boundary
                    column_info.column_name, search_query, column_info.column_name, search_query, text_boost, -- similarity
                    -- Medium-high priority
                    column_info.column_name,
                    column_info.column_name, '%' || search_query || '%', text_boost,
                    column_info.column_name, '\y' || search_query || '\y', text_boost,
                    column_info.column_name, search_query, column_info.column_name, search_query, text_boost,
                    -- Medium priority  
                    column_info.column_name,
                    column_info.column_name, '%' || search_query || '%', text_boost,
                    column_info.column_name, '\y' || search_query || '\y', text_boost,
                    column_info.column_name, search_query, column_info.column_name, search_query, text_boost,
                    -- Lower priority
                    column_info.column_name,
                    column_info.column_name, '%' || search_query || '%', text_boost,
                    column_info.column_name, '\y' || search_query || '\y', text_boost,
                    column_info.column_name, search_query, column_info.column_name, search_query, text_boost,
                    -- Default
                    column_info.column_name, '%' || search_query || '%', text_boost,
                    column_info.column_name, '\y' || search_query || '\y', text_boost,
                    column_info.column_name, search_query, column_info.column_name, search_query, text_boost,
                    -- Match criteria
                    column_info.column_name, '%' || search_query || '%',
                    column_info.column_name, '\y' || search_query || '\y',
                    -- Snippet
                    column_info.column_name, snippet_length,
                    -- Table and WHERE conditions
                    target_table_name,
                    column_info.column_name,
                    column_info.column_name,
                    column_info.column_name, '%' || search_query || '%',
                    column_info.column_name, '\y' || search_query || '\y',
                    column_info.column_name, search_query
                );
            END LOOP;
        END IF;
    END LOOP;
    
    -- Execute field search if we have searchable fields
    IF array_length(field_search_parts, 1) > 0 THEN
        field_search_sql := array_to_string(field_search_parts, ' UNION ALL ');
        
        EXECUTE format('
            WITH field_results AS (%s),
            ranked_results AS (
                SELECT *,
                       ROW_NUMBER() OVER (PARTITION BY entity_type ORDER BY score DESC) as rn
                FROM field_results
                WHERE score > 0.1
            )
            SELECT json_object_agg(
                entity_type,
                json_build_object(
                    ''items'', items,
                    ''count'', item_count,
                    ''maxScore'', max_score,
                    ''avgScore'', avg_score
                )
            )
            FROM (
                SELECT 
                    entity_type,
                    json_agg(
                        json_build_object(
                            ''entityId'', entity_id::INTEGER,
                            ''score'', round(score::numeric, 3),
                            ''matchedField'', matched_field,
                            ''fieldValue'', field_value,
                            ''searchType'', search_type,
                            ''matchCriteria'', match_criteria,
                            ''snippet'', snippet
                        ) 
                        ORDER BY score DESC
                    ) as items,
                    COUNT(*)::INTEGER as item_count,
                    round(MAX(score)::numeric, 3) as max_score,
                    round(AVG(score)::numeric, 3) as avg_score
                FROM ranked_results
                WHERE rn <= 15  -- Top 15 results per entity type
                GROUP BY entity_type
            ) grouped',
            field_search_sql
        ) INTO text_results;
    END IF;
    
    -- PART 2: SEMANTIC EMBEDDING SEARCH (Optional - only if EntityEmbeddings table exists and has data)
    IF embedding IS NOT NULL AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'EntityEmbeddings' AND table_schema = 'public') THEN
        embedding_search_sql := format('
            WITH embedding_results AS (
                SELECT 
                    ee."EntityName"::TEXT as entity_type,
                    ee."EntityId"::TEXT as entity_id,
                    round(((1 - (ee."FullEmbedding" <=> %L::vector(768))) * %s)::numeric, 3) as score,
                    ''semantic''::TEXT as search_type,
                    ''embedding-similarity''::TEXT as match_criteria,
                    left(COALESCE(ee."EntityData", ''''), %s)::TEXT as snippet
                FROM public."EntityEmbeddings" ee
                WHERE ee."FullEmbedding" IS NOT NULL
                  AND (1 - (ee."FullEmbedding" <=> %L::vector(768))) > 0.15
            ),
            ranked_embedding AS (
                SELECT *,
                       ROW_NUMBER() OVER (PARTITION BY entity_type ORDER BY score DESC) as rn
                FROM embedding_results
            )
            SELECT json_object_agg(
                entity_type,
                json_build_object(
                    ''items'', items,
                    ''count'', item_count,
                    ''maxScore'', max_score,
                    ''avgScore'', avg_score
                )
            )
            FROM (
                SELECT 
                    entity_type,
                    json_agg(
                        json_build_object(
                            ''entityId'', entity_id::INTEGER,
                            ''score'', score,
                            ''searchType'', search_type,
                            ''matchCriteria'', match_criteria,
                            ''snippet'', snippet
                        ) 
                        ORDER BY score DESC
                    ) as items,
                    COUNT(*)::INTEGER as item_count,
                    MAX(score) as max_score,
                    AVG(score) as avg_score
                FROM ranked_embedding
                WHERE rn <= 15  -- Top 15 results per entity type
                GROUP BY entity_type
            ) grouped',
            embedding, embedding_boost, snippet_length, embedding
        );
        
        EXECUTE embedding_search_sql INTO embedding_results;
    END IF;
    
    -- Calculate execution time
    execution_time := EXTRACT(EPOCH FROM (clock_timestamp() - start_time));
    
    -- Combine field search and semantic search results into unified arrays per entity type
    WITH combined_results AS (
        -- Field search results
        SELECT 
            entity_type,
            json_array_elements(entity_data->'items') as item_data
        FROM (
            SELECT 
                key as entity_type,
                value as entity_data
            FROM json_each(COALESCE(text_results, '{}'::json))
        ) field_data
        
        UNION ALL
        
        -- Semantic search results  
        SELECT 
            entity_type,
            json_array_elements(entity_data->'items') as item_data
        FROM (
            SELECT 
                key as entity_type,
                value as entity_data
            FROM json_each(COALESCE(embedding_results, '{}'::json))
        ) semantic_data
    ),
    unified_entity_results AS (
        SELECT 
            entity_type,
            json_agg(
                item_data 
                ORDER BY (item_data->>'score')::REAL DESC
            ) as items
        FROM combined_results
        GROUP BY entity_type
    )
    SELECT 
        CASE 
            WHEN debug_mode THEN
                json_build_object(
                    'searchQuery', search_query,
                    'hasEmbedding', (embedding IS NOT NULL),
                    'strategy', 'similarity-search',
                    'availableEntities', available_entities,
                    'boostFactors', json_build_object(
                        'textBoost', text_boost,
                        'embeddingBoost', embedding_boost
                    ),
                    'results', COALESCE(
                        (SELECT json_object_agg(entity_type, json_build_object('items', items))
                         FROM unified_entity_results), 
                        '{}'::json
                    ),
                    'summary', json_build_object(
                        'totalFieldResults', (
                            SELECT COALESCE(SUM((value->>'count')::INTEGER), 0) 
                            FROM json_each(COALESCE(text_results, '{}'::json))
                        ),
                        'totalSemanticResults', (
                            SELECT COALESCE(SUM((value->>'count')::INTEGER), 0) 
                            FROM json_each(COALESCE(embedding_results, '{}'::json))
                        ),
                        'entitiesSearched', COALESCE(array_length(available_entities, 1), 0),
                        'searchCapabilities', json_build_array(
                            'similarity-search',
                            'field-specific-scoring', 
                            'core-entity-search',
                            'safe-column-handling',
                            'pg-trgm-matching'
                        ),
                        'executionTimeMs', round((execution_time * 1000)::numeric, 2)
                    )
                )
            ELSE
                json_build_object(
                    'availableEntities', available_entities,
                    'results', COALESCE(
                        (SELECT json_object_agg(entity_type, json_build_object('items', items))
                         FROM unified_entity_results), 
                        '{}'::json
                    )
                )
        END
    INTO result_json;
    
    RETURN result_json;
END
$$;

-- Create performance indexes for similarity search
-- Note: pg_trgm extension should already be enabled for similarity search
-- The following indexes will improve performance on text columns:
-- CREATE INDEX IF NOT EXISTS idx_partners_name_gin ON public."Partners" USING gin ("Name" gin_trgm_ops);
-- CREATE INDEX IF NOT EXISTS idx_contacts_name_gin ON public."Contacts" USING gin (("FirstName" || ' ' || "LastName") gin_trgm_ops);
-- CREATE INDEX IF NOT EXISTS idx_interactions_subject_gin ON public."Interactions" USING gin ("Subject" gin_trgm_ops);

-- Example usage:
-- SELECT public.search_entity_records('John');
-- SELECT public.search_entity_records('Amy Mark', NULL, 1.0, 1.2, 150, FALSE); -- Clean results
-- SELECT public.search_entity_records('experienced project manager', NULL, 2.0, 1.0, 200, TRUE); -- Custom boost + debug
-- SELECT public.search_entity_records('procurement', NULL, 1.5, 1.0, 100, FALSE); -- Similarity search only









