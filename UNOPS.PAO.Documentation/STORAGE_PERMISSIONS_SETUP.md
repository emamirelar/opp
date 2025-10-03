# Google Cloud Storage Permissions Setup

## Problem

Images fail to load with error:
```
AccessDenied: drive-manager@unops-opportunityplus-qa.iam.gserviceaccount.com
does not have storage.objects.get access
```

## Solution

Grant **Storage Object User** role to the service account on the bucket.

## Steps

### 1. Open Google Cloud Console

Navigate to [console.cloud.google.com](https://console.cloud.google.com) and select your project.

### 2. Go to Cloud Storage

**Navigation menu (☰)** → **Cloud Storage** → **Buckets**

### 3. Select Your Bucket

| Environment | Bucket Name |
|------------|-------------|
| Development | `pno-file-storage-dev` |
| Test | `pno-file-storage-test` |
| QA | `pno-file-storage-qa` |
| Production | `pno-file-storage` |

### 4. Grant Access

1. Click **PERMISSIONS** tab
2. Click **GRANT ACCESS** button
3. Fill in:
   - **New principals**: Service account email (see table below)
   - **Role**: `Storage Object User`
4. Click **SAVE**

### Service Accounts by Environment

| Environment | Service Account |
|------------|-----------------|
| Development | `drive-manager@unops-opportunityplus-dev.iam.gserviceaccount.com` |
| Test | `drive-manager@unops-opportunityplus-test.iam.gserviceaccount.com` |
| QA | `drive-manager@unops-opportunityplus-qa.iam.gserviceaccount.com` |
| Production | `drive-manager@unops-opportunityplus-prod.iam.gserviceaccount.com` |

## Verification

1. Navigate to a partner page with an image in the PAO application
2. Image should load without errors
3. Check browser console (F12) - no `AccessDenied` errors

## Alternative: Using gcloud CLI

```bash
gcloud storage buckets add-iam-policy-binding gs://pno-file-storage-qa \
    --member="serviceAccount:drive-manager@unops-opportunityplus-qa.iam.gserviceaccount.com" \
    --role="roles/storage.objectUser"
```
