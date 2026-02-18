-- ============================================
-- PR #671 Database Verification SQL Script
-- ============================================
-- Run this script in pgAdmin, DBeaver, psql, or any PostgreSQL client
-- Database: OpportunityPlus (or your database name)
-- ============================================

-- Clear the output
\echo '========================================'
\echo 'PR #671 - Database Verification'
\echo 'Fix for Opportunity Screen Not Loading'
\echo '========================================'
\echo ''

-- ============================================
-- TEST 1: Check if migration was applied
-- ============================================
\echo '--- TEST 1: Migration Applied ---'
\echo 'Expected: 1 row returned with MigrationId'
\echo ''

SELECT 
    "MigrationId",
    "ProductVersion"
FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20260122185435_SetDefaultStageForOpportunity';

\echo ''
\echo 'Result: '
SELECT 
    CASE 
        WHEN COUNT(*) = 1 THEN '✅ PASS: Migration applied'
        ELSE '❌ FAIL: Migration not found (Expected: 1, Actual: ' || COUNT(*) || ')'
    END as "Test1_Result"
FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20260122185435_SetDefaultStageForOpportunity';

\echo ''
\echo ''

-- ============================================
-- TEST 2: Check for NULL/empty Stage values
-- ============================================
\echo '--- TEST 2: NULL/Empty Stage Check ---'
\echo 'Expected: 0 records with NULL or empty Stage'
\echo ''

SELECT COUNT(*) as "RecordsWithNULLorEmptyStage"
FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';

\echo ''
\echo 'Result: '
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ PASS: No NULL/empty stages found'
        ELSE '❌ FAIL: Found ' || COUNT(*) || ' records with NULL/empty Stage (Expected: 0)'
    END as "Test2_Result"
FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';

\echo ''
\echo ''

-- ============================================
-- TEST 3: Stage Distribution
-- ============================================
\echo '--- TEST 3: Stage Distribution ---'
\echo 'Expected: All records have valid Stage values'
\echo ''

SELECT 
    "Stage", 
    COUNT(*) as "Count",
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as "Percentage"
FROM public."Opportunities"
GROUP BY "Stage"
ORDER BY "Count" DESC;

\echo ''
\echo ''

-- ============================================
-- TEST 4: Sample Recent Opportunities
-- ============================================
\echo '--- TEST 4: Sample Opportunities (Most Recent 10) ---'
\echo 'Expected: All have valid Stage values'
\echo ''

SELECT 
    "Id",
    LEFT("Name", 50) as "Name",
    "Stage",
    "Status",
    TO_CHAR("CreatedDate", 'YYYY-MM-DD HH24:MI') as "CreatedDate"
FROM public."Opportunities"
ORDER BY "Id" DESC
LIMIT 10;

\echo ''
\echo ''

-- ============================================
-- TEST 5: Legacy Records Check
-- ============================================
\echo '--- TEST 5: Legacy Records (Created Before Jan 22, 2026) ---'
\echo 'Expected: 0 NULL stages in legacy records'
\echo ''

SELECT 
    COUNT(*) as "TotalLegacyRecords",
    COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) as "WithValidStage",
    COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) as "WithNULLStage"
FROM public."Opportunities"
WHERE "CreatedDate" < '2026-01-22 00:00:00';

\echo ''
\echo 'Result: '
SELECT 
    CASE 
        WHEN COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) = 0 
        THEN '✅ PASS: All legacy records have valid Stage values'
        ELSE '❌ FAIL: Found ' || COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) || ' legacy records with NULL/empty Stage'
    END as "Test5_Result"
FROM public."Opportunities"
WHERE "CreatedDate" < '2026-01-22 00:00:00';

\echo ''
\echo ''

-- ============================================
-- VERIFICATION SUMMARY
-- ============================================
\echo '========================================'
\echo 'VERIFICATION SUMMARY'
\echo '========================================'
\echo ''

SELECT 
    COUNT(*) as "TotalOpportunities",
    COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) as "ValidStage",
    COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) as "InvalidStage",
    ROUND(COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) * 100.0 / COUNT(*), 2) as "PercentageValid"
FROM public."Opportunities";

\echo ''
\echo 'Overall Result: '
SELECT 
    CASE 
        WHEN COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) = 0 
        THEN '✅✅✅ ALL TESTS PASS - APPROVED FOR PRODUCTION ✅✅✅'
        ELSE '❌❌❌ TESTS FAILED - ' || COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) || ' INVALID RECORDS FOUND ❌❌❌'
    END as "OverallResult"
FROM public."Opportunities";

\echo ''
\echo '========================================'
\echo 'END OF VERIFICATION'
\echo '========================================'
\echo ''
\echo 'Next Steps:'
\echo '1. If all tests PASS: Proceed to application smoke test'
\echo '2. If any test FAILS: DO NOT deploy - contact development team'
\echo ''
\echo 'Documentation: QA Tests/Test Execution Results/PR_671_INTERACTIVE_VERIFICATION.md'
\echo ''
