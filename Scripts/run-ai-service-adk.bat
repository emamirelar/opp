@echo off
REM ============================================================
REM Run AI Service with ADK Web Interface
REM ============================================================
REM This script:
REM 1. Gets a fresh IAM access token
REM 2. Sets DATABASE_URL with the token
REM 3. Activates the Python virtual environment
REM 4. Runs the AI Service with ADK Web UI
REM
REM Prerequisites:
REM   - SSH tunnel must be running (Scripts\connect-cloud-sql-tunnel.bat)
REM   - gcloud CLI authenticated
REM ============================================================

echo.
echo ============================================================
echo Starting AI Service with ADK Web Interface
echo ============================================================
echo.

REM Check if SSH tunnel reminder
echo NOTE: Make sure SSH tunnel is running!
echo       Run: Scripts\connect-cloud-sql-tunnel.bat
echo.

REM Get fresh access token
echo [1/3] Getting fresh IAM access token...
for /f "delims=" %%i in ('gcloud auth print-access-token') do set IAM_TOKEN=%%i

if "%IAM_TOKEN%"=="" (
    echo ERROR: Failed to get access token. Make sure you're logged in with gcloud.
    echo Run: gcloud auth login
    pause
    exit /b 1
)

echo Token obtained successfully!
echo.

REM Set DATABASE_URL with token
echo [2/3] Setting DATABASE_URL...
set DATABASE_URL=postgresql+asyncpg://anushas%%40unops.org:%IAM_TOKEN%@localhost:6364/unops-opportunityplus-dev-db-anushas
set CURRENT_ENV=local
set GOOGLE_CLOUD_PROJECT=unops-opportunityplus-dev

echo DATABASE_URL configured for unops-opportunityplus-dev-db-anushas database
echo.

REM Navigate to AI Service and activate venv
echo [3/3] Starting ADK Web Interface...
echo.
cd /d "%~dp0..\UNOPS.PAO.AIService"

REM Activate virtual environment and run adk web
call venv\Scripts\activate.bat

echo ============================================================
echo ADK Web Interface starting at: http://localhost:8080
echo This provides a UI for testing AI agents
echo Press Ctrl+C to stop
echo ============================================================
echo.

adk web --port 8080 --reload_agents

pause

