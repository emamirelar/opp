@echo off
echo Minimal Cloud Run deployment...
gcloud run deploy opportunity-plus-ai-test --source . --region=europe-west4 --platform=managed --allow-unauthenticated --port=8080 --memory=2Gi --set-env-vars=CURRENT_ENV=test,GOOGLE_CLOUD_PROJECT=unops-partneropportunity,GOOGLE_CLOUD_LOCATION=europe-west4,GOOGLE_GENAI_USE_VERTEXAI=TRUE,DEV_EMAIL=anushas@unops.org,IS_DEVELOPMENT=true
pause 