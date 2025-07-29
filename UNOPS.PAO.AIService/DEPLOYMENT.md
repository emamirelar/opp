# Google Cloud Run Deployment Guide

This guide will help you deploy the Opportunity+ AI Agent to Google Cloud Run.

## Prerequisites

1. **Google Cloud SDK** installed and configured
2. **Docker** installed (for local testing)
3. **Google Cloud Project** with billing enabled
4. **Required APIs** enabled:
   - Cloud Run API
   - Cloud Build API
   - Container Registry API
   - Cloud SQL Admin API (if using Cloud SQL)

## Environment Variables Required

Create a `.env` file in your project root with these variables:

```bash
# Application Environment
CURRENT_ENV=test

# Google Cloud Configuration
GOOGLE_CLOUD_PROJECT=your-project-id
GOOGLE_CLOUD_LOCATION=europe-west4
GOOGLE_GENAI_USE_VERTEXAI=TRUE

# Database Configuration
DATABASE_URL=postgresql://username:password@host:port/database

# Development Configuration
DEV_EMAIL=your-email@domain.com
IS_DEVELOPMENT=true
```

## Deployment Methods

### Method 1: Direct Deployment (Recommended for Testing)

1. **Set up your environment:**
   ```bash
   # Set your project ID
   export PROJECT_ID=your-project-id
   export REGION=europe-west4
   
   # Configure gcloud
   gcloud config set project $PROJECT_ID
   gcloud config set run/region $REGION
   ```

2. **Deploy directly to Cloud Run:**
   ```bash
   gcloud run deploy opportunity-plus-ai-test \
     --source . \
     --region=$REGION \
     --platform=managed \
     --port=8080 \
     --memory=2Gi \
     --cpu=1 \
     --max-instances=10 \
     --min-instances=1 \
     --concurrency=80 \
     --timeout=300 \
     --set-env-vars=CURRENT_ENV=test \
     --set-env-vars=GOOGLE_CLOUD_PROJECT=$PROJECT_ID \
     --set-env-vars=GOOGLE_CLOUD_LOCATION=$REGION \
     --set-env-vars=GOOGLE_GENAI_USE_VERTEXAI=TRUE \
     --set-env-vars=DEV_EMAIL=anushas@unops.org \
     --set-env-vars=IS_DEVELOPMENT=true
   ```

### Method 2: Using Cloud Build (Recommended for Production)

1. **Enable Cloud Build API:**
   ```bash
   gcloud services enable cloudbuild.googleapis.com
   ```

2. **Submit build:**
   ```bash
   gcloud builds submit --config=cloudbuild.yaml
   ```

### Method 3: Manual Docker Build and Push

1. **Build Docker image:**
   ```bash
   docker build -t gcr.io/$PROJECT_ID/opportunity-plus-ai:latest .
   ```

2. **Push to Google Container Registry:**
   ```bash
   docker push gcr.io/$PROJECT_ID/opportunity-plus-ai:latest
   ```

3. **Deploy to Cloud Run:**
   ```bash
   gcloud run deploy opportunity-plus-ai-test \
     --image gcr.io/$PROJECT_ID/opportunity-plus-ai:latest \
     --region=$REGION \
     --platform=managed \
     --allow-unauthenticated \
     --port=8080 \
     --memory=2Gi \
     --cpu=1 \
     --max-instances=10 \
     --min-instances=1 \
     --concurrency=80 \
     --timeout=300 \
     --set-env-vars=CURRENT_ENV=test \
     --set-env-vars=GOOGLE_CLOUD_PROJECT=$PROJECT_ID \
     --set-env-vars=GOOGLE_CLOUD_LOCATION=$REGION \
     --set-env-vars=GOOGLE_GENAI_USE_VERTEXAI=TRUE \
     --set-env-vars=DEV_EMAIL=anushas@unops.org \
     --set-env-vars=IS_DEVELOPMENT=true
   ```

## Database Setup

### Option 1: Cloud SQL (Recommended for Production)

1. **Create Cloud SQL instance:**
   ```bash
   gcloud sql instances create opportunityplus-test \
     --database-version=POSTGRES_15 \
     --tier=db-f1-micro \
     --region=$REGION \
     --root-password=your-secure-password
   ```

2. **Create database:**
   ```bash
   gcloud sql databases create partneropportunity-qa-db \
     --instance=opportunityplus-test
   ```

3. **Get connection name:**
   ```bash
   gcloud sql instances describe opportunityplus-test \
     --format="value(connectionName)"
   ```

4. **Update environment variables:**
   ```bash
   --set-env-vars=DATABASE_URL=postgresql://username:password@/database?host=/cloudsql/connection-name
   ```

### Option 2: External Database

Update the `DATABASE_URL` environment variable with your external database connection string:
```bash
--set-env-vars=DATABASE_URL=postgresql://username:password@host:port/database
```

## Testing the Deployment

1. **Get the service URL:**
   ```bash
   gcloud run services describe opportunity-plus-ai-test \
     --region=$REGION \
     --format="value(status.url)"
   ```

2. **Test the endpoints:**
   ```bash
   # Health check
   curl https://your-service-url/framework/info
   
   # Configuration check
   curl https://your-service-url/framework/config
   
   # Test chat endpoint
   curl -X POST https://your-service-url/chat \
     -H "Content-Type: application/json" \
     -d '{
       "app_name": "ai_assistant",
       "user_id": "test-user",
       "session_id": "test-session",
       "message": "Hello, how are you?",
       "streaming": false
     }'
   ```

## Security Considerations

### For Production Deployment:

1. **Remove public access:**
   ```bash
   gcloud run services remove-iam-policy-binding opportunity-plus-ai-test \
     --member="allUsers" \
     --role="roles/run.invoker" \
     --region=$REGION
   ```

2. **Enable Identity-Aware Proxy (IAP):**
   - Set up IAP in Google Cloud Console
   - Configure OAuth consent screen
   - Add authorized users

3. **Use Secret Manager for sensitive data:**
   ```bash
   # Store database password in Secret Manager
   echo "your-db-password" | gcloud secrets create db-password --data-file=-
   
   # Update Cloud Run service to use secrets
   gcloud run services update opportunity-plus-ai-test \
     --update-secrets=DATABASE_PASSWORD=db-password:latest \
     --region=$REGION
   ```

## Environment-Specific Configurations

### Test Environment
- Uses `framework_config_test.json`
- Environment variable: `CURRENT_ENV=test`
- Database: `partneropportunity-qa-db`

### Production Environment
- Uses `framework_config_prod.json` (create this file)
- Environment variable: `CURRENT_ENV=prod`
- Database: Production database

## Monitoring and Logging

1. **View logs:**
   ```bash
   gcloud run services logs read opportunity-plus-ai-test \
     --region=$REGION \
     --limit=50
   ```

2. **Monitor metrics:**
   - Go to Cloud Console → Cloud Run → your service
   - Check CPU, Memory, Request count, and Response time

## Troubleshooting

### Common Issues:

1. **Port binding error:**
   - Ensure your app listens on port 8080
   - Check the `CMD` in Dockerfile

2. **Database connection issues:**
   - Verify database URL format
   - Check firewall rules for external databases
   - For Cloud SQL, ensure proper IAM permissions

3. **Memory issues:**
   - Increase memory allocation: `--memory=4Gi`
   - Optimize your application code

4. **Timeout issues:**
   - Increase timeout: `--timeout=900`
   - Optimize slow database queries

### Debug Commands:

```bash
# Check service status
gcloud run services describe opportunity-plus-ai-test --region=$REGION

# View recent logs
gcloud run services logs tail opportunity-plus-ai-test --region=$REGION

# Update service configuration
gcloud run services update opportunity-plus-ai-test \
  --set-env-vars=NEW_VAR=new-value \
  --region=$REGION
```

## Scaling Configuration

```bash
# Auto-scaling configuration
gcloud run services update opportunity-plus-ai-test \
  --min-instances=2 \
  --max-instances=20 \
  --concurrency=100 \
  --cpu=2 \
  --memory=4Gi \
  --region=$REGION
```

## CI/CD Pipeline

For automated deployments, use the provided `cloudbuild.yaml` file with:

1. **Cloud Build triggers**
2. **GitHub integration**
3. **Automated testing**
4. **Multi-environment deployment**

---

## Quick Start Summary

1. Set project ID: `export PROJECT_ID=your-project-id`
2. Deploy: `gcloud run deploy opportunity-plus-ai-test --source . --region=europe-west4 --platform=managed --allow-unauthenticated --port=8080 --memory=2Gi --set-env-vars=CURRENT_ENV=test,GOOGLE_CLOUD_PROJECT=$PROJECT_ID`
3. Test: `curl https://your-service-url/framework/info`

🚀 **Your Opportunity+ AI Agent is now deployed to Google Cloud Run!** 