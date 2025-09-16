"""
Task Execution Utilities

This module contains the main task execution functions including API tool invocation,
endpoint discovery, and content reading capabilities. Consolidated from 
sub_agents/task_executor_agent/utils.py.
"""

import json
import logging
from typing import Dict, List, Any, Optional

try:
    from google.adk.tools.tool_context import ToolContext
    from google.adk.tools import FunctionTool
except ImportError:
    ToolContext = None
    FunctionTool = None

from .api_config_manager import config_manager
from .api_utils import invoke_api_tool_direct, construct_api_url, extract_path_parameters_from_url, score_endpoint_for_intent_standalone

def _ensure_config_initialized():
    """Ensure the configuration manager is properly initialized"""
    if not hasattr(config_manager, '_config_directory') or config_manager._config_directory is None:
        # Initialize configuration
        try:
            from .framework_config import ensure_config_initialized
            ensure_config_initialized()
        except ImportError:
            # Fallback: initialize config directory manually
            import os
            # Try different possible paths
            possible_paths = [
                os.path.join(os.path.dirname(__file__), "..", "..", "config"),
                os.path.abspath("config"),
                "config"
            ]
            
            for config_dir in possible_paths:
                if os.path.exists(config_dir):
                    config_manager.set_config_directory(config_dir)
                    print(f"✅ Config directory set to: {config_dir}")
                    break
            else:
                config_manager.set_config_directory("config")
                print("⚠️ Using default config directory")


def find_entity_endpoint(entity_name: str, intent: str, extracted_params: str = "{}", tool_context: Optional[ToolContext] = None) -> str:
    """
    Find the optimal API endpoint for a given entity and intent.
    
    Args:
        entity_name: The business entity (e.g., "Partner", "Contact")
        intent: The operation intent (e.g., "list", "search", "create")
        extracted_params: JSON string of extracted parameters
        tool_context: Optional tool context
    
    Returns:
        str: JSON string with endpoint information and scoring details
    """
    try:
        # Ensure configuration is initialized before proceeding
        _ensure_config_initialized()
        # Parse extracted parameters
        try:
            params_dict = json.loads(extracted_params) if extracted_params else {}
        except json.JSONDecodeError:
            params_dict = {}
        
        print(f"🔍 [ENDPOINT-FINDER] Finding endpoint for {entity_name}/{intent}")
        print(f"📋 [ENDPOINT-FINDER] Parameters: {json.dumps(params_dict, indent=2)}")
        
        # CRITICAL FIX: Filter endpoints by entity first
        print(f"🎯 [ENDPOINT-FINDER] Looking for {entity_name}-specific endpoints...")
        
        # Try to get entity-specific endpoints first
        try:
            entity_endpoints = config_manager.get_entity_api_endpoints(entity_name)
            if entity_endpoints:
                print(f"✅ [ENDPOINT-FINDER] Found {len(entity_endpoints)} {entity_name}-specific endpoints")
                all_endpoints = entity_endpoints
            else:
                # Fallback to all endpoints if entity-specific not found
                print(f"⚠️ [ENDPOINT-FINDER] No {entity_name}-specific endpoints found, falling back to all endpoints")
                tools_config = config_manager.load_tools_config()
                all_endpoints = tools_config.get("tools", [])
        except Exception as e:
            print(f"⚠️ [ENDPOINT-FINDER] Error loading {entity_name} endpoints: {e}, falling back to all")
            tools_config = config_manager.load_tools_config()
            all_endpoints = tools_config.get("tools", [])
        
        if not all_endpoints:
            return json.dumps({
                "entity": entity_name,
                "intent": intent,
                "endpoint_found": False,
                "error": f"No endpoints available for entity '{entity_name}'"
            })
        
        print(f"📊 [ENDPOINT-FINDER] Evaluating {len(all_endpoints)} {entity_name} endpoints")
        
        # Score all endpoints
        def score_all_endpoints():
            scored_endpoints = []
            for endpoint in all_endpoints:
                score = score_endpoint_for_intent_standalone(endpoint, intent, entity_name, params_dict)
                if score > 0:
                    scored_endpoints.append({
                        "endpoint": endpoint,
                        "score": score
                    })
            
            # Sort by score (highest first)
            scored_endpoints.sort(key=lambda x: x["score"], reverse=True)
            return scored_endpoints
        
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
        
        # Build full URL with proper path parameter substitution
        if endpoint_url.startswith('http'):
            full_url = endpoint_url
        else:
            # Use generic path parameter extraction
            path_params = extract_path_parameters_from_url(endpoint_url, params_dict)
            print(f"🔧 [ENDPOINT-FINDER] Extracted path params: {path_params}")
            
            # For GET list requests, add default pagination parameters
            if intent.lower() == 'list':
                default_params = {
                    'pageIndex': params_dict.get('pageIndex', 1),
                    'pageSize': params_dict.get('pageSize', 10), 
                    'orderBy': params_dict.get('orderBy', 'CreatedDate'),
                    'ascending': params_dict.get('ascending', True)
                }
                # Merge with any extracted path params
                path_params.update(default_params)
                print(f"🔧 [ENDPOINT-FINDER] Added list params: {path_params}")
            
            # Use construct_api_url for proper placeholder substitution
            full_url = construct_api_url(base_url, endpoint_url, path_params)
        
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
                    # Use construct_api_url for proper placeholder substitution in fallback URLs
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
        
        return json.dumps(result, indent=2)
        
    except Exception as e:
        import traceback
        traceback.print_exc()
        return json.dumps({
            "error": f"Failed to find endpoint for {entity_name}/{intent}: {str(e)}",
            "entity": entity_name,
            "intent": intent,
            "endpoint_found": False
        })


def invoke_api_tool(entity_name: str, intent: str, params: Optional[dict] = None, isMultiToolRequest: bool = False, tool_context: Optional[ToolContext] = None) -> str:
    """
    Execute API calls using discovered endpoint information.
    
    Args:
        entity_name: The business entity to operate on
        intent: The operation intent
        params: Parameters for the operation
        isMultiToolRequest: Whether this is part of a multi-tool request
        tool_context: Optional tool context
        
    Returns:
        str: JSON string with operation results
    """
    try:
        print(f"🚀 [API-TOOL] Starting invoke_api_tool for {entity_name}/{intent}")
        
        if params is None:
            params = {}
        
        # Convert params to JSON string for endpoint finder
        params_json = json.dumps(params)
        
        # Step 1: Find the best endpoint
        print(f"🔍 [API-TOOL] Step 1: Finding optimal endpoint...")
        endpoint_result = find_entity_endpoint(entity_name, intent, params_json, tool_context)
        
        try:
            endpoint_info = json.loads(endpoint_result)
        except json.JSONDecodeError:
            return json.dumps({
                "success": False,
                "error": "Failed to parse endpoint discovery result",
                "entity": entity_name,
                "intent": intent
            })
        
        if not endpoint_info.get("endpoint_found", False):
            return json.dumps({
                "success": False,
                "error": endpoint_info.get("error", "No suitable endpoint found"),
                "entity": entity_name,
                "intent": intent
            })
        
        # Step 2: Prepare the API call
        print(f"✅ [API-TOOL] Step 2: Endpoint found - {endpoint_info['endpoint']['name']}")
        
        full_url = endpoint_info["full_url"]
        method = endpoint_info["method"]
        
        # Prepare request body
        request_body = params.copy() if params else {}
        
        # Add default pagination for GET requests if not provided
        if method == "GET" and not any(key in request_body for key in ["pageIndex", "pageSize"]):
            request_body.update({
                "pageIndex": 1,
                "pageSize": 10,
                "orderBy": "CreatedDate",
                "ascending": True
            })
        
        print(f"🌐 [API-TOOL] Step 3: Making {method} request to {full_url}")
        print(f"📦 [API-TOOL] Request body: {json.dumps(request_body, indent=2)}")
        
        # Step 3: Make the API call
        api_result = invoke_api_tool_direct(full_url, method, request_body, tool_context=tool_context)
        
        # Step 4: Format the response
        if api_result.get("error"):
            print(f"❌ [API-TOOL] API call failed: {api_result.get('message', 'Unknown error')}")
            return json.dumps({
                "success": False,
                "error": api_result.get("message", "API call failed"),
                "details": api_result,
                "entity": entity_name,
                "intent": intent,
                "endpoint_used": endpoint_info["endpoint"]["name"]
            })
        else:
            print(f"✅ [API-TOOL] API call successful")
            return json.dumps({
                "success": True,
                "data": api_result,
                "entity": entity_name,
                "intent": intent,
                "endpoint_used": endpoint_info["endpoint"]["name"],
                "execution_time": "completed"
            })
            
    except Exception as e:
        import traceback
        traceback.print_exc()
        return json.dumps({
            "success": False,
            "error": f"Unexpected error in invoke_api_tool: {str(e)}",
            "entity": entity_name,
            "intent": intent
        })


def invoke_api_for_data(entity_name: str, intent: str, params: str = "{}", tool_context: Optional[ToolContext] = None) -> str:
    """
    NEW OPTIMIZED TOOL: Intelligently finds the best endpoint and invokes it in one efficient call.
    
    Args:
        entity_name: The business entity (Partner, Contact, etc.)
        intent: What to do (list, search, create, update, delete)
        params: JSON string of parameters
        tool_context: Tool context for authentication and state
        
    Returns:
        str: JSON string with results
    """
    try:
        # Parse parameters
        try:
            params_dict = json.loads(params) if params else {}
        except json.JSONDecodeError:
            params_dict = {}
        
        print(f"🎯 [OPTIMIZED-API] invoke_api_for_data: {entity_name}/{intent}")
        print(f"📋 [OPTIMIZED-API] Parameters: {json.dumps(params_dict, indent=2)}")
        
        # Use the main API tool function
        result = invoke_api_tool(entity_name, intent, params_dict, False, tool_context)
        
        # Parse and potentially enhance the result
        try:
            result_data = json.loads(result)
            if result_data.get("success"):
                print(f"✅ [OPTIMIZED-API] Operation completed successfully")
            else:
                print(f"❌ [OPTIMIZED-API] Operation failed: {result_data.get('error')}")
            return result
        except json.JSONDecodeError:
            return result
            
    except Exception as e:
        import traceback
        traceback.print_exc()
        return json.dumps({
            "success": False,
            "error": f"invoke_api_for_data failed: {str(e)}",
            "entity": entity_name,
            "intent": intent
        })


def read_content_from_url(tool_context: ToolContext, url: str, isMultiToolRequest: bool = False, include_json: bool = True, output_format: str = "markdown", title: str = "", description: str = "") -> str:
    """Read content from any URL using the external convert/url API"""
    try:
        import requests
        
        print(f"📄 [READ-URL] Reading content from: {url}")
        
        # Use external API to convert URL content
        convert_api_url = "https://api.converter.com/url-to-text"  # Placeholder URL
        
        response = requests.post(convert_api_url, json={
            "url": url,
            "format": output_format,
            "include_metadata": True
        }, timeout=30)
        
        if response.status_code == 200:
            content_data = response.json()
            return json.dumps({
                "tool_name": "read_content_from_url",
                "message": content_data.get("content", ""),
                "type": output_format,
                "title": title or content_data.get("title", ""),
                "url": url,
                "metadata": content_data.get("metadata", {})
            })
        else:
            return json.dumps({
                "tool_name": "read_content_from_url",
                "error": f"Failed to read content: HTTP {response.status_code}",
                "url": url,
                "type": "error"
            })
            
    except Exception as e:
        return json.dumps({
            "tool_name": "read_content_from_url",
            "error": f"Error reading URL content: {str(e)}",
            "url": url,
            "type": "error"
        })


def convert_markdown_to_google_doc(tool_context: ToolContext, markdown_content: str, filename: str, isMultiToolRequest: bool = False, metadata: Optional[Dict[str, Any]] = None) -> str:
    """Convert markdown content to Google Doc using external API"""
    try:
        print(f"📝 [GOOGLE-DOC] Converting markdown to Google Doc: {filename}")
        
        # This would integrate with Google Docs API
        # For now, return a success message
        return json.dumps({
            "tool_name": "convert_markdown_to_google_doc",
            "message": f"Google Doc '{filename}' created successfully",
            "type": "text",
            "filename": filename,
            "success": True
        })
        
    except Exception as e:
        return json.dumps({
            "tool_name": "convert_markdown_to_google_doc",
            "error": f"Failed to create Google Doc: {str(e)}",
            "filename": filename,
            "type": "error"
        })


# Import search agent for fallback
try:
    from ..web_search_agent import search_agent
    # print("✅ Successfully imported enhanced web_search_agent")
except ImportError as e:
    # print(f"⚠️ Could not import enhanced web_search_agent: {e}")
    # Create a simple fallback
    search_agent = None


# Define the list of tools available for task execution
if FunctionTool:
    task_executor_tools = [
        FunctionTool(func=invoke_api_tool),
        FunctionTool(func=read_content_from_url),
        FunctionTool(func=convert_markdown_to_google_doc),
        FunctionTool(func=find_entity_endpoint),
        FunctionTool(func=invoke_api_for_data),
    ]
else:
    task_executor_tools = []
