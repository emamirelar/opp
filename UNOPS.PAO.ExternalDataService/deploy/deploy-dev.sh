#!/bin/bash
# External Data Integration Service - Development Deployment Script
# Usage: ./deploy-dev.sh [project-id]

set -e  # Exit on any error

# Configuration
PROJECT_ID=${1:-unops-pao-development}
REGION="us-central1"
SERVICE_NAME="unops-pao-external-data-service-dev"

echo "🚀 Deploying External Data Integration Service (Development)"
echo "Project: $PROJECT_ID"
echo "Region: $REGION"
echo "Service: $SERVICE_NAME"
echo

# Set the project
gcloud config set project $PROJECT_ID

# Enable required APIs
echo "🔧 Enabling required Google Cloud APIs..."
gcloud services enable \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    bigquery.googleapis.com \
    secretmanager.googleapis.com

# Create a simple development database connection secret
DB_SECRET_NAME="dev-database-connection-string"
if ! gcloud secrets describe $DB_SECRET_NAME &>/dev/null; then
    echo "Creating development database connection string..."
    # Default development connection string
    DEV_CONNECTION="Host=localhost;Database=unops_pao_external_data_dev;Username=dev;Password=dev;Port=5432;"
    echo -n "$DEV_CONNECTION" | gcloud secrets create $DB_SECRET_NAME --data-file=-
    echo "✅ Created development database connection secret"
fi

# Create development API key
API_KEY_SECRET_NAME="dev-admin-api-key"
if ! gcloud secrets describe $API_KEY_SECRET_NAME &>/dev/null; then
    echo "Creating development admin API key..."
    echo -n "dev-api-key" | gcloud secrets create $API_KEY_SECRET_NAME --data-file=-
    echo "✅ Created development API key: dev-api-key"
fi

# Build and deploy with development settings
echo "🏗️  Building and deploying development service..."
cd "$(dirname "$0")/.."

# Build the Docker image
echo "Building Docker image..."
docker build -t gcr.io/$PROJECT_ID/unops-pao-external-data-service-dev:latest .

# Push the image
echo "Pushing Docker image..."
docker push gcr.io/$PROJECT_ID/unops-pao-external-data-service-dev:latest

# Deploy to Cloud Run with development settings
echo "Deploying to Cloud Run..."
gcloud run deploy $SERVICE_NAME \
    --image gcr.io/$PROJECT_ID/unops-pao-external-data-service-dev:latest \
    --region $REGION \
    --platform managed \
    --allow-unauthenticated \
    --cpu-throttling=false \
    --min-instances 0 \
    --max-instances 2 \
    --memory 2Gi \
    --cpu 1 \
    --timeout 1800 \
    --concurrency 5 \
    --set-env-vars ASPNETCORE_ENVIRONMENT=Development \
    --set-env-vars ExternalDataService__ConfigurationPath=/app/config \
    --set-env-vars ExternalDataService__AutoCreateTables=true \
    --set-env-vars ExternalDataService__TestMode=true \
    --set-env-vars BigQuery__UseDefaultCredentials=true

# Get the service URL
SERVICE_URL=$(gcloud run services describe $SERVICE_NAME --region=$REGION --format="value(status.url)")
echo "🌐 Development service deployed at: $SERVICE_URL"

# Test the health endpoint
echo "🏥 Testing health endpoint..."
sleep 10  # Wait for service to start
if curl -f -s "$SERVICE_URL/health" > /dev/null; then
    echo "✅ Health check passed"
else
    echo "⚠️  Health check failed - checking logs..."
    gcloud logs read --service=$SERVICE_NAME --region=$REGION --limit=10
fi

echo
echo "🎉 Development deployment completed!"
echo
echo "📋 Service Details:"
echo "   URL: $SERVICE_URL"
echo "   Admin Interface: $SERVICE_URL/admin"
echo "   API Documentation: $SERVICE_URL/swagger"
echo "   Health Check: $SERVICE_URL/health"
echo
echo "🔧 Development Settings:"
echo "   Auto-create tables: Enabled"
echo "   Test mode: Enabled"
echo "   Authentication: Disabled"
echo "   Configuration path: /app/config"
echo
echo "📖 Development Workflow:"
echo "   1. Create test configurations in config/development/"
echo "   2. Access admin interface at $SERVICE_URL/admin"
echo "   3. Use API key 'dev-api-key' if needed"
echo "   4. Monitor logs: gcloud logs tail --service=$SERVICE_NAME --region=$REGION"
echo
