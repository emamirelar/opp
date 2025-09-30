# IAP Authentication Guide

## Overview

This guide explains the Identity-Aware Proxy (IAP) authentication system used in the UNOPS AI Service, including token generation, service account impersonation, and the differences between various token types.

## Table of Contents

1. [Authentication Flow Overview](#authentication-flow-overview)
2. [Token Types Explained](#token-types-explained)
3. [Service Account Impersonation](#service-account-impersonation)
4. [Code Architecture](#code-architecture)
5. [Development vs Production](#development-vs-production)
6. [Troubleshooting](#troubleshooting)

---

## Authentication Flow Overview

The authentication system follows this high-level flow:

```mermaid
graph TD
    A[Client Request] --> B[build_request_headers()]
    B --> C{Environment Check}
    C -->|LOCAL| D[Add Dev IAP Headers]
    C -->|PRODUCTION| E[Generate IAP Token]
    E --> F[get_iap_token_with_impersonation()]
    F --> G[Service Account Impersonation]
    G --> H[Google Cloud ID Token]
    H --> I[Add Authorization Header]
    I --> J[Add x-unops-impersonated-user Header]
    J --> K[Send Request to API]
```

### Key Components

1. **`build_request_headers()`** - Main orchestrator function
2. **`get_iap_token_with_impersonation()`** - IAP token generation
3. **`get_impersonated_credentials()`** - Service account impersonation
4. **Environment detection** - Local vs Production behavior

---

## Token Types Explained

### 1. Google Cloud ID Token (Used for IAP)

**Purpose**: Authentication with Google Cloud IAP-protected resources

**Characteristics**:
- **Audience**: OAuth 2.0 Client ID (e.g., `1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com`)
- **Issuer**: `https://accounts.google.com`
- **Format**: Standard JWT with Google Cloud signature
- **Validation**: Can be validated by IAP using Google's public keys

**Example Token Claims**:
```json
{
  "iss": "https://accounts.google.com",
  "aud": "1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com",
  "sub": "pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com",
  "email": "pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com",
  "iat": 1759177337,
  "exp": 1759180937
}
```

### 2. Firebase/GCIP ID Token (NOT used for IAP)

**Purpose**: Authentication with Firebase/Google Cloud Identity Platform

**Characteristics**:
- **Audience**: Firebase Project ID (e.g., `unops-partneropportunity`)
- **Issuer**: `https://securetoken.google.com/unops-partneropportunity`
- **Format**: Firebase-specific JWT signature
- **Validation**: Can only be validated by Firebase/GCIP services

**Example Token Claims**:
```json
{
  "iss": "https://securetoken.google.com/unops-partneropportunity",
  "aud": "unops-partneropportunity",
  "sub": "MtPRTpOci6UDduRgbbwhdzrkL6L2",
  "email": "tushard@unops.org",
  "firebase": {
    "identities": {
      "google.com": ["107526752245434288101"],
      "email": ["tushard@unops.org"]
    }
  }
}
```

### Key Differences

| Aspect | Google Cloud ID Token | Firebase/GCIP Token |
|--------|----------------------|---------------------|
| **IAP Compatible** | ✅ Yes | ❌ No |
| **Audience** | OAuth Client ID | Firebase Project ID |
| **Issuer** | accounts.google.com | securetoken.google.com |
| **Signature** | Google Cloud format | Firebase format |
| **Use Case** | IAP authentication | Firebase services |

---

## Service Account Impersonation

### What is Service Account Impersonation?

Service Account Impersonation allows one service account (or user) to act on behalf of another service account without having direct access to the target service account's private key.

### How It Works

1. **Source Credentials**: Your local `gcloud` credentials or application default credentials
2. **Target Principal**: The service account you want to impersonate (`pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com`)
3. **IAM Permission**: Source must have `roles/iam.serviceAccountTokenCreator` on target
4. **Token Generation**: Google's IAM API generates tokens on behalf of the target service account

### Code Implementation

```python
def get_impersonated_credentials(
    target_scopes: list[str],
    target_principal: str,
    subject: Optional[str] = None,
    lifetime: int = 3600
) -> impersonated_credentials.Credentials:
    # Get your default credentials (from gcloud auth application-default login)
    default_creds, project = default(scopes=['https://www.googleapis.com/auth/cloud-platform'])

    # Create impersonated credentials for the target service account
    impersonated_creds = impersonated_credentials.Credentials(
        source_credentials=default_creds,           # Your credentials
        target_principal=target_principal,          # Service account to impersonate
        target_scopes=target_scopes,               # Scopes for the impersonated token
        lifetime=lifetime,                         # Token lifetime (max 3600 seconds)
        subject=subject                            # For domain-wide delegation (optional)
    )
    
    return impersonated_creds
```

### Why Use Service Account Impersonation?

1. **Security**: No need to store service account private keys
2. **Flexibility**: Can switch between different service accounts
3. **Audit Trail**: All impersonation is logged in Google Cloud
4. **Centralized Management**: IAM policies control who can impersonate what

---

## Code Architecture

### Main Functions

#### `build_request_headers(tool_context, additional_headers, url)`

**Purpose**: Main orchestrator that builds complete request headers for API calls

**Flow**:
1. Start with basic headers (`Content-Type`, `Accept`)
2. Add any additional headers passed as parameters
3. Check environment (`LOCAL` vs production)
4. Add development IAP headers if in local mode
5. Generate authentication token if not already present
6. Add impersonation header
7. Return complete headers dictionary

**Key Logic**:
```python
# Environment detection
current_environment = os.getenv('CURRENT_ENVIRONMENT', '').upper()
is_local = current_environment == 'LOCAL'

# Development mode - use simulation headers
if is_local and dev_email:
    iap_headers = {
        'x-goog-authenticated-user-email': f'accounts.google.com:{dev_email}',
        'x-goog-authenticated-user-id': f'accounts.google.com:dev-user-id-{timestamp}',
        'x-forwarded-user': dev_email,
        'x-forwarded-email': dev_email,
        'X-Dev-IAP-Simulation': 'true'
    }

# Production mode - generate real IAP token
else:
    idp_token = get_iap_token_with_impersonation(
        target_audience,
        target_principal, 
        user_email
    )
    request_headers['Authorization'] = f"Bearer {idp_token}"
    request_headers['x-unops-impersonated-user'] = user_email
```

#### `get_iap_token_with_impersonation(audience, target_principal, user_email)`

**Purpose**: Generate a proper Google Cloud ID token for IAP authentication

**Flow**:
1. Create impersonated credentials for the service account
2. Generate a Google Cloud ID token with the correct audience
3. Return the token (impersonation info goes in headers, not token claims)

**Key Logic**:
```python
# Create impersonated credentials
impersonated_creds = get_impersonated_credentials(
    target_scopes=['https://www.googleapis.com/auth/cloud-platform'],
    target_principal=target_principal,
    subject=None  # No domain-wide delegation
)

# Generate Google Cloud ID token (NOT Firebase token)
id_token_creds = impersonated_credentials.IDTokenCredentials(
    target_credentials=impersonated_creds,
    target_audience=audience,  # IAP OAuth Client ID
    include_email=True
)

# Get the token
id_token_creds.refresh(request)
return id_token_creds.token
```

#### `get_service_account_oidc_token(audience, target_principal, use_idp, subject)`

**Purpose**: Legacy function that can generate either Google Cloud or Firebase tokens

**Key Decision Logic**:
```python
if use_idp or subject:
    # Generate Firebase/GCIP token (for Firebase services)
    access_token = impersonated_creds.token
    return exchange_google_access_token_for_gcip_id_token(access_token)
else:
    # Generate Google Cloud ID token (for IAP)
    id_token_creds = impersonated_credentials.IDTokenCredentials(
        target_credentials=impersonated_creds,
        target_audience=audience,
        include_email=True
    )
    return id_token_creds.token
```

### Header Flow

The system adds multiple types of headers:

1. **Basic Headers**:
   ```
   Content-Type: application/json
   Accept: application/json
   ```

2. **Development IAP Simulation Headers** (LOCAL environment only):
   ```
   x-goog-authenticated-user-email: accounts.google.com:tushard@unops.org
   x-goog-authenticated-user-id: accounts.google.com:dev-user-id-1759177337
   x-forwarded-user: tushard@unops.org
   x-forwarded-email: tushard@unops.org
   X-Dev-IAP-Simulation: true
   X-Dev-Auth-Timestamp: 1759177337
   ```

3. **Production Authentication Headers**:
   ```
   Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
   x-unops-impersonated-user: tushard@unops.org
   ```

---

## Development vs Production

### Local Development Mode

**Trigger**: `CURRENT_ENVIRONMENT=LOCAL` environment variable

**Behavior**:
- Uses IAP simulation headers instead of real tokens
- No actual Google Cloud API calls for token generation
- Faster development cycle
- Uses developer email from config

**Headers Added**:
```
x-goog-authenticated-user-email: accounts.google.com:tushard@unops.org
x-goog-authenticated-user-id: accounts.google.com:dev-user-id-{timestamp}
x-forwarded-user: tushard@unops.org
x-forwarded-email: tushard@unops.org
X-Dev-IAP-Simulation: true
X-Dev-Auth-Timestamp: {timestamp}
```

### Production Mode

**Trigger**: `CURRENT_ENVIRONMENT` not set to `LOCAL`

**Behavior**:
- Generates real Google Cloud ID tokens
- Uses service account impersonation
- Makes actual Google Cloud IAM API calls
- Validates tokens with Google's public keys

**Headers Added**:
```
Authorization: Bearer {google-cloud-id-token}
x-unops-impersonated-user: tushard@unops.org
```

### Configuration

The system reads configuration from `local.json`:

```json
{
  "google_cloud": {
    "oauth": {
      "client_id": "1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com",
      "target_principal": "pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com"
    }
  },
  "developer": {
    "email": "tushard@unops.org"
  }
}
```

---

## Troubleshooting

### Common Issues

#### 1. "JWT signature is invalid"

**Cause**: Using Firebase token for IAP authentication

**Solution**: Ensure `get_iap_token_with_impersonation()` is used instead of the Firebase token flow

**Check**: Token audience should be OAuth Client ID, not Firebase Project ID

#### 2. "Credentials object has no attribute 'key_id'"

**Cause**: Trying to manually sign JWT with impersonated credentials

**Solution**: Use Google's `IDTokenCredentials` instead of manual JWT signing

#### 3. "Permission denied" during impersonation

**Cause**: Source credentials don't have `roles/iam.serviceAccountTokenCreator`

**Solution**: 
```bash
gcloud projects add-iam-policy-binding unops-partneropportunity \
  --member="user:your-email@unops.org" \
  --role="roles/iam.serviceAccountTokenCreator"
```

#### 4. "No user email available for impersonation header"

**Cause**: User email not properly extracted from tool context or config

**Solution**: Check that `user_email` is set in the authentication flow

### Debug Logging

The system provides extensive debug logging:

```
🔍 [AUTH-HEADERS] target_audience: 1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com
🔍 [AUTH-HEADERS] target_principal: pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com
🔍 [AUTH-HEADERS] is_google_api: False
🔍 [AUTH-HEADERS] user_email for impersonation: tushard@unops.org
🔐 [IAP-TOKEN] Getting impersonated credentials for IAP token generation
🔐 [IAP-TOKEN] Generated base Google Cloud ID token
🔐 [IAP-TOKEN] Using base Google Cloud ID token with impersonation header
```

### Verification Steps

1. **Check Environment Variable**:
   ```bash
   echo $CURRENT_ENVIRONMENT
   ```

2. **Verify Service Account Permissions**:
   ```bash
   gcloud auth application-default login
   gcloud projects get-iam-policy unops-partneropportunity
   ```

3. **Test Token Generation**:
   ```python
   from ai_assistant.utils.auth_helpers import get_iap_token_with_impersonation
   
   token = get_iap_token_with_impersonation(
       audience="1069310298210-ubl2naqi5bjeqlqrroiqb4qdm482aans.apps.googleusercontent.com",
       target_principal="pno-ai-service@unops-partneropportunity.iam.gserviceaccount.com",
       user_email="tushard@unops.org"
   )
   print(f"Token generated: {token is not None}")
   ```

4. **Decode Token Claims**:
   ```python
   import base64, json
   parts = token.split('.')
   payload = base64.b64decode(parts[1] + '==')
   claims = json.loads(payload)
   print(f"Audience: {claims.get('aud')}")
   print(f"Issuer: {claims.get('iss')}")
   ```

---

## Security Considerations

1. **Token Lifetime**: Tokens expire after 1 hour (3600 seconds)
2. **Scope Limitation**: Use minimal scopes required for the operation
3. **Audit Logging**: All impersonation activities are logged in Google Cloud
4. **Environment Separation**: Different behavior for development vs production
5. **Header Masking**: Sensitive headers are masked in logs

## Best Practices

1. **Use Environment Variables**: Always set `CURRENT_ENVIRONMENT` appropriately
2. **Minimal Permissions**: Grant only necessary IAM roles
3. **Token Caching**: Consider caching tokens to reduce API calls
4. **Error Handling**: Always handle token generation failures gracefully
5. **Logging**: Use the provided debug logging for troubleshooting

---

*This guide covers the authentication system as implemented in `auth_helpers.py`. For questions or issues, refer to the debug logs and troubleshooting section above.*
