"""
Common Callbacks and Utilities

This module contains shared functions and callbacks used by multiple agents.
"""

import json
import os
import re
import traceback
import time
from typing import Any, Dict, Optional
import requests
from google.adk.tools.tool_context import ToolContext
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from .api_config_manager import config_manager

# Define LlmResponse for type annotations
LlmResponse = types.GenerateContentResponse

from .auth_helpers import get_service_account_oidc_token
from .api_config_manager import config_manager

# Global timing storage for agents and models
_agent_timings = {}
_model_timings = {}

# Optional cache import with graceful fallback
try:
    from ai_assistant.utils.cache import entity_cache
    CACHE_AVAILABLE = True
except ImportError:
    CACHE_AVAILABLE = False
    print("ℹ️ Cache system not available - cache operations will be skipped")


def before_agent_callback(ctx: CallbackContext) -> None:
    """Callback to log agent start time"""
    agent_name = ctx.agent_name if hasattr(ctx, 'agent_name') else 'unknown_agent'
    session_id = ctx.session_id if hasattr(ctx, 'session_id') else 'unknown_session'
    
    start_time = time.time()
    _agent_timings[f"{session_id}_{agent_name}"] = start_time
    
    print(f"⏱️ [AGENT-TIMING] Starting agent: {agent_name} (session: {session_id})")

def after_agent_callback(ctx: CallbackContext) -> None:
    """Callback to log agent completion time"""
    agent_name = ctx.agent_name if hasattr(ctx, 'agent_name') else 'unknown_agent'
    session_id = ctx.session_id if hasattr(ctx, 'session_id') else 'unknown_session'
    
    timing_key = f"{session_id}_{agent_name}"
    if timing_key in _agent_timings:
        start_time = _agent_timings[timing_key]
        elapsed_time = time.time() - start_time
        del _agent_timings[timing_key]  # Clean up
        
        print(f"⏱️ [AGENT-TIMING] Agent completed: {agent_name} in {elapsed_time:.2f}s (session: {session_id})")
    else:
        print(f"⚠️ [AGENT-TIMING] No start time found for agent: {agent_name}")

def before_model_callback(ctx: CallbackContext) -> None:
    """Callback to log model start time"""
    model_name = getattr(ctx, 'model_name', 'unknown_model')
    session_id = getattr(ctx, 'session_id', 'unknown_session')
    
    start_time = time.time()
    _model_timings[f"{session_id}_{model_name}"] = start_time
    
    print(f"⏱️ [MODEL-TIMING] Starting model: {model_name} (session: {session_id})")

def after_model_callback(ctx: CallbackContext) -> None:
    """Callback to log model completion time"""
    model_name = getattr(ctx, 'model_name', 'unknown_model')
    session_id = getattr(ctx, 'session_id', 'unknown_session')
    
    timing_key = f"{session_id}_{model_name}"
    if timing_key in _model_timings:
        start_time = _model_timings[timing_key]
        elapsed_time = time.time() - start_time
        del _model_timings[timing_key]  # Clean up
        
        print(f"⏱️ [MODEL-TIMING] Model completed: {model_name} in {elapsed_time:.2f}s (session: {session_id})")
    else:
        print(f"⚠️ [MODEL-TIMING] No start time found for model: {model_name}")

def before_tool_callback(ctx: CallbackContext) -> None:
    """Callback to log tool start time"""
    tool_name = getattr(ctx, 'tool_name', 'unknown_tool')
    session_id = getattr(ctx, 'session_id', 'unknown_session')
    
    start_time = time.time()
    _agent_timings[f"{session_id}_tool_{tool_name}"] = start_time
    
    print(f"⏱️ [TOOL-TIMING] Starting tool: {tool_name} (session: {session_id})")

def after_tool_callback(ctx: CallbackContext) -> None:
    """Callback to log tool completion time"""
    tool_name = getattr(ctx, 'tool_name', 'unknown_tool')
    session_id = getattr(ctx, 'session_id', 'unknown_session')
    
    timing_key = f"{session_id}_tool_{tool_name}"
    if timing_key in _agent_timings:
        start_time = _agent_timings[timing_key]
        elapsed_time = time.time() - start_time
        del _agent_timings[timing_key]  # Clean up
        
        print(f"⏱️ [TOOL-TIMING] Tool completed: {tool_name} in {elapsed_time:.2f}s (session: {session_id})")
    else:
        print(f"⚠️ [TOOL-TIMING] No start time found for tool: {tool_name}")


def generate_image_with_gemini(prompt: str, style: str = "realistic", size: str = "1024x1024") -> Dict[str, Any]:
    """
    Generate an image using Google's Gemini Pro Vision model.
    
    Args:
        prompt: Text description of the image to generate
        style: Image style (realistic, artistic, cartoon, sketch, etc.)
        size: Image dimensions (1024x1024, 1792x1024, 1024x1792)
        
    Returns:
        Dict containing the generated image data or error information
    """
    try:
        print(f"🎨 [IMAGE-GEN] Generating image with prompt: '{prompt}' (style: {style}, size: {size})")
        
        # For now, we'll return a placeholder since Gemini Pro doesn't support image generation yet
        # This is a placeholder for future implementation when Gemini supports image generation
        # or when we integrate with other image generation services like DALL-E or Stable Diffusion
        
        # Placeholder response structure
        placeholder_response = {
            "success": False,
            "message": "Image generation is not yet available with the current Gemini model. This feature will be enabled when Gemini supports image generation or when we integrate with other image generation services.",
            "suggestion": "For now, I can create visual representations using charts, diagrams, and other visualization tools. Would you like me to create a diagram or chart instead?",
            "available_alternatives": [
                "Mermaid diagrams for process flows and relationships",
                "Chart.js charts for data visualization", 
                "Text-based visual representations",
                "Structured data displays"
            ]
        }
        
        print(f"⚠️ [IMAGE-GEN] Image generation not yet available: {placeholder_response['message']}")
        return placeholder_response
        
    except Exception as e:
        error_msg = f"Failed to generate image: {str(e)}"
        print(f"❌ [IMAGE-GEN] {error_msg}")
        return {
            "success": False,
            "message": error_msg,
            "error": str(e)
        }


def create_visual_representation(content_type: str, data: Any, style: str = "professional") -> Dict[str, Any]:
    """
    Create alternative visual representations when image generation is not available.
    
    Args:
        content_type: Type of content to visualize (data, process, relationship, etc.)
        data: The data or content to visualize
        style: Visual style preference
        
    Returns:
        Dict containing alternative visualization options
    """
    try:
        print(f"🎨 [VISUAL-REP] Creating visual representation for {content_type} with style: {style}")
        
        # Generate alternative visualization suggestions
        alternatives = {
            "data_visualization": {
                "type": "chartjs",
                "chartType": "bar",
                "message": "I can create a chart to visualize this data. Would you like me to generate a bar chart, pie chart, or line chart?",
                "entity": "Data Visualization"
            },
            "process_flow": {
                "type": "mermaid", 
                "message": "I can create a flowchart to show this process. Would you like me to generate a visual diagram?",
                "entity": "Process Flow"
            },
            "relationships": {
                "type": "mermaid",
                "message": "I can create a relationship diagram to show connections between entities. Would you like me to generate a visual map?",
                "entity": "Entity Relationships"
            },
            "organizational": {
                "type": "mermaid",
                "message": "I can create an organizational chart to show the structure. Would you like me to generate a visual hierarchy?",
                "entity": "Organizational Structure"
            }
        }
        
        # Select appropriate alternative based on content type
        if "data" in content_type.lower() or "statistics" in content_type.lower():
            selected = alternatives["data_visualization"]
        elif "process" in content_type.lower() or "flow" in content_type.lower():
            selected = alternatives["process_flow"]
        elif "relationship" in content_type.lower() or "connection" in content_type.lower():
            selected = alternatives["relationships"]
        elif "organization" in content_type.lower() or "structure" in content_type.lower():
            selected = alternatives["organizational"]
        else:
            selected = alternatives["data_visualization"]
        
        print(f"✅ [VISUAL-REP] Generated alternative visualization: {selected['type']}")
        return {
            "success": True,
            "alternative": selected,
            "message": f"Since image generation isn't available yet, I can create a {selected['type']} visualization instead. Would you like me to proceed with that?"
        }
        
    except Exception as e:
        error_msg = f"Failed to create visual representation: {str(e)}"
        print(f"❌ [VISUAL-REP] {error_msg}")
        return {
            "success": False,
            "message": error_msg,
            "error": str(e)
        }


def detect_entity_from_url(url: str) -> Optional[str]:
    """
    Detect entity type from API URL for cache invalidation.
    
    Args:
        url: API URL to analyze
        
    Returns:
        Optional[str]: Detected entity type or None
    """
    try:
        # Load URL patterns from configuration
        try:
            from .api_config_manager import config_manager
            entity_patterns = config_manager.get_url_patterns()
            print(f"✅ Loaded URL patterns from configuration: {list(entity_patterns.keys())}")
        except Exception as e:
            print(f"⚠️ Failed to load URL patterns from config, using defaults: {e}")
            # Fallback to default patterns
            entity_patterns = {
                'partner': 'Partner',
                'contact': 'Contact',
                'interaction': 'Interaction',
                'opportunity': 'Opportunity',
                'user': 'User',
                'userdata': 'UserData',
                'preferences': 'UserPreferences'
            }
        
        url_lower = url.lower()
        
        for pattern, entity in entity_patterns.items():
            if pattern in url_lower:
                return entity
                
        return None
        
    except Exception as e:
        print(f"⚠️ Error detecting entity from URL: {e}")
        return None




def exit_loop_on_success(tool_context: ToolContext):
    """
    Call this function when the CURRENT entity has been processed successfully.
    This immediately stops the loop by escalating.
    """
    print(f"🎯 [Tool Call] exit_loop_on_success triggered by {tool_context.agent_name}")
    print("✅ Entity processed successfully - STOPPING LOOP IMMEDIATELY")
    print("🛑 Setting escalate=True to force loop termination")
    
    # Force immediate loop termination
    tool_context.actions.escalate = True
    
    # Also set a completion flag in state
    if hasattr(tool_context, 'state'):
        tool_context.state['loop_completed'] = True
        tool_context.state['exit_reason'] = 'success'
    
    print("🔄 Loop will now terminate and return control to next agent")
    
    return {
        "status": "success", 
        "message": "Entity processed successfully - loop terminated",
        "escalated": True,
        "loop_stopped": True
    }


def invoke_api_tool(url: str, method: str, body: dict, headers: Optional[dict] = None, tool_context: Optional[ToolContext] = None) -> dict:
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
            from urllib.parse import urlparse
            parsed = urlparse(url)
            
            import socket
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
                            import base64
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
                    
                    # Detect entity from URL for cache invalidation (if cache is available)
                    if CACHE_AVAILABLE:
                        detected_entity = detect_entity_from_url(url)
                        if detected_entity:
                            print(f"🔄 [API] Detected entity '{detected_entity}' from URL - invalidating cache")
                            try:
                                entity_cache.invalidate_on_entity_change(detected_entity, 'update')
                            except Exception as cache_error:
                                print(f"⚠️ Cache invalidation failed: {cache_error}")
                    else:
                        print("ℹ️ [API] Cache not available - skipping cache invalidation")
                    
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


def inject_entity_specific_tools_before_model(
    callback_context: CallbackContext, 
    llm_request
) -> Optional[types.Content]:
    """
    Before model callback that injects entity-specific API tools information.
    This dynamically loads only the tools relevant to the current entity being processed.
    
    Based on UNOPS.PAO.AgenticAi implementation pattern.
    """
    
    # Get the agent name to check if this is the task_executor_agent
    agent_name = callback_context.agent_name
    
    if agent_name not in ["api_caller_agent", "task_executor_agent", "task_executor_llm_agent"]:
        print(f"ℹ️ Skipping entity-specific tools injection - not task_executor_agent (current: {agent_name})")
        return None  # Only apply to task_executor_agent
    
    print(f"🔧 [Callback] Injecting entity-specific API tools for {agent_name}")
    
    # Try to extract entity from context - look for previous agent output
    detected_entity = None
    detected_entities = []
    
    # Check if there's context from entity intent detection agent
    if hasattr(callback_context, 'context') and callback_context.context:
        context_data = callback_context.context
        print(f"🔍 Found context data: {type(context_data)}")
        
        # Look for entity information from previous agent
        if isinstance(context_data, dict):
            detected_entity = context_data.get('entity')
            if not detected_entity:
                # Try other possible keys
                detected_entity = context_data.get('detected_entity')
            if not detected_entity:
                # Try to find in nested structures
                for key, value in context_data.items():
                    if isinstance(value, dict) and 'entity' in value:
                        detected_entity = value['entity']
                        break
                    elif isinstance(value, list):
                        # Handle array of detected entities
                        for item in value:
                            if isinstance(item, dict) and 'entity' in item:
                                detected_entities.append(item)
        
        # If we found an array of entities, use the first one for now
        if detected_entities:
            detected_entity = detected_entities[0].get('entity')
            print(f"🎯 Found {len(detected_entities)} detected entities, using first: {detected_entity}")
    
    # Check current state for detected entities
    if not detected_entity and hasattr(callback_context, 'state') and callback_context.state:
        current_entities = callback_context.state.get('current_entities', [])
        current_entity_index = callback_context.state.get('current_entity_index', 0)
        
        if current_entities and current_entity_index < len(current_entities):
            current_entity_data = current_entities[current_entity_index]
            if isinstance(current_entity_data, dict):
                detected_entity = current_entity_data.get('entity')
                print(f"🔄 Using entity from state: {detected_entity} (index {current_entity_index})")
    
    # If no entity detected from context, try to parse from the messages
    if not detected_entity and llm_request and llm_request.contents:
        for content in llm_request.contents:
            if hasattr(content, 'text') and content.text:
                text = content.text.lower()
                # Look for entity patterns in the text
                if 'entity=' in text or 'entity":' in text:
                    # Extract entity from entity=value pattern
                    match = re.search(r'entity["\']?\s*[:=]\s*["\']?([^"\'\\s,}]+)["\']?', text)
                    if match:
                        detected_entity = match.group(1)
                        print(f"🔍 Extracted entity from text: {detected_entity}")
                        break
    
    print(f"🎯 Final detected entity: {detected_entity}")
    
    # Get entity-specific tools using new config_manager approach
    search_guidance = ""
    if detected_entity:
        try:
            # Try to load entity-specific configuration
            api_summary = config_manager.get_entity_api_tools(detected_entity.capitalize())
            print(f"✅ Loaded {detected_entity}-specific tools from {detected_entity}-tools.json")
            
            # Load search metadata for advanced search guidance (with graceful fallback)
            try:
                search_guidance = config_manager.format_search_guidance(detected_entity.capitalize())
                print(f"✅ Loaded search metadata for {detected_entity}")
            except Exception as search_e:
                print(f"ℹ️ Could not load search metadata for {detected_entity}: {search_e} - this is fine if entity doesn't support advanced search")
                search_guidance = ""  # Will be handled in search_section logic below
        except Exception as e:
            print(f"⚠️ Could not load entity-specific tools for {detected_entity}: {e}")
            # Fall back to all tools if entity-specific loading fails
            api_summary = config_manager.get_api_endpoints_summary()
            search_guidance = ""  # No search guidance available in fallback mode
            print("🔄 Falling back to all tools")
    else:
        # Fall back to all tools if no entity detected
        api_summary = config_manager.get_api_endpoints_summary()
        search_guidance = ""  # No search guidance available when no entity detected
        print("⚠️ No entity detected - loading all tools as fallback")
    
    # Build enhanced instruction with API information and search metadata
    search_section = ""
    if search_guidance and search_guidance.strip():
        search_section = f"""

{search_guidance}

**🔍 ADVANCED SEARCH INSTRUCTIONS:**
When forming advanced search queries (advancedSearch=true):
1. **Use the fields listed above** - Only use directFields and nestedFields from searchMetadata
2. **Use available operators** - Select appropriate operators from the list above  
3. **Include description** - Every search criteria object MUST include a description field
4. **Follow JSON format**: [{{"field": "partner.name", "operator": "like", "value": "UNICEF", "description": "Find interactions with UNICEF partners"}}]
5. **Use examples above** - Reference the exampleCriteria for proper formatting

**WHEN TO USE ADVANCED SEARCH:**
- Nested entity searches: "interactions with UNICEF partners", "contacts from WHO"
- Field-specific searches: "partners where status is Active"
- Multiple criteria: "meetings from Finance department contacts"

**WHEN TO USE SIMPLE TEXT SEARCH:**
- Simple keywords: "project", "meeting notes"
- General text searches across fields
- Single search terms
"""
    else:
        # Provide basic search guidance when no searchMetadata is available
        search_section = """

**🔍 BASIC SEARCH GUIDANCE:**
ℹ️ Advanced search metadata is not available for this entity - using basic search approach.

**RECOMMENDED SEARCH STRATEGY:**
1. **Use simple text search** - Prefer searchText parameter over complex searchCriteria
2. **Basic operators** - Use common operators: like, is, not, contains, startsWith, endsWith
3. **Avoid nested fields** - Stick to direct entity properties unless documented otherwise
4. **Fallback gracefully** - If advanced search fails, try simple text search

**WHEN TO USE SIMPLE TEXT SEARCH (RECOMMENDED):**
- All keyword searches: "project", "meeting notes", "UNICEF"
- General searches across entity fields
- When searchMetadata is not configured

**NOTE:** This entity may not support advanced nested field searches.
"""
    
    # Build enhanced instruction with API information
    from .api_config_manager import API_BASE_URL
    enhanced_instruction = f"""
🛠️ **API CALLER AGENT - ENHANCED WITH DYNAMIC CONFIGURATION**

You are responsible for executing real HTTP API calls based on detected entities and intents.

**BASE URL:** {API_BASE_URL}

{api_summary}{search_section}

**🔧 YOUR RESPONSIBILITIES:**

1. **Endpoint Selection**: Choose the correct API endpoint based on entity + intent
2. **URL Construction**: Build complete URLs with proper path parameters  
3. **Parameter Mapping**: Use endpoint configuration to format parameters correctly
4. **HTTP Execution**: Make real requests using `invoke_api_tool(url, method, body)`
5. **Error Handling**: Process responses and handle errors appropriately

**CRITICAL INSTRUCTIONS:**
- You MUST call `invoke_api_tool()` to make actual HTTP requests
- Do NOT just describe what you would do - ACTUALLY DO IT
- **ALWAYS call `exit_loop_on_success()` after completing your task**
- **If waiting for user input/confirmation, call `exit_loop_on_success()` to let user respond**
- **If you ask a question to the user, immediately call `exit_loop_on_success()`**

Remember: You are making REAL HTTP requests to actual endpoints. The configuration above shows you exactly which endpoints are available and how to use them.
"""
    
    # Update the LLM request instruction
    if llm_request.config and llm_request.config.system_instruction:
        llm_request.config.system_instruction = enhanced_instruction
    else:
        # Create config if it doesn't exist
        if not llm_request.config:
            llm_request.config = types.GenerateContentConfig()
        llm_request.config.system_instruction = enhanced_instruction
    
    print("✅ Entity-specific API tools information injected successfully")
    
    # Don't return content - just modify the request
    return None


def screen_context_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for screen_context_agent.
    
    This callback runs AFTER the model responds and caches the screen context result.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response or None to use original
    """
    
    print("💾 [SCREEN] Caching screen context result...")
    
    try:
        # Check if we have screen context data in state to cache
        screen_context = callback_context.state.get('screen_context')
        
        # Extract entity and ID from structured state data to build cache key
        screen_url_obj = callback_context.state.get('screen_url', {})
        user_viewing_panel = callback_context.state.get('user_viewing_panel', {})
        
        # First priority: screen_url object
        entity = screen_url_obj.get('entity', '')
        entity_id = screen_url_obj.get('id', None)
            
        # Second priority: user_viewing_panel if screen_url is empty
        if not entity and user_viewing_panel:
            entity = user_viewing_panel.get('entity', '')
            entity_id = user_viewing_panel.get('entity_id', None)
            # Convert string ID to int if needed
            if entity_id and isinstance(entity_id, str) and entity_id.isdigit():
                entity_id = int(entity_id)
        
        if screen_context and isinstance(screen_context, dict) and entity and CACHE_AVAILABLE:
            # Cache screen context using entity+id key
            cache_key = f"{entity}:{entity_id}" if entity_id else f"{entity}:list"
            entity_cache.set_screen_context(cache_key, screen_context)
                
            print(f"✅ [SCREEN] Cached screen context successfully for {cache_key}")
        
        return None  # Use original response
        
    except Exception as e:
        print(f"❌ [SCREEN] Error caching screen context: {e}")
        return None  # Use original response


def response_formatter_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for response_formatter_agent.
    
    This callback cleans JSON responses and ensures proper formatting without markdown blocks.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response with cleaned JSON or None to use original
    """
    
    print("🧹 [RESPONSE-FORMATTER] Cleaning response formatter agent response...")
    
    try:
        # Get the raw text response
        if not llm_response.candidates or not llm_response.candidates[0].content or not llm_response.candidates[0].content.parts:
            print("⚠️ [RESPONSE-FORMATTER] No content found in response")
            return None
        
        raw_text = llm_response.candidates[0].content.parts[0].text
        print(f"🔍 [RESPONSE-FORMATTER] Raw response length: {len(raw_text)} characters")
        
        # Clean the response by removing markdown code blocks and extra text
        cleaned_text = clean_json_response(raw_text)
        
        # Validate that it's proper JSON
        try:
            parsed_json = json.loads(cleaned_text)
            print("✅ [RESPONSE-FORMATTER] JSON validation passed")
        except json.JSONDecodeError as e:
            print(f"❌ [RESPONSE-FORMATTER] JSON validation failed: {e}")
            print(f"🔍 [RESPONSE-FORMATTER] Problematic text: {cleaned_text[:200]}...")
            return None  # Use original response if cleaning failed
        
        # Store the cleaned result in state for other agents
        callback_context.state['formatted_response'] = cleaned_text
        
        # Return modified response with cleaned content
        return LlmResponse(
            content=types.Content(
                role="model",
                parts=[types.Part(text=cleaned_text)],
            )
        )
        
    except Exception as e:
        print(f"❌ [RESPONSE-FORMATTER] Error cleaning response: {e}")
        return None  # Use original response on error


def clean_json_response(text: str) -> str:
    """
    Clean a response text to extract valid JSON.
    
    This function removes:
    - Markdown code blocks (```json, ```)
    - Extra explanatory text before/after JSON
    - Whitespace and formatting issues
    
    Args:
        text: Raw text response from LLM
        
    Returns:
        str: Cleaned JSON text
    """
    
    # Remove markdown code blocks
    cleaned = text.strip()
    
    # Remove ```json and ``` markers
    if cleaned.startswith('```json'):
        cleaned = cleaned[7:]  # Remove ```json
    elif cleaned.startswith('```'):
        cleaned = cleaned[3:]   # Remove ```
    
    if cleaned.endswith('```'):
        cleaned = cleaned[:-3]  # Remove trailing ```
    
    # Find JSON array or object boundaries
    json_start = -1
    json_end = -1
    
    # Look for JSON array start
    for i, char in enumerate(cleaned):
        if char == '[':
            json_start = i
            break
        elif char == '{':
            json_start = i
            break
    
    # Look for JSON array/object end (find the matching closing bracket)
    if json_start != -1:
        bracket_count = 0
        start_char = cleaned[json_start]
        end_char = ']' if start_char == '[' else '}'
        
        for i in range(json_start, len(cleaned)):
            if cleaned[i] == start_char:
                bracket_count += 1
            elif cleaned[i] == end_char:
                bracket_count -= 1
                if bracket_count == 0:
                    json_end = i
                    break
    
    # Extract JSON portion
    if json_start != -1 and json_end != -1:
        cleaned = cleaned[json_start:json_end + 1]
    
    # Final cleanup
    cleaned = cleaned.strip()
    
    print(f"🧹 [CLEAN] Original length: {len(text)}, Cleaned length: {len(cleaned)}")
    print(f"🔍 [CLEAN] Cleaned preview: {cleaned[:100]}...")
    
    return cleaned