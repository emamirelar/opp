-- ============================================================================
-- SIMPLIFIED DUPLICATE DETECTION FUNCTION - FIELD SIMILARITY ONLY
-- ============================================================================
-- This PostgreSQL function provides duplicate detection using field-specific 
-- matching and similarity scoring without semantic embeddings.
--
-- DUPLICATE DETECTION STRATEGY:
-- 1. Field-Specific Matching - Targets key identifying fields with weighted scoring
-- 2. Text Similarity - Uses pg_trgm for fuzzy text matching
-- 3. Configurable Thresholds - Allows fine-tuning for different entity types
--
-- SIMILARITY SCORE EXPLANATION:
-- - Uses PostgreSQL's similarity() function which measures text likeness using trigrams
-- - Trigrams are 3-character sequences (e.g., "John" → "  j", " jo", "joh", "ohn", "hn ")
-- - Score of 1.0 (100%) = Identical text
-- - Score of 0.9 (90%) = Very similar (minor differences like "Corp" vs "Corporation")
-- - Score of 0.7 (70%) = Moderately similar (some differences but clearly related)
-- - Score of 0.5 (50%) = Weakly similar (may share some common words)
--
-- CONFIDENCE LEVELS:
-- - High (80-100%): Very likely duplicates - immediate review recommended
-- - Medium (60-79%): Possible duplicates - investigation suggested
-- - Low (threshold-59%): Weak matches - manual review if resources permit
--
-- ENTITY-SPECIFIC LOGIC:
-- Contact Duplicates: Email (exact), Name similarity, Phone/Mobile, Same Partner
-- Partner Duplicates: Name similarity, Short Description, ERP Dimension Value
-- Interaction Duplicates: Subject similarity, Same date range, Contact/Partner overlap
--
-- STATUS ENUM VALUES:
-- 0 = Draft, 1 = Active (used in Contact status filtering)
-- ============================================================================

-- Enable required extensions
--CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Drop existing function versions
DROP FUNCTION IF EXISTS public.detect_duplicate_records(TEXT, JSON);
DROP FUNCTION IF EXISTS public.detect_duplicate_records(TEXT, JSON, vector);
DROP FUNCTION IF EXISTS public.detect_duplicate_records(TEXT, JSON, vector, REAL, REAL);
DROP FUNCTION IF EXISTS public.detect_duplicate_records(TEXT, JSON, vector, REAL, REAL, BOOLEAN);
DROP FUNCTION IF EXISTS public.detect_duplicate_records(TEXT, JSON, REAL, BOOLEAN);
DROP FUNCTION IF EXISTS public.detect_duplicate_records(TEXT, TEXT, REAL, BOOLEAN);

-- Create the simplified duplicate detection function
CREATE OR REPLACE FUNCTION public.detect_duplicate_records(
    entity_type TEXT,
    entity_data TEXT,
    field_match_threshold REAL DEFAULT 0.5,
    debug_mode BOOLEAN DEFAULT FALSE,
    exclude_record_id INTEGER DEFAULT NULL
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    result_json JSON;
    field_duplicates JSON;
    start_time TIMESTAMP;
    execution_time REAL;
    field_search_sql TEXT;
    parsed_entity_data JSON;
    
    -- Contact-specific variables
    input_email TEXT;
    input_firstname TEXT;
    input_lastname TEXT;
    input_name TEXT;
    input_phone TEXT;
    input_mobile TEXT;
    input_partner_id INTEGER;
    
    -- Partner-specific variables
    input_partner_name TEXT;
    input_partner_short_desc TEXT;
    input_erp_dim_value INTEGER;
    
    -- Interaction-specific variables
    input_subject TEXT;
    input_date TIMESTAMP;
    input_contact_ids INTEGER[];
    input_partner_ids INTEGER[];
    input_location TEXT;
    
BEGIN
    start_time := clock_timestamp();
    
    -- Validate inputs
    IF entity_type IS NULL OR entity_data IS NULL THEN
        RETURN json_build_object(
            'error', 'Entity type and data are required',
            'duplicates', json_build_array(),
            'summary', json_build_object('totalDuplicates', 0)
        );
    END IF;
    
    -- Parse the TEXT input as JSON
    BEGIN
        parsed_entity_data := entity_data::JSON;
    EXCEPTION WHEN OTHERS THEN
        RETURN json_build_object(
            'error', 'Invalid JSON format in entity_data parameter',
            'duplicates', json_build_array(),
            'summary', json_build_object('totalDuplicates', 0)
        );
    END;
    
    -- Initialize results
    field_duplicates := json_build_array();
    
    -- ============================================================================
    -- CONTACT DUPLICATE DETECTION
    -- ============================================================================
    IF UPPER(entity_type) = 'CONTACT' THEN
        -- Extract contact fields from JSON
        input_email := LOWER(TRIM(parsed_entity_data->>'email'));
        input_firstname := TRIM(parsed_entity_data->>'firstName');
        input_lastname := TRIM(parsed_entity_data->>'lastName');
        input_name := TRIM(parsed_entity_data->>'name');
        input_phone := TRIM(parsed_entity_data->>'phone');
        input_mobile := TRIM(parsed_entity_data->>'mobile');
        
        -- Safely handle partnerId conversion with null/empty string check
        BEGIN
            input_partner_id := CASE 
                WHEN (parsed_entity_data->>'partnerId') IS NULL OR TRIM(parsed_entity_data->>'partnerId') = '' 
                THEN NULL 
                ELSE (parsed_entity_data->>'partnerId')::INTEGER 
            END;
        EXCEPTION WHEN OTHERS THEN
            input_partner_id := NULL;
        END;
        
        -- Build name for comparison if not provided
        IF input_name IS NULL OR input_name = '' THEN
            input_name := TRIM(COALESCE(input_firstname, '') || ' ' || COALESCE(input_lastname, ''));
        END IF;
        
        -- Field-based duplicate detection for contacts
        field_search_sql := '
            WITH contact_matches AS (
                SELECT 
                    "Id",
                    "FirstName",
                    "LastName", 
                    "Name",
                    "Email",
                    "Phone",
                    "Mobile",
                    "PartnerId",
                    CASE 
                        -- Exact email match (highest priority)
                        WHEN $1 IS NOT NULL AND $1 != '''' AND LOWER("Email") = LOWER($1) THEN 1.0
                        -- Exact name match within same partner
                        WHEN $2 IS NOT NULL AND $2 != '''' AND "PartnerId" = $3 
                             AND (LOWER("Name") = LOWER($2) OR 
                                  LOWER(TRIM("FirstName" || '' '' || "LastName")) = LOWER($2)) THEN 0.95
                        -- High name similarity (90%%+) within same partner  
                        WHEN $2 IS NOT NULL AND $2 != '''' AND "PartnerId" = $3
                             AND (similarity("Name", $2) > 0.9 OR 
                                  similarity(TRIM("FirstName" || '' '' || "LastName"), $2) > 0.9) THEN 0.85
                        -- Exact phone/mobile match within same partner
                        WHEN $4 IS NOT NULL AND $4 != '''' AND "PartnerId" = $3 
                             AND ($4 IN ("Phone", "Mobile")) THEN 0.8
                        -- Exact name match across different partners (same person moved organizations)
                        WHEN $2 IS NOT NULL AND $2 != '''' AND "PartnerId" != $3
                             AND (
                                 -- Clean both names by removing common titles/prefixes for true exact match
                                 LOWER(REGEXP_REPLACE(TRIM("Name"), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi'')) = 
                                 LOWER(REGEXP_REPLACE(TRIM($2), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi''))
                                 OR 
                                 LOWER(REGEXP_REPLACE(TRIM("FirstName" || '' '' || "LastName"), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi'')) = 
                                 LOWER(REGEXP_REPLACE(TRIM($2), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi''))
                             ) THEN 0.75
                        -- High name similarity (70%%+) across different partners
                        WHEN $2 IS NOT NULL AND $2 != ''''
                             AND (similarity("Name", $2) > 0.7 OR 
                                  similarity(TRIM("FirstName" || '' '' || "LastName"), $2) > 0.7) THEN 0.6
                        -- Phone/email match across different partners (potential same person)
                        WHEN ($1 IS NOT NULL AND $1 != '''' AND LOWER("Email") = LOWER($1))
                             OR ($4 IS NOT NULL AND $4 != '''' AND $4 IN ("Phone", "Mobile")) THEN 0.7
                        ELSE 0
                    END as match_score,
                    CASE 
                        WHEN $1 IS NOT NULL AND $1 != '''' AND LOWER("Email") = LOWER($1) THEN 
                            ''Exact Email Match - Same email address found ('' || COALESCE("Email", ''N/A'') || ''). Score: 100% - Emails are identical, indicating very likely duplicate person.''
                        WHEN $2 IS NOT NULL AND $2 != '''' AND "PartnerId" = $3 
                             AND (LOWER("Name") = LOWER($2) OR 
                                  LOWER(TRIM("FirstName" || '' '' || "LastName")) = LOWER($2)) THEN 
                            ''Exact Name + Same Partner - Identical name within same organization ('' || COALESCE("Name", ''N/A'') || ''). Score: 95% - Same person name in same partner organization, very high confidence duplicate.''
                        WHEN $2 IS NOT NULL AND $2 != '''' AND "PartnerId" = $3
                             AND (similarity("Name", $2) > 0.9 OR 
                                  similarity(TRIM("FirstName" || '' '' || "LastName"), $2) > 0.9) THEN 
                            ''High Name Similarity + Same Partner - Very similar name within same organization ('' || COALESCE("Name", ''N/A'') || '' vs '' || $2 || ''). Score: 85% - Names are '' || 
                            CASE 
                                WHEN similarity("Name", $2) > similarity(TRIM("FirstName" || '' '' || "LastName"), $2) 
                                THEN round((similarity("Name", $2) * 100)::numeric, 0) || ''% similar''
                                ELSE round((similarity(TRIM("FirstName" || '' '' || "LastName"), $2) * 100)::numeric, 0) || ''% similar''
                            END || '', likely same person with minor spelling differences.''
                        WHEN $4 IS NOT NULL AND $4 != '''' AND "PartnerId" = $3 
                             AND ($4 IN ("Phone", "Mobile")) THEN 
                            ''Phone Match + Same Partner - Same phone number within same organization ('' || 
                            CASE WHEN "Phone" = $4 THEN ''Phone: '' || COALESCE("Phone", ''N/A'') 
                                 ELSE ''Mobile: '' || COALESCE("Mobile", ''N/A'') END || 
                            ''). Score: 80% - Identical phone number in same partner, strong indication of duplicate.''
                        WHEN $2 IS NOT NULL AND $2 != '''' AND "PartnerId" != $3
                             AND (
                                 -- Clean both names by removing common titles/prefixes for true exact match
                                 LOWER(REGEXP_REPLACE(TRIM("Name"), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi'')) = 
                                 LOWER(REGEXP_REPLACE(TRIM($2), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi''))
                                 OR 
                                 LOWER(REGEXP_REPLACE(TRIM("FirstName" || '' '' || "LastName"), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi'')) = 
                                 LOWER(REGEXP_REPLACE(TRIM($2), ''^(mr\.?|mrs\.?|ms\.?|dr\.?|prof\.?)\s+'', '''', ''gi''))
                             ) THEN 
                            ''Exact Name Match Across Organizations - Same core name in different organizations ('' || COALESCE("Name", ''N/A'') || '' vs '' || $2 || '', titles/prefixes ignored). Score: 75% - Same person likely moved between organizations, review recommended.''
                        WHEN $2 IS NOT NULL AND $2 != ''''
                             AND (similarity("Name", $2) > 0.7 OR 
                                  similarity(TRIM("FirstName" || '' '' || "LastName"), $2) > 0.7) THEN 
                            ''High Name Similarity - Similar name across organizations ('' || COALESCE("Name", ''N/A'') || '' vs '' || $2 || ''). Score: 60% - Names are '' ||
                            CASE 
                                WHEN similarity("Name", $2) > similarity(TRIM("FirstName" || '' '' || "LastName"), $2) 
                                THEN round((similarity("Name", $2) * 100)::numeric, 0) || ''% similar''
                                ELSE round((similarity(TRIM("FirstName" || '' '' || "LastName"), $2) * 100)::numeric, 0) || ''% similar''
                            END || '', could be same person in different organization or relative.''
                        WHEN ($1 IS NOT NULL AND $1 != '''' AND LOWER("Email") = LOWER($1))
                             OR ($4 IS NOT NULL AND $4 != '''' AND $4 IN ("Phone", "Mobile")) THEN 
                            ''Contact Info Match - Same contact details across organizations. Score: 70% - Identical email or phone suggests same person moved between organizations.''
                        ELSE ''No Match''
                    END as match_reason
                FROM public."Contacts"
                WHERE "Status"::INTEGER = 1  -- FIX: Cast Status to INTEGER for comparison
                AND "IsDeleted" = false
                AND ($6 IS NULL OR "Id" != $6)
            )
            SELECT json_agg(
                json_build_object(
                    ''entityId'', "Id",
                    ''entityType'', ''Contact'',
                    ''score'', round(match_score::numeric, 3),
                    ''matchReason'', match_reason,
                    ''matchedData'', json_build_object(
                        ''name'', "Name",
                        ''email'', "Email", 
                        ''phone'', "Phone",
                        ''mobile'', "Mobile",
                        ''partnerId'', "PartnerId"
                    ),
                    ''searchType'', ''field-matching''
                )
                ORDER BY match_score DESC
            )
            FROM contact_matches
            WHERE match_score >= $5
            LIMIT 10';
        
        EXECUTE field_search_sql INTO field_duplicates USING input_email, input_name, input_partner_id, COALESCE(input_phone, input_mobile), field_match_threshold, exclude_record_id;
        
    -- ============================================================================
    -- PARTNER DUPLICATE DETECTION  
    -- ============================================================================
    ELSIF UPPER(entity_type) = 'PARTNER' THEN
        -- Extract partner fields from JSON
        input_partner_name := TRIM(parsed_entity_data->>'name');
        input_partner_short_desc := TRIM(parsed_entity_data->>'partnerShortDescription');
        
        -- Safely handle erpDimValue conversion with null/empty string check
        BEGIN
            input_erp_dim_value := CASE 
                WHEN (parsed_entity_data->>'erpDimValue') IS NULL OR TRIM(parsed_entity_data->>'erpDimValue') = '' 
                THEN NULL 
                ELSE (parsed_entity_data->>'erpDimValue')::INTEGER 
            END;
        EXCEPTION WHEN OTHERS THEN
            input_erp_dim_value := NULL;
        END;
        
        field_search_sql := '
            WITH partner_matches AS (
                SELECT 
                    "Id",
                    "Name",
                    "PartnerShortDescription",
                    "ErpDimValue",
                    CASE 
                        -- Exact ERP Dimension Value match (highest priority)
                        WHEN $1 IS NOT NULL AND "ErpDimValue" = $1 THEN 1.0
                        -- Exact name match
                        WHEN $2 IS NOT NULL AND $2 != '''' AND LOWER("Name") = LOWER($2) THEN 0.95
                        -- High name similarity (85%%+)
                        WHEN $2 IS NOT NULL AND $2 != '''' AND similarity("Name", $2) > 0.85 THEN 0.8
                        -- Short description exact match
                        WHEN $3 IS NOT NULL AND $3 != '''' AND LOWER("PartnerShortDescription") = LOWER($3) THEN 0.9
                        -- Short description similarity
                        WHEN $3 IS NOT NULL AND $3 != '''' AND similarity("PartnerShortDescription", $3) > 0.8 THEN 0.75
                        -- Moderate name similarity (70%%+)
                        WHEN $2 IS NOT NULL AND $2 != '''' AND similarity("Name", $2) > 0.7 THEN 0.6
                        ELSE 0
                    END as match_score,
                    CASE 
                        WHEN $1 IS NOT NULL AND "ErpDimValue" = $1 THEN 
                            ''Exact ERP Dimension Value - Same ERP system identifier ('' || COALESCE("ErpDimValue"::text, ''N/A'') || ''). Score: 100% - Identical ERP code means this is definitely the same organization.''
                        WHEN $2 IS NOT NULL AND $2 != '''' AND LOWER("Name") = LOWER($2) THEN 
                            ''Exact Name Match - Identical organization name ('' || COALESCE("Name", ''N/A'') || ''). Score: 95% - Same name indicates very likely duplicate organization.''
                        WHEN $2 IS NOT NULL AND $2 != '''' AND similarity("Name", $2) > 0.85 THEN 
                            ''High Name Similarity - Very similar organization names ('' || COALESCE("Name", ''N/A'') || '' vs '' || $2 || ''). Score: 80% - Names are '' || 
                            round((similarity("Name", $2) * 100)::numeric, 0) || ''% similar, likely same organization with minor variations (abbreviations, legal suffixes).''
                        WHEN $3 IS NOT NULL AND $3 != '''' AND LOWER("PartnerShortDescription") = LOWER($3) THEN 
                            ''Exact Short Description - Identical partner description ('' || COALESCE("PartnerShortDescription", ''N/A'') || ''). Score: 90% - Same description suggests duplicate organization entry.''
                        WHEN $3 IS NOT NULL AND $3 != '''' AND similarity("PartnerShortDescription", $3) > 0.8 THEN 
                            ''Short Description Similarity - Similar partner descriptions ('' || COALESCE("PartnerShortDescription", ''N/A'') || '' vs '' || $3 || ''). Score: 75% - Descriptions are '' ||
                            round((similarity("PartnerShortDescription", $3) * 100)::numeric, 0) || ''% similar, could be same organization described differently.''
                        WHEN $2 IS NOT NULL AND $2 != '''' AND similarity("Name", $2) > 0.7 THEN 
                            ''Moderate Name Similarity - Similar organization names ('' || COALESCE("Name", ''N/A'') || '' vs '' || $2 || ''). Score: 60% - Names are '' ||
                            round((similarity("Name", $2) * 100)::numeric, 0) || ''% similar, could be related organizations or same organization with different naming.''
                        ELSE ''No Match''
                    END as match_reason
                FROM public."Partners"
                WHERE "IsDeleted" = false
                AND ($5 IS NULL OR "Id" != $5)
            )
            SELECT json_agg(
                json_build_object(
                    ''entityId'', "Id",
                    ''entityType'', ''Partner'',
                    ''score'', round(match_score::numeric, 3),
                    ''matchReason'', match_reason,
                    ''matchedData'', json_build_object(
                        ''name'', "Name",
                        ''partnerShortDescription'', "PartnerShortDescription",
                        ''erpDimValue'', "ErpDimValue"
                    ),
                    ''searchType'', ''field-matching''
                )
                ORDER BY match_score DESC
            )
            FROM partner_matches
            WHERE match_score >= $4
            LIMIT 10';
        
        EXECUTE field_search_sql INTO field_duplicates USING input_erp_dim_value, input_partner_name, input_partner_short_desc, field_match_threshold, exclude_record_id;
        
    -- ============================================================================
    -- INTERACTION DUPLICATE DETECTION
    -- ============================================================================
    ELSIF UPPER(entity_type) = 'INTERACTION' THEN
        -- Extract interaction fields from JSON
        input_subject := TRIM(parsed_entity_data->>'subject');
        
        -- Safely handle date conversion with null/empty string check
        BEGIN
            input_date := CASE 
                WHEN (parsed_entity_data->>'date') IS NULL OR TRIM(parsed_entity_data->>'date') = '' 
                THEN NULL 
                ELSE (parsed_entity_data->>'date')::TIMESTAMP 
            END;
        EXCEPTION WHEN OTHERS THEN
            input_date := NULL;
        END;
        
        input_location := TRIM(parsed_entity_data->>'location');
        
        -- Extract contact and partner IDs if provided as arrays
        -- Handle potential null arrays gracefully
        BEGIN
            SELECT ARRAY(SELECT json_array_elements_text(parsed_entity_data->'contactIds')::INTEGER) INTO input_contact_ids;
        EXCEPTION WHEN OTHERS THEN
            input_contact_ids := ARRAY[]::INTEGER[];
        END;
        
        BEGIN
            SELECT ARRAY(SELECT json_array_elements_text(parsed_entity_data->'partnerIds')::INTEGER) INTO input_partner_ids;
        EXCEPTION WHEN OTHERS THEN
            input_partner_ids := ARRAY[]::INTEGER[];
        END;
        
        field_search_sql := '
            WITH interaction_matches AS (
                SELECT 
                    i."Id",
                    i."Subject",
                    i."Date", 
                    i."Location",
                    COUNT(DISTINCT ic."ContactId") as contact_overlap,
                    COUNT(DISTINCT ip."PartnerId") as partner_overlap,
                    CASE 
                        -- Exact subject + same day
                        WHEN $1 IS NOT NULL AND $1 != '''' AND LOWER(i."Subject") = LOWER($1)
                             AND $2 IS NOT NULL AND DATE(i."Date") = DATE($2) THEN 0.95
                        -- High subject similarity + same day
                        WHEN $1 IS NOT NULL AND $1 != '''' AND similarity(i."Subject", $1) > 0.85
                             AND $2 IS NOT NULL AND DATE(i."Date") = DATE($2) THEN 0.85
                        -- Exact subject + within 7 days
                        WHEN $1 IS NOT NULL AND $1 != '''' AND LOWER(i."Subject") = LOWER($1)
                             AND $2 IS NOT NULL AND ABS(EXTRACT(EPOCH FROM (i."Date" - $2))/86400) <= 7 THEN 0.8
                        -- High subject similarity (80%%+)
                        WHEN $1 IS NOT NULL AND $1 != '''' AND similarity(i."Subject", $1) > 0.8 THEN 0.75
                        -- Moderate subject similarity + same day
                        WHEN $1 IS NOT NULL AND $1 != '''' AND similarity(i."Subject", $1) > 0.7
                             AND $2 IS NOT NULL AND DATE(i."Date") = DATE($2) THEN 0.65
                        ELSE 0
                    END as match_score,
                    CASE 
                        WHEN $1 IS NOT NULL AND $1 != '''' AND LOWER(i."Subject") = LOWER($1)
                             AND $2 IS NOT NULL AND DATE(i."Date") = DATE($2) THEN 
                            ''Exact Subject + Same Day - Identical meeting/interaction topic on same date ('' || COALESCE(i."Subject", ''N/A'') || '' on '' || 
                            TO_CHAR(i."Date", ''YYYY-MM-DD'') || ''). Score: 95% - Same subject and date strongly indicates duplicate interaction record.''
                        WHEN $1 IS NOT NULL AND $1 != '''' AND similarity(i."Subject", $1) > 0.85
                             AND $2 IS NOT NULL AND DATE(i."Date") = DATE($2) THEN 
                            ''High Subject Similarity + Same Day - Very similar interaction topics on same date ('' || COALESCE(i."Subject", ''N/A'') || '' vs '' || $1 || '' on '' ||
                            TO_CHAR(i."Date", ''YYYY-MM-DD'') || ''). Score: 85% - Subjects are '' || 
                            round((similarity(i."Subject", $1) * 100)::numeric, 0) || ''% similar, likely same meeting with minor description differences.''
                        WHEN $1 IS NOT NULL AND $1 != '''' AND LOWER(i."Subject") = LOWER($1)
                             AND $2 IS NOT NULL AND ABS(EXTRACT(EPOCH FROM (i."Date" - $2))/86400) <= 7 THEN 
                            ''Exact Subject + Within Week - Same interaction topic within 7 days ('' || COALESCE(i."Subject", ''N/A'') || '', '' ||
                            ABS(EXTRACT(EPOCH FROM (i."Date" - $2))/86400)::INTEGER || '' days apart). Score: 80% - Same subject close in time, could be follow-up or duplicate entry.''
                        WHEN $1 IS NOT NULL AND $1 != '''' AND similarity(i."Subject", $1) > 0.8 THEN 
                            ''High Subject Similarity - Very similar interaction topics ('' || COALESCE(i."Subject", ''N/A'') || '' vs '' || $1 || ''). Score: 75% - Subjects are '' ||
                            round((similarity(i."Subject", $1) * 100)::numeric, 0) || ''% similar, could be related interactions or duplicate with modified description.''
                        WHEN $1 IS NOT NULL AND $1 != '''' AND similarity(i."Subject", $1) > 0.7
                             AND $2 IS NOT NULL AND DATE(i."Date") = DATE($2) THEN 
                            ''Moderate Subject Similarity + Same Day - Similar interaction topics on same date ('' || COALESCE(i."Subject", ''N/A'') || '' vs '' || $1 || '' on '' ||
                            TO_CHAR(i."Date", ''YYYY-MM-DD'') || ''). Score: 65% - Subjects are '' ||
                            round((similarity(i."Subject", $1) * 100)::numeric, 0) || ''% similar, could be same meeting described differently.''
                        ELSE ''No Match''
                    END as match_reason
                FROM public."Interactions" i
                LEFT JOIN public."InteractionContacts" ic ON i."Id" = ic."InteractionId"
                LEFT JOIN public."InteractionPartners" ip ON i."Id" = ip."InteractionId"
                WHERE i."IsDeleted" = false
                AND ($4 IS NULL OR i."Id" != $4)
                GROUP BY i."Id", i."Subject", i."Date", i."Location"
            )
            SELECT json_agg(
                json_build_object(
                    ''entityId'', "Id",
                    ''entityType'', ''Interaction'',
                    ''score'', round(match_score::numeric, 3),
                    ''matchReason'', match_reason,
                    ''matchedData'', json_build_object(
                        ''subject'', "Subject",
                        ''date'', "Date",
                        ''location'', "Location",
                        ''contactOverlap'', contact_overlap,
                        ''partnerOverlap'', partner_overlap
                    ),
                    ''searchType'', ''field-matching''
                )
                ORDER BY match_score DESC
            )
            FROM interaction_matches
            WHERE match_score >= $3
            LIMIT 10';
        
        EXECUTE field_search_sql INTO field_duplicates USING input_subject, input_date, field_match_threshold, exclude_record_id;
        
    ELSE
        -- Unsupported entity type
        RETURN json_build_object(
            'error', 'Unsupported entity type: ' || entity_type,
            'supportedTypes', json_build_array('Contact', 'Partner', 'Interaction'),
            'duplicates', json_build_array(),
            'summary', json_build_object('totalDuplicates', 0)
        );
    END IF;
    
    -- Calculate execution time
    execution_time := EXTRACT(EPOCH FROM (clock_timestamp() - start_time));
    
    -- Return results
    WITH duplicates_with_scores AS (
        SELECT json_array_elements(COALESCE(field_duplicates, '[]'::json)) as duplicate_item
    )
    SELECT 
        CASE 
            WHEN debug_mode THEN
                json_build_object(
                    'entityType', entity_type,
                    'inputData', parsed_entity_data,
                    'thresholds', json_build_object(
                        'fieldMatchThreshold', field_match_threshold
                    ),
                    'scoringExplanation', json_build_object(
                        'methodology', 'PostgreSQL similarity() function + field-specific logic',
                        'similarityCalculation', 'Based on trigram matching - compares 3-character sequences between text strings',
                        'scoreRanges', json_build_object(
                            'highConfidence', '80-100% - Very likely duplicates, review recommended',
                            'mediumConfidence', '60-79% - Possible duplicates, investigation suggested', 
                            'lowConfidence', field_match_threshold || '-59% - Weak matches, manual review if time permits'
                        ),
                        'fieldWeights', CASE 
                            WHEN UPPER(entity_type) = 'CONTACT' THEN json_build_object(
                                'email', '100% - Exact email match (highest priority)',
                                'nameWithPartner', '95% - Exact name within same organization',
                                'nameSimilarityWithPartner', '85% - High name similarity (90%+) within same organization',
                                'phoneWithPartner', '80% - Phone match within same organization',
                                'exactNameAcrossPartners', '75% - Exact name match across different organizations',
                                'nameSimilarityAcrossPartners', '60% - Name similarity (70%+) across different organizations',
                                'contactInfoAcrossPartners', '70% - Email/phone match across organizations'
                            )
                            WHEN UPPER(entity_type) = 'PARTNER' THEN json_build_object(
                                'erpDimValue', '100% - Exact ERP identifier match (highest priority)',
                                'exactName', '95% - Identical organization name',
                                'nameSimilarity', '80% - High name similarity (85%+)',
                                'exactDescription', '90% - Identical short description',
                                'descriptionSimilarity', '75% - Description similarity (80%+)',
                                'moderateNameSimilarity', '60% - Moderate name similarity (70%+)'
                            )
                            WHEN UPPER(entity_type) = 'INTERACTION' THEN json_build_object(
                                'exactSubjectSameDay', '95% - Identical subject on same date',
                                'similarSubjectSameDay', '85% - Similar subject (85%+) on same date',
                                'exactSubjectWeek', '80% - Identical subject within 7 days',
                                'similarSubject', '75% - High subject similarity (80%+)',
                                'moderateSubjectSameDay', '65% - Moderate similarity (70%+) on same date'
                            )
                            ELSE json_build_object()
                        END
                    ),
                    'duplicates', COALESCE(json_agg(duplicate_item ORDER BY (duplicate_item->>'score')::REAL DESC), '[]'::json),
                    'summary', json_build_object(
                        'totalDuplicates', COUNT(*),
                        'highConfidence', COUNT(*) FILTER (WHERE (duplicate_item->>'score')::REAL >= 0.8),
                        'mediumConfidence', COUNT(*) FILTER (WHERE (duplicate_item->>'score')::REAL >= 0.6 AND (duplicate_item->>'score')::REAL < 0.8),
                        'lowConfidence', COUNT(*) FILTER (WHERE (duplicate_item->>'score')::REAL >= field_match_threshold AND (duplicate_item->>'score')::REAL < 0.6),
                        'executionTimeMs', round((execution_time * 1000)::numeric, 2)
                    )
                )
            ELSE
                json_build_object(
                    'entityType', entity_type,
                    'duplicates', COALESCE(json_agg(duplicate_item ORDER BY (duplicate_item->>'score')::REAL DESC), '[]'::json),
                    'summary', json_build_object(
                        'totalDuplicates', COUNT(*),
                        'highConfidence', COUNT(*) FILTER (WHERE (duplicate_item->>'score')::REAL >= 0.8),
                        'mediumConfidence', COUNT(*) FILTER (WHERE (duplicate_item->>'score')::REAL >= 0.6 AND (duplicate_item->>'score')::REAL < 0.8),
                        'lowConfidence', COUNT(*) FILTER (WHERE (duplicate_item->>'score')::REAL >= field_match_threshold AND (duplicate_item->>'score')::REAL < 0.6)
                    )
                )
        END
    INTO result_json
    FROM duplicates_with_scores;
    
    RETURN result_json;
END
$$;

-- ============================================================================
-- USAGE EXAMPLES
-- ============================================================================

-- Example 1: Contact duplicate detection
-- SELECT public.detect_duplicate_records(
--     'Contact',
--     '{"email": "john.doe@example.com", "firstName": "John", "lastName": "Doe", "phone": "+1234567890", "partnerId": 123}'
-- );

-- Example 2: Partner duplicate detection with debug
-- SELECT public.detect_duplicate_records(
--     'Partner', 
--     '{"name": "ACME Corporation", "partnerShortDescription": "ACME Corp", "erpDimValue": 12345}',
--     0.5,
--     TRUE
-- );

-- Example 3: Interaction duplicate detection
-- SELECT public.detect_duplicate_records(
--     'Interaction',
--     '{"subject": "Project Planning Meeting", "date": "2024-01-15T10:00:00", "contactIds": [1, 2, 3], "location": "New York"}'
-- );

-- Create recommended indexes for performance
CREATE INDEX IF NOT EXISTS idx_contacts_duplicate_detection 
ON public."Contacts"("Email", "Name", "Phone", "Mobile", "PartnerId", ("Status"::INTEGER)) 
WHERE "IsDeleted" = false AND "Status"::INTEGER = 1;

CREATE INDEX IF NOT EXISTS idx_partners_duplicate_detection
ON public."Partners"("Name", "PartnerShortDescription", "ErpDimValue")
WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS idx_interactions_duplicate_detection
ON public."Interactions"("Subject", "Date", "Location")
WHERE "IsDeleted" = false;
