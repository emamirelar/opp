"""
API Utilities for Task Execution

This module contains utilities for API endpoint discovery, parameter extraction,
and HTTP request execution. Consolidated from sub_agents/task_executor_agent/utils.py.
"""

import json
import logging
import os
import re
import requests
import traceback
from datetime import datetime
from typing import Dict, List, Any, Optional

try:
    from google.adk.tools.tool_context import ToolContext
except ImportError:
    ToolContext = None

from .api_config_manager import config_manager

# Initialize configuration if not already done
try:
    from .framework_config import ensure_config_initialized
    ensure_config_initialized()
except ImportError:
    # Fallback: initialize config directory manually
    import os
    config_dir = os.path.join(os.path.dirname(__file__), "..", "..", "config")
    if os.path.exists(config_dir):
        config_manager.set_config_directory(config_dir)
    else:
        config_manager.set_config_directory("config")


def invoke_api_tool_direct(url: str, method: str, body: dict, headers: Optional[dict] = None, tool_context: Optional['ToolContext'] = None) -> dict:
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
        
        # Prepare request headers - start with default headers
        request_headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
        
        # Handle development IAP headers
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
        else:
            # Try production IAP authentication
            try:
                from .auth_helpers import get_service_account_oidc_token
                
                # Try to get IAP audience from config
                try:
                    iap_audience = config_manager.get_iap_audience()
                    if iap_audience:
                        print(f"🔐 [API] Getting IAP token for audience: {iap_audience}")
                        oidc_token = get_service_account_oidc_token(iap_audience)
                        if oidc_token:
                            request_headers['X-Goog-IAP-JWT-Assertion'] = oidc_token
                            print("✅ [API] IAP token added to headers")
                        else:
                            print("⚠️ [API] Could not get IAP token")
                    else:
                        print("ℹ️ [API] No IAP audience configured - skipping IAP auth")
                except Exception as e:
                    print(f"⚠️ [API] IAP auth setup failed: {e}")
                    
            except ImportError:
                print("ℹ️ [API] Auth helpers not available - proceeding without IAP auth")
        
        # Add any additional headers passed as parameter
        if headers:
            print(f"🔐 Adding additional headers: {list(headers.keys())}")
            request_headers.update(headers)
        
        print("🔐 Final request headers being sent to backend:")
        print(f"📋 Total headers: {len(request_headers)}")
        print(f"📋 Header keys: {list(request_headers.keys())}")
        
        # Get API timeout from config (default to 30s if not available)
        api_timeout = 30  # Default timeout
        print(f"⏱️ [API] Using timeout: {api_timeout}s")
        
        print(f"📊 [API] Final request body: {json.dumps(body, indent=2)}")
        
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
                "error": True,
                "message": f"Unsupported HTTP method: {method}",
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


def construct_api_url(base_url: str, endpoint_path: str, path_params: Optional[Dict[str, Any]] = None) -> str:
    """
    Construct a complete API URL with path parameter substitution
    
    Args:
        base_url: Base API URL (e.g., https://localhost:44426)
        endpoint_path: Endpoint path with placeholders (e.g., /api/partners/{id})
        path_params: Dictionary of path parameters to substitute
    
    Returns:
        str: Complete URL with parameters substituted
    """
    url = base_url.rstrip('/') + '/' + endpoint_path.lstrip('/')
    
    if path_params:
        for key, value in path_params.items():
            placeholder = '{' + key + '}'
            if placeholder in url:
                url = url.replace(placeholder, str(value))
    
    return url


def extract_path_parameters_from_url(url: str, params_dict: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
    """
    Generic function to extract path parameters from URL and match them with available data.
    
    Args:
        url: URL with placeholders like /api/partners/{partnerId}/contacts
        params_dict: Dictionary of available parameters
    
    Returns:
        Dict with path parameters extracted and matched
    """
    path_params = {}
    
    if not params_dict:
        return path_params
    
    # Find all placeholders in the URL
    placeholders = re.findall(r'\{(\w+)\}', url)
    
    for placeholder in placeholders:
        # Try exact match first
        if placeholder in params_dict:
            path_params[placeholder] = params_dict[placeholder]
        else:
            # Try case-insensitive match
            for key, value in params_dict.items():
                if key.lower() == placeholder.lower():
                    path_params[placeholder] = value
                    break
            else:
                # Try partial matches (e.g., partnerId matches with id if it contains 'partner')
                placeholder_lower = placeholder.lower()
                for key, value in params_dict.items():
                    key_lower = key.lower()
                    if ('id' in placeholder_lower and 'id' in key_lower) or \
                       (placeholder_lower in key_lower) or (key_lower in placeholder_lower):
                        path_params[placeholder] = value
                        break
    
    return path_params


def score_endpoint_for_intent_standalone(endpoint: dict, intent: str, entity_name: str, extracted_params: Optional[dict] = None) -> int:
    """
    PERFECT SCORING ALGORITHM
    Score an endpoint based on how well it matches the intent and entity.
    
    Args:
        endpoint: Endpoint configuration dictionary
        intent: User intent (list, search, create, etc.)
        entity_name: Target entity name
        extracted_params: Optional parameters extracted from user query
    
    Returns:
        int: Score from 0-100 (higher is better)
    """
    try:
        if not endpoint:
            return 0
            
        endpoint_name = endpoint.get('name', '').lower()
        method = endpoint.get('method', 'GET').upper()
        url = endpoint.get('url', '').lower()
        description = endpoint.get('description', '').lower()
        when_to_use = endpoint.get('when_to_use', '').lower()
        parameters = endpoint.get('parameters', {})
        
        score = 0
        params_dict = {}
        
        if extracted_params:
            try:
                if isinstance(extracted_params, str):
                    params_dict = json.loads(extracted_params)
                elif isinstance(extracted_params, dict):
                    params_dict = extracted_params
            except (json.JSONDecodeError, TypeError):
                params_dict = {}
        
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
        
        # SMART SCORING: When we have search criteria, intelligently score endpoints based on their capabilities
        if extracted_params and ('query' in extracted_params or 'name' in extracted_params or 'searchText' in extracted_params):
            search_capability_score = 0
            
            # 1. Penalize endpoints that are clearly for listing all items (not searching)
            list_all_indicators = ['listall', 'getall', 'all']
            if any(indicator in endpoint_name.lower() for indicator in list_all_indicators):
                if 'search' not in endpoint_name.lower() and 'find' not in endpoint_name.lower():
                    search_capability_score -= 50
                    print(f"🔍 [ENDPOINT-SCORING] Penalized {endpoint_name} - list-all endpoint when searching")
            
            # 2. Analyze URL structure for search capabilities
            url_search_score = 0
            if '{searchtext}' in url.lower():
                url_search_score += 60  # Simple text search parameter - highly relevant
            elif 'search' in url.lower():
                url_search_score += 40  # Search in URL path
            elif any(param in url.lower() for param in ['{query}', '{name}', '{term}']):
                url_search_score += 50  # Other search parameters
            
            # 3. Analyze endpoint parameters for search sophistication
            param_search_score = 0
            if 'searchtext' in [p.lower() for p in parameters.keys()]:
                param_search_score += 50  # Simple text search parameter
            elif 'searchcriteria' in [p.lower() for p in parameters.keys()]:
                param_search_score += 30  # Advanced search (more complex, lower priority for simple searches)
            elif any(param in [p.lower() for p in parameters.keys()] for param in ['query', 'search', 'term', 'name']):
                param_search_score += 40  # Other search parameters
            
            # 4. Analyze description for search capabilities
            desc_search_score = 0
            if 'simple text search' in description.lower() or 'text search' in description.lower():
                desc_search_score += 40  # Perfect for text searches
            elif 'advanced search' in description.lower():
                desc_search_score += 20  # More complex than needed for simple searches
            elif 'search' in description.lower() or 'find' in description.lower():
                desc_search_score += 30  # General search capability
            
            # 5. Analyze when_to_use for search intent alignment
            when_search_score = 0
            when_to_use_text = when_to_use.lower()
            simple_search_phrases = ['simple', 'name', 'description', 'basic field', 'text search', 'keyword']
            complex_search_phrases = ['complex', 'criteria', 'advanced', 'structured', 'multiple field']
            
            if any(phrase in when_to_use_text for phrase in simple_search_phrases):
                when_search_score += 40  # Explicitly mentions simple search use cases
            elif any(phrase in when_to_use_text for phrase in complex_search_phrases):
                when_search_score += 20  # More complex than needed
            elif 'search' in when_to_use_text:
                when_search_score += 30  # General search mention
            
            # 6. Combine all search capability scores
            search_capability_score = url_search_score + param_search_score + desc_search_score + when_search_score
            score += search_capability_score
            
            if search_capability_score > 0:
                print(f"🔍 [ENDPOINT-SCORING] {endpoint_name} search score: URL({url_search_score}) + Params({param_search_score}) + Desc({desc_search_score}) + When({when_search_score}) = {search_capability_score}")
        
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
        
    except Exception as e:
        print(f"❌ [ENDPOINT-SCORING] Error scoring endpoint: {e}")
        return 0


# Import exit_loop_on_success function from common_callbacks
from .common_callbacks import exit_loop_on_success

# Define the list of tools available for task execution
try:
    from google.adk.tools import FunctionTool
    
    # This will be populated by importing modules that need these tools
    task_executor_tools = []
    
except ImportError:
    # Fallback if Google ADK tools are not available
    task_executor_tools = []
