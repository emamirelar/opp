#!/bin/bash
# External Data Integration Service - Deployment Script
# Usage: ./deploy.sh [environment] [project-id]

set -e  # Exit on any error

# Configuration
ENVIRONMENT=${1:-production}
PROJECT_ID=${2:-unops-pao-production}
REGION="us-central1"
SERVICE_NAME="unops-pao-external-data-service"

echo "🚀 Deploying External Data Integration Service"
echo "Environment: $ENVIRONMENT"
echo "Project: $PROJECT_ID"
echo "Region: $REGION"
echo "Service: $SERVICE_NAME"
echo

# Authenticate with gcloud (if not already authenticated)
if ! gcloud auth list --filter=status:ACTIVE --format="value(account)" | grep -q .; then
    echo "❌ Not authenticated with gcloud. Please run: gcloud auth login"
    exit 1
fi

# Set the project
gcloud config set project $PROJECT_ID

# Enable required APIs
echo "🔧 Enabling required Google Cloud APIs..."
gcloud services enable \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    cloudscheduler.googleapis.com \
    secretmanager.googleapis.com \
    bigquery.googleapis.com \
    sqladmin.googleapis.com

# Create service accounts if they don't exist
echo "👤 Setting up service accounts..."

# Service account for Cloud Run
SA_NAME="external-data-service"
SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

if ! gcloud iam service-accounts describe $SA_EMAIL &>/dev/null; then
    gcloud iam service-accounts create $SA_NAME \
        --display-name="External Data Integration Service" \
        --description="Service account for the external data integration service"
fi

# Grant necessary permissions to the service account
echo "🔑 Configuring IAM permissions..."
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/cloudsql.client"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/bigquery.jobUser"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/bigquery.dataViewer"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/secretmanager.secretAccessor"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/logging.logWriter"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/monitoring.metricWriter"

# Service account for Cloud Scheduler
SCHEDULER_SA_NAME="scheduler"
SCHEDULER_SA_EMAIL="${SCHEDULER_SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

if ! gcloud iam service-accounts describe $SCHEDULER_SA_EMAIL &>/dev/null; then
    gcloud iam service-accounts create $SCHEDULER_SA_NAME \
        --display-name="Cloud Scheduler Service Account" \
        --description="Service account for Cloud Scheduler jobs"
fi

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SCHEDULER_SA_EMAIL" \
    --role="roles/run.invoker"

# Create secrets if they don't exist
echo "🔐 Setting up secrets..."

# Database connection string secret
DB_SECRET_NAME="database-connection-string"
if ! gcloud secrets describe $DB_SECRET_NAME &>/dev/null; then
    echo "Creating database connection string secret..."
    echo "Please enter the database connection string:"
    read -s DB_CONNECTION_STRING
    echo -n "$DB_CONNECTION_STRING" | gcloud secrets create $DB_SECRET_NAME --data-file=-
fi

# Admin API key secret
API_KEY_SECRET_NAME="admin-api-key"
if ! gcloud secrets describe $API_KEY_SECRET_NAME &>/dev/null; then
    echo "Creating admin API key secret..."
    API_KEY=$(openssl rand -hex 32)
    echo -n "$API_KEY" | gcloud secrets create $API_KEY_SECRET_NAME --data-file=-
    echo "Generated admin API key: $API_KEY"
    echo "Save this API key securely - you'll need it for accessing the admin interface."
fi

# Grant access to secrets
gcloud secrets add-iam-policy-binding $DB_SECRET_NAME \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding $API_KEY_SECRET_NAME \
    --member="serviceAccount:$SA_EMAIL" \
    --role="roles/secretmanager.secretAccessor"

# Build and deploy the service
echo "🏗️  Building and deploying the service..."
cd "$(dirname "$0")/.."

# Submit the build
gcloud builds submit \
    --config=cloudbuild.yaml \
    --substitutions=_ENVIRONMENT=$ENVIRONMENT,_PROJECT_ID=$PROJECT_ID

echo "✅ Build completed successfully"

# Wait for the service to be ready
echo "⏳ Waiting for service to be ready..."
sleep 30

# Get the service URL
SERVICE_URL=$(gcloud run services describe $SERVICE_NAME --region=$REGION --format="value(status.url)")
echo "🌐 Service deployed at: $SERVICE_URL"

# Test the health endpoint
echo "🏥 Testing health endpoint..."
if curl -f -s "$SERVICE_URL/health" > /dev/null; then
    echo "✅ Health check passed"
else
    echo "⚠️  Health check failed - service might still be starting"
fi

# Deploy Cloud Scheduler jobs (optional)
read -p "🕐 Deploy Cloud Scheduler jobs? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Setting up Cloud Scheduler jobs..."
    
    # Get the admin API key for scheduler jobs
    ADMIN_API_KEY=$(gcloud secrets versions access latest --secret=$API_KEY_SECRET_NAME)
    
    # Create partner sync job
    if ! gcloud scheduler jobs describe external-data-partners-daily --location=$REGION &>/dev/null; then
        gcloud scheduler jobs create http external-data-partners-daily \
            --location=$REGION \
            --schedule="0 6 * * *" \
            --time-zone="UTC" \
            --uri="$SERVICE_URL/api/sync/execute/partners-prod" \
            --http-method=POST \
            --headers="Content-Type=application/json,X-API-Key=$ADMIN_API_KEY" \
            --oidc-service-account-email=$SCHEDULER_SA_EMAIL \
            --max-retry-attempts=3 \
            --max-retry-duration=1800s
        echo "✅ Created partners daily sync job"
    fi
    
    # Create health check job
    if ! gcloud scheduler jobs describe external-data-health-check --location=$REGION &>/dev/null; then
        gcloud scheduler jobs create http external-data-health-check \
            --location=$REGION \
            --schedule="*/15 * * * *" \
            --time-zone="UTC" \
            --uri="$SERVICE_URL/health" \
            --http-method=GET \
            --headers="X-API-Key=$ADMIN_API_KEY" \
            --oidc-service-account-email=$SCHEDULER_SA_EMAIL \
            --max-retry-attempts=1 \
            --max-retry-duration=60s
        echo "✅ Created health check job"
    fi
fi

echo
echo "🎉 Deployment completed successfully!"
echo
echo "📋 Service Details:"
echo "   URL: $SERVICE_URL"
echo "   Admin Interface: $SERVICE_URL/admin"
echo "   API Documentation: $SERVICE_URL/swagger"
echo "   Health Check: $SERVICE_URL/health"
echo
echo "🔑 To access the admin interface, use the API key:"
echo "   X-API-Key: $(gcloud secrets versions access latest --secret=$API_KEY_SECRET_NAME)"
echo
echo "📖 Next Steps:"
echo "   1. Upload your sync configuration files to the 'sync-configurations' secret"
echo "   2. Test the configurations using the admin interface"
echo "   3. Monitor the service logs and metrics"
echo "   4. Set up alerting for failed syncs"
echo
