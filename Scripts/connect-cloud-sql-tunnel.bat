@echo off
REM ============================================================
REM Cloud SQL SSH Tunnel Connection Script
REM ============================================================
REM This script creates an SSH tunnel through IAP to connect to
REM the Cloud SQL instance in the Dev environment.
REM 
REM After running this script, connect via:
REM   Host: localhost (or 127.0.0.1)
REM   Port: 6364
REM   Database: unops-opportunityplus-dev-db-[yourname]
REM   Username: yourname@unops.org
REM   Password: Run get-db-access-token.bat to get the token
REM ============================================================

echo.
echo ============================================================
echo Starting SSH Tunnel to Cloud SQL (Dev Environment)
echo ============================================================
echo.
echo Connection will be available at: localhost:6364
echo.
echo Press Ctrl+C to stop the tunnel when done.
echo.

gcloud compute ssh unopsgc567901-sql-proxy --tunnel-through-iap --project=unops-opportunityplus-dev --zone=europe-west4-b --ssh-flag="-L 6364:10.129.0.16:5432"

pause
