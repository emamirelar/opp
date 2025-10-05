"""
API invocation utilities for the AI Assistant

This module provides utilities for making HTTP requests to application APIs,
including URL preparation and request execution with proper authentication.
"""

import json
import requests
import traceback
from typing import Optional
from google.adk.tools.tool_context import ToolContext


def prepare_api_url(url: str) -> str:
    """
    Prepare the final URL for API calls by ensuring the base URL comes from config manager.
    For absolute URLs, extracts the path and combines it with the config base URL.
    For relative URLs, combines directly with the config base URL.
    
    Args:
        url: URL to prepare - can be relative (e.g., /api/partner) or absolute (e.g., https://localhost:44426/api/partner)
    
    Returns:
        str: The final URL to use for the API call with base URL from config
    
    Examples:
        # Relative URL - will be combined with base URL from config
        final_url = prepare_api_url("/api/user")
        
        # Absolute URL - path will be extracted and combined with config base URL
        final_url = prepare_api_url("https://someother.com/api/user")
        # Result: https://config-base-url/api/user
    """
    try:
        from ..utils.config import get_api_base_url
        base_url = get_api_base_url()
        
        # Extract path from URL (works for both absolute and relative URLs)
        if url.startswith(('http://', 'https://')):
            # For absolute URLs, extract the path part
            from urllib.parse import urlparse
            parsed_url = urlparse(url)
            path = parsed_url.path
            # Include query string and fragment if present
            if parsed_url.query:
                path += '?' + parsed_url.query
            if parsed_url.fragment:
                path += '#' + parsed_url.fragment
        else:
            # For relative URLs, use as-is
            path = url
        
        # Ensure proper URL joining with the config base URL
        if base_url.endswith('/') and path.startswith('/'):
            return base_url + path[1:]
        elif not base_url.endswith('/') and not path.startswith('/'):
            return base_url + '/' + path
        else:
            return base_url + path
            
    except ImportError:
        # Config manager not available, return url as-is
        return url


def invoke_app_api(url: str, method: str, params: Optional[dict] = None, headers: Optional[dict] = None, tool_context: Optional[ToolContext] = None) -> dict:
    """
    Invoke an API endpoint with minimal logging
    
    Args:
        url: URL to call - can be relative (e.g., /api/partners) or absolute (e.g., https://localhost:44426/api/partner)
        method: HTTP method (GET, POST, PUT, DELETE)
        params: Request parameters/body
        headers: Optional additional headers
        tool_context: Tool context for authentication and state
    
    Returns:
        dict: API response or error information
    
    Examples:
        # GET request with relative URL (will use base URL from config)
        result = invoke_app_api("/api/user", "GET")
        
        # GET request with absolute URL (will use as-is)
        result = invoke_app_api("https://api.example.com/user", "GET")
        
        # POST request with data
        result = invoke_app_api(
            "/api/user", 
            "POST", 
            params={"name": "John", "email": "john@example.com"}
        )
        
        # GET request with query parameters
        result = invoke_app_api(
            "/api/user", 
            "GET", 
            params={"search": "john", "limit": 10}
        )
        
        # PUT request with custom headers
        result = invoke_app_api(
            "/api/user/123", 
            "PUT", 
            params={"name": "John Updated"},
            headers={"X-Custom-Header": "value"}
        )
    """
    
    try:
        # Prepare the final URL using the dedicated function
        final_url = prepare_api_url(url)
        
        # Use the common utility to build request headers
        from ..utils.auth_helpers import build_request_headers
        request_headers = build_request_headers(
            tool_context=tool_context,
            additional_headers=headers,
            url=url
        )

        # Get API timeout (default to 30 seconds if config not available)
        api_timeout = 30
        try:
            from ..utils.config import get_api_timeout
            api_timeout = get_api_timeout()
        except ImportError:
            pass
        
        # Prepare request body
        body = params or {}
        
        # Make the appropriate HTTP request based on method
        if method.upper() == 'GET':
            # Handle GET parameters properly
            if body:
                # For GET requests, convert body to query parameters
                query_params = '&'.join([f"{k}={v}" for k, v in body.items() if v is not None])
                if query_params:
                    separator = '&' if '?' in final_url else '?'
                    final_url += separator + query_params
            
            # Make GET request
            print(f"🌐 Making GET request to: {final_url}")
            
            try:
                response = requests.get(final_url, headers=request_headers, timeout=api_timeout, verify=False)
                print(f"📊 Response status: {response.status_code}")
                print(f"📊 Response headers: {dict(response.headers)}")
                print(f"📊 Content-Type: {response.headers.get('content-type', 'Not specified')}")
                print(f"📊 Response body (first 500 chars): {response.text[:500]}")
                if response.status_code != 200:
                    print(f"❌ GET request failed - Status: {response.status_code}, Error: {response.text[:200]}")
            except requests.exceptions.RequestException as e:
                print(f"❌ GET request failed with exception: {e}")
                raise
            
        elif method.upper() == 'POST':
            # Make POST request with JSON body
            print(f"🌐 Making POST request to: {final_url}")
            
            try:
                response = requests.post(final_url, json=body, headers=request_headers, timeout=api_timeout, verify=False)
                print(f"📊 Response status: {response.status_code}")
                print(f"📊 Response headers: {dict(response.headers)}")
                print(f"📊 Content-Type: {response.headers.get('content-type', 'Not specified')}")
                print(f"📊 Response body (first 500 chars): {response.text[:500]}")
    
                if response.status_code != 200:
                    print(f"❌ POST request failed - Status: {response.status_code}, Error: {response.text[:200]}")
            except requests.exceptions.RequestException as e:
                print(f"❌ POST request failed with exception: {e}")
                raise
            
        elif method.upper() == 'PUT':
            # Make PUT request with JSON body
            print(f"🌐 Making PUT request to: {final_url}")
            response = requests.put(final_url, json=body, headers=request_headers, timeout=api_timeout, verify=False)
            
        elif method.upper() == 'DELETE':
            # Make DELETE request
            print(f"🌐 Making DELETE request to: {final_url}")
            response = requests.delete(final_url, headers=request_headers, timeout=api_timeout, verify=False)
            
        else:
            return {
                "status": "error",
                "error": f"Unsupported HTTP method: {method}",
                "supported_methods": ["GET", "POST", "PUT", "DELETE"]
            }
        
        # Process response
        if response.status_code >= 200 and response.status_code < 300:
            try:
                response_data = response.json()
                
                return {
                    "status": "success",
                    "status_code": response.status_code,
                    "response": response_data,
                    "api_call": f"{method.upper()} {final_url}"
                    # "headers_sent": list(request_headers.keys())
                }
                
            except json.JSONDecodeError:
                return {
                    "status": "success",
                    "status_code": response.status_code,
                    "response": {"text": response.text},
                    "api_call": f"{method.upper()} {final_url}",
                    # "headers_sent": list(request_headers.keys()),
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
            
            return {
                "status": "error",
                "status_code": response.status_code,
                "error": error_message,
                "api_call": f"{method.upper()} {final_url}",
                "headers_sent": list(request_headers.keys())
            }
            
    except requests.exceptions.ConnectionError as conn_error:
        return {
            "status": "error",
            "error": f"Connection error: {str(conn_error)}",
            "api_call": f"{method.upper()} {final_url}",
            "connection_error": True
        }
        
    except requests.exceptions.Timeout as timeout_error:
        return {
            "status": "error",
            "error": f"Request timeout: {str(timeout_error)}",
            "api_call": f"{method.upper()} {final_url}",
            "timeout_error": True
        }
        
    except Exception as request_error:
        return {
            "status": "error",
            "error": f"Request failed: {str(request_error)}",
            "api_call": f"{method.upper()} {final_url}",
            "traceback": traceback.format_exc()
        }
