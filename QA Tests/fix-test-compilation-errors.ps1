# PowerShell script to batch-fix common compilation errors in test files
# Created: January 23, 2026
# Purpose: Fix WorkflowStageId, Description, and other common errors

Write-Host "🔧 Starting batch fix of C# test compilation errors..." -ForegroundColor Cyan

$testProjectPath = "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
$filesFixed = 0
$totalReplacements = 0

# Target files with known errors
$targetFiles = @(
    "$testProjectPath/Opportunity/OpportunityPermissionTests.cs",
    "$testProjectPath/Opportunity/OpportunityIntegrationTests.cs",
    "$testProjectPath/Opportunity/OpportunityAdvancedFeaturesTests.cs",
    "$testProjectPath/Opportunity/OpportunityManagerIntegrationTests.cs",
    "$testProjectPath/Opportunity/IntegrationTestBase.cs",
    "$testProjectPath/Validation/OpportunityFieldLengthValidationTests.cs"
)

foreach ($file in $targetFiles) {
    if (Test-Path $file) {
        Write-Host "`n📄 Processing: $file" -ForegroundColor Yellow
        $content = Get-Content $file -Raw
        $originalContent = $content
        $fileReplacements = 0
        
        # Fix 1: Add missing using statements
        if ($content -notmatch "using UNOPS.PAO.DataAccess.Services;") {
            $content = $content -replace "(\busing Xunit;)", "using UNOPS.PAO.DataAccess.Services;`r`n`$1"
            $fileReplacements++
        }
        if ($content -notmatch "using UNOPS.PAO.DataAccess.Interfaces;") {
            $content = $content -replace "(\busing Xunit;)", "using UNOPS.PAO.DataAccess.Interfaces;`r`n`$1"
            $fileReplacements++
        }
        if ($content -notmatch "using UNOPS.PAO.Utilities.Helpers;") {
            $content = $content -replace "(\busing Xunit;)", "using UNOPS.PAO.Utilities.Helpers;`r`n`$1"
            $fileReplacements++
        }
        
        # Fix 2: Remove WorkflowStage seeding
        $content = $content -replace "[\s\S]*?_context\.WorkflowStages\.Add\(new WorkflowStage.*?\);", "        // Workflow stages are now stored as string values in Opportunity.Stage property"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Fix 3: Fix UserResolverService mock
        $content = $content -replace "new Mock<UserResolverService<int>>\(null\)", "new Mock<UserResolverService<int>>(MockBehavior.Loose, new object?[] { null })"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Fix 4: Initialize _mockExchangeRateService
        $content = $content -replace "(_mockServiceProvider = new Mock<IServiceProvider>\(\);)", "`$1`r`n        _mockExchangeRateService = new Mock<IExchangeRateService>();"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Fix 5: Replace WorkflowStageId = 1 with Stage = "IDENTIFY & PROFILE" + add Description
        $content = $content -replace "WorkflowStageId = 1,(\s+)Status = EntityStatus\.Draft,", "Description = ""Test Description"",`r`n            Stage = ""IDENTIFY & PROFILE"",`$1Status = EntityStatus.Draft,"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Fix 6: Add Description when missing in Opportunity initializers
        $content = $content -replace "(\s+Id = \d+,\s+Name = [^,]+,)(\s+Stage = )", "`$1`r`n            Description = ""Test Description"",`$2"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Fix 7: Replace EntityStatus.Draft with "Draft" in string contexts
        $content = $content -replace "Status = EntityStatus\.Draft\s*}\s*;", "Status = ""Draft"" };"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Fix 8: Fix FluentAssertions Because parameter (capital B to lowercase b)
        $content = $content -replace ",\s*Because:", ", because:"
        if ($content -ne $originalContent) { $fileReplacements++ }
        
        # Save if changes were made
        if ($content -ne $originalContent) {
            Set-Content -Path $file -Value $content -NoNewline
            $filesFixed++
            $totalReplacements += $fileReplacements
            Write-Host "  ✅ Fixed $fileReplacements issues" -ForegroundColor Green
        } else {
            Write-Host "  ℹ️  No changes needed" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ⚠️  File not found: $file" -ForegroundColor Red
    }
}

Write-Host "`n✅ Batch fix complete!" -ForegroundColor Cyan
Write-Host "Files processed: $filesFixed" -ForegroundColor White
Write-Host "Total replacements: $totalReplacements" -ForegroundColor White
Write-Host "`n🔨 Building project to check remaining errors..." -ForegroundColor Cyan

# Build and count remaining errors
$buildOutput = & dotnet build "$testProjectPath/UNOPS.PAO.Business.Tests.csproj" 2>&1
$errorCount = ($buildOutput | Select-String -Pattern "error CS" | Measure-Object).Count
Write-Host "Remaining compilation errors: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { "Green" } else { "Yellow" })

if ($errorCount -gt 0) {
    Write-Host "`n📋 Top error patterns:" -ForegroundColor Cyan
    $buildOutput | Select-String -Pattern "error CS" | Select-Object -First 10 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
}
