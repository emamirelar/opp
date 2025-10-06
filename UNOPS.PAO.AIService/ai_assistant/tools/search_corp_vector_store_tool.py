import os
import json
import logging
import asyncio
import concurrent.futures
from typing import List, Optional, Dict, Any
from google.adk.tools.tool_context import ToolContext

from ..utils.config import get_config
from ..utils.auth_helpers import get_service_account_oidc_token

def search_corp_vector_store(query: str, applicationId: Optional[str] = None, entityTypeId: Optional[str] = None, entityId: Optional[str] = None, maxResults: Optional[int] = 10, tool_context: Optional[ToolContext] = None) -> str:
    """
    Search corporate vector store using external API
    The tool provides access to ALL corporate information including information regarding engagements, projects, policies, processes, legal agreements, etc.
    
    Args:
        query: Search query string
        applicationId: Optional application ID to filter search
        entityTypeId: Optional entity type ID to filter search
        entityId: Optional entity ID to filter search
        maxResults: Maximum number of results to return (default: 10)
                
    Returns:
        JSON string with the search results
    """

    # print(f"🔍 [VECTOR-SEARCH] Searching corporate vector store with query: {query[:50]}...")
    try:
        import requests
        
        # TODO: Get from config
        search_endpoint = "https://api.ai.unops.org/v1/tools/vector-store/search"
        
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
            
        # print(f"📋 [VECTOR-SEARCH] Request payload: {json.dumps(payload, indent=2)}")
        
        # Use the common utility to build request headers
        from ..utils.auth_helpers import build_request_headers
        request_headers = build_request_headers(
            tool_context=tool_context,
            url=search_endpoint
        )
                
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
                # print(f"✅ [VECTOR-SEARCH] Successfully searched corporate vector store")
                # print(f"📊 [VECTOR-SEARCH] Found {len(response_data.get('results', []))} results")
                
                return json.dumps({
                    "status": "success",
                    "response": response_data,
                    "query": query
                })
            except json.JSONDecodeError:
                return json.dumps({
                    "status": "success",
                    "response": {"text": response.text},
                    "query": query,
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
            })
        
    except Exception as e:
        logging.error(f"Error searching corporate vector store: {str(e)}", exc_info=True)
        return json.dumps({
            "error": f"Failed to search corporate vector store: {str(e)}",
            "query": query,
            "suggestion": "Check if the vector store service is available and the query is valid"
        }) 
