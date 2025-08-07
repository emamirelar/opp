
from typing import Optional

import requests
from google.auth import default, impersonated_credentials
from google.auth.transport.requests import Request

SIGN_IN_WITH_IDP_API = 'https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp'

def get_identity_toolkit_api_key() -> str:
    """Get Identity Toolkit API key from configuration"""
    try:
        from .api_config_manager import api_config_manager
        return api_config_manager.get_identity_toolkit_api_key()
    except ImportError:
        print("⚠️ Warning: Could not import api_config_manager, using empty API key")
        return ""

def exchange_google_id_token_for_gcip_id_token(google_open_id_connect_token: str) -> str:
  api_key = get_identity_toolkit_api_key()
  if not api_key:
    raise Exception("Identity Toolkit API key is empty or not configured")
  
  url = SIGN_IN_WITH_IDP_API + '?key=' + api_key
  print(f"🔐 Fetching IdP token from: {url}")
  data={
    'requestUri': "http://localhost",
    'postBody':'id_token=' + google_open_id_connect_token + '&providerId=google.com',
    'returnSecureToken': True,
    'returnIdpCredential': True
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
  api_key = get_identity_toolkit_api_key()
  if not api_key:
    raise Exception("Identity Toolkit API key is empty or not configured")
  
  url = SIGN_IN_WITH_IDP_API + '?key=' + api_key
  print(f"🔐 Fetching IdP token from: {url}")
  data={
    'requestUri': "http://localhost",
    'postBody':'access_token=' + google_access_token + '&providerId=google.com',
    'returnSecureToken': True,
    'returnIdpCredential': True
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

    # Create impersonated credentials for the target service account
    impersonated_creds = impersonated_credentials.Credentials(
        source_credentials=default_creds,
        target_principal=target_principal,
        target_scopes=target_scopes,
        lifetime=lifetime,
        subject=subject
    )

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
            request = Request()
            impersonated_creds.refresh(request)
            access_token = impersonated_creds.token
            if(access_token):
                return exchange_google_access_token_for_gcip_id_token(access_token)
            else:
                raise Exception(f"Could not get access token for for target principal: {target_principal} with subject: {subject} and target scopes: {target_scopes}")
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
