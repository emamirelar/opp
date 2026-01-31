# PR #671 Database Verification Script
# Run this script to verify the database migration

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "PR #671 - Database Verification Script" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Function to run SQL query
function Run-SqlQuery {
    param(
        [string]$Query,
        [string]$TestName
    )
    
    Write-Host "`n--- $TestName ---" -ForegroundColor Yellow
    Write-Host "Query: $Query`n" -ForegroundColor Gray
    
    # Note: You need to update connection string
    # This is a template - replace with your actual connection details
    Write-Host "⚠️  Update connection string in this script before running" -ForegroundColor Red
    Write-Host "   See lines 15-20 for connection details`n" -ForegroundColor Red
}

# Connection Details (UPDATE THESE!)
$Server = "localhost"  # Your PostgreSQL server
$Port = "5432"        # Your PostgreSQL port
$Database = "opportunityplus"  # Your database name
$Username = "postgres"  # Your username
# Password should be stored securely, not in script

Write-Host "Current Connection Settings:" -ForegroundColor Cyan
Write-Host "  Server: $Server" -ForegroundColor White
Write-Host "  Port: $Port" -ForegroundColor White
Write-Host "  Database: $Database" -ForegroundColor White
Write-Host "  Username: $Username" -ForegroundColor White
Write-Host "`n⚠️  Make sure these settings are correct!`n" -ForegroundColor Yellow

$continue = Read-Host "Do you want to continue? (Y/N)"
if ($continue -ne "Y" -and $continue -ne "y") {
    Write-Host "`nExiting...`n" -ForegroundColor Red
    exit
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SQL QUERIES TO RUN MANUALLY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Copy and paste these queries into pgAdmin, DBeaver, or psql:`n" -ForegroundColor Yellow

# Test 1: Migration Applied
Write-Host "`n-- TEST 1: Check if migration was applied" -ForegroundColor Green
Write-Host @"
SELECT 
    'Test 1: Migration Applied' as "Test",
    CASE 
        WHEN COUNT(*) = 1 THEN '✅ PASS: Migration applied'
        ELSE '❌ FAIL: Migration not found'
    END as "Result"
FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20260122185435_SetDefaultStageForOpportunity';
"@ -ForegroundColor White

# Test 2: No NULL/Empty Stages
Write-Host "`n`n-- TEST 2: Check for NULL/empty Stage values" -ForegroundColor Green
Write-Host @"
SELECT 
    'Test 2: NULL/Empty Stage Check' as "Test",
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ PASS: No NULL/empty stages'
        ELSE '❌ FAIL: ' || COUNT(*) || ' records have NULL/empty Stage'
    END as "Result"
FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';
"@ -ForegroundColor White

# Test 3: Stage Distribution
Write-Host "`n`n-- TEST 3: Stage distribution" -ForegroundColor Green
Write-Host @"
SELECT 
    'Test 3: Stage Distribution' as "Test",
    "Stage", 
    COUNT(*) as "Count",
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as "Percentage"
FROM public."Opportunities"
GROUP BY "Stage"
ORDER BY "Count" DESC;
"@ -ForegroundColor White

# Test 4: Sample Records
Write-Host "`n`n-- TEST 4: Sample recent opportunities" -ForegroundColor Green
Write-Host @"
SELECT 
    'Test 4: Sample Opportunities' as "Test",
    "Id",
    LEFT("Name", 50) as "Name",
    "Stage",
    "Status",
    "CreatedDate"
FROM public."Opportunities"
ORDER BY "Id" DESC
LIMIT 10;
"@ -ForegroundColor White

# Test 5: Legacy Records
Write-Host "`n`n-- TEST 5: Legacy records check (before Jan 22, 2026)" -ForegroundColor Green
Write-Host @"
SELECT 
    'Test 5: Legacy Records' as "Test",
    COUNT(*) as "TotalLegacyRecords",
    COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) as "WithValidStage",
    COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) as "WithNULLStage"
FROM public."Opportunities"
WHERE "CreatedDate" < '2026-01-22';
"@ -ForegroundColor White

# Summary
Write-Host "`n`n-- SUMMARY: Overall verification" -ForegroundColor Green
Write-Host @"
SELECT 
    'VERIFICATION SUMMARY' as "Test",
    COUNT(*) as "TotalOpportunities",
    COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) as "ValidStage",
    COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) as "InvalidStage",
    CASE 
        WHEN COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) = 0 
        THEN '✅ ALL TESTS PASS'
        ELSE '❌ TESTS FAILED'
    END as "OverallResult"
FROM public."Opportunities";
"@ -ForegroundColor White

Write-Host "`n`n========================================" -ForegroundColor Cyan
Write-Host "EXPECTED RESULTS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Test 1: Should show '✅ PASS: Migration applied'" -ForegroundColor White
Write-Host "Test 2: Should show '✅ PASS: No NULL/empty stages'" -ForegroundColor White
Write-Host "Test 3: Should show stage distribution (mostly 'IDENTIFY & PROFILE')" -ForegroundColor White
Write-Host "Test 4: Should show 10 recent opportunities with valid Stage values" -ForegroundColor White
Write-Host "Test 5: Should show 0 NULL stages in legacy records" -ForegroundColor White
Write-Host "Summary: Should show 'InvalidStage = 0' and '✅ ALL TESTS PASS'" -ForegroundColor White

Write-Host "`n`n========================================" -ForegroundColor Cyan
Write-Host "NEXT STEPS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "1. Connect to your PostgreSQL database using:" -ForegroundColor Yellow
Write-Host "   - pgAdmin, DBeaver, or other GUI tool" -ForegroundColor White
Write-Host "   - OR psql command line:`n" -ForegroundColor White
Write-Host "     psql -h $Server -p $Port -U $Username -d $Database`n" -ForegroundColor Gray

Write-Host "2. Copy/paste each query above and run it" -ForegroundColor Yellow

Write-Host "`n3. Record your results in:" -ForegroundColor Yellow
Write-Host "   QA Tests\Test Execution Results\PR_671_INTERACTIVE_VERIFICATION.md`n" -ForegroundColor White

Write-Host "4. If all database tests pass, proceed to smoke test:" -ForegroundColor Yellow
Write-Host "   - Start the application (dotnet run or F5 in Visual Studio)" -ForegroundColor White
Write-Host "   - Open browser and follow test steps in PR_671_QUICK_TEST_GUIDE.md`n" -ForegroundColor White

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Press any key to exit..." -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
