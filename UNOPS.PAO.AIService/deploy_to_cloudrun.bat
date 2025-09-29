@echo off
echo.
echo ========================================
echo   Opportunity+ AI Agent - Cloud Run Deployment
echo ========================================
echo.

:: Set project configuration
set PROJECT_ID=unops-partneropportunity
set REGION=europe-west4
set SERVICE_NAME=opportunity-plus-ai-test

echo 📋 Configuration:
echo   Project ID: %PROJECT_ID%
echo   Region: %REGION%
echo   Service Name: %SERVICE_NAME%
echo.

echo 🔧 Checking gcloud authentication...
gcloud auth list --filter=status:ACTIVE --format="value(account)" > temp_auth.txt
set /p ACTIVE_ACCOUNT=<temp_auth.txt
del temp_auth.txt

if "%ACTIVE_ACCOUNT%"=="" (
    echo ❌ No active gcloud authentication found!
    echo Please run: gcloud auth login
    echo Then try again.
    pause
    exit /b 1
) else (
    echo ✅ Authenticated as: %ACTIVE_ACCOUNT%
)

echo.
echo 🔧 Configuring gcloud project...
gcloud config set project %PROJECT_ID% --quiet
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to set project ID.
    pause
    exit /b 1
)
echo ✅ Project set to: %PROJECT_ID%

echo.
echo 🔧 Configuring gcloud region...
gcloud config set run/region %REGION% --quiet
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to set region.
    pause
    exit /b 1
)
echo ✅ Region set to: %REGION%

echo.
echo 🔧 Checking Cloud Run API...
gcloud services list --enabled --filter="name:run.googleapis.com" --format="value(name)" > temp_api.txt
set /p API_ENABLED=<temp_api.txt
del temp_api.txt

if "%API_ENABLED%"=="" (
    echo ⚠️ Cloud Run API is not enabled. Enabling it now...
    gcloud services enable run.googleapis.com --quiet
    if %ERRORLEVEL% neq 0 (
        echo ❌ Failed to enable Cloud Run API.
        pause
        exit /b 1
    )
    echo ✅ Cloud Run API enabled successfully
) else (
    echo ✅ Cloud Run API is already enabled
)

echo.
echo 🚀 Starting deployment to Google Cloud Run...
echo ⏱️ This may take 3-5 minutes...
echo.

:: Use --quiet to avoid interactive prompts
gcloud run deploy %SERVICE_NAME% ^
  --source . ^
  --region=%REGION% ^
  --platform=managed ^
  --allow-unauthenticated ^
  --port=8080 ^
  --memory=2Gi ^
  --cpu=1 ^
  --max-instances=10 ^
  --min-instances=1 ^
  --concurrency=80 ^
  --timeout=300 ^
  --set-env-vars=CURRENT_ENV=test,GOOGLE_CLOUD_PROJECT=%PROJECT_ID%,GOOGLE_CLOUD_LOCATION=%REGION%,GOOGLE_GENAI_USE_VERTEXAI=TRUE,DEV_EMAIL=anushas@unops.org,IS_DEVELOPMENT=true ^
  --quiet

if %ERRORLEVEL% neq 0 (
    echo.
    echo ❌ Deployment failed! 
    echo.
    echo 🔍 Common issues and solutions:
    echo   1. Authentication: gcloud auth login
    echo   2. Billing: Enable billing for project %PROJECT_ID%
    echo   3. APIs: Enable Cloud Run API, Cloud Build API
    echo   4. Permissions: Ensure you have Cloud Run Admin role
    echo.
    echo 🛠️ Debug commands:
    echo   gcloud auth list
    echo   gcloud config list
    echo   gcloud services list --enabled
    echo.
    pause
    exit /b 1
)

echo.
echo ✅ Deployment completed successfully!
echo.

echo 🌐 Retrieving service URL...
gcloud run services describe %SERVICE_NAME% --region=%REGION% --format="value(status.url)" > temp_url.txt
set /p SERVICE_URL=<temp_url.txt
del temp_url.txt

if defined SERVICE_URL (
    echo.
    echo 🎉 SUCCESS! Your application is now live at:
    echo %SERVICE_URL%
    echo.
    echo 🧪 Test these endpoints:
    echo   Health check: %SERVICE_URL%/framework/info
    echo   Configuration: %SERVICE_URL%/framework/config
    echo   Chat API: %SERVICE_URL%/chat
    echo.
    echo 💡 Quick test command:
    echo curl "%SERVICE_URL%/framework/info"
    echo.
) else (
    echo ⚠️ Could not retrieve service URL automatically.
    echo Check the Cloud Console: https://console.cloud.google.com/run
)

echo 📊 Deployment Summary:
echo   ✅ Service: %SERVICE_NAME%
echo   ✅ Environment: test (framework_config_test.json)
echo   ✅ Database: partneropportunity-qa-db
echo   ✅ Web UI: Disabled (API-only for security)
echo   ✅ Runtime configuration: Fixed and working
echo.

echo 🎯 What's next?
echo   1. Test your application using the URLs above
echo   2. View logs: gcloud run services logs read %SERVICE_NAME% --region=%REGION%
echo   3. Monitor in Console: https://console.cloud.google.com/run
echo   4. Update your frontend to use the new API URL
echo.

echo Press any key to exit...
pause > nul 