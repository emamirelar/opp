# Google Cloud Authentication Setup Guide for Tests
**Purpose**: Configure Google Cloud authentication for integration testing  
**Audience**: Developers, QA Engineers, DevOps  
**Estimated Time**: 20-30 minutes

---

## 🎯 **OVERVIEW**

This guide provides multiple options for handling Google Cloud authentication in tests. Choose the approach that best fits your environment:

1. **Mock Services** (Recommended for local development)
2. **Service Account** (Recommended for CI/CD)
3. **Test Categorization** (Quick fix to skip these tests)

**Current Issue**: 17 tests fail with:
```
Grpc.Core.RpcException: Status(StatusCode="PermissionDenied", 
Detail="Request had insufficient authentication scopes.")
```

---

## 🚀 **OPTION 1: MOCK GOOGLE CLOUD SERVICES (RECOMMENDED FOR LOCAL DEV)**

### Advantages
✅ No Google Cloud credentials needed  
✅ Tests run offline  
✅ Faster execution  
✅ No API quota consumption  
✅ Consistent test behavior

### Implementation

#### Step 1: Update Test Configuration

Add to `appsettings.Testing.json`:

```json
{
  "GoogleCloud": {
    "UseMockServices": true,
    "ProjectId": "test-project-local",
    "PubSubTopic": "test-topic",
    "BucketName": "test-bucket"
  },
  "AISettings": {
    "DisableExternalCalls": true,
    "ModelName": "gemini-pro",
    "ProjectId": "test-project-local",
    "Location": "us-central1"
  }
}
```

#### Step 2: Create Mock Service Implementations

Create file: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Mocks/MockGoogleCloudServices.cs`

```csharp
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Cloud.PubSub.V1;
using Google.Cloud.Storage.V1;

namespace UNOPS.PAO.Business.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of Google Cloud Pub/Sub for testing
    /// </summary>
    public class MockPubSubService
    {
        public Task<string> PublishMessageAsync(string topic, string message)
        {
            // Simulate successful publish
            return Task.FromResult($"mock-message-id-{Guid.NewGuid()}");
        }

        public Task<IEnumerable<PubsubMessage>> PullMessagesAsync(string subscription, int maxMessages)
        {
            // Return empty list for testing
            return Task.FromResult<IEnumerable<PubsubMessage>>(new List<PubsubMessage>());
        }
    }

    /// <summary>
    /// Mock implementation of Google Cloud Storage for testing
    /// </summary>
    public class MockStorageService
    {
        private Dictionary<string, byte[]> _mockStorage = new();

        public Task<string> UploadFileAsync(string bucketName, string objectName, byte[] content)
        {
            _mockStorage[objectName] = content;
            return Task.FromResult($"https://storage.googleapis.com/{bucketName}/{objectName}");
        }

        public Task<byte[]> DownloadFileAsync(string bucketName, string objectName)
        {
            if (_mockStorage.TryGetValue(objectName, out var content))
            {
                return Task.FromResult(content);
            }
            throw new KeyNotFoundException($"Object {objectName} not found in mock storage");
        }

        public Task DeleteFileAsync(string bucketName, string objectName)
        {
            _mockStorage.Remove(objectName);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mock implementation of Google Cloud AI services for testing
    /// </summary>
    public class MockAIService
    {
        public Task<string> GenerateContentAsync(string prompt, string model = "gemini-pro")
        {
            // Return mock AI response
            return Task.FromResult($"[Mock AI Response to: {prompt.Substring(0, Math.Min(50, prompt.Length))}...]");
        }

        public Task<Dictionary<string, float>> AnalyzeSentimentAsync(string text)
        {
            return Task.FromResult(new Dictionary<string, float>
            {
                { "score", 0.5f },
                { "magnitude", 0.8f }
            });
        }
    }
}
```

#### Step 3: Register Mock Services in Tests

Update test base class or individual test constructors:

```csharp
public class IntegrationTestBase : IDisposable
{
    protected Mock<IPubSubService> MockPubSubService;
    protected Mock<IStorageService> MockStorageService;
    protected Mock<IAIService> MockAIService;

    public IntegrationTestBase()
    {
        // Read configuration
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json")
            .Build();

        var useMockServices = config.GetValue<bool>("GoogleCloud:UseMockServices");

        if (useMockServices)
        {
            // Use mock implementations
            MockPubSubService = new Mock<IPubSubService>();
            MockPubSubService.Setup(s => s.PublishMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("mock-message-id");

            MockStorageService = new Mock<IStorageService>();
            MockStorageService.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync("https://mock-storage-url.com/file");

            MockAIService = new Mock<IAIService>();
            MockAIService.Setup(s => s.GenerateContentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("Mock AI generated content");
        }
        else
        {
            // Use real Google Cloud services (requires authentication)
            // Service initialization code here
        }
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

#### Step 4: Verify Mock Implementation

Run tests to verify mocks work:

```powershell
cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
dotnet test --filter "Category=GoogleCloud" --logger "console;verbosity=detailed"

# Expected: All 17 Google Cloud tests should PASS with mocked services
```

---

## 🔐 **OPTION 2: SERVICE ACCOUNT AUTHENTICATION (RECOMMENDED FOR CI/CD)**

### Advantages
✅ Tests use real Google Cloud services  
✅ Validates actual API behavior  
✅ Better for staging/production-like testing  
✅ Required for end-to-end integration tests

### Prerequisites
- Google Cloud Project access
- Permission to create service accounts
- Billing enabled on Google Cloud project

### Step 1: Create Service Account

#### Using Google Cloud Console (Web UI)

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your project (or create a new test project)
3. Navigate to **IAM & Admin** > **Service Accounts**
4. Click **"+ CREATE SERVICE ACCOUNT"**
5. Enter details:
   - **Name**: `pao-test-service-account`
   - **Description**: `Service account for PAO automated tests`
   - Click **"CREATE AND CONTINUE"**
6. Grant roles:
   - **Pub/Sub Publisher**
   - **Pub/Sub Subscriber**
   - **Storage Object Admin**
   - **Vertex AI User** (for AI services)
   - Click **"CONTINUE"** then **"DONE"**
7. Click on the created service account
8. Go to **"KEYS"** tab
9. Click **"ADD KEY"** > **"Create new key"**
10. Select **JSON** format
11. Click **"CREATE"** - key file downloads automatically
12. **IMPORTANT**: Store this file securely - it contains credentials

#### Using gcloud CLI

```bash
# Set project
gcloud config set project YOUR_PROJECT_ID

# Create service account
gcloud iam service-accounts create pao-test-service-account \
    --display-name="PAO Test Service Account" \
    --description="Service account for automated tests"

# Grant necessary roles
gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
    --member="serviceAccount:pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/pubsub.publisher"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
    --member="serviceAccount:pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/pubsub.subscriber"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
    --member="serviceAccount:pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/storage.objectAdmin"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
    --member="serviceAccount:pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/aiplatform.user"

# Create and download key
gcloud iam service-accounts keys create pao-test-key.json \
    --iam-account=pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com

echo "✅ Service account key created: pao-test-key.json"
```

### Step 2: Configure Local Environment

#### Windows (PowerShell)

```powershell
# Set environment variable (current session)
$env:GOOGLE_APPLICATION_CREDENTIALS="C:\path\to\pao-test-key.json"

# Set permanently for user
[System.Environment]::SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    "C:\path\to\pao-test-key.json",
    [System.EnvironmentVariableTarget]::User
)

# Verify
Write-Host "GOOGLE_APPLICATION_CREDENTIALS: $env:GOOGLE_APPLICATION_CREDENTIALS"
```

#### Linux/Mac (Bash)

```bash
# Set environment variable (current session)
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/pao-test-key.json"

# Set permanently (add to ~/.bashrc or ~/.zshrc)
echo 'export GOOGLE_APPLICATION_CREDENTIALS="/path/to/pao-test-key.json"' >> ~/.bashrc
source ~/.bashrc

# Verify
echo $GOOGLE_APPLICATION_CREDENTIALS
```

### Step 3: Update Test Configuration

Update `appsettings.Testing.json`:

```json
{
  "GoogleCloud": {
    "UseMockServices": false,
    "ProjectId": "your-actual-project-id",
    "PubSubTopic": "pao-test-topic",
    "BucketName": "pao-test-bucket",
    "Location": "us-central1",
    "ServiceAccountKeyPath": "" // Leave empty to use GOOGLE_APPLICATION_CREDENTIALS env var
  },
  "AISettings": {
    "DisableExternalCalls": false,
    "ModelName": "gemini-pro",
    "ProjectId": "your-actual-project-id",
    "Location": "us-central1"
  }
}
```

### Step 4: Create Required Google Cloud Resources

```bash
# Create Pub/Sub topic
gcloud pubsub topics create pao-test-topic --project=YOUR_PROJECT_ID

# Create Pub/Sub subscription
gcloud pubsub subscriptions create pao-test-subscription \
    --topic=pao-test-topic \
    --project=YOUR_PROJECT_ID

# Create Cloud Storage bucket
gsutil mb -p YOUR_PROJECT_ID -l us-central1 gs://pao-test-bucket/

echo "✅ Google Cloud resources created"
```

### Step 5: Verify Authentication

```powershell
# Run Google Cloud tests
cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
dotnet test --filter "Category=GoogleCloud" --logger "console;verbosity=detailed"

# Expected: All 17 tests should PASS using real Google Cloud services
```

---

## 🏷️ **OPTION 3: TEST CATEGORIZATION (QUICK FIX)**

### Use Case
- Don't have Google Cloud access yet
- Want to run other tests without setup
- Will configure Google Cloud later

### Implementation

#### Step 1: Add Test Traits

Update test methods that use Google Cloud services:

```csharp
[Fact]
[Trait("Category", "RequiresGoogleCloud")]
[Trait("Environment", "GoogleCloudServices")]
public async Task TestUsingPubSub()
{
    // Test implementation
}

[Fact]
[Trait("Category", "RequiresGoogleCloud")]
[Trait("Environment", "GoogleCloudServices")]
public async Task TestUsingCloudStorage()
{
    // Test implementation
}
```

#### Step 2: Run Tests Excluding Google Cloud

```powershell
# Run all tests EXCEPT Google Cloud tests
dotnet test --filter "Category!=RequiresGoogleCloud"

# Run only Google Cloud tests (when ready)
dotnet test --filter "Category=RequiresGoogleCloud"
```

#### Step 3: Update Test Scripts

Create script: `run-tests-without-google-cloud.ps1`

```powershell
#!/usr/bin/env pwsh
Write-Host "Running tests WITHOUT Google Cloud dependencies..." -ForegroundColor Cyan

# Fast Tests (no Google Cloud dependencies)
Write-Host "`n=== Running Fast Tests ===" -ForegroundColor Yellow
cd "QA Tests/C# Tests/UNOPS.PAO.FastTests"
dotnet test --logger "console;verbosity=normal"

# Business Tests (excluding Google Cloud)
Write-Host "`n=== Running Business Tests (excluding Google Cloud) ===" -ForegroundColor Yellow
cd "../UNOPS.PAO.Business.Tests"
dotnet test --filter "Category!=RequiresGoogleCloud" --logger "console;verbosity=normal"

# Integration Tests (excluding Google Cloud and environment-specific)
Write-Host "`n=== Running Integration Tests (excluding external dependencies) ===" -ForegroundColor Yellow
cd "../../Integration Tests"
dotnet test --filter "Category!=RequiresGoogleCloud&Category!=RequiresEnvironment" --logger "console;verbosity=normal"

Write-Host "`n✅ Tests completed (Google Cloud tests skipped)" -ForegroundColor Green
```

---

## 🔧 **CI/CD INTEGRATION**

### GitHub Actions with Service Account

Create `.github/workflows/tests-with-google-cloud.yml`:

```yaml
name: Tests with Google Cloud

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Authenticate to Google Cloud
        uses: google-github-actions/auth@v1
        with:
          credentials_json: '${{ secrets.GCP_SA_KEY }}'
      
      - name: Set up Cloud SDK
        uses: google-github-actions/setup-gcloud@v1
      
      - name: Run tests with Google Cloud
        env:
          ASPNETCORE_ENVIRONMENT: Testing
        run: |
          cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
          dotnet test --logger "trx;LogFileName=test-results.trx"
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: "**/test-results.trx"
```

**Setup Required**:
1. Go to GitHub repository → Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Name: `GCP_SA_KEY`
4. Value: Paste contents of service account JSON key file
5. Click "Add secret"

### Azure DevOps Pipeline

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  - group: google-cloud-credentials # Variable group with GCP_SA_KEY

steps:
  - task: UseDotNet@2
    inputs:
      packageType: 'sdk'
      version: '9.0.x'

  - script: |
      echo "$(GCP_SA_KEY)" > $(Agent.TempDirectory)/gcp-key.json
      export GOOGLE_APPLICATION_CREDENTIALS=$(Agent.TempDirectory)/gcp-key.json
      cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
      dotnet test --logger trx
    displayName: 'Run tests with Google Cloud'
    env:
      ASPNETCORE_ENVIRONMENT: Testing

  - task: PublishTestResults@2
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/*.trx'
    condition: always()
```

---

## 🔐 **SECURITY BEST PRACTICES**

### ⚠️ **NEVER commit service account keys to version control**

#### Add to .gitignore

```gitignore
# Google Cloud credentials
*-key.json
*-credentials.json
service-account*.json
gcp-*.json
```

#### Use Secret Management

**For Local Development:**
- Store keys outside the repository
- Use environment variables
- Consider using Google Cloud Secret Manager

**For CI/CD:**
- Use GitHub Secrets / Azure Key Vault
- Rotate keys regularly (every 90 days)
- Use least-privilege service accounts

#### Rotate Keys Regularly

```bash
# Delete old key
gcloud iam service-accounts keys delete KEY_ID \
    --iam-account=pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com

# Create new key
gcloud iam service-accounts keys create pao-test-key-new.json \
    --iam-account=pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com

# Update environment variable to point to new key
```

---

## 🧪 **VERIFICATION**

### Test Google Cloud Authentication

Create verification script: `verify-google-cloud-auth.ps1`

```powershell
#!/usr/bin/env pwsh
Write-Host "Verifying Google Cloud authentication..." -ForegroundColor Cyan

# Check environment variable
if ($env:GOOGLE_APPLICATION_CREDENTIALS) {
    Write-Host "✅ GOOGLE_APPLICATION_CREDENTIALS is set" -ForegroundColor Green
    Write-Host "   Path: $env:GOOGLE_APPLICATION_CREDENTIALS" -ForegroundColor Gray
    
    # Check if file exists
    if (Test-Path $env:GOOGLE_APPLICATION_CREDENTIALS) {
        Write-Host "✅ Service account key file exists" -ForegroundColor Green
    } else {
        Write-Host "❌ Service account key file NOT FOUND" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "⚠️  GOOGLE_APPLICATION_CREDENTIALS not set" -ForegroundColor Yellow
    Write-Host "   Will attempt to use default credentials or mocked services" -ForegroundColor Gray
}

# Run a simple Google Cloud test
Write-Host "`nRunning Google Cloud connectivity test..." -ForegroundColor Cyan
cd "QA Tests/C# Tests/UNOPS.PAO.Business.Tests"
dotnet test --filter "FullyQualifiedName~GoogleCloudConnectivityTest" --logger "console;verbosity=detailed"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Google Cloud authentication verified!" -ForegroundColor Green
} else {
    Write-Host "`n❌ Google Cloud authentication failed" -ForegroundColor Red
    Write-Host "   Check configuration and credentials" -ForegroundColor Yellow
    exit 1
}
```

Run verification:

```powershell
pwsh verify-google-cloud-auth.ps1
```

---

## 📊 **EXPECTED RESULTS**

After completing setup (Option 1 or 2):

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Google Cloud auth tests | 17 failing | **0 failing** | 100% fixed |
| Integration test pass rate | 91.9% | **95%+** | +3.1% |
| Tests executable locally | Requires credentials | **Works offline (mocks)** | ✅ |

---

## 🔄 **SWITCHING BETWEEN MODES**

You can easily switch between mocked and real services:

```json
// appsettings.Testing.json

// For local development (mocked)
{
  "GoogleCloud": {
    "UseMockServices": true
  }
}

// For CI/CD or staging (real services)
{
  "GoogleCloud": {
    "UseMockServices": false,
    "ProjectId": "your-project-id"
  }
}
```

Or use environment variable:

```powershell
# Use mocks
$env:USE_MOCK_GOOGLE_SERVICES = "true"

# Use real services
$env:USE_MOCK_GOOGLE_SERVICES = "false"
```

---

## 🆘 **TROUBLESHOOTING**

### Issue: "Default credentials could not be determined"

**Solution**: Set `GOOGLE_APPLICATION_CREDENTIALS` environment variable:
```powershell
$env:GOOGLE_APPLICATION_CREDENTIALS="C:\path\to\key.json"
```

### Issue: "Permission denied" errors

**Solution**: Ensure service account has required IAM roles:
```bash
# Check current roles
gcloud projects get-iam-policy YOUR_PROJECT_ID \
    --flatten="bindings[].members" \
    --filter="bindings.members:serviceAccount:pao-test-service-account@YOUR_PROJECT_ID.iam.gserviceaccount.com"

# Add missing roles if needed (see Step 1 of Option 2)
```

### Issue: "Project not found"

**Solution**: Verify project ID in configuration matches Google Cloud project:
```bash
gcloud config get-value project
```

### Issue: Tests still failing with mock services

**Solution**: Verify `UseMockServices` is set to `true` in `appsettings.Testing.json` and rebuild:
```powershell
dotnet clean
dotnet build
dotnet test
```

---

## ✅ **VERIFICATION CHECKLIST**

**For Option 1 (Mock Services):**
- [ ] `appsettings.Testing.json` has `UseMockServices: true`
- [ ] Mock service implementations created
- [ ] Tests run offline without Google Cloud credentials
- [ ] All 17 Google Cloud tests pass with mocks

**For Option 2 (Service Account):**
- [ ] Service account created in Google Cloud
- [ ] Service account has required IAM roles
- [ ] JSON key file downloaded and secured
- [ ] `GOOGLE_APPLICATION_CREDENTIALS` environment variable set
- [ ] Required Google Cloud resources created (topics, buckets)
- [ ] Tests pass using real Google Cloud services

**For Option 3 (Test Categorization):**
- [ ] Test traits added to Google Cloud tests
- [ ] Filter works to exclude/include Google Cloud tests
- [ ] Other tests run successfully without Google Cloud setup

---

## 📚 **ADDITIONAL RESOURCES**

- [Google Cloud Authentication Documentation](https://cloud.google.com/docs/authentication)
- [Service Account Best Practices](https://cloud.google.com/iam/docs/best-practices-service-accounts)
- [Google Cloud .NET SDK](https://cloud.google.com/dotnet/docs)
- [Pub/Sub Quickstart](https://cloud.google.com/pubsub/docs/quickstart-dotnet)

---

**Last Updated**: January 23, 2026  
**Maintainer**: UNOPS Opportunity+ Development Team  
**Recommended Approach**: Option 1 (Mock Services) for local dev, Option 2 (Service Account) for CI/CD
