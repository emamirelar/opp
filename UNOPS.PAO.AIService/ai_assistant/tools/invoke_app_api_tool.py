import os
import json
import logging
import asyncio
import base64
import concurrent.futures
import socket
import traceback
import time
import requests
from typing import List, Optional, Dict, Any
from urllib.parse import urlparse
from google.adk.tools.tool_context import ToolContext

from ..utils.api_config_manager import config_manager
from ..utils.auth_helpers import get_service_account_oidc_token

def construct_api_url(base_url: str, endpoint_path: str, path_params: Optional[Dict[str, Any]] = None) -> str:
    """
    Construct a complete API URL with path parameter substitution
    
    Args:
        base_url: Base URL (e.g., "https://localhost:44426")
        endpoint_path: Endpoint path (e.g., "/api/partner/{id}")
        path_params: Dictionary of path parameters to substitute
    
    Returns:
        str: Complete URL with path parameters substituted
    """
    print(f"🔧 [URL] Constructing URL from base: '{base_url}' + path: '{endpoint_path}'")
    
    # Remove trailing slash from base_url if present
    base_url = base_url.rstrip('/')
    print(f"🔧 [URL] Base URL after rstrip: '{base_url}'")
    
    # Ensure endpoint_path starts with /
    if not endpoint_path.startswith('/'):
        endpoint_path = '/' + endpoint_path
    print(f"🔧 [URL] Endpoint path after ensuring slash: '{endpoint_path}'")
    
    # Construct base URL
    full_url = base_url + endpoint_path
    print(f"🔧 [URL] Combined URL: '{full_url}'")
    
    # Fix any double slashes (except after protocol)
    # Keep protocol slashes (https://) but fix any other double slashes
    if '://' in full_url:
        protocol_part, rest_part = full_url.split('://', 1)
        rest_part = rest_part.replace('//', '/')
        full_url = protocol_part + '://' + rest_part
        print(f"🔧 [URL] Fixed double slashes: '{full_url}'")
    
    # Substitute path parameters if provided
    if path_params:
        print(f"🔧 [URL] Substituting path parameters: {path_params}")
        for param_name, param_value in path_params.items():
            # Replace both {param} and [param] patterns
            old_url = full_url
            full_url = full_url.replace(f'{{{param_name}}}', str(param_value))
            full_url = full_url.replace(f'[{param_name}]', str(param_value))
            if old_url != full_url:
                print(f"🔧 [URL] Replaced {param_name}: '{old_url}' → '{full_url}'")
    
    print(f"🔧 [URL] Final constructed URL: '{full_url}'")
    return full_url


def find_entity_endpoint(entity_name: str, intent: str, extracted_params: str = "{}") -> str:
    """
    Intelligent endpoint finder that works with task planner JSON plans
    
    This function is designed to process plans from the task_planner_agent and find the optimal
    API endpoint for executing the planned step. It includes enhanced scoring, retry logic,
    and smart parameter mapping for better task execution.
    
    Args:
        entity_name: The entity name from task planner (e.g., "Partner", "Contact", "Document")
        intent: The intent from task planner (e.g., "search", "create", "update", "delete")
        extracted_params: JSON string of parameters from task planner step (e.g., '{"query": "ACME", "step": 1}')
        
    Returns:
        JSON string containing endpoint details, retry information, and execution guidance
    """
    try:
        # Parse extracted parameters
        try:
            params_dict = json.loads(extracted_params) if extracted_params and extracted_params != "{}" else None
        except (json.JSONDecodeError, TypeError):
            params_dict = None
        
        print(f"🎯 [ENDPOINT-FINDER] Finding endpoint for entity='{entity_name}', intent='{intent}'")
        if params_dict:
            print(f"   📊 Extracted params: {params_dict}")
        
        # Get all endpoints for the entity to enable retry logic
        all_endpoints = config_manager.get_entity_api_endpoints(entity_name)
        if not all_endpoints:
            return json.dumps({
                "entity": entity_name,
                "intent": intent,
                "endpoint_found": False,
                "error": f"No endpoints available for entity '{entity_name}'"
            })
        
        # Use enhanced scoring to get ranked list of endpoints
        def score_all_endpoints():
            """Get all endpoints scored and ranked"""
            candidates = []
            for endpoint in all_endpoints:
                # Use the enhanced scoring logic from api_config_manager
                score = score_endpoint_for_intent_standalone(endpoint, intent, entity_name, params_dict)
                if score > 0:
                    candidates.append({
                        'endpoint': endpoint,
                        'score': score
                    })
            
            # Sort by score (highest first)
            candidates.sort(key=lambda x: x['score'], reverse=True)
            return candidates
        
        def score_endpoint_for_intent_standalone(endpoint: dict, intent: str, entity_name: str, extracted_params: Optional[dict] = None) -> int:
            """Enhanced scoring function optimized for task planner integration"""
            score = 0
            endpoint_name = endpoint.get('name', '').lower()
            description = endpoint.get('description', '').lower()
            url = endpoint.get('url', '').lower()
            method = endpoint.get('method', 'GET').upper()
            when_to_use = endpoint.get('when_to_use', '').lower()
            example_uses = endpoint.get('example_uses', [])
            parameters = endpoint.get('parameters', {})
            
            # Enhanced intent to HTTP method mapping for task planner
            intent_method_mapping = {
                'search': 'GET', 'list': 'GET', 'get': 'GET', 'find': 'GET', 'retrieve': 'GET',
                'create': 'POST', 'add': 'POST', 'new': 'POST', 'insert': 'POST',
                'update': 'PUT', 'modify': 'PUT', 'edit': 'PUT', 'change': 'PUT', 'patch': 'PATCH',
                'delete': 'DELETE', 'remove': 'DELETE', 'destroy': 'DELETE'
            }
            
            target_method = intent_method_mapping.get(intent.lower(), 'GET')
            
            # CRITICAL: Must match HTTP method
            if method != target_method:
                return 0  # Wrong method = zero score

            # ENHANCED: Base scoring for intent matching
            intent_lower = intent.lower()
            if intent_lower == endpoint_name:
                score += 60  # Perfect match - increased for task planner accuracy
            elif intent_lower in endpoint_name:
                score += 40  # Intent is part of name - increased
            elif any(synonym in endpoint_name for synonym in intent_method_mapping.keys() if intent_method_mapping[synonym] == target_method):
                score += 25  # Related intent word in name - increased
            
            # ENHANCED: Intent matching in description and when_to_use
            intent_keywords = [intent_lower] + [k for k, v in intent_method_mapping.items() if v == target_method]
            for keyword in intent_keywords:
                if keyword in description:
                    score += 20  # Increased weight
                if keyword in when_to_use:
                    score += 20  # Increased weight
            
            # NEW: Smart parameter matching for task planner
            if extracted_params:
                param_bonus = 0
                
                # Bonus for endpoints that expect the parameters we have
                if 'id' in extracted_params and 'id' in url:
                    param_bonus += 15  # ID-based endpoint bonus
                if 'query' in extracted_params and ('search' in endpoint_name or 'find' in endpoint_name):
                    param_bonus += 15  # Search endpoint bonus
                if 'comprehensive_search' in extracted_params and extracted_params.get('comprehensive_search'):
                    if 'search' in endpoint_name or 'find' in endpoint_name:
                        param_bonus += 10  # Comprehensive search bonus
                
                # NEW: Task planner step information bonus
                if 'step' in extracted_params:
                    param_bonus += 5  # Multi-step workflow bonus
                if 'previous_step_result' in extracted_params:
                    param_bonus += 5  # Sequential dependency bonus
                
                score += param_bonus
            
            # NEW: Entity-specific endpoint bonus
            entity_lower = entity_name.lower()
            if entity_lower in endpoint_name or entity_lower in url:
                score += 10  # Entity name match bonus
            
            return max(0, score)
        
        # Get ranked candidates
        ranked_candidates = score_all_endpoints()
        
        if not ranked_candidates:
            return json.dumps({
                "entity": entity_name,
                "intent": intent,
                "endpoint_found": False,
                "error": f"No suitable endpoints found for entity '{entity_name}' with intent '{intent}'"
            })
        
        # Return the best endpoint with retry information
        best_candidate = ranked_candidates[0]
        best_endpoint = best_candidate['endpoint']
        
        base_url = config_manager.get_api_base_url()
        endpoint_url = best_endpoint.get('url', '')
        
        # Use construct_api_url for proper path parameter substitution
        
        if endpoint_url.startswith('http'):
            full_url = endpoint_url
        else:
            # Extract path parameters from params_dict for URL substitution
            path_params = None
            print(f"🔧 [PATH-PARAMS] params_dict: {params_dict}")
            print(f"🔧 [PATH-PARAMS] endpoint_url: {endpoint_url}")
            
            if params_dict:
                # Create path_params dict from params that might be path parameters
                path_params = {}
                endpoint_params = best_endpoint.get('parameters', {})
                print(f"🔧 [PATH-PARAMS] endpoint_params: {endpoint_params}")
                
                for param_name, param_info in endpoint_params.items():
                    if param_name in params_dict:
                        path_params[param_name] = params_dict[param_name]
                        print(f"🔧 [PATH-PARAMS] Added from endpoint_params: {param_name} = {params_dict[param_name]}")
                
                # Also check for common path parameter names and their variations
                common_param_mappings = {
                    'id': 'id',
                    'partnerId': 'partnerId', 
                    'partner_id': 'partnerId',
                    'contactId': 'contactId',
                    'contact_id': 'contactId', 
                    'partnerName': 'partnerName',
                    'partner_name': 'partnerName',  # Map snake_case to camelCase
                    'entityId': 'entityId',
                    'entity_id': 'entityId'
                }
                
                for param_key, url_param_name in common_param_mappings.items():
                    if param_key in params_dict:
                        path_params[url_param_name] = params_dict[param_key]
                        print(f"🔧 [PATH-PARAMS] Added from common_params: {param_key} -> {url_param_name} = {params_dict[param_key]}")
            
            print(f"🔧 [PATH-PARAMS] Final path_params: {path_params}")
            full_url = construct_api_url(base_url, endpoint_url, path_params)
            print(f"🔧 [PATH-PARAMS] Constructed URL: {full_url}")
        
        # Prepare retry information for the agent
        fallback_endpoints = []
        if len(ranked_candidates) > 1:
            for i, candidate in enumerate(ranked_candidates[1:3], 1):  # Next 2 best alternatives
                ep = candidate['endpoint']
                ep_url = ep.get('url', '')
                
                # Use construct_api_url for fallback endpoints too
                if ep_url.startswith('http'):
                    fallback_url = ep_url
                else:
                    # Use the same path_params for fallback URLs
                    fallback_url = construct_api_url(base_url, ep_url, path_params)
                
                fallback_endpoints.append({
                    "rank": i + 1,
                    "name": ep.get('name'),
                    "url": ep.get('url'),
                    "full_url": fallback_url,
                    "method": ep.get('method', 'GET'),
                    "score": candidate['score'],
                    "description": ep.get('description', '')
                })
        
        result = {
            "entity": entity_name,
            "intent": intent,
            "endpoint_found": True,
            "endpoint": best_endpoint,
            "full_url": full_url,
            "method": best_endpoint.get('method', 'GET'),
            "parameters": best_endpoint.get('parameters', {}),
            "when_to_use": best_endpoint.get('when_to_use', ''),
            "example_uses": best_endpoint.get('example_uses', []),
            "score": best_candidate['score'],
            "retry_info": {
                "total_candidates": len(ranked_candidates),
                "fallback_endpoints": fallback_endpoints,
                "has_fallbacks": len(fallback_endpoints) > 0,
                "recommendation": f"If this endpoint fails, try the fallback endpoints in order. Always try at least 2 endpoints before giving up."
            },
            # NEW: Task completion guidance for loop management
            "execution_guidance": {
                "next_action": "call_invoke_api_tool",
                "requires_followup": intent.lower() in ['create', 'update'],
                "exit_after_success": True,
                "exit_after_retries_exhausted": True,
                "task_planner_step_complete": True if params_dict and 'step' in params_dict else False
            }
        }
        
        print(f"🎯 [ENDPOINT-FINDER] Selected: {best_endpoint.get('name')} (score: {best_candidate['score']})")
        if fallback_endpoints:
            print(f"   🔄 Fallbacks available: {len(fallback_endpoints)} alternatives")
            for fb in fallback_endpoints[:2]:  # Show first 2 fallbacks
                print(f"      {fb['rank']}. {fb['name']} (score: {fb['score']})")
        
        return json.dumps(result, indent=2)
        
    except Exception as e:
        print(f"❌ [ENDPOINT-FINDER] Error finding endpoint for {entity_name}/{intent}: {e}")
        traceback.print_exc()
        return json.dumps({
            "error": f"Failed to find endpoint for {entity_name}/{intent}: {str(e)}",
            "entity": entity_name,
            "intent": intent,
            "endpoint_found": False
        })


def base_invoke_api_tool(url: str, method: str, body: dict, headers: Optional[dict] = None, tool_context: Optional[ToolContext] = None) -> dict:
    """
    Invoke an API endpoint with real HTTP requests using configuration from tools.json
    
    Args:
        url: Full URL to call (e.g., https://localhost:44426/api/partners)
        method: HTTP method (GET, POST, PUT, DELETE)
        body: Request body/parameters
        headers: Optional additional headers (e.g., IAP headers for authentication)
        tool_context: Optional tool context containing session state
    
    Returns:
        dict: API response or error information
    """
    try:
        # If tool_context is not provided, try to extract it from the current execution context
        if tool_context is None:
            try:
                import inspect
                # Get the current frame and look for tool_context in the calling frames
                frame = inspect.currentframe()
                while frame:
                    # Look for tool_context in the local variables
                    if 'tool_context' in frame.f_locals:
                        tool_context = frame.f_locals['tool_context']
                        print(f"📧 Auto-extracted tool_context from execution context {tool_context}")
                        break
                    frame = frame.f_back
            except Exception as e:
                print(f"⚠️ Could not auto-extract tool_context: {e}")
        
        print(f"🌐 [API] Making {method} request to: {url}")
        print(f"📦 [API] Original request body: {json.dumps(body, indent=2)}")
        
        # Pre-flight connectivity check
        try:
            parsed = urlparse(url)
            host = parsed.hostname or 'localhost'
            port = parsed.port or (443 if parsed.scheme == 'https' else 80)
            
            # Quick socket test
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(5)
            result = sock.connect_ex((host, port))
            sock.close()
            
            if result == 0:
                print(f"✅ Pre-flight check passed - {host}:{port} is reachable")
            else:
                print(f"⚠️ Pre-flight check failed - {host}:{port} is not reachable (error: {result})")
                print("💡 This likely means the backend server is not running!")
                
        except Exception as e:
            print(f"⚠️ Pre-flight check error: {e}")
            print("💡 Proceeding with request anyway...")
        
        try:
            # Prepare request headers - start with default headers
            request_headers = {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
            
            is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
            dev_email = os.getenv('DEV_EMAIL', '')
            
            if is_development and dev_email:
                print("🧪 Adding development IAP headers...")
                current_timestamp = str(int(time.time()))
                
                iap_headers = {
                    'x-goog-authenticated-user-email': f'accounts.google.com:{dev_email}',
                    'x-goog-authenticated-user-id': f'accounts.google.com:dev-user-id-{current_timestamp}',
                    'x-forwarded-user': dev_email,
                    'x-forwarded-email': dev_email,
                    'X-Dev-IAP-Simulation': 'true',
                    'X-Dev-Auth-Timestamp': current_timestamp
                }
                print(f"✅ Added development IAP headers for email: {dev_email}")
                request_headers.update(iap_headers)
            
            # Add any additional headers passed as parameter
            if headers:
                print(f"🔐 Adding additional headers: {list(headers.keys())}")
                request_headers.update(headers)
            
            # Add IDP token to request headers if not already present
            if not request_headers.get('Authorization'):
                # Get OAuth configuration from config manager
                oauth_config = config_manager.get_oauth_config()
                target_principal = oauth_config.get('target_principal')
                target_audience = oauth_config.get('client_id')
                if (target_principal and target_audience):
                    # Get user email from tool_context if available, otherwise use dev_email for development
                    user_email = None
                    if tool_context and hasattr(tool_context, 'state') and tool_context.state:
                        user_email = tool_context.state.get('user_email')
                        print(f"🔍 [AUTH] Retrieved user_email from tool_context.state: {user_email}")
                        print(f"🔍 [AUTH] tool_context.state type: {type(tool_context.state)}")
                        print(f"🔍 [AUTH] tool_context.state has user_email: {hasattr(tool_context.state, 'user_email')}")
                    elif is_development and dev_email:
                        user_email = dev_email
                        print(f"🧪 Using dev_email for IDP token: {dev_email}")
                    else:
                        print(f"🔍 [AUTH] No tool_context or state available")
                    
                    # Check if this is a Google-related external API call
                    is_google_api = any(google_path in url for google_path in [
                        '/google-drive/', '/convert/url', '/convert/markdown-to-google-doc'
                    ])
                    
                    if is_google_api:
                        # For Google APIs, use service account token without impersonation
                        print(f"🔍 Detected Google API call - using service account token without impersonation")
                        print(f"🔍 [AUTH-PARAMS] target_audience: {target_audience}")
                        print(f"🔍 [AUTH-PARAMS] target_principal: {target_principal}")
                        print(f"🔍 [AUTH-PARAMS] use_idp: False")
                        print(f"🔍 [AUTH-PARAMS] subject: None")
                        print(f"🔍 [AUTH-PARAMS] user_email for impersonation header: {user_email}")
                        idp_token = get_service_account_oidc_token(
                            target_audience,
                            target_principal,  # Use original service account email from config
                            use_idp=False,
                            subject=None
                        )
                    else:
                        # For regular APIs, use impersonated token
                        idp_token = get_service_account_oidc_token(
                            target_audience,
                            target_principal,
                            use_idp=True,
                            subject=user_email
                        ) 
                    if idp_token:
                        request_headers['Authorization'] = f"Bearer {idp_token}"
                        print(f"🔐 Added IDP token to Authorization header")
                        
                        # Log token details for debugging
                        try:
                            parts = idp_token.split('.')
                            if len(parts) >= 2:
                                payload = parts[1]
                                # Add padding if needed
                                payload += '=' * (4 - len(payload) % 4)
                                decoded = base64.b64decode(payload)
                                token_data = json.loads(decoded)
                                print(f"🔍 [TOKEN-DEBUG] Token details:")
                                print(f"   sub: {token_data.get('sub', 'Not Present')}")
                                print(f"   email: {token_data.get('email', 'Not Present')}")
                                print(f"   aud: {token_data.get('aud', 'Not Present')}")
                                print(f"   iss: {token_data.get('iss', 'Not Present')}")
                        except Exception as e:
                            print(f"❌ [TOKEN-DEBUG] Could not decode token: {e}")
                    else:
                        print(f"❌ Failed to get IDP token - will proceed without Authorization header")
                        # This might cause authentication failures, but better than "Bearer None"
                else:
                    print(f"⚠️ Missing OAuth config - target_principal: {target_principal}, client_id: {target_audience}")
            
            # Add impersonated user header for ALL APIs
            # Reuse the user_email that was already successfully retrieved above
            is_google_api = any(google_path in url for google_path in [
                '/google-drive/', '/convert/url', '/convert/markdown-to-google-doc'
            ])
            
            print(f"🔍 [IMPERSONATION] Using user_email for impersonation header: {user_email}")
            print(f"   is_google_api: {is_google_api}")
            
            # Add impersonation header for ALL APIs when user email is available
            if user_email:
                request_headers['x-unops-impersonated-user'] = user_email
                print(f"🔐 Added impersonated user header: {user_email}")
            else:
                print(f"⚠️ No impersonation header added - no user email available")
            
            print("🔐 Final request headers being sent to backend:")
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
                    print(f"   {key}: {value}")
            
            # Get API timeout from config
            api_timeout = config_manager.get_api_timeout()
            print(f"⏱️ [API] Using timeout: {api_timeout}s from config")
            
            
            print(f"📊 [API] Enhanced body (post-filter): {json.dumps(body, indent=2)}")
            
            # Make the appropriate HTTP request based on method
            if method.upper() == 'GET':
                # Handle GET parameters properly
                if body:
                    # For GET requests, convert body to query parameters
                    query_params = '&'.join([f"{k}={v}" for k, v in body.items() if v is not None])
                    if query_params:
                        separator = '&' if '?' in url else '?'
                        url += separator + query_params
                    print(f"🔗 [API] GET URL with query params: {url}")
                
                # Make GET request
                response = requests.get(url, headers=request_headers, timeout=api_timeout, verify=False)
                
            elif method.upper() == 'POST':
                # Make POST request with JSON body
                response = requests.post(url, json=body, headers=request_headers, timeout=api_timeout, verify=False)
                
            elif method.upper() == 'PUT':
                # Make PUT request with JSON body
                response = requests.put(url, json=body, headers=request_headers, timeout=api_timeout, verify=False)
                
            elif method.upper() == 'DELETE':
                # Make DELETE request
                response = requests.delete(url, headers=request_headers, timeout=api_timeout, verify=False)
                
            else:
                return {
                    "status": "error",
                    "error": f"Unsupported HTTP method: {method}",
                    "supported_methods": ["GET", "POST", "PUT", "DELETE"]
                }
            
            print(f"📡 [API] Response status: {response.status_code}")
            
            # Process response
            if response.status_code >= 200 and response.status_code < 300:
                try:
                    response_data = response.json()
                    print(f"✅ [API] Success - Response data keys: {list(response_data.keys()) if isinstance(response_data, dict) else 'Not a dict'}")
                    
                    return {
                        "status": "success",
                        "status_code": response.status_code,
                        "response": response_data,
                        "api_call": f"{method.upper()} {url}",
                        "headers_sent": list(request_headers.keys())
                    }
                    
                except json.JSONDecodeError as json_error:
                    print(f"⚠️ [API] Response is not JSON: {json_error}")
                    return {
                        "status": "success",
                        "status_code": response.status_code,
                        "response": {"text": response.text},
                        "api_call": f"{method.upper()} {url}",
                        "headers_sent": list(request_headers.keys()),
                        "note": "Response was not JSON"
                    }
                    
            else:
                error_message = f"HTTP {response.status_code}"
                try:
                    error_data = response.json()
                    if isinstance(error_data, dict):
                        error_message = error_data.get('message', error_data.get('error', error_message))
                except:
                    error_message = response.text if response.text else error_message
                
                print(f"❌ [API] Error {response.status_code}: {error_message}")
                
                return {
                    "status": "error",
                    "status_code": response.status_code,
                    "error": error_message,
                    "api_call": f"{method.upper()} {url}",
                    "headers_sent": list(request_headers.keys())
                }
                
        except requests.exceptions.ConnectionError as conn_error:
            error_msg = f"Connection error: {str(conn_error)}"
            print(f"❌ [API] {error_msg}")
            return {
                "status": "error",
                "error": error_msg,
                "api_call": f"{method.upper()} {url}",
                "connection_error": True
            }
            
        except requests.exceptions.Timeout as timeout_error:
            error_msg = f"Request timeout: {str(timeout_error)}"
            print(f"❌ [API] {error_msg}")
            return {
                "status": "error",
                "error": error_msg,
                "api_call": f"{method.upper()} {url}",
                "timeout_error": True
            }
            
        except Exception as request_error:
            error_msg = f"Request failed: {str(request_error)}"
            print(f"❌ [API] {error_msg}")
            return {
                "status": "error",
                "error": error_msg,
                "api_call": f"{method.upper()} {url}",
                "traceback": traceback.format_exc()
            }
            
    except Exception as e:
        error_msg = f"Function error: {str(e)}"
        print(f"❌ [API] {error_msg}")
        return {
            "status": "error",
            "error": error_msg,
            "api_call": f"{method.upper()} {url}",
            "traceback": traceback.format_exc()
        }


# Enhanced invoke_api_tool that automatically finds endpoints
def invoke_api_tool(entity_name: str, intent: str, params: Optional[dict] = None, isMultiToolRequest: bool = False, tool_context: Optional[ToolContext] = None) -> str:
    """
    Enhanced API tool that automatically finds the appropriate endpoint and invokes it.
    This replaces the two-step process of find_entity_endpoint + invoke_api_tool.
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Interaction")
        intent: The intent (e.g., "search", "create", "update", "delete")
        params: Parameters for the API call (optional)
        isMultiToolRequest: True if this is part of a multi-tool workflow (optional)
        tool_context: Tool context for session state (optional)
        
    Returns:
        JSON string containing the API response or error information
    """
    try:
        print(f"🚀 [ENHANCED-API-TOOL] Starting {intent} for {entity_name}")
        if params:
            print(f"   📊 Parameters: {params}")
            print(f"   📊 Parameter keys: {list(params.keys()) if params else 'None'}")
            print(f"   📊 Parameter values: {list(params.values()) if params else 'None'}")
        
        # Step 1: Find the appropriate endpoint
        params_json = json.dumps(params) if params else "{}"
        endpoint_result = find_entity_endpoint(entity_name, intent, params_json)
        
        try:
            endpoint_data = json.loads(endpoint_result)
        except (json.JSONDecodeError, TypeError):
            error_result = json.dumps({
                "error": f"Failed to parse endpoint finder result for {entity_name}/{intent}",
                "raw_result": endpoint_result
            })
            
            return error_result
        
        if not endpoint_data.get("endpoint_found", False):
            print(f"❌ [ENHANCED-API-TOOL] No endpoint found for {entity_name}/{intent}")
            error_result = json.dumps({
                "error": f"No suitable endpoint found for entity '{entity_name}' with intent '{intent}'",
                "entity": entity_name,
                "intent": intent,
                "endpoint_finder_result": endpoint_data
            })
            
            return error_result
        
        # Step 2: Extract endpoint details
        full_url = endpoint_data.get("full_url")
        method = endpoint_data.get("method", "GET")
        endpoint_params = endpoint_data.get("parameters", {})
        
        if not full_url:
            error_result = json.dumps({
                "error": f"No URL found in endpoint data for {entity_name}/{intent}",
                "endpoint_data": endpoint_data
            })
            
            return error_result
        
        print(f"🎯 [ENHANCED-API-TOOL] Found endpoint: {method} {full_url}")
        
        # Step 3: Prepare the request body
        request_body = params or {}
        
        # Step 4: Make the API call using the base invoke_api_tool
        try:
            api_result = base_invoke_api_tool(
                url=full_url,
                method=method,
                body=request_body,
                headers=None,
                tool_context=tool_context
            )
            
            print(f"✅ [ENHANCED-API-TOOL] API call completed for {entity_name}/{intent}")
            
            # Convert API result to string format
            if isinstance(api_result, dict):
                result_str = json.dumps(api_result)
            else:
                result_str = str(api_result)
            
            return result_str
                
        except Exception as api_error:
            print(f"❌ [ENHANCED-API-TOOL] API call failed: {api_error}")
            
            # Try fallback endpoints if available
            retry_info = endpoint_data.get("retry_info", {})
            fallback_endpoints = retry_info.get("fallback_endpoints", [])
            
            if fallback_endpoints:
                print(f"🔄 [ENHANCED-API-TOOL] Trying {len(fallback_endpoints)} fallback endpoints...")
                
                for i, fallback in enumerate(fallback_endpoints[:2], 1):  # Try up to 2 fallbacks
                    try:
                        fallback_url = fallback.get("full_url")
                        fallback_method = fallback.get("method", "GET")
                        
                        print(f"🔄 [ENHANCED-API-TOOL] Fallback {i}: {fallback_method} {fallback_url}")
                        
                        fallback_result = base_invoke_api_tool(
                            url=fallback_url,
                            method=fallback_method,
                            body=request_body,
                            headers=None,
                            tool_context=tool_context
                        )
                        
                        print(f"✅ [ENHANCED-API-TOOL] Fallback {i} succeeded!")
                        
                        # Convert fallback result to string format
                        if isinstance(fallback_result, dict):
                            result_str = json.dumps(fallback_result)
                        else:
                            result_str = str(fallback_result)
                        
                        return result_str
                            
                    except Exception as fallback_error:
                        print(f"❌ [ENHANCED-API-TOOL] Fallback {i} failed: {fallback_error}")
                        continue
            
            # All attempts failed
            error_result = json.dumps({
                "error": f"All API attempts failed for {entity_name}/{intent}",
                "primary_error": str(api_error),
                "fallbacks_attempted": len(fallback_endpoints),
            "entity": entity_name,
                "intent": intent
            })
            
            return error_result
        
    except Exception as e:
        print(f"❌ [ENHANCED-API-TOOL] Unexpected error in {entity_name}/{intent}: {e}")
        traceback.print_exc()
        error_result = json.dumps({
            "error": f"Unexpected error in enhanced API tool: {str(e)}",
            "entity": entity_name,
            "intent": intent
        })
        
        return error_result
