@echo off
REM ============================================================
REM Full Cloud SQL Connection Setup Script
REM ============================================================
REM This script:
REM 1. Starts the SSH tunnel in a new window
REM 2. Generates an access token for authentication
REM 
REM After running, you can connect to the database via:
REM   Host: localhost (or 127.0.0.1)
REM   Port: 6364
REM   Database: unops-opportunityplus-dev-db-[yourname]
REM   Username: yourname@unops.org
REM   Password: The access token displayed/copied
REM ============================================================

echo.
echo ============================================================
echo Cloud SQL Connection Setup (Dev Environment)
echo ============================================================
echo.

REM Start SSH tunnel in a new window
echo [1/2] Starting SSH tunnel in new window...
start "Cloud SQL SSH Tunnel" cmd /k gcloud compute ssh unopsgc567901-sql-proxy --tunnel-through-iap --project=unops-opportunityplus-dev --zone=europe-west4-b --ssh-flag="-L 6364:10.129.0.16:5432"

REM Wait a moment for the tunnel to establish
echo Waiting for tunnel to establish...
timeout /t 5 /nobreak > nul

REM Generate access token
echo.
echo [2/2] Generating access token...
echo.

for /f "delims=" %%i in ('gcloud auth print-access-token') do set ACCESS_TOKEN=%%i

echo ============================================================
echo CONNECTION DETAILS:
echo ============================================================
echo   Host:     localhost (or 127.0.0.1)
echo   Port:     6364
echo   Database: unops-opportunityplus-dev-db-[yourname]
echo   Username: yourname@unops.org
echo   Password: (copied to clipboard - see below)
echo ============================================================
echo.
echo ACCESS TOKEN (use as password):
echo %ACCESS_TOKEN%
echo.

REM Copy to clipboard
echo %ACCESS_TOKEN% | clip
echo Token has been copied to clipboard!
echo.
echo NOTE: Token expires in ~1 hour. Run get-db-access-token.bat for a new one.
echo NOTE: Keep the SSH tunnel window open while using the database.
echo.

pause
