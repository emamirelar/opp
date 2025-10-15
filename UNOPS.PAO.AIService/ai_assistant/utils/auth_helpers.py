
from typing import Optional

import requests
from google.auth import default, impersonated_credentials
from google.auth.transport.requests import Request

SIGN_IN_WITH_IDP_API = 'https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp'

def exchange_google_id_token_for_gcip_id_token(google_open_id_connect_token: str) -> str:
  from .config import get_identity_toolkit_api_key, get_tenant_id
  api_key = get_identity_toolkit_api_key()
  if not api_key:
    raise Exception("Identity Toolkit API key is empty or not configured")
  
  tenant_id = get_tenant_id()
  if not tenant_id:
    raise Exception("Tenant ID is empty or not configured")
  
  url = SIGN_IN_WITH_IDP_API + '?key=' + api_key
  print(f"🔐 Fetching IdP token from: {url}")
  data={
    'requestUri': "http://localhost",
    'postBody':'id_token=' + google_open_id_connect_token + '&providerId=google.com',
    'returnSecureToken': True,
    'returnIdpCredential': True,
    'tenantId': tenant_id
  }
  print(f"🔐 Exchanging Google ID token for GCIP ID token: {data}")
  resp = requests.post(url, data)
  
  # Check if request was successful
  if resp.status_code != 200:
    raise Exception(f"Identity Toolkit API request failed with status {resp.status_code}: {resp.text}")
  
  res = resp.json()
  print(f"🔐 Exchanged Google ID token for GCIP ID token: {res}")
  
  # Check if response contains error
  if 'error' in res:
    error_msg = res.get('error', {}).get('message', 'Unknown error')
    raise Exception(f"Identity Toolkit API error: {error_msg}")
  
  # Check if idToken exists and is valid
  id_token = res.get('idToken')
  if not id_token:
    raise Exception("No idToken returned from Identity Toolkit API")
  
  # Validate JWT format (should have 3 parts separated by dots)
  if len(id_token.split('.')) != 3:
    raise Exception(f"Invalid JWT format: Expected 3 parts separated by '.' but got {len(id_token.split('.'))} parts. Token: {id_token[:50]}...")
  
  return id_token

def exchange_google_access_token_for_gcip_id_token(google_access_token: str) -> str:
  from .config import get_identity_toolkit_api_key, get_tenant_id
  api_key = get_identity_toolkit_api_key()
  if not api_key:
    raise Exception("Identity Toolkit API key is empty or not configured")
  
  tenant_id = get_tenant_id()
  if not tenant_id:
    raise Exception("Tenant ID is empty or not configured")
  
  url = SIGN_IN_WITH_IDP_API + '?key=' + api_key
  print(f"🔐 Fetching IdP token from: {url}")
  data={
    'requestUri': "http://localhost",
    'postBody':'access_token=' + google_access_token + '&providerId=google.com',
    'returnSecureToken': True,
    'returnIdpCredential': True,
    'tenantId': tenant_id
  }
  print(f"🔐 Exchanging Google access token for GCIP ID token: {data}")
  resp = requests.post(url, data)
  
  # Check if request was successful
  if resp.status_code != 200:
    raise Exception(f"Identity Toolkit API request failed with status {resp.status_code}: {resp.text}")
  
  res = resp.json()
  print(f"🔐 Exchanged Google access token for GCIP ID token: {res}")
  
  # Check if response contains error
  if 'error' in res:
    error_msg = res.get('error', {}).get('message', 'Unknown error')
    raise Exception(f"Identity Toolkit API error: {error_msg}")
  
  # Check if idToken exists and is valid
  id_token = res.get('idToken')
  if not id_token:
    raise Exception("No idToken returned from Identity Toolkit API")
  
  # Validate JWT format (should have 3 parts separated by dots)
  if len(id_token.split('.')) != 3:
    raise Exception(f"Invalid JWT format: Expected 3 parts separated by '.' but got {len(id_token.split('.'))} parts. Token: {id_token[:50]}...")
  
  return id_token

def get_impersonated_credentials(
  target_scopes: list[str],
  target_principal: Optional[str] = None,
  subject: Optional[str] = None,
  lifetime: int = 3600
) -> impersonated_credentials.Credentials:
    """
    Get impersonated credentials for the target service account.

    Args:
      target_principal: The service account to impersonate.
          It uses the default credentials provided by the enviroment.
          Locally, do `gcloud auth application-default login` to get the default credentials.
          The dfefault credentials must have iam.tokenCreator
      target_scopes: The authorized scopes for the returned credentials.
      subject(Optional): The subject to impersonate if target_principal is setup for domain wide delegation for target scopes.
      lifetime(Optional): The lifetime of the impersonated credentials. Defaults to 3600 (maximum allowed by GCP).
    Returns:
      impersonated_credentials.Credentials: The impersonated credentials.

    """
    default_creds, project = default(scopes=['https://www.googleapis.com/auth/cloud-platform'])
    print(f"Using service account: {default_creds.service_account_email if hasattr(default_creds, 'service_account_email') else 'N/A'}")
    print(f"Project context: {project}")

    # Create impersonated credentials for the target service account
    impersonated_creds = impersonated_credentials.Credentials(
        source_credentials=default_creds,
        target_principal=target_principal,
        target_scopes=target_scopes,
        lifetime=lifetime,
        subject=subject
    )
    print(f"Impersonating service account: {impersonated_creds.service_account_email if hasattr(impersonated_creds, 'service_account_email') else 'N/A'}")
    

    return impersonated_creds

def get_service_account_oidc_token(
  audience: str,
  target_principal: str,
  use_idp: bool = False,
  subject: Optional[str] = None
) -> Optional[str]:
    """
    Gets an OpenID Connect ID token for a target service account using impersonated credentials.
    The target principal is read from the TARGET_PRINCIPAL environment variable.
    The audience is read from the IAP_AUDIENCE environment variable.
    
    Args:
      audience: The audience for the ID token. For IAP this should be the client id whitelisted with the authentication provider.
      target_principal: The service account to impersonate.
      use_idp: Whether to use IDP to exchange the Google ID token for a GCIP ID token.
          Must be true if trying to go through IAP that is configured to use Identity Platform (External Identites).
          Must be false if authenticating to IAP setup with Google Identities
      subject(Optional): The subject to impersonate if target_principal is setup for domain wide delegation for target scopes.
    Returns:
        A signed JWT token as a string, or None if an error occurs.
    """
    try:
        # Create impersonated credentials with necessary scopes
        target_scopes = [
            'openid',
            'https://www.googleapis.com/auth/userinfo.email',
            'https://www.googleapis.com/auth/userinfo.profile'
        ]
        # Get the default application credentials
        print(f"🔐 Getting impersonated credentials for target principal: {target_principal} and audience: {audience} with target scopes: {target_scopes}")
        impersonated_creds = get_impersonated_credentials(
          target_scopes=target_scopes,
          target_principal=target_principal,
          subject=subject
        )

        if use_idp or subject:
            id_token_creds = impersonated_credentials.IDTokenCredentials(
               target_credentials=impersonated_creds,
                target_audience=audience,
                include_email=True
            )
            request = Request()
            id_token_creds.refresh(request)
            id_token = id_token_creds.token
            if(id_token):
                return exchange_google_id_token_for_gcip_id_token(id_token)
            else:
                raise Exception(f"Could not get ID token for target principal: {target_principal} with subject: {subject} and target scopes: {target_scopes}")
        
        # This only issues id token for target_principal. DWD is not supported.
        id_token_creds = impersonated_credentials.IDTokenCredentials(
            target_credentials=impersonated_creds,
            target_audience=audience,
            include_email=True
        )

        # Refresh the credentials to ensure they're valid
        request = Request()
        id_token_creds.refresh(request)
        oidc_token = id_token_creds.token
        # TODO: to support DwD, may be decode the token, set the sub claim to subject and sign it with the impersonated_creds
        print(f"🔐 Fetched oidc token for email: {subject or target_principal} and audience: {audience}")
        return oidc_token
    except Exception as e:
        import traceback
        print(f"Error getting service account token: {e}")
        traceback.print_exc()
        return None

def get_iap_token_with_impersonation(
    audience: str,
    target_principal: str,
    user_email: Optional[str] = None
) -> Optional[str]:
    """
    Generate a proper Google Cloud ID token for IAP with user impersonation.
    
    This creates a native Google Cloud ID token (not Firebase/GCIP) that IAP can validate,
    and modifies the claims to include user impersonation information.
    
    Args:
        audience: The IAP client ID (OAuth 2.0 client ID)
        target_principal: The service account to impersonate
        user_email: The user email to impersonate (optional)
        
    Returns:
        A signed JWT token for IAP authentication, or None if an error occurs.
    """
    try:
        
        # Create impersonated credentials with minimal scopes for ID token generation
        target_scopes = ['https://www.googleapis.com/auth/cloud-platform']
        
        print(f"🔐 [IAP-TOKEN] Getting impersonated credentials for IAP token generation")
        impersonated_creds = get_impersonated_credentials(
            target_scopes=target_scopes,
            target_principal=target_principal,
            subject=None  # No domain-wide delegation for this step
        )
        
        # Generate a standard Google Cloud ID token first
        id_token_creds = impersonated_credentials.IDTokenCredentials(
            target_credentials=impersonated_creds,
            target_audience=audience,
            include_email=True
        )
        
        # Refresh to get the token
        request = Request()
        id_token_creds.refresh(request)
        base_token = id_token_creds.token
        
        if not base_token:
            raise Exception("Failed to generate base ID token")
            
        print(f"🔐 [IAP-TOKEN] Generated base Google Cloud ID token")
        
        # For IAP, we'll use the base token and rely on the x-unops-impersonated-user header
        # for impersonation information. This is more reliable than modifying JWT claims.
        print(f"🔐 [IAP-TOKEN] Using base Google Cloud ID token with impersonation header")
        print(f"🔐 [IAP-TOKEN] Impersonation will be handled via x-unops-impersonated-user header: {user_email}")
        return base_token
            
    except Exception as e:
        import traceback
        print(f"❌ [IAP-TOKEN] Error generating IAP token: {e}")
        traceback.print_exc()
        return None

def build_request_headers(
    tool_context: Optional = None,
    additional_headers: Optional[dict] = None,
    url: Optional[str] = None
) -> dict:
    """
    Build standardized request headers for API calls with authentication and development support.
    
    Args:
        tool_context: Tool context for authentication and state (optional)
        additional_headers: Optional additional headers to include
        url: Optional URL to determine if this is a Google API call
        
    Returns:
        dict: Complete request headers including authentication and development headers
    """
    import os
    import time
    from .config import get_config
    config = get_config()
    
    print("=======================START: BUILD REQUEST HEADERS======================================")
    # Start with default headers
    request_headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    }
    
    # Add any additional headers passed as parameter
    if additional_headers:
        request_headers.update(additional_headers)
    
    # Check local environment
    current_environment = os.getenv('CURRENT_ENVIRONMENT', '').upper()
    is_local = current_environment == 'LOCAL'
    dev_email = config.get('developer', {}).get('email', '')
    
    # Add development IAP headers if in development mode
    if is_local and dev_email:
        current_timestamp = str(int(time.time()))
        iap_headers = {
            'x-goog-authenticated-user-email': f'accounts.google.com:{dev_email}',
            'x-goog-authenticated-user-id': f'accounts.google.com:dev-user-id-{current_timestamp}',
            'x-forwarded-user': dev_email,
            'x-forwarded-email': dev_email,
            'X-Dev-IAP-Simulation': 'true',
            'X-Dev-Auth-Timestamp': current_timestamp
        }
        request_headers.update(iap_headers)
        print(f"✅ [AUTH-HEADERS] Added development IAP headers for email: {dev_email}")
    
    # Add IDP token to request headers if not already present
    if not request_headers.get('Authorization'):
        try:
            from .config import get_oauth_config
            
            # Try to get OAuth config from different sources
            oauth_config = None
            try:
                oauth_config = get_oauth_config()
            except:
                # Fallback to get_config for vector store compatibility
                try:
                    oauth_config = config.get('oauth', {})
                except:
                    pass
            
            if oauth_config:
                target_principal = oauth_config.get('target_principal')
                target_audience = oauth_config.get('client_id')
                
                if target_principal and target_audience:
                    # Get user email from tool_context if available
                    user_email = None
                    if tool_context:
                        if hasattr(tool_context, 'state') and tool_context.state:
                            user_email = tool_context.state.get('user_email')
                        else:
                            print(f"🔍 [AUTH-HEADERS] tool_context.state is None or missing")
                    else:
                        print(f"🔍 [AUTH-HEADERS] tool_context is None")
                    
                    # Always fall back to dev_email in development if user_email is not available
                    if not user_email and is_local and dev_email:
                        user_email = dev_email
                    
                    # Check if this is a Google-related external API call
                    is_google_api = False
                    if url:
                        is_google_api = any(google_path in url for google_path in [
                            '/google-drive/', '/vector-store/', '/convert/url', '/convert/markdown-to-google-doc'
                        ])

                    print(f"🔍 [AUTH-HEADERS] target_audience: {target_audience}")
                    print(f"🔍 [AUTH-HEADERS] target_principal: {target_principal}")
                    print(f"🔍 [AUTH-HEADERS] is_google_api: {is_google_api}")
                    print(f"🔍 [AUTH-HEADERS] user_email for impersonation: {user_email}")
                    
                    # Get the appropriate token based on API type
                    if is_google_api:
                        # For Google APIs, use service account token without impersonation
                        idp_token = get_service_account_oidc_token(
                            target_audience,
                            target_principal,
                            use_idp=False,
                            subject=None
                        )
                        print(f"🔍 [AUTH-HEADERS] Using service account token for Google API")
                    else:
                        # For IAP with Identity Platform (GCIP), we need a GCIP token
                        # The token identifies the service account (for trust check)
                        # Impersonation is handled via the x-unops-impersonated-user header
                        idp_token = get_service_account_oidc_token(
                            target_audience,
                            target_principal,
                            use_idp=True,
                            subject=None  # ← Service account identity, NOT user
                        )
                        print(f"🔍 [AUTH-HEADERS] Using GCIP token for service account with impersonation header")
                    
                    if idp_token:
                        request_headers['Authorization'] = f"Bearer {idp_token}"
                        
                        # Log token details for debugging (only for vector store compatibility)
                        try:
                            import base64
                            import json
                            parts = idp_token.split('.')
                            if len(parts) >= 2:
                                payload_part = parts[1]
                                # Add padding if needed
                                payload_part += '=' * (4 - len(payload_part) % 4)
                                decoded = base64.b64decode(payload_part)
                                token_data = json.loads(decoded)
                                print(f"🔍 [AUTH-HEADERS-TOKEN] Token details:")
                                print(f"   sub: {token_data.get('sub', 'Not Present')}")
                                print(f"   email: {token_data.get('email', 'Not Present')}")
                                print(f"   aud: {token_data.get('aud', 'Not Present')}")
                                print(f"   iss: {token_data.get('iss', 'Not Present')}")
                        except Exception as e:
                            print(f"❌ [AUTH-HEADERS-TOKEN] Could not decode token: {e}")
                    else:
                        print(f"❌ [AUTH-HEADERS] Failed to get IDP token - will proceed without Authorization header")
                    
                    # Add impersonation header for ALL APIs when user email is available
                    if user_email:
                        request_headers['x-unops-impersonated-user'] = user_email
                    else:
                        print(f"⚠️ [AUTH-HEADERS] No user email available for impersonation header")
                        
                else:
                    print(f"⚠️ [AUTH-HEADERS] Missing OAuth config - target_principal: {target_principal}, client_id: {target_audience}")
            else:
                print(f"⚠️ [AUTH-HEADERS] No OAuth config available")
                
        except ImportError as e:
            print(f"⚠️ [AUTH-HEADERS] Config manager or auth helpers not available: {e}")
            # Continue without auth
            pass
        except Exception as e:
            print(f"❌ [AUTH-HEADERS] Error setting up authentication: {e}")
            # Continue without auth
            pass
    
    print(f"🔐 [AUTH-HEADERS] Final request headers prepared:")
    print(f"📋 Total headers: {len(request_headers)}")
    print(f"📋 Header keys: {list(request_headers.keys())}")
    
    # Log final headers safely
    for key, value in request_headers.items():
        if any(sensitive in key.lower() for sensitive in ['authorization', 'token', 'jwt', 'secret', 'password']):
            masked_value = f"{value[:10]}..." if len(value) > 10 else "***"
            print(f"   {key}: {masked_value} (masked)")
        elif 'email' in key.lower():
            print(f"   {key}: {value}")
        elif len(str(value)) > 100:
            print(f"   {key}: {str(value)[:50]}... (truncated)")
        elif key.lower() in ['content-type', 'accept', 'user-agent']:
            print(f"   {key}: {value}")
        else:
            print(f"   {key}: {str(value)[:50]}...")
    
    print("=======================END: BUILD REQUEST HEADERS======================================")
    return request_headers