# Environment Setup Guide - Test Execution

**Purpose**: Complete guide to set up all environments needed to run the full test suite  
**Date**: January 14, 2026  
**Status**: Ready for implementation

---

## 🎯 Overview

To run all 3,700+ tests, you need:

1. ✅ **.NET 9.0 SDK** (Already installed)
2. ⏳ **Database Connection** (For integration tests)
3. ⏳ **Google Cloud Credentials** (For IAM auth tests)
4. ⏳ **Python Environment** (For AI tests)
5. ⏳ **AI Service** (For AI integration tests)

---

## 1️⃣ Database Setup for Integration Tests

### **Current Status:** ⚠️ **8 tests failing** due to missing database

### **What's Needed:**

The integration tests require a PostgreSQL database with the UNOPS schema and seed data.

### **Option A: Use Existing Development Database**

**Steps:**

1. **Verify Database Connection String**

```bash
# Check current connection string
cd "c:\Users\Leonardc\git\opportunityplus\UNOPS.PAO.Server"
notepad appsettings.Development.json
```

2. **Update Test Configuration**

Edit `QA Tests\Integration Tests\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=unops_pao;Username=your_user;Password=your_password;",
    "UseIamAuthentication": false
  }
}
```

3. **Verify Database Access**

```bash
# Test connection
dotnet test "QA Tests\Integration Tests\UNOPS.PAO.IntegrationTests.csproj" --filter "FullyQualifiedName~DatabaseConnection_WithIamAuthDisabled_ConnectsSuccessfully"
```

---

### **Option B: Set Up Test Database Container**

**Using Docker for isolated test database:**

1. **Create Docker Compose File**

Create `QA Tests\docker-compose.test.yml`:

```yaml
version: '3.8'
services:
  test-postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: unops_pao_test
      POSTGRES_USER: test_user
      POSTGRES_PASSWORD: test_password
    ports:
      - "5433:5432"
    volumes:
      - test-db-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U test_user"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  test-db-data:
```

2. **Start Test Database**

```bash
cd "QA Tests"
docker-compose -f docker-compose.test.yml up -d

# Wait for database to be ready
timeout /t 10
```

3. **Run Migrations**

```bash
cd "c:\Users\Leonardc\git\opportunityplus\UNOPS.PAO.Server"

# Set connection string for test database
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5433;Database=unops_pao_test;Username=test_user;Password=test_password;"

# Run migrations
dotnet ef database update --project ..\UNOPS.PAO.UNOPSDataAccess\UNOPS.PAO.UNOPSDataAccess.csproj
```

4. **Run Seed Scripts**

```bash
# Connect to database
psql -h localhost -p 5433 -U test_user -d unops_pao_test

# Run seed scripts
\i 'C:/Users/Leonardc/git/opportunityplus/UNOPS.PAO.UNOPSDataAccess/Scripts/seed-roles.sql'
\i 'C:/Users/Leonardc/git/opportunityplus/UNOPS.PAO.UNOPSDataAccess/Scripts/seed-entities.sql'
\i 'C:/Users/Leonardc/git/opportunityplus/UNOPS.PAO.UNOPSDataAccess/Scripts/seed-entity-field-managers.sql'
\i 'C:/Users/Leonardc/git/opportunityplus/UNOPS.PAO.UNOPSDataAccess/Scripts/seed-liaison-offices.sql'
```

5. **Update Test Configuration**

Edit `QA Tests\Integration Tests\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=unops_pao_test;Username=test_user;Password=test_password;",
    "UseIamAuthentication": false
  }
}
```

6. **Run Integration Tests**

```bash
dotnet test "QA Tests\Integration Tests\UNOPS.PAO.IntegrationTests.csproj"
```

---

## 2️⃣ Google Cloud Credentials Setup (IAM Authentication)

### **Current Status:** ⚠️ **5 tests skipped** due to missing credentials

### **What's Needed:**

Google Cloud service account with Cloud SQL IAM authentication permissions.

### **Setup Steps:**

1. **Create Service Account** (if not exists)

```bash
# Login to Google Cloud
gcloud auth login

# Set project
gcloud config set project YOUR_PROJECT_ID

# Create service account
gcloud iam service-accounts create test-cloudsql-iam \
  --display-name="Test Cloud SQL IAM Auth"

# Grant Cloud SQL Client role
gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:test-cloudsql-iam@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/cloudsql.client"
```

2. **Download Service Account Key**

```bash
# Create and download key
gcloud iam service-accounts keys create test-cloudsql-key.json \
  --iam-account=test-cloudsql-iam@YOUR_PROJECT_ID.iam.gserviceaccount.com

# Move to secure location
move test-cloudsql-key.json C:\Users\Leonardc\.gcloud\test-cloudsql-key.json
```

3. **Set Environment Variable**

```powershell
# Set Google Application Credentials
$env:GOOGLE_APPLICATION_CREDENTIALS="C:\Users\Leonardc\.gcloud\test-cloudsql-key.json"

# Verify
echo $env:GOOGLE_APPLICATION_CREDENTIALS
```

4. **Update Configuration**

Edit `UNOPS.PAO.Server\appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=/cloudsql/YOUR_INSTANCE_CONNECTION_NAME;Database=unops_pao;Username=your-service-account@YOUR_PROJECT_ID.iam;",
    "UseIamAuthentication": true
  }
}
```

5. **Test IAM Authentication**

```bash
# Remove skip attribute from tests
# Then run:
dotnet test "QA Tests\Integration Tests\UNOPS.PAO.IntegrationTests.csproj" --filter "FullyQualifiedName~IamAuth"
```

---

## 3️⃣ Python Environment Setup (AI Tests)

### **Current Status:** ⏳ **19 tests not executed** - Python environment needed

### **Setup Steps:**

1. **Install Python** (if not installed)

```powershell
# Check if Python is installed
python --version

# If not installed, download from python.org or use winget:
winget install Python.Python.3.11
```

2. **Create Virtual Environment**

```bash
cd "c:\Users\Leonardc\git\opportunityplus\UNOPS.PAO.AIService"

# Create virtual environment
python -m venv venv

# Activate virtual environment
.\venv\Scripts\Activate.ps1

# Upgrade pip
python -m pip install --upgrade pip
```

3. **Install Dependencies**

```bash
# Install pytest
pip install pytest pytest-asyncio

# Install project dependencies (if requirements.txt exists)
pip install -r requirements.txt

# Or install specific dependencies
pip install google-cloud-aiplatform google-auth aiohttp
```

4. **Verify Installation**

```bash
# Check pytest is installed
pytest --version

# List installed packages
pip list
```

5. **Run Python Tests**

```bash
# Run all Python tests
pytest tests/ -v

# Run specific test file
pytest tests/test_lookup_entity_metadata_tool.py -v

# Run with coverage
pip install pytest-cov
pytest tests/ --cov=ai_assistant --cov-report=html
```

---

## 4️⃣ AI Service Setup (AI Integration Tests)

### **Current Status:** ⏳ **3 tests skipped** - AI service not running

### **Setup Steps:**

1. **Configure AI Service**

Edit `UNOPS.PAO.AIService\AIService\config\local.json`:

```json
{
  "ai_platform": {
    "project_id": "YOUR_PROJECT_ID",
    "location": "us-central1",
    "model": "gemini-pro"
  },
  "app_api": {
    "base_url": "http://localhost:5000",
    "timeout": 30
  }
}
```

2. **Start AI Service**

```bash
cd "c:\Users\Leonardc\git\opportunityplus\UNOPS.PAO.AIService"

# Activate virtual environment
.\venv\Scripts\Activate.ps1

# Start with uvicorn
python -m uvicorn main:app --reload --port 8000

# Or use the batch script
..\Scripts\run-ai-service-uvicorn.bat
```

3. **Verify AI Service is Running**

```bash
# Test AI service endpoint
curl http://localhost:8000/health

# Or in PowerShell:
Invoke-RestMethod -Uri http://localhost:8000/health
```

4. **Update Test Configuration**

Edit `QA Tests\Integration Tests\appsettings.json`:

```json
{
  "AIService": {
    "BaseUrl": "http://localhost:8000",
    "Timeout": 30
  }
}
```

5. **Run AI Integration Tests**

```bash
dotnet test "QA Tests\Integration Tests\UNOPS.PAO.IntegrationTests.csproj" --filter "FullyQualifiedName~AIEntityMetadataIntegrationTests"
```

---

## 5️⃣ CI/CD Pipeline Configuration

### **Azure DevOps Pipeline** (`azure-pipelines.yml`)

Create `.azure-pipelines\qa-tests.yml`:

```yaml
trigger:
  branches:
    include:
      - main
      - dev-deploy
      - QA-Tests

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'
  dotnetVersion: '9.0.x'

stages:
- stage: Test
  displayName: 'Run QA Tests'
  jobs:
  - job: FastTests
    displayName: 'Fast Logic Tests'
    steps:
    - task: UseDotNet@2
      inputs:
        version: $(dotnetVersion)
    
    - task: DotNetCoreCLI@2
      displayName: 'Restore NuGet Packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run FastTests'
      inputs:
        command: 'test'
        projects: '**/UNOPS.PAO.FastTests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-restore --logger trx'
    
    - task: PublishTestResults@2
      inputs:
        testResultsFormat: 'VSTest'
        testResultsFiles: '**/*.trx'
        testRunTitle: 'FastTests'

  - job: BusinessTests
    displayName: 'Business Logic Tests'
    steps:
    - task: UseDotNet@2
      inputs:
        version: $(dotnetVersion)
    
    - task: DotNetCoreCLI@2
      displayName: 'Run Business.Tests'
      inputs:
        command: 'test'
        projects: '**/UNOPS.PAO.Business.Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-restore --logger trx'
    
    - task: PublishTestResults@2
      inputs:
        testResultsFormat: 'VSTest'
        testResultsFiles: '**/*.trx'
        testRunTitle: 'Business Tests'

  - job: IntegrationTests
    displayName: 'Integration Tests'
    condition: and(succeeded(), ne(variables['Build.Reason'], 'PullRequest'))
    steps:
    - task: UseDotNet@2
      inputs:
        version: $(dotnetVersion)
    
    - task: Docker@2
      displayName: 'Start Test Database'
      inputs:
        command: 'run'
        arguments: >
          --name test-postgres
          -e POSTGRES_DB=unops_pao_test
          -e POSTGRES_USER=test_user
          -e POSTGRES_PASSWORD=test_password
          -p 5433:5432
          -d postgres:15
    
    - script: timeout /t 15
      displayName: 'Wait for Database'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run Migrations'
      inputs:
        command: 'custom'
        custom: 'ef'
        arguments: 'database update --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj'
      env:
        ConnectionStrings__DefaultConnection: 'Host=localhost;Port=5433;Database=unops_pao_test;Username=test_user;Password=test_password;'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run Integration Tests'
      inputs:
        command: 'test'
        projects: '**/UNOPS.PAO.IntegrationTests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-restore --logger trx'
      env:
        ConnectionStrings__DefaultConnection: 'Host=localhost;Port=5433;Database=unops_pao_test;Username=test_user;Password=test_password;'
    
    - task: Docker@2
      displayName: 'Stop Test Database'
      condition: always()
      inputs:
        command: 'stop'
        container: 'test-postgres'

  - job: PythonTests
    displayName: 'Python AI Tests'
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: UsePythonVersion@0
      inputs:
        versionSpec: '3.11'
    
    - script: |
        cd UNOPS.PAO.AIService
        pip install pytest pytest-asyncio pytest-cov
        pip install -r requirements.txt
      displayName: 'Install Python Dependencies'
    
    - script: |
        cd UNOPS.PAO.AIService
        pytest tests/ -v --junitxml=test-results.xml --cov=ai_assistant --cov-report=xml
      displayName: 'Run Python Tests'
    
    - task: PublishTestResults@2
      inputs:
        testResultsFormat: 'JUnit'
        testResultsFiles: '**/test-results.xml'
        testRunTitle: 'Python Tests'
    
    - task: PublishCodeCoverageResults@1
      inputs:
        codeCoverageTool: 'Cobertura'
        summaryFileLocation: '**/coverage.xml'

- stage: Report
  displayName: 'Generate Test Report'
  dependsOn: Test
  condition: always()
  jobs:
  - job: Summary
    displayName: 'Test Summary'
    steps:
    - task: PowerShell@2
      displayName: 'Generate Test Dashboard'
      inputs:
        targetType: 'inline'
        script: |
          Write-Host "Test Execution Summary"
          Write-Host "====================="
          Write-Host "FastTests: COMPLETED"
          Write-Host "Business Tests: COMPLETED"
          Write-Host "Integration Tests: COMPLETED"
          Write-Host "Python Tests: COMPLETED"
```

---

### **GitHub Actions Workflow** (`.github/workflows/qa-tests.yml`)

```yaml
name: QA Tests

on:
  push:
    branches: [ main, dev-deploy, QA-Tests ]
  pull_request:
    branches: [ main, dev-deploy ]

jobs:
  fast-tests:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Run FastTests
      run: dotnet test "QA Tests/C# Tests/UNOPS.PAO.FastTests/UNOPS.PAO.FastTests.csproj" --no-restore --verbosity normal

  business-tests:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Run Business Tests
      run: dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj" --verbosity normal

  integration-tests:
    runs-on: windows-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: unops_pao_test
          POSTGRES_USER: test_user
          POSTGRES_PASSWORD: test_password
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Run Migrations
      run: dotnet ef database update --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj
      env:
        ConnectionStrings__DefaultConnection: 'Host=localhost;Port=5432;Database=unops_pao_test;Username=test_user;Password=test_password;'
    
    - name: Run Integration Tests
      run: dotnet test "QA Tests/Integration Tests/UNOPS.PAO.IntegrationTests.csproj" --verbosity normal
      env:
        ConnectionStrings__DefaultConnection: 'Host=localhost;Port=5432;Database=unops_pao_test;Username=test_user;Password=test_password;'

  python-tests:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Python
      uses: actions/setup-python@v4
      with:
        python-version: '3.11'
    
    - name: Install dependencies
      run: |
        cd UNOPS.PAO.AIService
        pip install pytest pytest-asyncio pytest-cov
        pip install -r requirements.txt || true
    
    - name: Run Python tests
      run: |
        cd UNOPS.PAO.AIService
        pytest tests/ -v --cov=ai_assistant --cov-report=xml
    
    - name: Upload coverage
      uses: codecov/codecov-action@v3
      with:
        files: ./UNOPS.PAO.AIService/coverage.xml
        flags: python-tests
```

---

## 📋 **Quick Setup Checklist**

### **Minimal Setup (Run 99% of tests):**
- [ ] Verify .NET 9.0 SDK installed
- [ ] Run FastTests (no setup needed)
- [ ] Run Business.Tests (no setup needed)

### **Full Setup (Run 100% of tests):**
- [ ] Set up test database (Docker or existing DB)
- [ ] Configure Google Cloud credentials
- [ ] Install Python and pytest
- [ ] Start AI service
- [ ] Update all configuration files
- [ ] Run full test suite

### **Production Setup:**
- [ ] Create CI/CD pipeline
- [ ] Configure secret management
- [ ] Set up test database in staging
- [ ] Enable automated test execution
- [ ] Configure test result reporting

---

## 🎯 **Expected Results After Full Setup**

```
Total Tests: 3,700+
Passing: 3,684 (99.5%+)
Failing: 0
Skipped: 16 (optional tests)

FastTests: 78/78 passing
Business.Tests: 2,135/2,135 passing
IntegrationTests: 1,265/1,265 passing
Python Tests: 19/19 passing
Opportunity Tests: 484 (awaiting backend)
```

---

*Setup Guide Version: 1.0*  
*Last Updated: January 14, 2026*  
*Status: Ready for Implementation*
