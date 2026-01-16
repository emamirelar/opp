@echo off
REM ============================================================================
REM External Data Service (EDS) Runner
REM ============================================================================
REM Purpose: Runs the External Data Service to populate database with external data
REM
REM Prerequisites:
REM   - Cloud SQL tunnel must be running (connect-cloud-sql-tunnel.bat)
REM   - .NET 9.0 SDK installed
REM   - Git submodules initialized
REM
REM Usage:
REM   1. Ensure Cloud SQL tunnel is running in another terminal
REM   2. Run this script: Scripts\run-external-data-service.bat
REM   3. Service will populate data from external sources
REM ============================================================================

echo.
echo ============================================================================
echo  External Data Service (EDS) Runner
echo ============================================================================
echo.

REM Navigate to project root
cd /d "%~dp0.."

echo [1/3] Initializing Git submodules...
echo ----------------------------------------
git submodule update --init --recursive

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ ERROR: Failed to initialize Git submodules
    echo.
    echo Troubleshooting:
    echo   - Ensure you have Git installed
    echo   - Check your network connection
    echo   - Verify repository access permissions
    echo.
    pause
    exit /b 1
)

echo ✅ Git submodules initialized
echo.

echo [2/3] Navigating to External Data Service...
echo ----------------------------------------
cd UNOPS.PAO.ExternalDataService

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ ERROR: UNOPS.PAO.ExternalDataService directory not found
    echo.
    echo Troubleshooting:
    echo   - Verify the directory exists in your repository
    echo   - Check your current working directory
    echo.
    pause
    exit /b 1
)

echo ✅ Located External Data Service
echo.

echo [3/3] Starting External Data Service...
echo ----------------------------------------
echo.
echo 🔑 Getting fresh IAM access token...

REM Get fresh access token for database authentication
for /f "delims=" %%i in ('gcloud auth print-access-token') do set IAM_TOKEN=%%i

if "%IAM_TOKEN%"=="" (
    echo.
    echo ❌ ERROR: Failed to get IAM access token
    echo.
    echo Troubleshooting:
    echo   - Run: gcloud auth login
    echo   - Run: gcloud auth application-default login
    echo   - Verify you're logged into the correct Google account
    echo.
    pause
    exit /b 1
)

echo ✅ IAM token obtained successfully
echo.

REM Get current Google Cloud account email
for /f "tokens=*" %%i in ('gcloud config get-value account') do set GCLOUD_ACCOUNT=%%i

if "%GCLOUD_ACCOUNT%"=="" (
    echo.
    echo ❌ ERROR: Could not determine Google Cloud account
    echo.
    echo Troubleshooting:
    echo   - Run: gcloud auth login
    echo   - Verify you're logged into your UNOPS account
    echo.
    pause
    exit /b 1
)

REM Extract username from email (before @)
for /f "tokens=1 delims=@" %%a in ("%GCLOUD_ACCOUNT%") do set DB_USERNAME=%%a

REM Construct database name
set DB_NAME=unops-opportunityplus-dev-db-%DB_USERNAME%

echo ✅ Google Cloud Account: %GCLOUD_ACCOUNT%
echo ✅ Database Name: %DB_NAME%
echo.

REM Set ASPNETCORE_ENVIRONMENT to Local
set ASPNETCORE_ENVIRONMENT=Local
set DOTNET_ENVIRONMENT=Local

REM Set connection strings with IAM token as environment variables
REM This overrides the appsettings.Local.json connection strings
set ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=6364;Database=%DB_NAME%;Username=%GCLOUD_ACCOUNT%;Password=%IAM_TOKEN%;
set ConnectionStrings__ApplicationConnection=Host=127.0.0.1;Port=6364;Database=%DB_NAME%;Username=%GCLOUD_ACCOUNT%;Password=%IAM_TOKEN%;

echo 🚀 Running External Data Service with Local environment
echo.
echo Environment Variables Set:
echo   - ASPNETCORE_ENVIRONMENT=Local
echo   - Database: %DB_NAME%
echo   - Username: %GCLOUD_ACCOUNT%
echo   - Connection strings configured with IAM token
echo.
echo ⚠️  IMPORTANT: Ensure Cloud SQL tunnel is running before proceeding!
echo     If not running, press Ctrl+C and run: Scripts\connect-cloud-sql-tunnel.bat
echo.
echo Press any key to continue...
pause >nul

dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ ERROR: External Data Service failed to run
    echo.
    echo Troubleshooting:
    echo   - Verify Cloud SQL tunnel is running (port 6364)
    echo   - Check appsettings.json configuration
    echo   - Ensure database connection is working
    echo   - Review error messages above
    echo.
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo ✅ External Data Service completed successfully
echo ============================================================================
echo.
pause
