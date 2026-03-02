#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies test configuration for UNOPS Opportunity+ test suite
.DESCRIPTION
    Checks that all required configuration files exist and are properly formatted
    Verifies database connectivity (optional)
    Provides guidance for fixing any issues
.NOTES
    Author: UNOPS Opportunity+ Development Team
    Date: January 23, 2026
#>

param(
    [switch]$CheckDatabase = $false
)

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Test Configuration Verification" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check configuration files
Write-Host "[STEP 1] Checking configuration files..." -ForegroundColor Yellow
Write-Host ""

$configFiles = @(
    "QA Tests\C# Tests\UNOPS.PAO.Business.Tests\appsettings.Testing.json",
    "QA Tests\C# Tests\UNOPS.PAO.FastTests\appsettings.Testing.json",
    "QA Tests\Integration Tests\appsettings.Testing.json"
)

foreach ($file in $configFiles) {
    if (Test-Path $file) {
        Write-Host "  ✅ $file" -ForegroundColor Green
        
        # Validate JSON format
        try {
            $content = Get-Content $file -Raw | ConvertFrom-Json
            
            # Check key settings
            if ($content.GoogleCloud.UseMockServices -eq $true) {
                Write-Host "     • Using mocked Google Cloud services ✓" -ForegroundColor Gray
            }
            
            if ($content.AISettings.DisableExternalCalls -eq $true) {
                Write-Host "     • External AI calls disabled ✓" -ForegroundColor Gray
            }
        } catch {
            Write-Host "     ⚠️  WARNING: JSON format issue" -ForegroundColor Yellow
            $allGood = $false
        }
    } else {
        Write-Host "  ❌ $file (MISSING)" -ForegroundColor Red
        $allGood = $false
    }
    Write-Host ""
}

# Check Angular test utilities
Write-Host "[STEP 2] Checking Angular test utilities..." -ForegroundColor Yellow
Write-Host ""

$angularTestUtils = "UNOPS.PAO.ClientApp\src\app\shared\testing\test-utilities.ts"
if (Test-Path $angularTestUtils) {
    Write-Host "  ✅ $angularTestUtils" -ForegroundColor Green
    Write-Host "     • Angular test mocks available ✓" -ForegroundColor Gray
} else {
    Write-Host "  ❌ $angularTestUtils (MISSING)" -ForegroundColor Red
    $allGood = $false
}

Write-Host ""

# Check database connectivity (optional)
if ($CheckDatabase) {
    Write-Host "[STEP 3] Checking database connectivity..." -ForegroundColor Yellow
    Write-Host ""
    
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if ($psqlPath) {
        Write-Host "  ✅ PostgreSQL (psql) found" -ForegroundColor Green
        
        # Try to connect to test database
        $env:PGPASSWORD = "Test_Pass_123!"
        $dbCheck = psql -U pao_test_user -h localhost -d unops_pao_test -c "SELECT version();" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Test database connection successful" -ForegroundColor Green
            Write-Host "     • Database: unops_pao_test ✓" -ForegroundColor Gray
            Write-Host "     • User: pao_test_user ✓" -ForegroundColor Gray
        } else {
            Write-Host "  ❌ Cannot connect to test database" -ForegroundColor Red
            Write-Host "     Run: pwsh setup-test-database.ps1" -ForegroundColor Yellow
            $allGood = $false
        }
        
        Remove-Item env:PGPASSWORD
    } else {
        Write-Host "  ⚠️  PostgreSQL not found - skipping database check" -ForegroundColor Yellow
        Write-Host "     Install PostgreSQL to enable database tests" -ForegroundColor Gray
    }
    
    Write-Host ""
}

# Summary
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

if ($allGood) {
    Write-Host "✅ All checks passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now run the test suite:" -ForegroundColor White
    Write-Host "  cd 'QA Tests\C# Tests\UNOPS.PAO.FastTests'" -ForegroundColor Gray
    Write-Host "  dotnet test" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  cd '..\UNOPS.PAO.Business.Tests'" -ForegroundColor Gray
    Write-Host "  dotnet test" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  cd UNOPS.PAO.ClientApp" -ForegroundColor Gray
    Write-Host "  npm run test" -ForegroundColor Gray
} else {
    Write-Host "⚠️  Some issues found" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Fix missing configuration files:" -ForegroundColor White
    Write-Host "  - Configuration files should be created automatically" -ForegroundColor Gray
    Write-Host "  - Check TEST_DATABASE_SETUP_GUIDE.md for manual setup" -ForegroundColor Gray
    Write-Host ""
    
    if ($CheckDatabase -and -not $dbCheck) {
        Write-Host "Set up test database:" -ForegroundColor White
        Write-Host "  pwsh setup-test-database.ps1" -ForegroundColor Gray
    }
}

Write-Host ""
