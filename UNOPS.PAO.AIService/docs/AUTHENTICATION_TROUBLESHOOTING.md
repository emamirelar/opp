# Authentication Troubleshooting Guide

## Google Cloud Service Account Impersonation Issues

### Problem Description

When using the markdown-to-Google Docs conversion feature, you may encounter the following error:

```
Error getting ID token: {'error': {'code': 403, 'message': "Permission 'iam.serviceAccounts.getOpenIdToken' denied on resource (or it may not exist).", 'status': 'PERMISSION_DENIED'}}
```

### Root Cause

This error occurs when the current user's credentials don't have the required IAM permissions to generate OpenID Connect tokens for the configured service account (`pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com`).

### Authentication Flow

The application uses the following authentication flow:
1. Gets default application credentials (user's credentials)
2. Impersonates the service account `pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com`
3. Generates an OpenID Connect token for API authentication
4. Uses this token to authenticate with external APIs (Google Docs conversion, etc.)

### Solution

#### Step 1: Grant Required IAM Permissions

Grant the `roles/iam.serviceAccountOpenIdTokenCreator` role to users who need to use the markdown-to-Google Docs functionality:

```bash
# For a specific user
gcloud iam service-accounts add-iam-policy-binding \
    pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com \
    --member="user:username@unops.org" \
    --role="roles/iam.serviceAccountOpenIdTokenCreator" \
    --project=unops-partneropportunity
```

#### Step 2: Set Up Application Default Credentials

Ensure your application default credentials are properly configured:

```bash
# Login and set up application default credentials
gcloud auth application-default login

# Verify the correct project is selected
gcloud config get-value project
# Should return: unops-partneropportunity
```

#### Step 3: Verify Permissions

Check that the permissions were granted correctly:

```bash
gcloud iam service-accounts get-iam-policy \
    pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com \
    --project=unops-partneropportunity
```

You should see your user listed under both:
- `roles/iam.serviceAccountTokenCreator`
- `roles/iam.serviceAccountOpenIdTokenCreator`

### Alternative Solutions

#### Option 1: Use a Different Service Account

If you don't have permissions to modify IAM policies, you can:
1. Create a new service account that you control
2. Update the configuration in `config/framework/dev.json` or `config/framework/test.json`:

```json
{
  "google_cloud": {
    "oauth": {
      "target_principal": "your-service-account@unops-partneropportunity.iam.gserviceaccount.com"
    }
  }
}
```

#### Option 2: Use Direct Service Account Authentication

Instead of impersonation, you can use direct service account authentication by:
1. Downloading a service account key file
2. Setting the `GOOGLE_APPLICATION_CREDENTIALS` environment variable
3. Modifying the authentication flow to use service account credentials directly

### Configuration Files

The service account configuration is stored in:
- **Development**: `UNOPS.PAO.AIService/config/framework/dev.json`
- **Test**: `UNOPS.PAO.AIService/config/framework/test.json`

Key configuration section:
```json
{
  "google_cloud": {
    "project": "unops-partneropportunity",
    "oauth": {
      "client_id": "1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com",
      "target_principal": "pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com"
    }
  }
}
```

### Common Issues

#### Issue: Wrong Google Account

**Error**: User email doesn't match expected user
**Solution**: 
```bash
# Switch to correct account
gcloud config set account username@unops.org
gcloud auth login
gcloud auth application-default login
```

#### Issue: Wrong Project

**Error**: Resources not found in project
**Solution**:
```bash
# Set correct project
gcloud config set project unops-partneropportunity
```

#### Issue: Cached Credentials

**Error**: Permissions still denied after granting roles
**Solution**:
```bash
# Clear and re-authenticate
gcloud auth revoke
gcloud auth login
gcloud auth application-default login
```

### Testing the Fix

After implementing the solution, test the markdown-to-Google Docs functionality:

1. Start your AI service application
2. Try to convert a markdown document to Google Docs
3. Verify that the authentication succeeds without errors

### Security Notes

- The `roles/iam.serviceAccountOpenIdTokenCreator` role allows generating OpenID Connect tokens for the service account
- This is required for the impersonation flow used by the application
- Only grant this permission to trusted users who need the functionality
- Consider using groups instead of individual user grants for easier management

### Support

For additional support with authentication issues:
- Check the application logs for detailed error messages
- Verify that all required services are enabled in the Google Cloud project
- Contact your Google Cloud administrator if you need higher-level permissions
