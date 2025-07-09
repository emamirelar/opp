@echo off
echo 🔐 Setting up Secret Manager permissions for Cloud Run...
echo.

set PROJECT_ID=unops-partneropportunity
set REGION=europe-west4
set SERVICE_NAME=unops-pai-service

echo 📋 Project: %PROJECT_ID%
echo 🌍 Region: %REGION%
echo 🚀 Service: %SERVICE_NAME%
echo.

echo 🔧 Getting Cloud Run service account...
for /f "tokens=*" %%i in ('gcloud run services describe %SERVICE_NAME% --region=%REGION% --format="value(spec.template.spec.serviceAccountName)" 2^>nul') do set SERVICE_ACCOUNT=%%i

if "%SERVICE_ACCOUNT%"=="" (
    echo 📝 No custom service account found, using default Compute Engine service account...
    set SERVICE_ACCOUNT=%PROJECT_ID%-compute@developer.gserviceaccount.com
) else (
    echo ✅ Found service account: %SERVICE_ACCOUNT%
)

echo.
echo 🔑 Granting Secret Manager permissions...

:: Grant Secret Manager Secret Accessor role
gcloud projects add-iam-policy-binding %PROJECT_ID% ^
    --member="serviceAccount:%SERVICE_ACCOUNT%" ^
    --role="roles/secretmanager.secretAccessor"

if %ERRORLEVEL% equ 0 (
    echo ✅ Secret Manager permissions granted successfully!
    echo.
    echo 🎯 The service account %SERVICE_ACCOUNT% can now access secrets.
    echo.
    echo 🔧 Next steps:
    echo   1. Run setup_secret.bat to create your database secret
    echo   2. Deploy your application using deploy_simple.bat
    echo   3. Your app will now use Secret Manager for database connections
) else (
    echo ❌ Failed to grant permissions!
    echo Check your authentication and project access.
)

echo.
pause 