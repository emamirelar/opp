"""
Utility functions for API Worker Agent
"""

import requests
import json
import traceback
import os
from typing import Dict, Any, Optional
from google.adk.tools.tool_context import ToolContext


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


def extract_iap_headers_from_context() -> Dict[str, str]:
    """
    Extract IAP headers from environment or development mode.
    Since Google ADK context access is not available, we use environment variables.
    
    Returns:
        Dict[str, str]: Dictionary of IAP headers to forward to backend
    """
    try:
        iap_headers = {}
        
        # Check if we're in development mode
        is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
        dev_email = os.getenv('DEV_EMAIL', '')
        
        if is_development and dev_email:
            print("🧪 Development mode - Creating IAP headers from environment...")
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
            print(f"✅ Created development IAP headers for email: {dev_email}")
        else:
            print("⚠️ Not in development mode or DEV_EMAIL not set - no IAP headers created")
        
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


def invoke_api_tool(url: str, method: str, body: dict, headers: Optional[dict] = None) -> dict:
    """
    Invoke an API endpoint with real HTTP requests using configuration from tools.json
    
    Args:
        url: Full URL to call (e.g., https://localhost:44426/api/partners)
        method: HTTP method (GET, POST, PUT, DELETE)
        body: Request body/parameters
        headers: Optional additional headers (e.g., IAP headers for authentication)
    
    Returns:
        dict: API response or error information
    """
    print("\n" + "="*60)
    print("🚀🚨 INVOKE_API_TOOL FUNCTION CALLED!!!")
    print("🚨 This proves the LLM is using the correct tool!")
    print("🚀 INVOKING API CALL")
    print("="*60)
    print(f"📍 URL: {url}")
    print(f"🔧 Method: {method.upper()}")
    print(f"📦 Body/Parameters: {body}")
    print(f"📊 Body Type: {type(body)}")
    print(f"📏 Body Length: {len(str(body))}")
    
    # Pre-flight connectivity check for connection issues
    from urllib.parse import urlparse
    parsed = urlparse(url)
    base_url = f"{parsed.scheme}://{parsed.netloc}"
    
    print(f"\n🔍 Pre-flight connectivity check...")
    print(f"📍 Base URL: {base_url}")
    
    try:
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
        iap_headers = extract_iap_headers_from_context()
        
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
        
        print(f"📋 Final Headers ({len(request_headers)} total): {list(request_headers.keys())}")
        
        # Show IAP headers specifically (without values for security)
        iap_header_keys = [k for k in request_headers.keys() if 'iap' in k.lower() or 'auth' in k.lower() or 'user' in k.lower()]
        if iap_header_keys:
            print(f"🔐 IAP/Auth Headers being sent: {iap_header_keys}")
        
        # Handle different HTTP methods
        print(f"\n🔄 Executing {method.upper()} request...")
        
        # Common request settings for all methods
        request_kwargs = {
            'headers': request_headers,
            'timeout': 200,
            'verify': False,  # Disable SSL verification for localhost/development
            'allow_redirects': True
        }
        
        print(f"🔧 Request settings: timeout=200s, verify=False, allow_redirects=True")
        
        if method.upper() == 'GET':
            print("📝 GET Request - Parameters will be sent as query string")
            print(f"🔗 Full URL with params: {url}?{requests.compat.urlencode(body) if body else 'no-params'}")
            response = requests.get(url, params=body, **request_kwargs)
        elif method.upper() == 'POST':
            print("📝 POST Request - Parameters will be sent as JSON body")
            print(f"📦 JSON Body: {json.dumps(body, indent=2)}")
            response = requests.post(url, json=body, **request_kwargs)
        elif method.upper() == 'PUT':
            print("📝 PUT Request - Parameters will be sent as JSON body")
            print(f"📦 JSON Body: {json.dumps(body, indent=2)}")
            response = requests.put(url, json=body, **request_kwargs)
        elif method.upper() == 'DELETE':
            print("📝 DELETE Request - Parameters will be sent as query string")
            print(f"🔗 Full URL with params: {url}?{requests.compat.urlencode(body) if body else 'no-params'}")
            response = requests.delete(url, params=body, **request_kwargs)
        else:
            error_msg = f"Unsupported HTTP method: {method}"
            print(f"❌ ERROR: {error_msg}")
            return {
                "status": "error",
                "error": error_msg,
                "api_call": f"{method} {url}"
            }
        
        print(f"\n📡 RESPONSE RECEIVED:")
        print(f"📊 Status Code: {response.status_code}")
        print(f"📋 Response Headers: {dict(response.headers)}")
        print(f"⏱️ Response Time: {response.elapsed.total_seconds():.3f} seconds")
        print(f"📏 Response Size: {len(response.content)} bytes")
        
        # Parse response
        print(f"\n🔍 PARSING RESPONSE:")
        print(f"📄 Raw Response Text (first 500 chars): {response.text[:500]}...")
        
        try:
            response_data = response.json()
            print(f"✅ Successfully parsed JSON response")
            print(f"📊 JSON Keys: {list(response_data.keys()) if isinstance(response_data, dict) else 'Not a dict'}")
        except json.JSONDecodeError as e:
            print(f"⚠️ Failed to parse JSON: {e}")
            response_data = {"text": response.text}
            print(f"📝 Using raw text response instead")
        
        success = response.status_code < 400
        result = {
            "status": "success" if success else "error",
            "status_code": response.status_code,
            "api_call": f"{method} {url}",
            "parameters": body,
            "response": response_data,
            "headers": dict(response.headers)
        }
        
        print(f"\n📋 FINAL RESULT:")
        print(f"✅ Status: {result['status']}")
        print(f"📊 Status Code: {result['status_code']}")
        print(f"📦 Response Data Type: {type(result['response'])}")
        if isinstance(result['response'], dict) and 'data' in result['response']:
            data = result['response']['data']
            if isinstance(data, list):
                print(f"📊 Data Array Length: {len(data)}")
            else:
                print(f"📊 Data Type: {type(data)}")
        print("="*60)
        
        return result
        
    except requests.exceptions.Timeout as e:
        error_msg = f"Request timeout (200 seconds): {str(e)}"
        print(f"❌ TIMEOUT ERROR: {error_msg}")
        print("="*60)
        return {
            "status": "error",
            "error": error_msg,
            "api_call": f"{method} {url}",
            "parameters": body
        }
    except requests.exceptions.SSLError as e:
        error_msg = f"SSL Certificate error: {str(e)}"
        print(f"❌ SSL ERROR: {error_msg}")
        print(f"🔍 Common fixes for SSL errors:")
        print(f"   1. Backend server SSL certificate may be self-signed or invalid")
        print(f"   2. Already using verify=False to bypass SSL verification")
        print(f"   3. Check if server is running with proper SSL configuration")
        print(f"🌐 URL: {url}")
        print("="*60)
        return {
            "status": "error",
            "error": error_msg,
            "api_call": f"{method} {url}",
            "parameters": body
        }
    except requests.exceptions.ConnectionError as e:
        error_msg = f"Connection error - API server may be unavailable: {str(e)}"
        print(f"❌ CONNECTION ERROR: {error_msg}")
        print(f"🔍 Detailed diagnostics:")
        print(f"   📍 Target URL: {url}")
        print(f"   🔧 Method: {method}")
        print(f"   ⏱️ Timeout: 200 seconds")
        print(f"   🔐 SSL Verify: False (disabled for localhost)")
        
        # Parse URL to provide specific guidance
        from urllib.parse import urlparse
        parsed = urlparse(url)
        print(f"   🌐 Scheme: {parsed.scheme}")
        print(f"   🏠 Hostname: {parsed.hostname}")
        print(f"   🚪 Port: {parsed.port}")
        
        print(f"\n💡 Troubleshooting steps:")
        if parsed.hostname in ['localhost', '127.0.0.1']:
            print(f"   1. ✅ Localhost detected - Check if backend server is running")
            print(f"   2. 🚪 Verify port {parsed.port or ('443' if parsed.scheme == 'https' else '80')} is correct")
            print(f"   3. 🔄 Try starting the backend server")
        else:
            print(f"   1. 🌐 Remote server - Check network connectivity")
            print(f"   2. 🔥 Check firewall settings")
            print(f"   3. 🌍 Verify DNS resolution")
        
        print(f"   4. 🔍 Check if the API endpoint path is correct")
        print(f"   5. 📋 Verify the backend server logs for any startup errors")
        print("="*60)
        return {
            "status": "error", 
            "error": error_msg,
            "api_call": f"{method} {url}",
            "parameters": body,
            "troubleshooting": {
                "parsed_url": {
                    "scheme": parsed.scheme,
                    "hostname": parsed.hostname,
                    "port": parsed.port,
                    "path": parsed.path
                },
                "is_localhost": parsed.hostname in ['localhost', '127.0.0.1'],
                "suggestions": [
                    "Check if backend server is running",
                    f"Verify port {parsed.port or ('443' if parsed.scheme == 'https' else '80')}",
                    "Check firewall settings",
                    "Verify API endpoint path"
                ]
            }
        }
    except Exception as e:
        error_msg = f"Unexpected error: {str(e)}"
        print(f"❌ UNEXPECTED ERROR: {error_msg}")
        print(f"🔍 Exception Type: {type(e).__name__}")
        import traceback
        print(f"📋 Traceback: {traceback.format_exc()}")
        print("="*60)
        return {
            "status": "error",
            "error": error_msg,
            "api_call": f"{method} {url}",
            "parameters": body
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