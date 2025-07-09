@echo off
echo 🔧 Setting up gcloud configuration...

set PROJECT_ID=unops-partneropportunity
set REGION=europe-west4

echo Setting project to: %PROJECT_ID%
gcloud config set project %PROJECT_ID% --quiet

echo Setting region to: %REGION%
gcloud config set run/region %REGION% --quiet

echo ✅ gcloud configuration complete!
echo Project: %PROJECT_ID%
echo Region: %REGION%
pause 