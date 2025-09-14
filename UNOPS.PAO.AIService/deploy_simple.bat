@echo off
echo 🚀 Starting Cloud Run deployment...

set SERVICE_NAME=opportunity-plus-ai-test
set PROJECT_ID=unops-partneropportunity
set REGION=europe-west4

echo Deploying %SERVICE_NAME% to %PROJECT_ID% in %REGION%...
echo This will take a few minutes...

gcloud run deploy %SERVICE_NAME% --source . --region=%REGION% --platform=managed --port=8080 --memory=2Gi --cpu=1 --max-instances=10 --min-instances=1 --concurrency=80 --timeout=300 --set-env-vars=CURRENT_ENV=test,GOOGLE_CLOUD_PROJECT=%PROJECT_ID%,GOOGLE_CLOUD_LOCATION=%REGION%,GOOGLE_GENAI_USE_VERTEXAI=TRUE --quiet

echo.
if %ERRORLEVEL% equ 0 (
    echo ✅ Deployment successful!
    echo.
    echo Getting service URL...
    gcloud run services describe %SERVICE_NAME% --region=%REGION% --format="value(status.url)"
) else (
    echo ❌ Deployment failed!
)

pause 