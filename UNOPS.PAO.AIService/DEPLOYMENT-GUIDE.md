# 🚀 Deployment Guide

## Quick Cloud Run Deployment

### 1. Configure Your Team Settings
Edit these files with your team's Google Cloud project details:

**`team-setup-gcloud.bat`:**
```batch
set PROJECT_ID=your-google-cloud-project  
set REGION=your-preferred-region
```

**`team-deploy-simple.bat`:**
```batch
set SERVICE_NAME=your-team-ai-service
set PROJECT_ID=your-google-cloud-project
set REGION=your-preferred-region
```

### 2. Deploy Options

**Option A: Simple Deployment**
```bash
# Setup and deploy
./team-setup-gcloud.bat
./team-deploy-simple.bat
```

**Option B: Cloud Build (CI/CD)**
```bash
# Edit cloudbuild.yaml with your service name
# Then trigger build
gcloud builds submit --config cloudbuild.yaml
```

### 3. Local Docker Testing
```bash
# Build and test locally
docker build -t your-team-ai .
docker run -p 8080:8080 your-team-ai
```

## Files Included
- `team-setup-gcloud.bat` - Google Cloud setup
- `team-deploy-simple.bat` - Simple deployment script  
- `cloudbuild.yaml` - Cloud Build configuration
- `Dockerfile` - Container configuration

## Support
Check the main framework documentation for detailed deployment instructions.
