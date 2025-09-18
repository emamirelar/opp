import os
import json
import logging
import asyncio
import concurrent.futures
from typing import List, Optional, Dict, Any
from google.adk.tools.tool_context import ToolContext

def convert_markdown_to_google_doc(tool_context: ToolContext, markdown_content: str, filename: str, metadata: Optional[Dict[str, Any]] = None) -> str:
    """
    Convert markdown content to Google Doc using external API
    
    Args:
        markdown_content: Markdown content as string
        filename: Name of the file
        metadata: Optional metadata for the document
        
    Returns:
        JSON string with the conversion result
    """
    try:
        from ai_assistant.utils.common_callbacks import invoke_api_tool
        import requests
        
        convert_endpoint = "https://api.ai.dev.unops.org/v1/convert/markdown-to-google-doc"
        
        print(f"📄 [MARKDOWN-TO-GDOC] Converting markdown file: {filename}")
        
        # For multipart/form-data, we need to use requests directly since invoke_api_tool expects JSON
        # But we'll still get the headers from invoke_api_tool's pattern
        headers = {}  # Don't set Content-Type for multipart, requests will handle it
        
        # Convert string content to bytes for file upload
        file_data = markdown_content.encode('utf-8')
        
        # Prepare the files and data for multipart upload
        files = {
            'file': (filename, file_data, 'text/markdown')
        }
        
        data = {
            'data': json.dumps(metadata or {})
        }
        
        # Use invoke_api_tool but with a special handling for multipart
        # We'll make the request directly but follow the same auth pattern
        # tool_context is now passed as a parameter
        
        # Build headers following the same pattern as invoke_api_tool
        request_headers = {}
        
        # Add development headers if needed
        import os
        is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
        dev_email = os.getenv('DEV_EMAIL', '')
        
        if is_development and dev_email:
            print("🧪 [MARKDOWN-TO-GDOC] Adding development IAP headers...")
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
            print(f"✅ [MARKDOWN-TO-GDOC] Added development IAP headers for email: {dev_email}")
        
        # Add IDP token if available (same logic as invoke_api_tool)
        if not request_headers.get('Authorization'):
            print("🔐 [MARKDOWN-TO-GDOC] Attempting to get IDP token...")
            from ai_assistant.utils.api_config_manager import config_manager
            from ai_assistant.utils.auth_helpers import get_service_account_oidc_token
            
            oauth_config = config_manager.get_oauth_config()
            target_principal = oauth_config.get('target_principal')
            target_audience = oauth_config.get('client_id')
            if target_principal and target_audience:
                # Get user email from tool_context if available, otherwise use dev_email for development
                user_email = None
                print(f"🔍 [MARKDOWN-TO-GDOC] Debugging tool_context:")
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
                            print(f"   tool_context.state has user_email: {hasattr(tool_context.state, 'user_email')}")
                        else:
                            print(f"   tool_context.state is None")
                    else:
                        print(f"   tool_context has no state attribute")
                else:
                    print(f"   tool_context is None")
                
                # Always fall back to dev_email in development if user_email is not available
                if not user_email and is_development and dev_email:
                    user_email = dev_email
                    print(f"🧪 [MARKDOWN-TO-GDOC] Using dev_email for IDP token: {dev_email}")
                
                # For Google Doc creation, use service account token without impersonation
                print(f"🔍 [MARKDOWN-TO-GDOC] Using service account as target_principal: {target_principal}")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] target_audience: {target_audience}")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] target_principal: {target_principal}")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] use_idp: False")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] subject: None")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] user_email for impersonation header: {user_email}")
                idp_token = get_service_account_oidc_token(
                    target_audience,
                    target_principal,  # Use original service account email from config
                    use_idp=False,
                    subject=None
                )
                if idp_token:
                    request_headers['Authorization'] = f"Bearer {idp_token}"
                    print(f"🔐 [MARKDOWN-TO-GDOC] Added IDP token to Authorization header")
                    
                    # Log token details for debugging
                    try:
                        import base64
                        parts = idp_token.split('.')
                        if len(parts) >= 2:
                            payload = parts[1]
                            # Add padding if needed
                            payload += '=' * (4 - len(payload) % 4)
                            decoded = base64.b64decode(payload)
                            token_data = json.loads(decoded)
                            print(f"🔍 [MARKDOWN-TO-GDOC-TOKEN] Token details:")
                            print(f"   sub: {token_data.get('sub', 'Not Present')}")
                            print(f"   email: {token_data.get('email', 'Not Present')}")
                            print(f"   aud: {token_data.get('aud', 'Not Present')}")
                            print(f"   iss: {token_data.get('iss', 'Not Present')}")
                    except Exception as e:
                        print(f"❌ [MARKDOWN-TO-GDOC-TOKEN] Could not decode token: {e}")
                else:
                    print(f"❌ [MARKDOWN-TO-GDOC] Failed to get IDP token - will proceed without Authorization header")
            else:
                print(f"⚠️ [MARKDOWN-TO-GDOC] Missing OAuth config - target_principal: {target_principal}, client_id: {target_audience}")
        
        # Add impersonated user header for Google Doc creation
        # Reuse the user_email that was already successfully retrieved above
        print(f"🔍 [MARKDOWN-TO-GDOC] Using user_email for impersonation header: {user_email}")
        
        if user_email:
            request_headers['x-unops-impersonated-user'] = user_email
            print(f"🔐 [MARKDOWN-TO-GDOC] Added impersonated user header: {user_email}")
        else:
            print(f"⚠️ [MARKDOWN-TO-GDOC] No user email available for impersonation header")
        
        print("🔐 [MARKDOWN-TO-GDOC] Final request headers being sent:")
        print(f"📋 Total headers: {len(request_headers)}")
        print(f"📋 Header keys: {list(request_headers.keys())}")
        
        # Log final headers safely (same as invoke_api_tool)
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
                
        print(f"📄 [MARKDOWN-TO-GDOC] Making multipart request to: {convert_endpoint}")
        
        # Make the multipart request
        response = requests.post(
            convert_endpoint,
            files=files,
            data=data,
            headers=request_headers,
            timeout=60,
            verify=False
        )
        
        print(f"📄 [MARKDOWN-TO-GDOC] Response status: {response.status_code}")
        print(f"📄 [MARKDOWN-TO-GDOC] Request headers sent: {list(request_headers.keys())}")
        
        # Enhanced error logging for IAP issues
        if response.status_code == 401:
            print("❌ [MARKDOWN-TO-GDOC] 401 Unauthorized - IAP credentials invalid")
            print(f"   Request headers: {list(request_headers.keys())}")
            if 'Authorization' in request_headers:
                print(f"   Authorization header present: {request_headers['Authorization'][:20]}...")
            print(f"   Response text: {response.text[:200]}")
        elif response.status_code == 403:
            print("❌ [MARKDOWN-TO-GDOC] 403 Forbidden - IAP access denied")
            print(f"   Response text: {response.text[:200]}")
        
        if response.status_code >= 200 and response.status_code < 300:
            try:
                response_data = response.json()
                print(f"✅ [MARKDOWN-TO-GDOC] Successfully converted markdown to Google Doc")
                
                return json.dumps({
                    "status": "success",
                    "response": response_data,
                    "filename": filename
                })
            except json.JSONDecodeError:
                return json.dumps({
                    "status": "success",
                    "response": {"text": response.text},
                    "filename": filename,
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
            
            print(f"❌ [MARKDOWN-TO-GDOC] Error {response.status_code}: {error_message}")
        
        return json.dumps({
                "status": "error",
                "error": error_message,
                "filename": filename
            })
        
    except Exception as e:
        logging.error(f"Error converting markdown to Google Doc: {str(e)}", exc_info=True)
        return json.dumps({
            "error": f"Failed to convert markdown to Google Doc: {str(e)}",
            "filename": filename,
            "suggestion": "Check if the convert service is available and the file is valid markdown"
        }) 