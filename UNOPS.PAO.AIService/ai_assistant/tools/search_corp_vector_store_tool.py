import os
import json
import logging
import asyncio
import concurrent.futures
from typing import List, Optional, Dict, Any
from google.adk.tools.tool_context import ToolContext

from ..utils.api_config_manager import config_manager
from ..utils.auth_helpers import get_service_account_oidc_token

def search_corp_vector_store(tool_context: ToolContext, query: str, applicationId: Optional[str] = None, entityTypeId: Optional[str] = None, entityId: Optional[str] = None, maxResults: Optional[int] = 10, filename: Optional[str] = None, metadata: Optional[Dict[str, Any]] = None) -> str:
    """
    Search corporate vector store using external API
    The tool provides access to ALL corporate information including information regarding engagements, projects, policies, processes, legal agreements, etc.
    
    Args:
        tool_context: Tool context for authentication and state
        query: Search query string
        applicationId: Optional application ID to filter search
        entityTypeId: Optional entity type ID to filter search
        entityId: Optional entity ID to filter search
        maxResults: Maximum number of results to return (default: 10)
        filename: Optional filename for logging purposes
        metadata: Optional metadata for the search
        
    Returns:
        JSON string with the search results
    """

    print(f"🔍 [VECTOR-SEARCH] Searching corporate vector store with query: {query[:50]}...")
    try:
        import requests
        
        search_endpoint = "https://api.ai.unops.org/v1/tools/vector-store/search"
        
        print(f"🔍 [VECTOR-SEARCH] Searching corporate vector store with query: {query[:50]}...")
        if filename:
            print(f"📄 [VECTOR-SEARCH] Associated filename: {filename}")
        
        # Prepare the request payload
        payload = {
            "query": query,
            "maxResults": maxResults or 10
        }
        
        # Add optional parameters if provided
        if entityTypeId:
            payload["entityTypeId"] = entityTypeId
        if entityId:
            payload["entityId"] = entityId
        if applicationId:
            payload["applicationId"] = applicationId
            
        print(f"📋 [VECTOR-SEARCH] Request payload: {json.dumps(payload, indent=2)}")
        
        # Build headers following the same pattern as other functions
        request_headers = {
            'Content-Type': 'application/json'
        }
        
        # Add development headers if needed
        import os
        is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
        dev_email = os.getenv('DEV_EMAIL', '')

        # is_development = True
        # dev_email = "tushard@unops.org"
        
        if is_development and dev_email:
            print("🧪 [VECTOR-SEARCH] Adding development IAP headers...")
            import time
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
            print(f"✅ [VECTOR-SEARCH] Added development IAP headers for email: {dev_email}")
        
        # Add IDP token if available (same logic as other functions)
        if not request_headers.get('Authorization'):
            print("🔐 [VECTOR-SEARCH] Attempting to get IDP token...")
            
            oauth_config = config_manager.get_oauth_config()
            target_principal = oauth_config.get('target_principal')
            target_audience = oauth_config.get('client_id')
            if target_principal and target_audience:
                # Get user email from tool_context if available
                user_email = None
                print(f"🔍 [VECTOR-SEARCH] Debugging tool_context:")
                print(f"   tool_context exists: {tool_context is not None}")
                if tool_context:
                    print(f"   tool_context type: {type(tool_context)}")
                    print(f"   tool_context has state: {hasattr(tool_context, 'state')}")
                    if hasattr(tool_context, 'state'):
                        print(f"   tool_context.state exists: {tool_context.state is not None}")
                        if tool_context.state:
                            print(f"   tool_context.state type: {type(tool_context.state)}")
                            user_email = tool_context.state.get('user_email')
                            print(f"   Retrieved user_email from tool_context.state: {user_email}")
                        else:
                            print(f"   tool_context.state is None")
                    else:
                        print(f"   tool_context has no state attribute")
                else:
                    print(f"   tool_context is None")
                
                # Always fall back to dev_email in development if user_email is not available
                if not user_email and is_development and dev_email:
                    user_email = dev_email
                    print(f"🧪 [VECTOR-SEARCH] Using dev_email for IDP token: {dev_email}")
                
                print(f"🔍 [VECTOR-SEARCH-AUTH-PARAMS] target_audience: {target_audience}")
                print(f"🔍 [VECTOR-SEARCH-AUTH-PARAMS] target_principal: {target_principal}")
                print(f"🔍 [VECTOR-SEARCH-AUTH-PARAMS] use_idp: False")
                print(f"🔍 [VECTOR-SEARCH-AUTH-PARAMS] subject: None")
                print(f"🔍 [VECTOR-SEARCH-AUTH-PARAMS] user_email for impersonation header: {user_email}")
                idp_token = get_service_account_oidc_token(
                    target_audience,
                    target_principal,
                    use_idp=False,
                    subject=None
                )
                if idp_token:
                    request_headers['Authorization'] = f"Bearer {idp_token}"
                    print(f"🔐 [VECTOR-SEARCH] Added IDP token to Authorization header")
                    
                    # Log token details for debugging
                    try:
                        import base64
                        parts = idp_token.split('.')
                        if len(parts) >= 2:
                            payload_part = parts[1]
                            # Add padding if needed
                            payload_part += '=' * (4 - len(payload_part) % 4)
                            decoded = base64.b64decode(payload_part)
                            token_data = json.loads(decoded)
                            print(f"🔍 [VECTOR-SEARCH-TOKEN] Token details:")
                            print(f"   sub: {token_data.get('sub', 'Not Present')}")
                            print(f"   email: {token_data.get('email', 'Not Present')}")
                            print(f"   aud: {token_data.get('aud', 'Not Present')}")
                            print(f"   iss: {token_data.get('iss', 'Not Present')}")
                    except Exception as e:
                        print(f"❌ [VECTOR-SEARCH-TOKEN] Could not decode token: {e}")
                else:
                    print(f"❌ [VECTOR-SEARCH] Failed to get IDP token - will proceed without Authorization header")
            else:
                print(f"⚠️ [VECTOR-SEARCH] Missing OAuth config - target_principal: {target_principal}, client_id: {target_audience}")
        
        # Add impersonated user header
        if user_email:
            request_headers['x-unops-impersonated-user'] = user_email
            print(f"🔐 [VECTOR-SEARCH] Added impersonated user header: {user_email}")
        else:
            print(f"⚠️ [VECTOR-SEARCH] No user email available for impersonation header")
        
        print("🔐 [VECTOR-SEARCH] Final request headers being sent:")
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
                
        print(f"🔍 [VECTOR-SEARCH] Making request to: {search_endpoint}")
        
        # Make the request
        response = requests.post(
            search_endpoint,
            json=payload,
            headers=request_headers,
            timeout=60,
            verify=False
        )
        
        print(f"🔍 [VECTOR-SEARCH] Response status: {response.status_code}")
        print(f"🔍 [VECTOR-SEARCH] Request headers sent: {list(request_headers.keys())}")
        
        # Enhanced error logging for IAP issues
        if response.status_code == 401:
            print("❌ [VECTOR-SEARCH] 401 Unauthorized - IAP credentials invalid")
            print(f"   Request headers: {list(request_headers.keys())}")
            if 'Authorization' in request_headers:
                print(f"   Authorization header present: {request_headers['Authorization'][:20]}...")
            print(f"   Response text: {response.text[:200]}")
        elif response.status_code == 403:
            print("❌ [VECTOR-SEARCH] 403 Forbidden - IAP access denied")
            print(f"   Response text: {response.text[:200]}")
        
        if response.status_code >= 200 and response.status_code < 300:
            try:
                response_data = response.json()
                print(f"✅ [VECTOR-SEARCH] Successfully searched corporate vector store")
                print(f"📊 [VECTOR-SEARCH] Found {len(response_data.get('results', []))} results")
                
                return json.dumps({
                    "status": "success",
                    "response": response_data,
                    "query": query,
                    "metadata": metadata
                })
            except json.JSONDecodeError:
                return json.dumps({
                    "status": "success",
                    "response": {"text": response.text},
                    "query": query,
                    "metadata": metadata,
                    "note": "Response was not JSON"
                })
        else:
            error_message = f"HTTP {response.status_code}"
            try:
                error_data = response.json()
                if isinstance(error_data, dict):
                    error_message = error_data.get('message', error_data.get('error', error_message))
            except:
                error_message = response.text if response.text else error_message
            
            print(f"❌ [VECTOR-SEARCH] Error {response.status_code}: {error_message}")
        
            return json.dumps({
                "status": "error",
                "error": error_message,
                "query": query,
                "metadata": metadata
            })
        
    except Exception as e:
        logging.error(f"Error searching corporate vector store: {str(e)}", exc_info=True)
        return json.dumps({
            "error": f"Failed to search corporate vector store: {str(e)}",
            "query": query,
            "metadata": metadata,
            "suggestion": "Check if the vector store service is available and the query is valid"
        }) 
