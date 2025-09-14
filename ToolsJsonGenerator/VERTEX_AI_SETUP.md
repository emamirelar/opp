# Vertex AI Setup Guide

This guide helps you set up Google Cloud Vertex AI for the Tools.json Generator.

## 🔧 Prerequisites

1. **Google Cloud Project** - You need an active Google Cloud project
2. **Billing Account** - Vertex AI requires a billing account attached to your project
3. **gcloud CLI** (optional but recommended)

## 🚀 Quick Setup

### 1. Enable Vertex AI API

In Google Cloud Console:
1. Go to [Vertex AI API page](https://console.cloud.google.com/apis/library/aiplatform.googleapis.com)
2. Select your project
3. Click "Enable"

Or via gcloud CLI:
```bash
gcloud services enable aiplatform.googleapis.com --project=your-project-id
```

### 2. Set Up Authentication

**Option A: Using gcloud CLI (Recommended for development)**
```bash
# Install gcloud CLI if not already installed
# Download from: https://cloud.google.com/sdk/docs/install

# Authenticate
gcloud auth application-default login

# Set your default project
gcloud config set project your-project-id
```

**Option B: Using Environment Variables**
```bash
# Windows
set GOOGLE_CLOUD_PROJECT=your-project-id
set GOOGLE_CLOUD_LOCATION=us-central1

# Or permanently
setx GOOGLE_CLOUD_PROJECT "your-project-id"
setx GOOGLE_CLOUD_LOCATION "us-central1"
```

**Option C: Using Service Account (For production/CI/CD)**
1. Create a service account in Google Cloud Console
2. Grant it "Vertex AI User" role
3. Download the JSON key file
4. Set environment variable:
```bash
set GOOGLE_APPLICATION_CREDENTIALS=path\to\service-account-key.json
```

### 3. Choose Your Region

Common Vertex AI regions:
- `us-central1` (Iowa) - Default, good for most use cases
- `us-east1` (South Carolina) 
- `us-west1` (Oregon)
- `europe-west1` (Belgium)
- `asia-southeast1` (Singapore)

Set your preferred region:
```bash
set GOOGLE_CLOUD_LOCATION=us-central1
```

## 🧪 Test Your Setup

Run this test to verify everything works:

```bash
cd ToolsJsonGenerator
python -c "
import vertexai
from vertexai.generative_models import GenerativeModel
import os

project = os.getenv('GOOGLE_CLOUD_PROJECT', 'your-project-id')
location = os.getenv('GOOGLE_CLOUD_LOCATION', 'us-central1')

print(f'Testing Vertex AI connection...')
print(f'Project: {project}')
print(f'Location: {location}')

try:
    vertexai.init(project=project, location=location)
    model = GenerativeModel('gemini-1.5-pro')
    print('✅ Vertex AI connection successful!')
except Exception as e:
    print(f'❌ Error: {e}')
"
```

## 🔍 Troubleshooting

### "Project not found" error
- Verify your project ID is correct
- Make sure you have access to the project
- Check if billing is enabled

### "Permission denied" error
- Run `gcloud auth application-default login`
- Verify your account has Vertex AI permissions
- Make sure Vertex AI API is enabled

### "Region not supported" error
- Check [available regions](https://cloud.google.com/vertex-ai/docs/general/locations)
- Use a supported region like `us-central1`

### "Quota exceeded" error
- Check your [Vertex AI quotas](https://console.cloud.google.com/iam-admin/quotas)
- Request quota increase if needed

## 💰 Cost Considerations

Vertex AI Gemini pricing (as of 2024):
- **Input tokens**: ~$0.00025 per 1K tokens
- **Output tokens**: ~$0.0005 per 1K tokens

For a typical API with 100 endpoints:
- Estimated cost per generation: $0.01 - $0.05
- Running during every build: Minimal cost impact

## 🔒 Security Best Practices

1. **Use service accounts** for production environments
2. **Limit permissions** to only what's needed (Vertex AI User role)
3. **Rotate credentials** regularly
4. **Don't commit** credentials to version control
5. **Use environment variables** for configuration

## 📚 Additional Resources

- [Vertex AI Documentation](https://cloud.google.com/vertex-ai/docs)
- [Gemini API Reference](https://cloud.google.com/vertex-ai/docs/generative-ai/model-reference/gemini)
- [Authentication Guide](https://cloud.google.com/docs/authentication)
- [Pricing Calculator](https://cloud.google.com/products/calculator)

---

✅ **Once setup is complete, your tools.json generation will work automatically with every build!** 