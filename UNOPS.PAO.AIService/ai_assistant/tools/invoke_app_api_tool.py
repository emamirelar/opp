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


def _merge_with_stored_proposal(params: Optional[dict], tool_context: Optional[ToolContext]) -> dict:
    """
    Merge params with stored proposal data for create-from-proposal calls.
    This ensures that extracted data from generate-proposal is not lost.
    """
    params = params or {}
    
    if not tool_context:
        return params
    
    # Try to get stored proposal from tool context state
    stored_proposal = None
    if hasattr(tool_context, 'state') and tool_context.state:
        stored_proposal = tool_context.state.get('last_generated_proposal')
    
    if not stored_proposal:
        print("⚠️ No stored proposal found in tool context - using params as-is")
        return params
    
    print(f"📋 Found stored proposal with keys: {list(stored_proposal.keys())}")
    
    # Fields that should be merged from stored proposal if missing/empty in params
    proposal_fields = [
        'deliverables', 'countries', 'sdGs', 'fundingPartners', 'clientPartners', 
        'stakeholders', 'targetSigningDate', 'targetDeliveryDate', 'initiativeBudgetUSD',
        'responsibleOrgUnitId', 'proposedInitiativeTypeId', 'strategicAlignment',
        'resultsFocus', 'expectedImpact', 'expectedOutcomes', 'expectedBeneficiaries', 
        'challenges', 'deliveryModality', 'partnerReference'
    ]
    
    merged_count = 0
    for field in proposal_fields:
        # Check if param is missing or empty (None, [], {}, '')
        param_value = params.get(field)
        proposal_value = stored_proposal.get(field)
        
        is_param_empty = (
            param_value is None or 
            param_value == [] or 
            param_value == {} or 
            param_value == ''
        )
        
        is_proposal_has_value = (
            proposal_value is not None and 
            proposal_value != [] and 
            proposal_value != {} and 
            proposal_value != ''
        )
        
        if is_param_empty and is_proposal_has_value:
            params[field] = proposal_value
            merged_count += 1
            print(f"  ✅ Merged '{field}' from stored proposal")
    
    if merged_count > 0:
        print(f"📋 Merged {merged_count} fields from stored proposal into create-from-proposal request")
    else:
        print("📋 No fields needed merging from stored proposal")
    
    return params


def _store_proposal_from_response(response_data: dict, tool_context: Optional[ToolContext]):
    """
    Store proposal data from generate-proposal response for later use in create-from-proposal.
    The response_data here is the RAW API response (before our wrapper is added).
    """
    if not tool_context:
        print("⚠️ No tool_context available to store proposal")
        return
    
    # Ensure state exists
    if not hasattr(tool_context, 'state') or tool_context.state is None:
        tool_context.state = {}
    
    # The response_data is the RAW API response from generate-proposal
    # It should contain the proposal fields directly (name, description, deliverables, etc.)
    proposal = response_data
    
    # Check if proposal is nested (some APIs wrap the response)
    if isinstance(response_data, dict):
        if 'proposal' in response_data:
            proposal = response_data.get('proposal', {})
        elif 'data' in response_data:
            proposal = response_data.get('data', {})
        # Otherwise use response_data directly as the proposal
    
    if proposal and isinstance(proposal, dict) and len(proposal) > 0:
        tool_context.state['last_generated_proposal'] = proposal
        print(f"💾 Stored proposal with {len(proposal)} fields for later use in create-from-proposal")
        # Log meaningful fields that were extracted
        meaningful_fields = ['name', 'description', 'deliverables', 'countries', 'sdGs', 'fundingPartners', 'clientPartners']
        found_fields = [f for f in meaningful_fields if proposal.get(f)]
        print(f"💾 Proposal contains: {found_fields}")
    else:
        print(f"⚠️ Could not extract valid proposal from response: {type(proposal)}")


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
            final_url = base_url + path[1:]
        elif not base_url.endswith('/') and not path.startswith('/'):
            final_url = base_url + '/' + path
        else:
            final_url = base_url + path
        
        # Defensive check: Remove duplicate /api/ patterns
        while '/api/api/' in final_url:
            final_url = final_url.replace('/api/api/', '/api/')
        
        return final_url
            
    except ImportError:
        # Config manager not available, return url as-is
        return url


def invoke_app_api(url: str, method: str, params: Optional[dict] = None, headers: Optional[dict] = None, tool_context: Optional[ToolContext] = None) -> dict:
    """
    Invoke an API endpoint with minimal logging
    
    Args:
        url: URL to call - can be relative (e.g., /api/partner) or absolute (e.g., https://localhost:44426/api/partner)
        method: HTTP method (GET, POST, PUT, DELETE)
        params: Request parameters/body
        headers: Optional additional headers
        tool_context: Tool context for authentication and state
    
    Returns:
        dict: API response or error information
    """
    
    try:
        # Prepare the final URL using the dedicated function
        final_url = prepare_api_url(url)
        
        # WORKFLOW SUPPORT: For create-from-proposal, merge with stored proposal if params are mostly empty
        if 'create-from-proposal' in url.lower() and method.upper() == 'POST':
            params = _merge_with_stored_proposal(params, tool_context)
            print(f"📋 After merge with stored proposal, params keys: {list(params.keys()) if params else 'None'}")
        
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
            print(f"📤 REQUEST BODY BEING SENT:")
            print(f"📤 {json.dumps(body, indent=2, default=str)}")
            
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
                
                # WORKFLOW SUPPORT: Store proposal data from generate-proposal for later use
                if 'generate-proposal' in url.lower() and method.upper() == 'POST':
                    _store_proposal_from_response(response_data, tool_context)
                
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
