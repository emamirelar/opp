# Deployment Guide - External Data Integration Service

This guide provides detailed instructions for deploying the External Data Integration Service to Google Cloud Platform.

## 📋 Prerequisites

### Google Cloud Setup
1. **Google Cloud Project** with billing enabled
2. **Required APIs** enabled:
   - Cloud Run API
   - Cloud Build API
   - Cloud Scheduler API
   - Secret Manager API
   - BigQuery API
   - Cloud SQL Admin API (if using Cloud SQL)

3. **IAM Permissions**:
   - Cloud Run Admin
   - Service Account Admin
   - Secret Manager Admin
   - Project IAM Admin

### Local Tools
- [Google Cloud SDK](https://cloud.google.com/sdk) installed and configured
- Docker installed (for local testing)
- `curl` for testing endpoints

## 🚀 Automated Deployment

### Production Deployment

The automated deployment script handles all setup:

```bash
cd deploy
chmod +x deploy.sh
./deploy.sh production your-project-id
```

This script will:
1. ✅ Enable required Google Cloud APIs
2. 👤 Create service accounts with proper permissions
3. 🔐 Create secrets for database and API key
4. 🏗️ Build and deploy the Docker image
5. 🌐 Deploy to Cloud Run
6. 🕐 Optionally set up Cloud Scheduler jobs

### Development Deployment

For development/testing environments:

```bash
chmod +x deploy-dev.sh
./deploy-dev.sh your-dev-project-id
```

Development deployment differences:
- Smaller resource limits (1 CPU, 2GB RAM)
- Authentication disabled
- Test mode enabled
- Auto-table creation enabled

## 🔧 Manual Deployment

### Step 1: Enable APIs

```bash
gcloud services enable \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    cloudscheduler.googleapis.com \
    secretmanager.googleapis.com \
    bigquery.googleapis.com
```

### Step 2: Create Service Accounts

**Cloud Run Service Account:**
```bash
gcloud iam service-accounts create external-data-service \
    --display-name="External Data Integration Service"

# Grant necessary permissions
gcloud projects add-iam-policy-binding PROJECT_ID \
    --member="serviceAccount:external-data-service@PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/cloudsql.client"

gcloud projects add-iam-policy-binding PROJECT_ID \
    --member="serviceAccount:external-data-service@PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/bigquery.jobUser"

gcloud projects add-iam-policy-binding PROJECT_ID \
    --member="serviceAccount:external-data-service@PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/secretmanager.secretAccessor"
```

**Cloud Scheduler Service Account:**
```bash
gcloud iam service-accounts create scheduler \
    --display-name="Cloud Scheduler Service Account"

gcloud projects add-iam-policy-binding PROJECT_ID \
    --member="serviceAccount:scheduler@PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/run.invoker"
```

### Step 3: Create Secrets

**Database Connection String:**
```bash
echo -n "Host=your-db-host;Database=your-db;Username=user;Password=pass;" | \
gcloud secrets create database-connection-string --data-file=-
```

**Admin API Key:**
```bash
openssl rand -hex 32 | \
gcloud secrets create admin-api-key --data-file=-
```

### Step 4: Build and Deploy

**Build the image:**
```bash
gcloud builds submit --tag gcr.io/PROJECT_ID/external-data-service
```

**Deploy to Cloud Run:**
```bash
gcloud run deploy external-data-service \
    --image gcr.io/PROJECT_ID/external-data-service \
    --region us-central1 \
    --platform managed \
    --service-account external-data-service@PROJECT_ID.iam.gserviceaccount.com \
    --cpu-throttling=false \
    --min-instances 0 \
    --max-instances 10 \
    --memory 4Gi \
    --cpu 2 \
    --timeout 3600 \
    --concurrency 10 \
    --set-env-vars ASPNETCORE_ENVIRONMENT=Production \
    --no-allow-unauthenticated
```

### Step 5: Configure Secrets Access

```bash
gcloud secrets add-iam-policy-binding database-connection-string \
    --member="serviceAccount:external-data-service@PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding admin-api-key \
    --member="serviceAccount:external-data-service@PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/secretmanager.secretAccessor"
```

## 🕐 Cloud Scheduler Setup

### Create Sync Jobs

**Daily Partner Sync:**
```bash
gcloud scheduler jobs create http partners-daily-sync \
    --location=us-central1 \
    --schedule="0 6 * * *" \
    --time-zone="UTC" \
    --uri="https://SERVICE_URL/api/sync/execute/partners-prod" \
    --http-method=POST \
    --headers="Content-Type=application/json" \
    --oidc-service-account-email=scheduler@PROJECT_ID.iam.gserviceaccount.com \
    --max-retry-attempts=3
```

**Health Check:**
```bash
gcloud scheduler jobs create http health-check \
    --location=us-central1 \
    --schedule="*/15 * * * *" \
    --time-zone="UTC" \
    --uri="https://SERVICE_URL/health" \
    --http-method=GET \
    --oidc-service-account-email=scheduler@PROJECT_ID.iam.gserviceaccount.com
```

## 🔐 Security Configuration

### Network Security

**Private Cloud Run (recommended for production):**
```bash
gcloud run deploy external-data-service \
    --ingress internal-and-cloud-load-balancing \
    --vpc-connector your-connector
```

**Cloud Armor (for DDoS protection):**
```bash
gcloud compute security-policies create external-data-policy \
    --description="Security policy for external data service"

gcloud compute security-policies rules create 1000 \
    --security-policy external-data-policy \
    --action "allow" \
    --src-ip-ranges "0.0.0.0/0"
```

### SSL/TLS Configuration

Cloud Run automatically provides HTTPS endpoints with managed certificates.

For custom domains:
```bash
gcloud run domain-mappings create \
    --service external-data-service \
    --domain your-custom-domain.com \
    --region us-central1
```

## 📊 Monitoring Setup

### Cloud Logging

Logs are automatically sent to Cloud Logging. Create log-based metrics:

```bash
gcloud logging metrics create sync_errors \
    --description="Count of sync errors" \
    --log-filter='resource.type="cloud_run_revision" AND severity="ERROR"'
```

### Cloud Monitoring

Create alerting policies:

```bash
# Alert on high error rate
gcloud alpha monitoring policies create \
    --policy-from-file=monitoring/error-rate-policy.yaml

# Alert on sync failures
gcloud alpha monitoring policies create \
    --policy-from-file=monitoring/sync-failure-policy.yaml
```

### Custom Dashboards

Import the provided dashboard configuration:
```bash
gcloud monitoring dashboards create \
    --config-from-file=monitoring/service-dashboard.json
```

## 🧪 Testing the Deployment

### Health Check
```bash
SERVICE_URL=$(gcloud run services describe external-data-service \
    --region us-central1 --format 'value(status.url)')

curl -H "Authorization: Bearer $(gcloud auth print-identity-token)" \
    $SERVICE_URL/health
```

### Test Sync Execution
```bash
# Get admin API key
API_KEY=$(gcloud secrets versions access latest --secret admin-api-key)

# Execute a test sync
curl -X POST \
    -H "Authorization: Bearer $(gcloud auth print-identity-token)" \
    -H "X-API-Key: $API_KEY" \
    $SERVICE_URL/api/sync/execute/test-config
```

### Admin Interface Access
```bash
echo "Admin Interface: $SERVICE_URL/admin"
echo "API Key: $(gcloud secrets versions access latest --secret admin-api-key)"
```

## 🔄 CI/CD Integration

### Cloud Build Trigger

Create a trigger for automatic deployments:

```bash
gcloud builds triggers create github \
    --name="external-data-service-deploy" \
    --repo-name="your-repo" \
    --repo-owner="your-org" \
    --branch-pattern="^main$" \
    --build-config="UNOPS.PAO.ExternalDataService/cloudbuild.yaml"
```

### Multi-Environment Setup

**Staging Environment:**
```bash
./deploy.sh staging your-staging-project
```

**Production Environment:**
```bash
./deploy.sh production your-prod-project
```

## 🚨 Troubleshooting

### Common Deployment Issues

**Build Failures:**
```bash
# Check build logs
gcloud builds log BUILD_ID

# Test Docker build locally
docker build -t external-data-service .
```

**Service Account Permissions:**
```bash
# List service account permissions
gcloud projects get-iam-policy PROJECT_ID \
    --filter="bindings.members:external-data-service@PROJECT_ID.iam.gserviceaccount.com"
```

**Secret Access Issues:**
```bash
# Test secret access
gcloud secrets access-versions latest --secret database-connection-string
```

**Cloud Run Deployment Issues:**
```bash
# Check service logs
gcloud logs read \
    --service external-data-service \
    --region us-central1 \
    --limit 100
```

### Performance Tuning

**Memory Issues:**
```bash
# Increase memory allocation
gcloud run services update external-data-service \
    --region us-central1 \
    --memory 8Gi
```

**CPU Issues:**
```bash
# Increase CPU allocation
gcloud run services update external-data-service \
    --region us-central1 \
    --cpu 4
```

**Timeout Issues:**
```bash
# Increase timeout
gcloud run services update external-data-service \
    --region us-central1 \
    --timeout 3600
```

## 🔧 Maintenance

### Update Deployment
```bash
# Redeploy with latest code
gcloud builds submit --tag gcr.io/PROJECT_ID/external-data-service

# Update service
gcloud run deploy external-data-service \
    --image gcr.io/PROJECT_ID/external-data-service:latest \
    --region us-central1
```

### Rotate API Keys
```bash
# Generate new API key
NEW_API_KEY=$(openssl rand -hex 32)

# Update secret
echo -n "$NEW_API_KEY" | gcloud secrets create admin-api-key-new --data-file=-

# Update service to use new secret
# (then update scheduler jobs with new key)
```

### Database Migrations
```bash
# Connect to service for manual migration
gcloud run services proxy external-data-service \
    --region us-central1 \
    --port 8080

# Or use Cloud SQL proxy for direct database access
```

## 📋 Checklist

Before going live:
- [ ] All secrets are created and accessible
- [ ] Service accounts have minimum required permissions
- [ ] SSL certificates are configured
- [ ] Monitoring and alerting are set up
- [ ] Backup and disaster recovery plan is in place
- [ ] Load testing completed
- [ ] Security review completed
- [ ] Documentation is updated
- [ ] Team is trained on operations procedures

## 📞 Support

For deployment issues:
1. Check Cloud Console logs
2. Review service configuration
3. Verify IAM permissions
4. Test endpoints manually
5. Contact the development team

---

**Note**: Replace `PROJECT_ID` with your actual Google Cloud project ID in all commands.
