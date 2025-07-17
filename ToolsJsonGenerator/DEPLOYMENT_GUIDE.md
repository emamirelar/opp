# Deployment Guide

🚀 **Flexible Environment Configuration for Multiple Google Cloud Projects**

This guide shows you how to easily deploy the Tools.json Generator to different Google Cloud projects and environments.

## 🎯 Configuration Methods

### **Method 1: Environment Variables (Quick & Simple)**
Set environment variables before building:

```bash
# Windows
set GOOGLE_CLOUD_PROJECT=my-dev-project
set GOOGLE_CLOUD_LOCATION=us-central1
dotnet build UNOPS.PAO.Presentation

# PowerShell
$env:GOOGLE_CLOUD_PROJECT="my-dev-project"
$env:GOOGLE_CLOUD_LOCATION="us-central1"
dotnet build UNOPS.PAO.Presentation
```

### **Method 2: MSBuild Properties (Explicit)**
Pass configuration directly to the build command:

```bash
dotnet build UNOPS.PAO.Presentation -p:GoogleCloudProject=my-project-id -p:GoogleCloudLocation=us-west1
```

### **Method 3: Environment Files (Organized)**
Use predefined environment configurations:

```bash
# Set up environment
cd ToolsJsonGenerator
set-environment.bat dev

# Then build
dotnet build UNOPS.PAO.Presentation
```

### **Method 4: CI/CD Pipeline Variables**
Configure in your CI/CD system (GitHub Actions, Azure DevOps, etc.)

## 🌍 Environment Management

### **Pre-configured Environments**

We provide template environment files in `ToolsJsonGenerator/environments/`:

```
environments/
├── dev.env        # Development project
├── staging.env    # Staging project  
└── production.env # Production project
```

### **Setting Up Your Environments**

1. **Edit the environment files:**
   ```bash
   # ToolsJsonGenerator/environments/dev.env
   GOOGLE_CLOUD_PROJECT=unops-pao-dev
   GOOGLE_CLOUD_LOCATION=us-central1
   
   # ToolsJsonGenerator/environments/production.env
   GOOGLE_CLOUD_PROJECT=unops-pao-prod
   GOOGLE_CLOUD_LOCATION=europe-west1
   ```

2. **Use the environment:**
   ```bash
   cd ToolsJsonGenerator
   set-environment.bat production
   cd ..
   dotnet build UNOPS.PAO.Presentation
   ```

## 🔧 Different Deployment Scenarios

### **Scenario 1: Local Development**
```bash
# Quick setup for development
set GOOGLE_CLOUD_PROJECT=my-dev-project
gcloud auth application-default login
dotnet build UNOPS.PAO.Presentation
```

### **Scenario 2: Team Development**
```bash
# Use shared development environment
cd ToolsJsonGenerator
set-environment.bat dev
cd ..
dotnet build UNOPS.PAO.Presentation
```

### **Scenario 3: CI/CD Pipeline (GitHub Actions)**
```yaml
# .github/workflows/build.yml
env:
  GOOGLE_CLOUD_PROJECT: ${{ secrets.GOOGLE_CLOUD_PROJECT }}
  GOOGLE_CLOUD_LOCATION: us-central1

steps:
  - name: Build with tools.json generation
    run: dotnet build UNOPS.PAO.Presentation
```

### **Scenario 4: CI/CD Pipeline (Azure DevOps)**
```yaml
# azure-pipelines.yml
variables:
  GoogleCloudProject: $(GOOGLE_CLOUD_PROJECT)
  GoogleCloudLocation: 'us-central1'

steps:
  - task: DotNetCoreCLI@2
    inputs:
      command: 'build'
      projects: 'UNOPS.PAO.Presentation'
      arguments: '-p:GoogleCloudProject=$(GoogleCloudProject) -p:GoogleCloudLocation=$(GoogleCloudLocation)'
```

### **Scenario 5: Docker Build**
```dockerfile
# Dockerfile
ARG GOOGLE_CLOUD_PROJECT
ARG GOOGLE_CLOUD_LOCATION=us-central1

ENV GOOGLE_CLOUD_PROJECT=${GOOGLE_CLOUD_PROJECT}
ENV GOOGLE_CLOUD_LOCATION=${GOOGLE_CLOUD_LOCATION}

RUN dotnet build UNOPS.PAO.Presentation
```

```bash
# Build with Docker
docker build --build-arg GOOGLE_CLOUD_PROJECT=my-project-id .
```

### **Scenario 6: Multiple Projects in Same Build**
```bash
# Build for different projects sequentially
dotnet build UNOPS.PAO.Presentation -p:GoogleCloudProject=project-1 -p:GoogleCloudLocation=us-central1
dotnet build UNOPS.PAO.Presentation -p:GoogleCloudProject=project-2 -p:GoogleCloudLocation=europe-west1
```

## 🏭 Production Best Practices

### **1. Use Service Accounts**
```bash
# Set up service account authentication
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/service-account-key.json"
set GOOGLE_CLOUD_PROJECT=production-project-id
dotnet build UNOPS.PAO.Presentation
```

### **2. Regional Deployment**
Choose the optimal region for your deployment:
```bash
# US deployment
dotnet build -p:GoogleCloudProject=us-project -p:GoogleCloudLocation=us-central1

# Europe deployment  
dotnet build -p:GoogleCloudProject=eu-project -p:GoogleCloudLocation=europe-west1

# Asia deployment
dotnet build -p:GoogleCloudProject=asia-project -p:GoogleCloudLocation=asia-southeast1
```

### **3. Environment Separation**
Keep different environments completely separate:
```bash
# Development
dotnet build -p:GoogleCloudProject=company-pao-dev

# Staging
dotnet build -p:GoogleCloudProject=company-pao-staging  

# Production
dotnet build -p:GoogleCloudProject=company-pao-prod
```

## 🔍 Troubleshooting

### **"Project not found" errors**
```bash
# Verify your project ID
gcloud projects list

# Check current configuration
echo %GOOGLE_CLOUD_PROJECT%
echo %GOOGLE_CLOUD_LOCATION%
```

### **Authentication issues**
```bash
# Re-authenticate
gcloud auth application-default login

# Check authentication status
gcloud auth list
```

### **Build-time configuration verification**
The build process will show you the current configuration:
```
🤖 Step 2: Generating tools.json with Vertex AI...
   🌐 Project: my-project-id
   📍 Location: us-central1
```

## 📋 Configuration Priority

The system uses this priority order for configuration:

1. **MSBuild Properties** (`-p:GoogleCloudProject=...`)
2. **Environment Variables** (`GOOGLE_CLOUD_PROJECT`)  
3. **Default Values** (`us-central1` for location)

## 🚀 Quick Commands Reference

```bash
# Method 1: Environment variables
set GOOGLE_CLOUD_PROJECT=my-project && dotnet build UNOPS.PAO.Presentation

# Method 2: MSBuild properties
dotnet build UNOPS.PAO.Presentation -p:GoogleCloudProject=my-project

# Method 3: Environment files
set-environment.bat production && dotnet build UNOPS.PAO.Presentation

# Method 4: PowerShell
.\set-environment.ps1 staging; dotnet build UNOPS.PAO.Presentation
```

---

✅ **With this flexible system, you can easily deploy to any Google Cloud project without code changes!** 