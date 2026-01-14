@echo off
REM ============================================================
REM Get Database Access Token for IAM Authentication
REM ============================================================
REM This script generates an OAuth access token to use as the
REM password when connecting to Cloud SQL with IAM authentication.
REM 
REM The token expires in approximately 1 hour.
REM ============================================================

echo.
echo ============================================================
echo Generating Access Token for Cloud SQL IAM Authentication
echo ============================================================
echo.

REM Generate and display the access token
for /f "delims=" %%i in ('gcloud auth print-access-token') do set ACCESS_TOKEN=%%i

echo Access Token (use this as your password in pgAdmin):
echo.
echo %ACCESS_TOKEN%
echo.
echo ============================================================
echo NOTE: This token expires in approximately 1 hour.
echo       Run this script again to get a new token when needed.
echo ============================================================
echo.

REM Also copy to clipboard if clip command is available
echo %ACCESS_TOKEN% | clip
echo Token has been copied to clipboard!
echo.

pause

