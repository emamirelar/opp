"""
Utility functions for API Worker Agent
"""

import requests
import json
import traceback
import os
from typing import Dict, Any, Optional
from google.adk.tools.tool_context import ToolContext


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
            from ...config_manager import config_manager
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


def invalidate_cache_after_api_success(url: str, method: str, response_data: dict) -> None:
    """
    Invalidate relevant caches after successful API operations.
    
    Args:
        url: API URL that was called
        method: HTTP method used
        response_data: Response data from the API
    """
    
    try:
        # Only invalidate for data-modifying operations
        if method.upper() not in ['POST', 'PUT', 'DELETE', 'PATCH']:
            return
            
        # Try to import cache system
        try:
            from ...agent_callbacks import api_success_callback
        except ImportError:
            print("⚠️ Cache system not available - skipping cache invalidation")
            return
        
        # Detect entity from URL
        entity_type = detect_entity_from_url(url)
        
        if not entity_type:
            print(f"⚠️ Could not detect entity type from URL: {url}")
            return
        
        # Determine operation type
        operation_mapping = {
            'POST': 'create',
            'PUT': 'update',
            'PATCH': 'update',
            'DELETE': 'delete'
        }
        
        operation = operation_mapping.get(method.upper(), 'update')
        
        print(f"🔄 Cache invalidation triggered: {entity_type} {operation} from {method} {url}")
        
        # Call cache invalidation
        api_success_callback(entity_type, operation, response_data)
        
    except Exception as e:
        print(f"⚠️ Error in cache invalidation: {e}")
        # Don't fail the API call if cache invalidation fails


def advance_entity_index(tool_context: ToolContext):
    """
    Advance to the next entity after successful processing of current entity.
    Call this after successfully processing an entity but before exit_loop_on_success.
    """
    print(f"➡️ [Tool Call] advance_entity_index triggered by {tool_context.agent_name}")
    print("🔄 Advancing to next entity in processing queue")
    
    # This will be handled by the callback, but we return success
    return {
        "status": "success",
        "message": "Advanced to next entity",
        "advanced": True
    }


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


def extract_iap_headers_from_context(tool_context: Optional[ToolContext] = None) -> Dict[str, str]:
    """
    Extract IAP headers from the current context (session state).
    
    This function tries to get IAP headers from the session state first,
    then falls back to environment variables for development.
    
    Args:
        tool_context: Tool context containing session state
        
    Returns:
        Dict[str, str]: Dictionary of IAP headers to forward to backend
    """
    try:
        iap_headers = {}
        
        # First try to get email from session state
        user_email = None
        if tool_context and hasattr(tool_context, 'state') and tool_context.state:
            user_email = tool_context.state.get('header_email')
            if user_email and '@' in user_email:
                print(f"📧 Using header_email from session state: {user_email}")
        
        # If no email in session state, check environment variables
        if not user_email:
            is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
            dev_email = os.getenv('DEV_EMAIL', '')
            
            if is_development and dev_email:
                user_email = dev_email
                print(f"📧 Using DEV_EMAIL from environment: {user_email}")
        
        # Create IAP headers if we have a valid email
        if user_email and '@' in user_email:
            print("🧪 Creating IAP headers from user email...")
            import time
            current_timestamp = str(int(time.time()))
            
            iap_headers = {
                'x-goog-authenticated-user-email': f'accounts.google.com:{user_email}',
                'x-goog-authenticated-user-id': f'accounts.google.com:user-id-{current_timestamp}',
                'x-forwarded-user': user_email,
                'x-forwarded-email': user_email,
                'X-Dev-IAP-Simulation': 'true',
                'X-Dev-Auth-Timestamp': current_timestamp
            }
            print(f"✅ Created IAP headers for email: {user_email}")
        else:
            print("⚠️ No valid user email found - no IAP headers created")
        
        return iap_headers
            
    except Exception as e:
        print(f"⚠️ Could not create IAP headers: {e}")
        return {}


def test_api_connectivity(base_url: str) -> dict:
    """
    Test basic connectivity to the API server
    
    Args:
        base_url: Base URL to test (e.g., https://localhost:44426)
    
    Returns:
        dict: Connectivity test results
    """
    print(f"\n🔍 TESTING API CONNECTIVITY")
    print("="*50)
    print(f"📍 Testing URL: {base_url}")
    
    try:
        import socket
        from urllib.parse import urlparse
        
        parsed = urlparse(base_url)
        host = parsed.hostname or 'localhost'
        port = parsed.port or (443 if parsed.scheme == 'https' else 80)
        
        print(f"🌐 Host: {host}")
        print(f"🚪 Port: {port}")
        
        # Test socket connection
        print(f"🔌 Testing socket connection...")
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(10)
        result = sock.connect_ex((host, port))
        sock.close()
        
        if result == 0:
            print(f"✅ Socket connection successful!")
            
            # Test HTTP request
            print(f"📡 Testing HTTP request...")
            response = requests.get(base_url, verify=False, timeout=10)
            print(f"✅ HTTP request successful! Status: {response.status_code}")
            
            return {
                "status": "success",
                "socket_connection": True,
                "http_response": True,
                "status_code": response.status_code
            }
        else:
            print(f"❌ Socket connection failed! Error code: {result}")
            return {
                "status": "error",
                "socket_connection": False,
                "error": f"Cannot connect to {host}:{port}"
            }
            
    except Exception as e:
        print(f"❌ Connectivity test failed: {e}")
        return {
            "status": "error",
            "connectivity_test": False,
            "error": str(e)
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
                        print(f"📧 Auto-extracted tool_context from execution context")
                        break
                    frame = frame.f_back
            except Exception as e:
                print(f"⚠️ Could not auto-extract tool_context: {e}")
        
        print(f"🌐 [API] Making {method} request to: {url}")
        print(f"📦 [API] Request body: {json.dumps(body, indent=2)}")
        
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
                print(f"💡 This likely means the backend server is not running!")
                
        except Exception as e:
            print(f"⚠️ Pre-flight check error: {e}")
            print(f"💡 Proceeding with request anyway...")
        
        try:
            # Prepare request headers - start with default headers
            request_headers = {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
            
            # Automatically extract and add IAP headers from current context
            print("\n🔐 Extracting IAP headers for authentication...")
            iap_headers = extract_iap_headers_from_context(tool_context)
            
            # If no IAP headers found and we're in development mode, add development headers
            if not iap_headers:
                is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
                dev_email = os.getenv('DEV_EMAIL', '')
                
                if is_development and dev_email:
                    print("🧪 No IAP headers found - Adding development IAP headers...")
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
            
            if iap_headers:
                request_headers.update(iap_headers)
            
            # Add any additional headers passed as parameter
            if headers:
                print(f"🔐 Adding additional headers: {list(headers.keys())}")
                request_headers.update(headers)
            
            print(f"🔐 Final request headers: {list(request_headers.keys())}")
            
            # Prepare request data based on HTTP method
            if method.upper() == 'GET':
                # For GET requests, add body parameters as query string
                if body:
                    import urllib.parse
                    query_params = urllib.parse.urlencode(body)
                    if '?' in url:
                        url += '&' + query_params
                    else:
                        url += '?' + query_params
                    print(f"🔗 [API] GET URL with query params: {url}")
                
                # Make GET request
                response = requests.get(url, headers=request_headers, timeout=30, verify=False)
                
            elif method.upper() == 'POST':
                # Make POST request with JSON body
                response = requests.post(url, json=body, headers=request_headers, timeout=30, verify=False)
                
            elif method.upper() == 'PUT':
                # Make PUT request with JSON body
                response = requests.put(url, json=body, headers=request_headers, timeout=30, verify=False)
                
            elif method.upper() == 'DELETE':
                # Make DELETE request
                response = requests.delete(url, headers=request_headers, timeout=30, verify=False)
                
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
                    
                    # Detect entity from URL for cache invalidation
                    detected_entity = detect_entity_from_url(url)
                    if detected_entity:
                        print(f"🔄 [API] Detected entity '{detected_entity}' from URL - invalidating cache")
                        try:
                            from ...cache import entity_cache
                            entity_cache.invalidate_entity_cache(detected_entity)
                        except Exception as cache_error:
                            print(f"⚠️ Cache invalidation failed: {cache_error}")
                    
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


def construct_api_url(base_url: str, endpoint_path: str, path_params: Dict[str, Any] = None) -> str:
    """
    Construct a complete API URL with path parameter substitution
    
    Args:
        base_url: Base URL (e.g., "https://localhost:44426")
        endpoint_path: Endpoint path (e.g., "/api/partner/{id}")
        path_params: Dictionary of path parameters to substitute
    
    Returns:
        str: Complete URL with path parameters substituted
    """
    # Remove trailing slash from base_url if present
    base_url = base_url.rstrip('/')
    
    # Ensure endpoint_path starts with /
    if not endpoint_path.startswith('/'):
        endpoint_path = '/' + endpoint_path
    
    # Construct base URL
    full_url = base_url + endpoint_path
    
    # Substitute path parameters if provided
    if path_params:
        for param_name, param_value in path_params.items():
            # Replace both {param} and [param] patterns
            full_url = full_url.replace(f'{{{param_name}}}', str(param_value))
            full_url = full_url.replace(f'[{param_name}]', str(param_value))
    
    return full_url


def separate_path_and_query_params(parameters: Dict[str, Any], endpoint_path: str) -> tuple:
    """
    Separate parameters into path parameters and query/body parameters.
    Also handles parameter mapping (e.g., top → pageSize).
    
    Args:
        parameters: All parameters from the request
        endpoint_path: The endpoint path to check for path parameter placeholders
    
    Returns:
        tuple: (path_params, other_params)
    """
    path_params = {}
    other_params = {}
    
    # Parameter mapping - convert common parameter names to API-expected names
    parameter_mapping = {
        "top": "pageSize",
        "limit": "pageSize", 
        "count": "pageSize",
        "max": "pageSize",
        "size": "pageSize"
    }
    
    print(f"🔄 Processing parameters: {parameters}")
    
    for param_name, param_value in parameters.items():
        # Check if this parameter is used in the path
        if f'{{{param_name}}}' in endpoint_path or f'[{param_name}]' in endpoint_path:
            path_params[param_name] = param_value
            print(f"📍 Path parameter: {param_name} = {param_value}")
        else:
            # Check if parameter needs mapping
            mapped_name = parameter_mapping.get(param_name.lower(), param_name)
            
            if mapped_name != param_name:
                print(f"🔄 Parameter mapping: {param_name} → {mapped_name} = {param_value}")
                # Convert to integer for pageSize parameters
                if mapped_name == "pageSize":
                    try:
                        param_value = int(param_value)
                        print(f"✅ Converted to integer: {param_value}")
                    except (ValueError, TypeError):
                        print(f"⚠️ Could not convert {param_value} to integer, using as-is")
            
            other_params[mapped_name] = param_value
    
    print(f"📍 Final path_params: {path_params}")
    print(f"📦 Final other_params: {other_params}")
    
    return path_params, other_params


def format_api_response(response_data: Dict[str, Any]) -> str:
    """
    Format API response for user-friendly display
    
    Args:
        response_data: The response from invoke_api_tool
    
    Returns:
        str: Formatted response message
    """
    if response_data.get("status") == "success":
        api_call = response_data.get("api_call", "API call")
        response = response_data.get("response", {})
        
        if isinstance(response, dict) and "data" in response:
            data = response["data"]
            if isinstance(data, list):
                count = len(data)
                return f"✅ {api_call} completed successfully. Found {count} records."
            else:
                return f"✅ {api_call} completed successfully."
        else:
            return f"✅ {api_call} completed successfully."
    else:
        error = response_data.get("error", "Unknown error")
        api_call = response_data.get("api_call", "API call")
        return f"❌ {api_call} failed: {error}" 