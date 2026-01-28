#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the PostgreSQL test database for UNOPS Opportunity+ integration tests
.DESCRIPTION
    This script creates the test database, test user, and runs EF Core migrations
    Requires PostgreSQL to be installed and running locally
.NOTES
    Author: UNOPS Opportunity+ Development Team
    Date: January 23, 2026
#>

param(
    [string]$PostgresUser = "postgres",
    [string]$PostgresPassword = "",
    [switch]$SkipMigrations = $false
)

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  UNOPS PAO Test Database Setup" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL is installed
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "❌ ERROR: PostgreSQL (psql) not found in PATH" -ForegroundColor Red
    Write-Host "   Please install PostgreSQL and ensure psql is in your PATH" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ PostgreSQL found: $($psqlPath.Source)" -ForegroundColor Green
Write-Host ""

# Create database and user
Write-Host "[STEP 1] Creating test database and user..." -ForegroundColor Yellow

$sqlScript = @"
-- Create test database
CREATE DATABASE unops_pao_test;

-- Create test user
CREATE USER pao_test_user WITH ENCRYPTED PASSWORD 'Test_Pass_123!';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE unops_pao_test TO pao_test_user;
"@

# Run SQL script
if ($PostgresPassword) {
    $env:PGPASSWORD = $PostgresPassword
}

try {
    $sqlScript | psql -U $PostgresUser -h localhost 2>&1 | Out-Null
    
    # Check if database was created
    $dbExists = psql -U $PostgresUser -h localhost -t -c "SELECT 1 FROM pg_database WHERE datname='unops_pao_test'" 2>&1
    
    if ($dbExists -match "1") {
        Write-Host "✅ Database 'unops_pao_test' created successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Database may already exist or creation had issues" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Database creation encountered an issue: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   This may be normal if database already exists" -ForegroundColor Gray
}

# Set up schema and permissions
Write-Host "[STEP 2] Configuring schema and permissions..." -ForegroundColor Yellow

$schemaScript = @"
-- Set up schema
CREATE SCHEMA IF NOT EXISTS public;
GRANT ALL ON SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO pao_test_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO pao_test_user;

-- Grant default privileges
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO pao_test_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO pao_test_user;
"@

try {
    $schemaScript | psql -U $PostgresUser -h localhost -d unops_pao_test 2>&1 | Out-Null
    Write-Host "✅ Schema and permissions configured" -ForegroundColor Green
} catch {
    Write-Host "❌ ERROR configuring schema: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Run EF Core migrations (unless skipped)
if (-not $SkipMigrations) {
    Write-Host "[STEP 3] Running EF Core migrations..." -ForegroundColor Yellow
    
    $env:ASPNETCORE_ENVIRONMENT = "Testing"
    
    try {
        $migrationOutput = dotnet ef database update `
            --project "UNOPS.PAO.UNOPSDataAccess\UNOPS.PAO.UNOPSDataAccess.csproj" `
            --startup-project "UNOPS.PAO.Server\UNOPS.PAO.Server.csproj" `
            --context UNOPSAppDbContext `
            2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migrations applied successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Migration failed" -ForegroundColor Red
            Write-Host $migrationOutput -ForegroundColor Gray
        }
    } catch {
        Write-Host "❌ ERROR running migrations: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[STEP 3] Skipping migrations (--SkipMigrations flag set)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test Database Configuration:" -ForegroundColor White
Write-Host "  Host: localhost" -ForegroundColor Gray
Write-Host "  Port: 5432" -ForegroundColor Gray
Write-Host "  Database: unops_pao_test" -ForegroundColor Gray
Write-Host "  Username: pao_test_user" -ForegroundColor Gray
Write-Host "  Password: Test_Pass_123!" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor White
Write-Host "  1. Verify configuration files exist:" -ForegroundColor Gray
Write-Host "     - QA Tests/C# Tests/UNOPS.PAO.Business.Tests/appsettings.Testing.json" -ForegroundColor Gray
Write-Host "     - QA Tests/C# Tests/UNOPS.PAO.FastTests/appsettings.Testing.json" -ForegroundColor Gray
Write-Host "     - QA Tests/Integration Tests/appsettings.Testing.json" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Run tests:" -ForegroundColor Gray
Write-Host "     cd 'QA Tests/C# Tests/UNOPS.PAO.Business.Tests'" -ForegroundColor Gray
Write-Host "     dotnet test" -ForegroundColor Gray
Write-Host ""

if ($env:PGPASSWORD) {
    Remove-Item env:PGPASSWORD
}
