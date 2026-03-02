# Simple PostgreSQL Database Setup
$psqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
$dbName = "TestDb"
$dbUser = "test"
$dbPassword = "test"
$postgresPassword = "postgres"

Write-Host "Creating database and user..." -ForegroundColor Yellow

# Create SQL commands
$sqlCommands = @"
DROP DATABASE IF EXISTS "$dbName";
DROP USER IF EXISTS $dbUser;
CREATE DATABASE "$dbName";
CREATE USER $dbUser WITH PASSWORD '$dbPassword';
GRANT ALL PRIVILEGES ON DATABASE "$dbName" TO $dbUser;
\c "$dbName"
GRANT ALL ON SCHEMA public TO $dbUser;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO $dbUser;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO $dbUser;
"@

# Save to temp file
$tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
$sqlCommands | Out-File -FilePath $tempFile -Encoding UTF8

# Run psql
$env:PGPASSWORD = $postgresPassword
& $psqlPath -U postgres -f $tempFile 2>&1

# Test connection
Write-Host "`nTesting connection..." -ForegroundColor Yellow
$env:PGPASSWORD = $dbPassword
& $psqlPath -U $dbUser -d $dbName -c "SELECT 'Database ready!' as status;" 2>&1

# Cleanup
Remove-Item $tempFile -ErrorAction SilentlyContinue
Remove-Item Env:\PGPASSWORD

Write-Host "`nDatabase setup complete!" -ForegroundColor Green
Write-Host "Database: $dbName" -ForegroundColor Cyan
Write-Host "User: $dbUser" -ForegroundColor Cyan
Write-Host "Password: $dbPassword" -ForegroundColor Cyan
