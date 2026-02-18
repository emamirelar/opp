# =============================================================================
# Generate Database Schema for Pre-built Test Database
# =============================================================================
# This script:
# 1. Starts a temporary PostgreSQL container
# 2. Runs all EF Core migrations
# 3. Exports the schema to a SQL file
# 4. Stops the container
#
# Run this whenever migrations are added to update the pre-built image.
# =============================================================================

param(
    [switch]$SkipMigrations,
    [string]$OutputFile = "init-scripts/02-schema.sql"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Generating Pre-built Database Schema ===" -ForegroundColor Cyan

# Container settings
$containerName = "opportunityplus-schema-gen"
$dbName = "TestDb"
$dbUser = "test"
$dbPassword = "test"
$dbPort = 5433  # Use different port to avoid conflicts

# Cleanup any existing container
Write-Host "`n[1/6] Cleaning up existing containers..." -ForegroundColor Yellow
docker rm -f $containerName 2>$null

# Start PostgreSQL with pgvector
Write-Host "`n[2/6] Starting PostgreSQL with pgvector..." -ForegroundColor Yellow
docker run -d `
    --name $containerName `
    -e POSTGRES_DB=$dbName `
    -e POSTGRES_USER=$dbUser `
    -e POSTGRES_PASSWORD=$dbPassword `
    -p "${dbPort}:5432" `
    pgvector/pgvector:pg16

# Wait for PostgreSQL to be ready
Write-Host "`n[3/6] Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
do {
    Start-Sleep -Seconds 2
    $attempt++
    $ready = docker exec $containerName pg_isready -U $dbUser -d $dbName 2>$null
    Write-Host "  Attempt $attempt/$maxAttempts..."
} while ($LASTEXITCODE -ne 0 -and $attempt -lt $maxAttempts)

if ($attempt -ge $maxAttempts) {
    Write-Host "ERROR: PostgreSQL did not become ready in time" -ForegroundColor Red
    docker logs $containerName
    docker rm -f $containerName
    exit 1
}

Write-Host "  PostgreSQL is ready!" -ForegroundColor Green

# Create extensions
Write-Host "`n[4/6] Creating extensions..." -ForegroundColor Yellow
$env:PGPASSWORD = $dbPassword
docker exec $containerName psql -U $dbUser -d $dbName -c "CREATE EXTENSION IF NOT EXISTS vector;"
docker exec $containerName psql -U $dbUser -d $dbName -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"

if (-not $SkipMigrations) {
    # Run EF Core migrations
    Write-Host "`n[5/6] Running EF Core migrations (this may take 20-30 minutes)..." -ForegroundColor Yellow
    $connectionString = "Host=localhost;Port=$dbPort;Database=$dbName;Username=$dbUser;Password=$dbPassword;Include Error Detail=true"
    
    # Change to project root
    Push-Location (Join-Path $PSScriptRoot "../..")
    
    try {
        # Run Identity migrations first
        Write-Host "  Running Identity migrations..." -ForegroundColor Gray
        dotnet ef database update `
            --project UNOPS.PAO.DataAccess `
            --startup-project UNOPS.PAO.Server `
            --context PAOIdentityDbContext `
            --connection $connectionString
        
        # Run Application migrations
        Write-Host "  Running Application migrations (376 migrations)..." -ForegroundColor Gray
        $startTime = Get-Date
        dotnet ef database update `
            --project UNOPS.PAO.UNOPSDataAccess `
            --startup-project UNOPS.PAO.Server `
            --context UNOPSAppDbContext `
            --connection $connectionString
        $duration = (Get-Date) - $startTime
        Write-Host "  Migrations completed in $($duration.TotalMinutes.ToString('F1')) minutes" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Export schema
Write-Host "`n[6/6] Exporting database schema..." -ForegroundColor Yellow
$schemaFile = Join-Path $PSScriptRoot $OutputFile

# Use pg_dump to export schema and data
docker exec $containerName pg_dump -U $dbUser -d $dbName --no-owner --no-privileges > $schemaFile

Write-Host "  Schema exported to: $schemaFile" -ForegroundColor Green
$fileSize = (Get-Item $schemaFile).Length / 1MB
Write-Host "  File size: $($fileSize.ToString('F2')) MB" -ForegroundColor Gray

# Cleanup
Write-Host "`nCleaning up temporary container..." -ForegroundColor Yellow
docker rm -f $containerName

Write-Host "`n=== Schema Generation Complete ===" -ForegroundColor Cyan
Write-Host @"

Next steps:
1. Review the generated schema: $schemaFile
2. Build the Docker image:
   cd docker/test-database
   docker build -t ghcr.io/unops-itg/opportunityplus-testdb:latest .

3. Push to GitHub Container Registry:
   docker push ghcr.io/unops-itg/opportunityplus-testdb:latest

4. Update .github/workflows/playwright-tests.yml to use the new image

"@ -ForegroundColor White
