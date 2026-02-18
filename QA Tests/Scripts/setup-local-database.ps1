# PostgreSQL Local Database Setup Script
# Run this AFTER PostgreSQL is installed

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "UNOPS Opportunity+ Local Database Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$dbName = "TestDb"
$dbUser = "test"
$dbPassword = "test"
$postgresPassword = "postgres@99"  # Your PostgreSQL superuser password

Write-Host "Step 1: Testing PostgreSQL Connection..." -ForegroundColor Yellow

# Test if PostgreSQL is running
$pgService = Get-Service -Name postgresql* -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq 'Running' } | Select-Object -First 1

if ($pgService) {
    Write-Host "✅ PostgreSQL service is running: $($pgService.Name)" -ForegroundColor Green
} else {
    Write-Host "❌ PostgreSQL service not found or not running" -ForegroundColor Red
    Write-Host "   Please ensure PostgreSQL is installed and the service is started" -ForegroundColor Yellow
    exit 1
}

# Test psql command
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue

if (-not $psqlPath) {
    Write-Host "❌ psql command not found in PATH" -ForegroundColor Red
    Write-Host "   Add PostgreSQL bin directory to PATH:" -ForegroundColor Yellow
    Write-Host "   Example: C:\Program Files\PostgreSQL\16\bin" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Or run this script from PostgreSQL bin directory" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ psql command found: $($psqlPath.Source)" -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Creating Database and User..." -ForegroundColor Yellow

# Create SQL commands
$sqlCommands = @"
-- Drop database if exists (for clean setup)
DROP DATABASE IF EXISTS "$dbName";

-- Drop user if exists
DROP USER IF EXISTS $dbUser;

-- Create database
CREATE DATABASE "$dbName";

-- Create user with password
CREATE USER $dbUser WITH PASSWORD '$dbPassword';

-- Grant all privileges on database
GRANT ALL PRIVILEGES ON DATABASE "$dbName" TO $dbUser;

-- Connect to the new database and grant schema privileges
\c "$dbName"

-- Grant schema privileges
GRANT ALL ON SCHEMA public TO $dbUser;

-- Grant default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO $dbUser;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO $dbUser;
"@

# Save SQL to temp file
$tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
$sqlCommands | Out-File -FilePath $tempSqlFile -Encoding UTF8

try {
    # Execute SQL commands
    Write-Host "   Creating database: $dbName" -ForegroundColor Cyan
    Write-Host "   Creating user: $dbUser" -ForegroundColor Cyan
    
    # Set environment variable for password (avoids prompt)
    $env:PGPASSWORD = $postgresPassword
    
    # Execute SQL file
    & psql -U postgres -f $tempSqlFile 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database and user created successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Some warnings occurred (this is usually OK)" -ForegroundColor Yellow
    }
    
    # Clear password from environment
    Remove-Item Env:\PGPASSWORD
    
} catch {
    Write-Host "❌ Error creating database: $_" -ForegroundColor Red
    exit 1
} finally {
    # Clean up temp file
    Remove-Item $tempSqlFile -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "Step 3: Verifying Database..." -ForegroundColor Yellow

# Test connection to new database
$env:PGPASSWORD = $dbPassword
$testResult = & psql -U $dbUser -d $dbName -c "SELECT version();" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Successfully connected to $dbName as $dbUser" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to connect to database" -ForegroundColor Red
    Write-Host "   Error: $testResult" -ForegroundColor Red
}

Remove-Item Env:\PGPASSWORD

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Connection Details:" -ForegroundColor Cyan
Write-Host "  Host:     localhost" -ForegroundColor White
Write-Host "  Port:     5432" -ForegroundColor White
Write-Host "  Database: $dbName" -ForegroundColor White
Write-Host "  Username: $dbUser" -ForegroundColor White
Write-Host "  Password: $dbPassword" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Run Entity Framework migrations:" -ForegroundColor White
Write-Host "     cd UNOPS.PAO.Server" -ForegroundColor Cyan
Write-Host "     dotnet ef database update" -ForegroundColor Cyan
Write-Host ""
Write-Host "  2. Start the backend server:" -ForegroundColor White
Write-Host "     dotnet run" -ForegroundColor Cyan
Write-Host ""
