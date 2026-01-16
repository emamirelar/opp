-- ============================================================================
-- MODULAR INTELLIGENT HYBRID ENTITY SEARCH FUNCTIONS WITH NESTED PROPERTIES
-- ============================================================================
-- This PostgreSQL script provides advanced hybrid search capabilities that
-- intelligently combines field-specific text search with semantic embedding search
-- across all entity types in the PAO system, INCLUDING NESTED PROPERTIES.
--
-- MODULAR APPROACH:
-- - Separate functions for each entity type for better maintainability
-- - Main orchestrator function that calls entity-specific functions
-- - Consistent scoring and pattern matching across all entities
-- - Supports entity filtering for both global and specific searches
-- ============================================================================

-- Enable required extensions
--CREATE EXTENSION IF NOT EXISTS pg_trgm;
--CREATE EXTENSION IF NOT EXISTS vector;

-- Drop existing functions to ensure clean recreation
DROP FUNCTION IF EXISTS public.search_partners_with_nested(TEXT, REAL, INTEGER);
DROP FUNCTION IF EXISTS public.search_contacts_with_nested(TEXT, REAL, INTEGER);
DROP FUNCTION IF EXISTS public.search_interactions_with_nested(TEXT, REAL, INTEGER);
DROP FUNCTION IF EXISTS public.search_opportunities_with_nested(TEXT, REAL, INTEGER);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector, REAL, REAL, INTEGER);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector, REAL, REAL, INTEGER, BOOLEAN);
DROP FUNCTION IF EXISTS public.search_entity_records(TEXT, vector, REAL, REAL, INTEGER, BOOLEAN, TEXT[]);

-- ============================================================================
-- PARTNERS SEARCH FUNCTION WITH NESTED PROPERTIES
-- OPTIMIZED: Prioritizes primary fields (Id, Name, ShortDesc, LongDesc) with comprehensive nested search
-- ============================================================================
CREATE OR REPLACE FUNCTION public.search_partners_with_nested(
    search_query TEXT,
    text_boost REAL DEFAULT 1.0,
    snippet_length INTEGER DEFAULT 150
)
RETURNS TABLE (
    entity_type TEXT,
    entity_id TEXT,
    matched_field TEXT,
    field_value TEXT,
    score DOUBLE PRECISION,
    search_type TEXT,
    match_criteria TEXT,
    snippet TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    exact_pattern TEXT;
    word_pattern TEXT;
    search_id INTEGER;
    is_numeric_search BOOLEAN;
BEGIN
    -- Prepare search patterns
    exact_pattern := '%' || search_query || '%';
    word_pattern := '% ' || search_query || ' %';
    
    -- Check if search query is numeric for ID search
    BEGIN
        search_id := search_query::INTEGER;
        is_numeric_search := TRUE;
    EXCEPTION WHEN OTHERS THEN
        is_numeric_search := FALSE;
    END;
    
    RETURN QUERY
    WITH all_matches AS (
        SELECT 
            'Partners'::TEXT as entity_type,
            p."Id"::TEXT as entity_id,
            fields.matched_field,
            fields.field_value,
            fields.score,
            'field-search'::TEXT as search_type,
            CASE 
                WHEN fields.field_value ILIKE exact_pattern THEN 'Exact Match'
                WHEN fields.field_value ILIKE word_pattern THEN 'Word Match'
                ELSE 'Similarity Match'
            END::TEXT as match_criteria,
            left(fields.field_value, snippet_length)::TEXT as snippet
        FROM public."Partners" p
        LEFT JOIN public."PartnerTrees" pg ON p."PartnerGroupId" = pg."Id"
        LEFT JOIN public."LiaisonOffices" lo ON p."LiaisonOfficeId" = lo."Id"
        LEFT JOIN public."Contacts" c ON p."Id" = c."PartnerId"
        CROSS JOIN LATERAL (
            VALUES
                -- TIER 1 - PRIMARY FIELDS (Score: 1.0-0.85) - PRIORITY ORDER
                -- ID exact match (highest priority)
                ('Id', p."Id"::TEXT,
                 CASE WHEN is_numeric_search AND p."Id" = search_id THEN 1.0 * text_boost
                      ELSE 0 END),
                
                -- Name (primary identifier)
                ('Name', COALESCE(p."Name", ''), 
                 CASE WHEN p."Name" ILIKE exact_pattern THEN 0.95 * text_boost
                      WHEN p."Name" ILIKE word_pattern THEN 0.90 * text_boost
                      WHEN similarity(COALESCE(p."Name", ''), search_query) > 0.2 THEN similarity(COALESCE(p."Name", ''), search_query) * 0.85 * text_boost
                      ELSE 0 END),
                
                -- PartnerShortDescription
                ('PartnerShortDescription', COALESCE(p."PartnerShortDescription", ''),
                 CASE WHEN p."PartnerShortDescription" ILIKE exact_pattern THEN 0.90 * text_boost
                      WHEN p."PartnerShortDescription" ILIKE word_pattern THEN 0.85 * text_boost
                      WHEN similarity(COALESCE(p."PartnerShortDescription", ''), search_query) > 0.2 THEN similarity(COALESCE(p."PartnerShortDescription", ''), search_query) * 0.80 * text_boost
                      ELSE 0 END),
                
                -- PartnerLongDescription
                ('PartnerLongDescription', COALESCE(p."PartnerLongDescription", ''),
                 CASE WHEN p."PartnerLongDescription" ILIKE exact_pattern THEN 0.85 * text_boost
                      WHEN p."PartnerLongDescription" ILIKE word_pattern THEN 0.80 * text_boost
                      WHEN similarity(COALESCE(p."PartnerLongDescription", ''), search_query) > 0.2 THEN similarity(COALESCE(p."PartnerLongDescription", ''), search_query) * 0.75 * text_boost
                      ELSE 0 END),
                
                -- TIER 2 - NESTED PROPERTIES (Score: 0.7-0.5)
                ('PartnerGroup.Name', COALESCE(pg."Name", ''),
                 CASE WHEN pg."Name" ILIKE exact_pattern THEN 0.7 * text_boost
                      WHEN pg."Name" ILIKE word_pattern THEN 0.6 * text_boost
                      WHEN similarity(COALESCE(pg."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(pg."Name", ''), search_query) * 0.5 * text_boost
                      ELSE 0 END),
                ('PartnerGroup.Code', COALESCE(pg."Code", ''),
                 CASE WHEN pg."Code" ILIKE exact_pattern THEN 0.6 * text_boost
                      WHEN pg."Code" ILIKE word_pattern THEN 0.5 * text_boost
                      WHEN similarity(COALESCE(pg."Code", ''), search_query) > 0.3 THEN similarity(COALESCE(pg."Code", ''), search_query) * 0.4 * text_boost
                      ELSE 0 END),
                
                ('LiaisonOffice.Name', COALESCE(lo."Name", ''),
                 CASE WHEN lo."Name" ILIKE exact_pattern THEN 0.5 * text_boost
                      WHEN lo."Name" ILIKE word_pattern THEN 0.4 * text_boost
                      WHEN similarity(COALESCE(lo."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(lo."Name", ''), search_query) * 0.3 * text_boost
                      ELSE 0 END),
                
                -- TIER 3 - RELATED CONTACTS (Score: 0.3-0.2)
                ('Contact.FullName', COALESCE(c."FirstName" || ' ' || c."LastName", ''),
                 CASE WHEN (c."FirstName" || ' ' || c."LastName") ILIKE exact_pattern THEN 0.3 * text_boost
                      WHEN (c."FirstName" || ' ' || c."LastName") ILIKE word_pattern THEN 0.2 * text_boost
                      WHEN similarity(COALESCE(c."FirstName" || ' ' || c."LastName", ''), search_query) > 0.3 THEN similarity(COALESCE(c."FirstName" || ' ' || c."LastName", ''), search_query) * 0.15 * text_boost
                      ELSE 0 END),
                ('Contact.Email', COALESCE(c."Email", ''),
                 CASE WHEN c."Email" ILIKE exact_pattern THEN 0.2 * text_boost
                      WHEN c."Email" ILIKE word_pattern THEN 0.15 * text_boost
                      WHEN similarity(COALESCE(c."Email", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Email", ''), search_query) * 0.1 * text_boost
                      ELSE 0 END)
        ) AS fields(matched_field, field_value, score)
        WHERE fields.score > 0.1
        AND fields.field_value IS NOT NULL 
        AND fields.field_value != ''
    ),
    best_matches AS (
        SELECT DISTINCT ON (all_matches.entity_id)
            all_matches.entity_type,
            all_matches.entity_id,
            all_matches.matched_field,
            all_matches.field_value,
            all_matches.score,
            all_matches.search_type,
            all_matches.match_criteria,
            all_matches.snippet
        FROM all_matches
        ORDER BY all_matches.entity_id, all_matches.score DESC
    )
    SELECT * FROM best_matches
    ORDER BY score DESC;
END
$$;

-- ============================================================================
-- CONTACTS SEARCH FUNCTION WITH NESTED PROPERTIES
-- COMPREHENSIVE: All fields for advanced search with proper tiering
-- ============================================================================
CREATE OR REPLACE FUNCTION public.search_contacts_with_nested(
    search_query TEXT,
    text_boost REAL DEFAULT 1.0,
    snippet_length INTEGER DEFAULT 150
)
RETURNS TABLE (
    entity_type TEXT,
    entity_id TEXT,
    matched_field TEXT,
    field_value TEXT,
    score DOUBLE PRECISION,
    search_type TEXT,
    match_criteria TEXT,
    snippet TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    exact_pattern TEXT;
    word_pattern TEXT;
    search_id INTEGER;
    is_numeric_search BOOLEAN;
BEGIN
    -- Prepare search patterns
    exact_pattern := '%' || search_query || '%';
    word_pattern := '% ' || search_query || ' %';
    
    -- Check if search query is numeric for ID search
    BEGIN
        search_id := search_query::INTEGER;
        is_numeric_search := TRUE;
    EXCEPTION WHEN OTHERS THEN
        is_numeric_search := FALSE;
    END;
    
    RETURN QUERY
    WITH all_matches AS (
        SELECT 
            'Contacts'::TEXT as entity_type,
            c."Id"::TEXT as entity_id,
            fields.matched_field,
            fields.field_value,
            fields.score,
            'field-search'::TEXT as search_type,
            CASE 
                WHEN fields.field_value ILIKE exact_pattern THEN 'Exact Match'
                WHEN fields.field_value ILIKE word_pattern THEN 'Word Match'
                ELSE 'Similarity Match'
            END::TEXT as match_criteria,
            left(fields.field_value, snippet_length)::TEXT as snippet
        FROM public."Contacts" c
        LEFT JOIN public."Partners" p ON c."PartnerId" = p."Id"
        LEFT JOIN public."PartnerTrees" pg ON p."PartnerGroupId" = pg."Id"
        LEFT JOIN public."LiaisonOffices" lo ON p."LiaisonOfficeId" = lo."Id"
        CROSS JOIN LATERAL (
            VALUES
                -- TIER 1 - CORE IDENTITY FIELDS (Score: 1.0-0.7)
                -- ID exact match (highest priority)
                ('Id', c."Id"::TEXT,
                 CASE WHEN is_numeric_search AND c."Id" = search_id THEN 1.0 * text_boost
                      ELSE 0 END),
                
                -- Full Name (concatenated)
                ('FullName', COALESCE(TRIM(CONCAT(c."FirstName", ' ', c."MiddleName", ' ', c."LastName")), ''),
                 CASE WHEN TRIM(CONCAT(c."FirstName", ' ', c."MiddleName", ' ', c."LastName")) ILIKE exact_pattern THEN 0.95 * text_boost
                      WHEN TRIM(CONCAT(c."FirstName", ' ', c."MiddleName", ' ', c."LastName")) ILIKE word_pattern THEN 0.90 * text_boost
                      WHEN similarity(COALESCE(TRIM(CONCAT(c."FirstName", ' ', c."MiddleName", ' ', c."LastName")), ''), search_query) > 0.2 THEN similarity(COALESCE(TRIM(CONCAT(c."FirstName", ' ', c."MiddleName", ' ', c."LastName")), ''), search_query) * 0.85 * text_boost
                      ELSE 0 END),
                
                -- FirstName
                ('FirstName', COALESCE(c."FirstName", ''), 
                 CASE WHEN c."FirstName" ILIKE exact_pattern THEN 0.95 * text_boost
                      WHEN c."FirstName" ILIKE word_pattern THEN 0.90 * text_boost
                      WHEN similarity(COALESCE(c."FirstName", ''), search_query) > 0.2 THEN similarity(COALESCE(c."FirstName", ''), search_query) * 0.85 * text_boost
                      ELSE 0 END),
                
                -- MiddleName
                ('MiddleName', COALESCE(c."MiddleName", ''),
                 CASE WHEN c."MiddleName" ILIKE exact_pattern THEN 0.90 * text_boost
                      WHEN c."MiddleName" ILIKE word_pattern THEN 0.85 * text_boost
                      WHEN similarity(COALESCE(c."MiddleName", ''), search_query) > 0.2 THEN similarity(COALESCE(c."MiddleName", ''), search_query) * 0.80 * text_boost
                      ELSE 0 END),
                
                -- LastName
                ('LastName', COALESCE(c."LastName", ''),
                 CASE WHEN c."LastName" ILIKE exact_pattern THEN 0.95 * text_boost
                      WHEN c."LastName" ILIKE word_pattern THEN 0.90 * text_boost
                      WHEN similarity(COALESCE(c."LastName", ''), search_query) > 0.2 THEN similarity(COALESCE(c."LastName", ''), search_query) * 0.85 * text_boost
                      ELSE 0 END),
                
                -- Email
                ('Email', COALESCE(c."Email", ''),
                 CASE WHEN c."Email" ILIKE exact_pattern THEN 0.85 * text_boost
                      WHEN c."Email" ILIKE word_pattern THEN 0.80 * text_boost
                      WHEN similarity(COALESCE(c."Email", ''), search_query) > 0.2 THEN similarity(COALESCE(c."Email", ''), search_query) * 0.75 * text_boost
                      ELSE 0 END),
                
                -- Title
                ('Title', COALESCE(c."Title", ''),
                 CASE WHEN c."Title" ILIKE exact_pattern THEN 0.80 * text_boost
                      WHEN c."Title" ILIKE word_pattern THEN 0.75 * text_boost
                      WHEN similarity(COALESCE(c."Title", ''), search_query) > 0.2 THEN similarity(COALESCE(c."Title", ''), search_query) * 0.70 * text_boost
                      ELSE 0 END),
                
                -- TIER 2 - ADDITIONAL CONTACT DETAILS (Score: 0.65-0.45)
                ('Salutation', COALESCE(c."Salutation", ''),
                 CASE WHEN c."Salutation" ILIKE exact_pattern THEN 0.65 * text_boost
                      WHEN c."Salutation" ILIKE word_pattern THEN 0.60 * text_boost
                      WHEN similarity(COALESCE(c."Salutation", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Salutation", ''), search_query) * 0.55 * text_boost
                      ELSE 0 END),
                
                ('Suffix', COALESCE(c."Suffix", ''),
                 CASE WHEN c."Suffix" ILIKE exact_pattern THEN 0.65 * text_boost
                      WHEN c."Suffix" ILIKE word_pattern THEN 0.60 * text_boost
                      WHEN similarity(COALESCE(c."Suffix", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Suffix", ''), search_query) * 0.55 * text_boost
                      ELSE 0 END),
                
                ('Department', COALESCE(c."Department", ''),
                 CASE WHEN c."Department" ILIKE exact_pattern THEN 0.60 * text_boost
                      WHEN c."Department" ILIKE word_pattern THEN 0.55 * text_boost
                      WHEN similarity(COALESCE(c."Department", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Department", ''), search_query) * 0.50 * text_boost
                      ELSE 0 END),
                
                ('Description', COALESCE(c."Description", ''),
                 CASE WHEN c."Description" ILIKE exact_pattern THEN 0.60 * text_boost
                      WHEN c."Description" ILIKE word_pattern THEN 0.55 * text_boost
                      WHEN similarity(COALESCE(c."Description", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Description", ''), search_query) * 0.50 * text_boost
                      ELSE 0 END),
                
                ('Phone', COALESCE(c."Phone", ''),
                 CASE WHEN c."Phone" ILIKE exact_pattern THEN 0.55 * text_boost
                      WHEN c."Phone" ILIKE word_pattern THEN 0.50 * text_boost
                      WHEN similarity(COALESCE(c."Phone", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Phone", ''), search_query) * 0.45 * text_boost
                      ELSE 0 END),
                
                ('Mobile', COALESCE(c."Mobile", ''),
                 CASE WHEN c."Mobile" ILIKE exact_pattern THEN 0.55 * text_boost
                      WHEN c."Mobile" ILIKE word_pattern THEN 0.50 * text_boost
                      WHEN similarity(COALESCE(c."Mobile", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Mobile", ''), search_query) * 0.45 * text_boost
                      ELSE 0 END),
                
                ('Assistant', COALESCE(c."Assistant", ''),
                 CASE WHEN c."Assistant" ILIKE exact_pattern THEN 0.50 * text_boost
                      WHEN c."Assistant" ILIKE word_pattern THEN 0.45 * text_boost
                      WHEN similarity(COALESCE(c."Assistant", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Assistant", ''), search_query) * 0.40 * text_boost
                      ELSE 0 END),
                
                ('AssistantPhone', COALESCE(c."AssistantPhone", ''),
                 CASE WHEN c."AssistantPhone" ILIKE exact_pattern THEN 0.50 * text_boost
                      WHEN c."AssistantPhone" ILIKE word_pattern THEN 0.45 * text_boost
                      WHEN similarity(COALESCE(c."AssistantPhone", ''), search_query) > 0.3 THEN similarity(COALESCE(c."AssistantPhone", ''), search_query) * 0.40 * text_boost
                      ELSE 0 END),
                
                ('AssistantEmail', COALESCE(c."AssistantEmail", ''),
                 CASE WHEN c."AssistantEmail" ILIKE exact_pattern THEN 0.50 * text_boost
                      WHEN c."AssistantEmail" ILIKE word_pattern THEN 0.45 * text_boost
                      WHEN similarity(COALESCE(c."AssistantEmail", ''), search_query) > 0.3 THEN similarity(COALESCE(c."AssistantEmail", ''), search_query) * 0.40 * text_boost
                      ELSE 0 END),
                
                -- TIER 3 - NESTED/RELATED FIELDS (Score: 0.4-0.2)
                ('Partner.Name', COALESCE(p."Name", ''),
                 CASE WHEN p."Name" ILIKE exact_pattern THEN 0.40 * text_boost
                      WHEN p."Name" ILIKE word_pattern THEN 0.35 * text_boost
                      WHEN similarity(COALESCE(p."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(p."Name", ''), search_query) * 0.30 * text_boost
                      ELSE 0 END),
                
                ('Partner.PartnerShortDescription', COALESCE(p."PartnerShortDescription", ''),
                 CASE WHEN p."PartnerShortDescription" ILIKE exact_pattern THEN 0.35 * text_boost
                      WHEN p."PartnerShortDescription" ILIKE word_pattern THEN 0.30 * text_boost
                      WHEN similarity(COALESCE(p."PartnerShortDescription", ''), search_query) > 0.3 THEN similarity(COALESCE(p."PartnerShortDescription", ''), search_query) * 0.25 * text_boost
                      ELSE 0 END),
                
                ('Partner.PartnerLongDescription', COALESCE(p."PartnerLongDescription", ''),
                 CASE WHEN p."PartnerLongDescription" ILIKE exact_pattern THEN 0.35 * text_boost
                      WHEN p."PartnerLongDescription" ILIKE word_pattern THEN 0.30 * text_boost
                      WHEN similarity(COALESCE(p."PartnerLongDescription", ''), search_query) > 0.3 THEN similarity(COALESCE(p."PartnerLongDescription", ''), search_query) * 0.25 * text_boost
                      ELSE 0 END),
                
                ('PartnerGroup.Name', COALESCE(pg."Name", ''),
                 CASE WHEN pg."Name" ILIKE exact_pattern THEN 0.30 * text_boost
                      WHEN pg."Name" ILIKE word_pattern THEN 0.25 * text_boost
                      WHEN similarity(COALESCE(pg."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(pg."Name", ''), search_query) * 0.20 * text_boost
                      ELSE 0 END),
                
                ('LiaisonOffice.Name', COALESCE(lo."Name", ''),
                 CASE WHEN lo."Name" ILIKE exact_pattern THEN 0.30 * text_boost
                      WHEN lo."Name" ILIKE word_pattern THEN 0.25 * text_boost
                      WHEN similarity(COALESCE(lo."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(lo."Name", ''), search_query) * 0.20 * text_boost
                      ELSE 0 END)
        ) AS fields(matched_field, field_value, score)
        WHERE fields.score > 0.1
        AND fields.field_value IS NOT NULL 
        AND fields.field_value != ''
    ),
    best_matches AS (
        SELECT DISTINCT ON (all_matches.entity_id)
            all_matches.entity_type,
            all_matches.entity_id,
            all_matches.matched_field,
            all_matches.field_value,
            all_matches.score,
            all_matches.search_type,
            all_matches.match_criteria,
            all_matches.snippet
        FROM all_matches
        ORDER BY all_matches.entity_id, all_matches.score DESC
    )
    SELECT * FROM best_matches
    ORDER BY score DESC;
END
$$;

-- ============================================================================
-- INTERACTIONS SEARCH FUNCTION WITH NESTED PROPERTIES
-- OPTIMIZED: Prioritizes primary fields (Id, Type, Subject, Description, Location) with nested search
-- ============================================================================
CREATE OR REPLACE FUNCTION public.search_interactions_with_nested(
    search_query TEXT,
    text_boost REAL DEFAULT 1.0,
    snippet_length INTEGER DEFAULT 150
)
RETURNS TABLE (
    entity_type TEXT,
    entity_id TEXT,
    matched_field TEXT,
    field_value TEXT,
    score DOUBLE PRECISION,
    search_type TEXT,
    match_criteria TEXT,
    snippet TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    exact_pattern TEXT;
    word_pattern TEXT;
    search_id INTEGER;
    is_numeric_search BOOLEAN;
BEGIN
    -- Prepare search patterns
    exact_pattern := '%' || search_query || '%';
    word_pattern := '% ' || search_query || ' %';
    
    -- Check if search query is numeric for ID search
    BEGIN
        search_id := search_query::INTEGER;
        is_numeric_search := TRUE;
    EXCEPTION WHEN OTHERS THEN
        is_numeric_search := FALSE;
    END;
    
    RETURN QUERY
    WITH all_matches AS (
        SELECT 
            'Interactions'::TEXT as entity_type,
            i."Id"::TEXT as entity_id,
            fields.matched_field,
            fields.field_value,
            fields.score,
            'field-search'::TEXT as search_type,
            CASE 
                WHEN fields.field_value ILIKE exact_pattern THEN 'Exact Match'
                WHEN fields.field_value ILIKE word_pattern THEN 'Word Match'
                ELSE 'Similarity Match'
            END::TEXT as match_criteria,
            left(fields.field_value, snippet_length)::TEXT as snippet
        FROM public."Interactions" i
        LEFT JOIN public."InteractionContacts" ic ON i."Id" = ic."InteractionId"
        LEFT JOIN public."Contacts" c ON ic."ContactId" = c."Id"
        LEFT JOIN public."InteractionPartners" ip ON i."Id" = ip."InteractionId"
        LEFT JOIN public."Partners" p ON ip."PartnerId" = p."Id"
        CROSS JOIN LATERAL (
            VALUES
                -- TIER 1 - PRIMARY FIELDS (Score: 1.0-0.7) - PRIORITY ORDER
                -- ID exact match (highest priority)
                ('Id', i."Id"::TEXT,
                 CASE WHEN is_numeric_search AND i."Id" = search_id THEN 1.0 * text_boost
                      ELSE 0 END),
                
                -- Type (enum) - cast to text for searching
                ('Type', i."Type"::TEXT,
                 CASE WHEN i."Type"::TEXT ILIKE exact_pattern THEN 0.95 * text_boost
                      WHEN i."Type"::TEXT ILIKE word_pattern THEN 0.90 * text_boost
                      -- Human-readable type matching
                      WHEN LOWER(search_query) LIKE '%virtual%' AND i."Type"::TEXT = 'VirtualMeeting' THEN 0.95 * text_boost
                      WHEN LOWER(search_query) LIKE '%person%' AND i."Type"::TEXT = 'InPersonMeeting' THEN 0.95 * text_boost
                      WHEN LOWER(search_query) LIKE '%meeting%' AND (i."Type"::TEXT = 'VirtualMeeting' OR i."Type"::TEXT = 'InPersonMeeting') THEN 0.90 * text_boost
                      WHEN LOWER(search_query) LIKE '%email%' AND i."Type"::TEXT = 'Email' THEN 0.95 * text_boost
                      WHEN LOWER(search_query) LIKE '%chat%' AND i."Type"::TEXT = 'Chat' THEN 0.95 * text_boost
                      WHEN LOWER(search_query) LIKE '%call%' AND i."Type"::TEXT = 'Call' THEN 0.95 * text_boost
                      ELSE 0 END),
                
                -- Subject
                ('Subject', COALESCE(i."Subject", ''), 
                 CASE WHEN i."Subject" ILIKE exact_pattern THEN 0.90 * text_boost
                      WHEN i."Subject" ILIKE word_pattern THEN 0.85 * text_boost
                      WHEN similarity(COALESCE(i."Subject", ''), search_query) > 0.2 THEN similarity(COALESCE(i."Subject", ''), search_query) * 0.80 * text_boost
                      ELSE 0 END),
                
                -- Description
                ('Description', COALESCE(i."Description", ''),
                 CASE WHEN i."Description" ILIKE exact_pattern THEN 0.80 * text_boost
                      WHEN i."Description" ILIKE word_pattern THEN 0.75 * text_boost
                      WHEN similarity(COALESCE(i."Description", ''), search_query) > 0.2 THEN similarity(COALESCE(i."Description", ''), search_query) * 0.70 * text_boost
                      ELSE 0 END),
                
                -- Location
                ('Location', COALESCE(i."Location", ''),
                 CASE WHEN i."Location" ILIKE exact_pattern THEN 0.70 * text_boost
                      WHEN i."Location" ILIKE word_pattern THEN 0.65 * text_boost
                      WHEN similarity(COALESCE(i."Location", ''), search_query) > 0.2 THEN similarity(COALESCE(i."Location", ''), search_query) * 0.60 * text_boost
                      ELSE 0 END),
                
                -- TIER 2 - RELATED ENTITIES (Score: 0.3-0.2)
                ('Contact.FullName', COALESCE(c."FirstName" || ' ' || c."LastName", ''),
                 CASE WHEN (c."FirstName" || ' ' || c."LastName") ILIKE exact_pattern THEN 0.3 * text_boost
                      WHEN (c."FirstName" || ' ' || c."LastName") ILIKE word_pattern THEN 0.2 * text_boost
                      WHEN similarity(COALESCE(c."FirstName" || ' ' || c."LastName", ''), search_query) > 0.3 THEN similarity(COALESCE(c."FirstName" || ' ' || c."LastName", ''), search_query) * 0.1 * text_boost
                      ELSE 0 END),
                ('Partner.Name', COALESCE(p."Name", ''),
                 CASE WHEN p."Name" ILIKE exact_pattern THEN 0.2 * text_boost
                      WHEN p."Name" ILIKE word_pattern THEN 0.1 * text_boost
                      WHEN similarity(COALESCE(p."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(p."Name", ''), search_query) * 0.05 * text_boost
                      ELSE 0 END)
        ) AS fields(matched_field, field_value, score)
        WHERE fields.score > 0.1
        AND fields.field_value IS NOT NULL 
        AND fields.field_value != ''
    ),
    best_matches AS (
        SELECT DISTINCT ON (all_matches.entity_id)
            all_matches.entity_type,
            all_matches.entity_id,
            all_matches.matched_field,
            all_matches.field_value,
            all_matches.score,
            all_matches.search_type,
            all_matches.match_criteria,
            all_matches.snippet
        FROM all_matches
        ORDER BY all_matches.entity_id, all_matches.score DESC
    )
    SELECT * FROM best_matches
    ORDER BY score DESC;
END
$$;

-- ============================================================================
-- OPPORTUNITIES SEARCH FUNCTION WITH NESTED PROPERTIES
-- OPTIMIZED: Prioritizes primary fields (Id, Name, Description, Challenges) with nested search
-- ============================================================================
CREATE OR REPLACE FUNCTION public.search_opportunities_with_nested(
    search_query TEXT,
    text_boost REAL DEFAULT 1.0,
    snippet_length INTEGER DEFAULT 150
)
RETURNS TABLE (
    entity_type TEXT,
    entity_id TEXT,
    matched_field TEXT,
    field_value TEXT,
    score DOUBLE PRECISION,
    search_type TEXT,
    match_criteria TEXT,
    snippet TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    exact_pattern TEXT;
    word_pattern TEXT;
    search_id INTEGER;
    is_numeric_search BOOLEAN;
BEGIN
    -- Prepare search patterns
    exact_pattern := '%' || search_query || '%';
    word_pattern := '% ' || search_query || ' %';
    
    -- Check if search query is numeric for ID search
    BEGIN
        search_id := search_query::INTEGER;
        is_numeric_search := TRUE;
    EXCEPTION WHEN OTHERS THEN
        is_numeric_search := FALSE;
    END;
    
    RETURN QUERY
    WITH all_matches AS (
        SELECT 
            'Opportunities'::TEXT as entity_type,
            o."Id"::TEXT as entity_id,
            fields.matched_field,
            fields.field_value,
            fields.score,
            'field-search'::TEXT as search_type,
            CASE 
                WHEN fields.field_value ILIKE exact_pattern THEN 'Exact Match'
                WHEN fields.field_value ILIKE word_pattern THEN 'Word Match'
                ELSE 'Similarity Match'
            END::TEXT as match_criteria,
            left(fields.field_value, snippet_length)::TEXT as snippet   
        FROM public."Opportunities" o
        LEFT JOIN public."WorkflowStages" ws ON o."WorkflowStageId" = ws."Id"
        LEFT JOIN public."OrganizationHierarchies" org ON o."ResponsibleOrgUnitId" = org."Id"
        LEFT JOIN public."ProposedInitiativeTypes" pit ON o."ProposedInitiativeTypeId" = pit."Id"
        LEFT JOIN public."OpportunityCountries" oc ON o."Id" = oc."OpportunityId"
        LEFT JOIN public."Countries" c ON oc."CountryId" = c."Id"
        LEFT JOIN public."OpportunitySDGs" osdg ON o."Id" = osdg."OpportunityId"
        LEFT JOIN public."SDGs" sdg ON osdg."SDGId" = sdg."Id"
        CROSS JOIN LATERAL (
            VALUES
                -- TIER 1 - PRIMARY FIELDS (Score: 1.0-0.7) - PRIORITY ORDER
                -- ID exact match (highest priority)
                ('Id', o."Id"::TEXT,
                 CASE WHEN is_numeric_search AND o."Id" = search_id THEN 1.0 * text_boost
                      ELSE 0 END),
                
                -- Name
                ('Name', COALESCE(o."Name", ''), 
                 CASE WHEN o."Name" ILIKE exact_pattern THEN 0.95 * text_boost
                      WHEN o."Name" ILIKE word_pattern THEN 0.90 * text_boost
                      WHEN similarity(COALESCE(o."Name", ''), search_query) > 0.2 THEN similarity(COALESCE(o."Name", ''), search_query) * 0.85 * text_boost
                      ELSE 0 END),
                
                -- Description
                ('Description', COALESCE(o."Description", ''),
                 CASE WHEN o."Description" ILIKE exact_pattern THEN 0.85 * text_boost
                      WHEN o."Description" ILIKE word_pattern THEN 0.80 * text_boost
                      WHEN similarity(COALESCE(o."Description", ''), search_query) > 0.2 THEN similarity(COALESCE(o."Description", ''), search_query) * 0.75 * text_boost
                      ELSE 0 END),
                
                -- Challenges
                ('Challenges', COALESCE(o."Challenges", ''),
                 CASE WHEN o."Challenges" ILIKE exact_pattern THEN 0.75 * text_boost
                      WHEN o."Challenges" ILIKE word_pattern THEN 0.70 * text_boost
                      WHEN similarity(COALESCE(o."Challenges", ''), search_query) > 0.2 THEN similarity(COALESCE(o."Challenges", ''), search_query) * 0.65 * text_boost
                      ELSE 0 END),
                
                -- TIER 2 - STRATEGIC INFORMATION (Score: 0.6-0.5)
                ('PartnerReference', COALESCE(o."PartnerReference", ''),
                 CASE WHEN o."PartnerReference" ILIKE exact_pattern THEN 0.6 * text_boost
                      WHEN o."PartnerReference" ILIKE word_pattern THEN 0.55 * text_boost
                      WHEN similarity(COALESCE(o."PartnerReference", ''), search_query) > 0.3 THEN similarity(COALESCE(o."PartnerReference", ''), search_query) * 0.50 * text_boost
                      ELSE 0 END),
                ('ResultsFocus', COALESCE(o."ResultsFocus", ''),
                 CASE WHEN o."ResultsFocus" ILIKE exact_pattern THEN 0.55 * text_boost
                      WHEN o."ResultsFocus" ILIKE word_pattern THEN 0.50 * text_boost
                      WHEN similarity(COALESCE(o."ResultsFocus", ''), search_query) > 0.2 THEN similarity(COALESCE(o."ResultsFocus", ''), search_query) * 0.45 * text_boost
                      ELSE 0 END),
                ('ExpectedImpact', COALESCE(o."ExpectedImpact", ''),
                 CASE WHEN o."ExpectedImpact" ILIKE exact_pattern THEN 0.55 * text_boost
                      WHEN o."ExpectedImpact" ILIKE word_pattern THEN 0.50 * text_boost
                      WHEN similarity(COALESCE(o."ExpectedImpact", ''), search_query) > 0.2 THEN similarity(COALESCE(o."ExpectedImpact", ''), search_query) * 0.45 * text_boost
                      ELSE 0 END),
                ('ExpectedOutcomes', COALESCE(o."ExpectedOutcomes", ''),
                 CASE WHEN o."ExpectedOutcomes" ILIKE exact_pattern THEN 0.55 * text_boost
                      WHEN o."ExpectedOutcomes" ILIKE word_pattern THEN 0.50 * text_boost
                      WHEN similarity(COALESCE(o."ExpectedOutcomes", ''), search_query) > 0.2 THEN similarity(COALESCE(o."ExpectedOutcomes", ''), search_query) * 0.45 * text_boost
                      ELSE 0 END),
                ('ExpectedBeneficiaries', COALESCE(o."ExpectedBeneficiaries", ''),
                 CASE WHEN o."ExpectedBeneficiaries" ILIKE exact_pattern THEN 0.50 * text_boost
                      WHEN o."ExpectedBeneficiaries" ILIKE word_pattern THEN 0.45 * text_boost
                      WHEN similarity(COALESCE(o."ExpectedBeneficiaries", ''), search_query) > 0.3 THEN similarity(COALESCE(o."ExpectedBeneficiaries", ''), search_query) * 0.40 * text_boost
                      ELSE 0 END),
                
                -- TIER 3 - NESTED PROPERTIES (Score: 0.4-0.2)
                ('ResponsibleOrgUnit.Name', COALESCE(org."Name", ''),
                 CASE WHEN org."Name" ILIKE exact_pattern THEN 0.4 * text_boost
                      WHEN org."Name" ILIKE word_pattern THEN 0.35 * text_boost
                      WHEN similarity(COALESCE(org."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(org."Name", ''), search_query) * 0.30 * text_boost
                      ELSE 0 END),
                ('ProposedInitiativeType.Name', COALESCE(pit."Name", ''),
                 CASE WHEN pit."Name" ILIKE exact_pattern THEN 0.35 * text_boost
                      WHEN pit."Name" ILIKE word_pattern THEN 0.30 * text_boost
                      WHEN similarity(COALESCE(pit."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(pit."Name", ''), search_query) * 0.25 * text_boost
                      ELSE 0 END),
                ('WorkflowStage.Name', COALESCE(ws."Name", ''),
                 CASE WHEN ws."Name" ILIKE exact_pattern THEN 0.30 * text_boost
                      WHEN ws."Name" ILIKE word_pattern THEN 0.25 * text_boost
                      WHEN similarity(COALESCE(ws."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(ws."Name", ''), search_query) * 0.20 * text_boost
                      ELSE 0 END),
                ('Country.Name', COALESCE(c."Name", ''),
                 CASE WHEN c."Name" ILIKE exact_pattern THEN 0.25 * text_boost
                      WHEN c."Name" ILIKE word_pattern THEN 0.20 * text_boost
                      WHEN similarity(COALESCE(c."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(c."Name", ''), search_query) * 0.15 * text_boost
                      ELSE 0 END),
                ('SDG.Name', COALESCE(sdg."Name", ''),
                 CASE WHEN sdg."Name" ILIKE exact_pattern THEN 0.20 * text_boost
                      WHEN sdg."Name" ILIKE word_pattern THEN 0.15 * text_boost
                      WHEN similarity(COALESCE(sdg."Name", ''), search_query) > 0.3 THEN similarity(COALESCE(sdg."Name", ''), search_query) * 0.10 * text_boost
                      ELSE 0 END)
        ) AS fields(matched_field, field_value, score)
        WHERE fields.score > 0.1
        AND fields.field_value IS NOT NULL 
        AND fields.field_value != ''
    ),
    best_matches AS (
        SELECT DISTINCT ON (all_matches.entity_id)
            all_matches.entity_type,
            all_matches.entity_id,
            all_matches.matched_field,
            all_matches.field_value,
            all_matches.score,
            all_matches.search_type,
            all_matches.match_criteria,
            all_matches.snippet
        FROM all_matches
        ORDER BY all_matches.entity_id, all_matches.score DESC
    )
    SELECT * FROM best_matches
    ORDER BY score DESC;
END
$$;

-- ============================================================================
-- MAIN ORCHESTRATOR FUNCTION
-- ============================================================================
CREATE OR REPLACE FUNCTION public.search_entity_records(
    search_query TEXT,
    embedding vector DEFAULT NULL,
    text_boost REAL DEFAULT 1.0,
    embedding_boost REAL DEFAULT 1.2,
    snippet_length INTEGER DEFAULT 150,
    debug_mode BOOLEAN DEFAULT FALSE,
    entity_filter TEXT[] DEFAULT NULL
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    result_json JSON;
    available_entities TEXT[];
    entity_name TEXT;
    text_results JSON;
    embedding_results JSON;
    start_time TIMESTAMP;
    execution_time REAL;
BEGIN
    start_time := clock_timestamp();
    
    -- Use entity_filter if provided, otherwise default to core entity types
    IF entity_filter IS NOT NULL AND array_length(entity_filter, 1) > 0 THEN
        available_entities := entity_filter;
    ELSE
        -- Default to core entity types including Opportunities
    available_entities := ARRAY['Partners', 'Contacts', 'Interactions', 'Opportunities'];
    END IF;
    
    -- Initialize empty results
    text_results := '{}'::json;
    embedding_results := '{}'::json;
    
    -- PART 1: FIELD SEARCH USING MODULAR FUNCTIONS
    WITH all_field_results AS (
        -- Partners search
        SELECT * FROM public.search_partners_with_nested(search_query, text_boost, snippet_length)
        WHERE 'Partners' = ANY(available_entities)
        
        UNION ALL
        
        -- Contacts search
        SELECT * FROM public.search_contacts_with_nested(search_query, text_boost, snippet_length)
        WHERE 'Contacts' = ANY(available_entities)
        
        UNION ALL
        
        -- Interactions search
        SELECT * FROM public.search_interactions_with_nested(search_query, text_boost, snippet_length)
        WHERE 'Interactions' = ANY(available_entities)
        
        UNION ALL
        
        -- Opportunities search
        SELECT * FROM public.search_opportunities_with_nested(search_query, text_boost, snippet_length)
        WHERE 'Opportunities' = ANY(available_entities)
    ),
    -- Aggregate by entity_id to get the best match per entity
    entity_best_matches AS (
        SELECT 
            entity_type,
            entity_id,
            MAX(score) as best_score,
            -- Get the field details for the best match
            (SELECT json_build_object(
                'matchedField', matched_field,
                'fieldValue', field_value,
                'searchType', search_type,
                'matchCriteria', match_criteria,
                'snippet', snippet
            ) FROM all_field_results afr2 
            WHERE afr2.entity_type = afr.entity_type 
            AND afr2.entity_id = afr.entity_id 
            AND afr2.score = MAX(afr.score)
            LIMIT 1) as best_match_details
        FROM all_field_results afr
        WHERE score > 0.1
        GROUP BY entity_type, entity_id
    ),
    ranked_entities AS (
        SELECT *,
               ROW_NUMBER() OVER (PARTITION BY entity_type ORDER BY best_score DESC) as rn
        FROM entity_best_matches
    )
    SELECT json_object_agg(
        entity_type,
        json_build_object(
            'items', items,
            'count', item_count,
            'maxScore', max_score,
            'avgScore', avg_score
        )
    )
    INTO text_results
    FROM (
        SELECT 
            entity_type,
            json_agg(
                json_build_object(
                    'entityId', entity_id::INTEGER,
                    'score', round(best_score::numeric, 3),
                    'matchedField', best_match_details->>'matchedField',
                    'fieldValue', best_match_details->>'fieldValue',
                    'searchType', best_match_details->>'searchType',
                    'matchCriteria', best_match_details->>'matchCriteria',
                    'snippet', best_match_details->>'snippet'
                ) 
                ORDER BY best_score DESC
            ) as items,
            COUNT(*)::INTEGER as item_count,
            round(MAX(best_score)::numeric, 3) as max_score,
            round(AVG(best_score)::numeric, 3) as avg_score
        FROM ranked_entities
        WHERE rn <= 15  -- Top 15 results per entity type
        GROUP BY entity_type
    ) grouped;
    
    -- PART 2: SEMANTIC EMBEDDING SEARCH (Optional)
    IF embedding IS NOT NULL AND EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'EntityEmbeddings' AND table_schema = 'public') THEN
        WITH embedding_results_cte AS (
                SELECT 
                    ee."EntityName"::TEXT as entity_type,
                    ee."EntityId"::TEXT as entity_id,
                round(((1 - (ee."FullEmbedding" <=> embedding::vector(768))) * embedding_boost)::numeric, 3) as score,
                'semantic'::TEXT as search_type,
                'embedding-similarity'::TEXT as match_criteria,
                left(COALESCE(ee."EntityData", ''), snippet_length)::TEXT as snippet
                FROM public."EntityEmbeddings" ee
                WHERE ee."FullEmbedding" IS NOT NULL
              AND (1 - (ee."FullEmbedding" <=> embedding::vector(768))) > 0.15
              AND (entity_filter IS NULL OR ee."EntityName" = ANY(available_entities))
            ),
            ranked_embedding AS (
                SELECT *,
                       ROW_NUMBER() OVER (PARTITION BY entity_type ORDER BY score DESC) as rn
            FROM embedding_results_cte
            )
            SELECT json_object_agg(
                entity_type,
                json_build_object(
                'items', items,
                'count', item_count,
                'maxScore', max_score,
                'avgScore', avg_score
            )
        )
        INTO embedding_results
            FROM (
                SELECT 
                    entity_type,
                    json_agg(
                        json_build_object(
                        'entityId', entity_id::INTEGER,
                        'score', score,
                        'searchType', search_type,
                        'matchCriteria', match_criteria,
                        'snippet', snippet
                        ) 
                        ORDER BY score DESC
                    ) as items,
                    COUNT(*)::INTEGER as item_count,
                    MAX(score) as max_score,
                    AVG(score) as avg_score
                FROM ranked_embedding
                WHERE rn <= 15  -- Top 15 results per entity type
                GROUP BY entity_type
        ) grouped;
    END IF;
    
    -- Calculate execution time
    execution_time := EXTRACT(EPOCH FROM (clock_timestamp() - start_time));
    
    -- Combine field search and semantic search results
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
                    'strategy', 'modular-nested-search',
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
                            'modular-nested-search',
                            'partner-groups-liaison-offices',
                            'contact-partner-relationships', 
                            'interaction-contacts-partners',
                            'field-specific-scoring', 
                            'pg-trgm-similarity'
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

-- Example usage:
-- Global search (all entities with nested properties):
-- SELECT public.search_entity_records('John');

-- Entity-specific search (Partners only with nested properties):
-- SELECT public.search_entity_records('procurement', NULL, 1.0, 1.2, 150, FALSE, ARRAY['Partners']);

-- Test individual entity functions:
-- SELECT * FROM public.search_partners_with_nested('UNICEF');
-- SELECT * FROM public.search_contacts_with_nested('john smith');
-- SELECT * FROM public.search_interactions_with_nested('meeting');
-- SELECT * FROM public.search_opportunities_with_nested('infrastructure');

-- Debug mode to see search capabilities:
-- SELECT public.search_entity_records('experienced project manager', NULL, 2.0, 1.0, 200, TRUE);
