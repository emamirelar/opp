-- SQL Script to Generate PubSub Messages Array for Embedding Creation
-- This script creates a single JSON array containing all entities that need embeddings
-- Copy the result and paste directly into PubSub

WITH entity_messages AS (
    -- Contacts
    SELECT 
        json_build_object(
            'EntityName', 'Contacts',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Contacts"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- Partners
    SELECT 
        json_build_object(
            'EntityName', 'Partners',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Partners"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- Interactions
    SELECT 
        json_build_object(
            'EntityName', 'Interactions',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Interactions"
    WHERE "Id" IS NOT NULL
)

-- Aggregate all messages into a single JSON array and cast to text to avoid escaping
SELECT 
    json_agg(message ORDER BY (message->>'EntityName'), (message->>'EntityId')::int)::text as pubsub_messages_array
FROM entity_messages;

-- Alternative version with formatted output for easier reading
-- Uncomment the following query if you want a pretty-printed version:

/*
WITH entity_messages AS (
    -- Contacts
    SELECT 
        json_build_object(
            'EntityName', 'Contacts',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Contacts"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- Partners
    SELECT 
        json_build_object(
            'EntityName', 'Partners',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Partners"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- Interactions
    SELECT 
        json_build_object(
            'EntityName', 'Interactions',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Interactions"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- Projects
    SELECT 
        json_build_object(
            'EntityName', 'Projects',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Projects"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- WorkPackages
    SELECT 
        json_build_object(
            'EntityName', 'WorkPackages',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."WorkPackages"
    WHERE "Id" IS NOT NULL
    
    UNION ALL
    
    -- Documents
    SELECT 
        json_build_object(
            'EntityName', 'Documents',
            'EntityId', "Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Documents"
    WHERE "Id" IS NOT NULL
)

-- Pretty-printed version
SELECT 
    jsonb_pretty(json_agg(message ORDER BY (message->>'EntityName'), (message->>'EntityId')::int)::jsonb) as formatted_pubsub_messages
FROM entity_messages;
*/

-- Query to check how many messages will be generated per entity type
-- Uncomment to see counts before generating the full array:

/*
SELECT 
    'Contacts' as entity_type,
    COUNT(*) as message_count
FROM public."Contacts"
WHERE "Id" IS NOT NULL

UNION ALL

SELECT 
    'Partners' as entity_type,
    COUNT(*) as message_count
FROM public."Partners"
WHERE "Id" IS NOT NULL

UNION ALL

SELECT 
    'Interactions' as entity_type,
    COUNT(*) as message_count
FROM public."Interactions"
WHERE "Id" IS NOT NULL

UNION ALL

SELECT 
    'Projects' as entity_type,
    COUNT(*) as message_count
FROM public."Projects"
WHERE "Id" IS NOT NULL

UNION ALL

SELECT 
    'WorkPackages' as entity_type,
    COUNT(*) as message_count
FROM public."WorkPackages"
WHERE "Id" IS NOT NULL

UNION ALL

SELECT 
    'Documents' as entity_type,
    COUNT(*) as message_count
FROM public."Documents"
WHERE "Id" IS NOT NULL

ORDER BY entity_type;
*/

-- Query to exclude entities that already have embeddings (optional)
-- Uncomment this version if you only want to create embeddings for entities that don't have them yet:

/*
WITH entity_messages AS (
    -- Contacts without embeddings
    SELECT 
        json_build_object(
            'EntityName', 'Contacts',
            'EntityId', c."Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Contacts" c
    LEFT JOIN public."EntityEmbeddings" e ON e."EntityType" = 'Contacts' AND e."EntityId" = c."Id"
    WHERE c."Id" IS NOT NULL AND e."Id" IS NULL
    
    UNION ALL
    
    -- Partners without embeddings
    SELECT 
        json_build_object(
            'EntityName', 'Partners',
            'EntityId', p."Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Partners" p
    LEFT JOIN public."EntityEmbeddings" e ON e."EntityType" = 'Partners' AND e."EntityId" = p."Id"
    WHERE p."Id" IS NOT NULL AND e."Id" IS NULL
    
    UNION ALL
    
    -- Interactions without embeddings
    SELECT 
        json_build_object(
            'EntityName', 'Interactions',
            'EntityId', i."Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Interactions" i
    LEFT JOIN public."EntityEmbeddings" e ON e."EntityType" = 'Interactions' AND e."EntityId" = i."Id"
    WHERE i."Id" IS NOT NULL AND e."Id" IS NULL
    
    UNION ALL
    
    -- Projects without embeddings
    SELECT 
        json_build_object(
            'EntityName', 'Projects',
            'EntityId', pr."Id",
            'MessageType', 'EntityProcessing'
        ) as message
    FROM public."Projects" pr
    LEFT JOIN public."EntityEmbeddings" e ON e."EntityType" = 'Projects' AND e."EntityId" = pr."Id"
    WHERE pr."Id" IS NOT NULL AND e."Id" IS NULL
)

-- Generate array only for entities without existing embeddings
SELECT 
    json_agg(message ORDER BY (message->>'EntityName'), (message->>'EntityId')::int) as pubsub_messages_array_missing_embeddings
FROM entity_messages;
*/ 