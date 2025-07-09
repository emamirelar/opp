@echo off
echo.
echo ========================================
echo   Opportunity+ AI Agent - Master Deployment
echo ========================================
echo.

echo 📋 What would you like to do?
echo.
echo   1. Full deployment (setup + deploy)
echo   2. Setup gcloud only
echo   3. Deploy only (skip setup)
echo   4. Manual setup instructions
echo.
set /p choice="Enter your choice (1-4): "

if "%choice%"=="1" (
    echo.
    echo 🔄 Running full deployment process...
    echo.
    echo Step 1: Setting up gcloud...
    call setup_gcloud.bat
    echo.
    echo Step 2: Deploying to Cloud Run...
    call deploy_simple.bat
    goto end
)

if "%choice%"=="2" (
    echo.
    echo 🔧 Running gcloud setup only...
    call setup_gcloud.bat
    goto end
)

if "%choice%"=="3" (
    echo.
    echo 🚀 Running deployment only...
    echo Make sure gcloud is configured first!
    pause
    call deploy_simple.bat
    goto end
)

if "%choice%"=="4" (
    echo.
    echo 📋 Manual Setup Instructions:
    echo.
    echo 1. Authenticate with Google Cloud:
    echo    gcloud auth login
    echo.
    echo 2. Set your project:
    echo    gcloud config set project unops-partneropportunity
    echo.
    echo 3. Set your region:
    echo    gcloud config set run/region europe-west4
    echo.
    echo 4. Enable APIs:
    echo    gcloud services enable run.googleapis.com
    echo    gcloud services enable secretmanager.googleapis.com
    echo.
    echo 5. Then run: deploy_simple.bat
    echo.
    goto end
)

echo ❌ Invalid choice. Please run the script again.

:end
echo.
echo 🎯 Deployment process complete!
echo.
echo 📚 Available scripts:
echo   - deploy_master.bat   (this file - main menu)
echo   - setup_gcloud.bat    (gcloud configuration only)
echo   - deploy_simple.bat   (deployment only)
echo.
pause 