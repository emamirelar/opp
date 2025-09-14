"""
Task Executor Agent Utilities

This module contains utility functions, tools list, and callbacks for the task executor agent.
Self-contained implementation with all required functions moved here.
"""

import json
import logging
import asyncio
import os
import re
from datetime import datetime
from typing import Dict, List, Any, Optional

def score_endpoint_for_intent_standalone(endpoint: dict, intent: str, entity_name: str, extracted_params: Optional[dict] = None) -> int:
    """
    PERFECT SCORING ALGORITHM
    
    Intelligently scores endpoints based on:
    1. HTTP method compatibility (MANDATORY)
    2. Search capability analysis (for search intents)
    3. Intent-endpoint alignment
    4. Parameter compatibility
    5. Fallback ranking
    """
    
    # Extract endpoint metadata
    endpoint_name = endpoint.get('name', '')
    description = endpoint.get('description', '')
    url = endpoint.get('url', '')
    method = endpoint.get('method', 'GET').upper()
    when_to_use = endpoint.get('when_to_use', '')
    example_uses = endpoint.get('example_uses', [])
    parameters = endpoint.get('parameters', {})
    
    # Normalize for analysis
    name_lower = endpoint_name.lower()
    desc_lower = description.lower()
    url_lower = url.lower()
    when_lower = when_to_use.lower()
    
    print(f"🎯 [PERFECT-SCORING] Evaluating {endpoint_name} for {intent} on {entity_name}")
    
    # STEP 1: HTTP METHOD COMPATIBILITY (MANDATORY)
    intent_method_map = {
        'search': 'GET', 'find': 'GET', 'get': 'GET', 'list': 'GET', 'retrieve': 'GET',
        'create': 'POST', 'add': 'POST', 'new': 'POST', 'insert': 'POST',
        'update': 'PUT', 'modify': 'PUT', 'edit': 'PUT', 'change': 'PUT', 'patch': 'PATCH',
        'delete': 'DELETE', 'remove': 'DELETE', 'destroy': 'DELETE'
    }
    
    required_method = intent_method_map.get(intent.lower(), 'GET')
    if method != required_method:
        print(f"❌ [PERFECT-SCORING] {endpoint_name}: Wrong method {method} (need {required_method}) = 0")
        return 0
    
    # STEP 2: SEARCH INTENT ANALYSIS (CRITICAL FOR SEARCH QUERIES)
    score = 0
    has_search_params = extracted_params and any(key in extracted_params for key in ['query', 'name', 'searchText', 'search'])
    
    if intent.lower() in ['search', 'find'] and has_search_params:
        search_value = extracted_params.get('query') or extracted_params.get('name') or extracted_params.get('searchText') or extracted_params.get('search')
        
        print(f"🔍 [PERFECT-SCORING] Search query detected: '{search_value}'")
        
        # TIER 1: SIMPLE TEXT SEARCH ENDPOINTS (BEST FOR NAME SEARCHES)
        if '{searchtext}' in url_lower:
            score += 100  # Perfect match for simple text search
            print(f"✅ [PERFECT-SCORING] {endpoint_name}: TIER 1 - Simple text search URL parameter (+100)")
            
        # TIER 2: DEDICATED SEARCH ENDPOINTS
        elif 'search' in name_lower and 'advanced' not in name_lower:
            score += 80  # High priority for simple search endpoints
            print(f"✅ [PERFECT-SCORING] {endpoint_name}: TIER 2 - Dedicated simple search endpoint (+80)")
            
        # TIER 3: ADVANCED SEARCH ENDPOINTS (FALLBACK)
        elif 'advanced' in name_lower and 'search' in name_lower:
            score += 60  # Good fallback for complex queries
            print(f"⚡ [PERFECT-SCORING] {endpoint_name}: TIER 3 - Advanced search fallback (+60)")
            
        # TIER 4: GENERIC SEARCH ENDPOINTS
        elif 'search' in desc_lower or 'find' in desc_lower:
            score += 40  # General search capability
            print(f"🔄 [PERFECT-SCORING] {endpoint_name}: TIER 4 - Generic search capability (+40)")
            
        # PENALTY: LIST ALL ENDPOINTS (WRONG FOR SEARCH)
        elif any(indicator in name_lower for indicator in ['listall', 'getall', 'all']) and 'search' not in name_lower:
            score -= 100  # Heavy penalty for list-all when searching
            print(f"❌ [PERFECT-SCORING] {endpoint_name}: PENALTY - List-all endpoint for search query (-100)")
            
        # STEP 2.1: SEARCH CAPABILITY BONUSES
        capability_bonus = 0
        
        # URL structure analysis
        if 'search' in url_lower:
            capability_bonus += 20
        if any(param in url_lower for param in ['{query}', '{name}', '{term}']):
            capability_bonus += 25
            
        # Parameter analysis
        param_names_lower = [p.lower() for p in parameters.keys()]
        if 'searchtext' in param_names_lower:
            capability_bonus += 30  # Simple search parameter
        elif 'searchcriteria' in param_names_lower:
            capability_bonus += 15  # Advanced search (lower for simple queries)
            
        # Description analysis
        if 'simple text search' in desc_lower:
            capability_bonus += 25  # Perfect match
        elif 'text search' in desc_lower:
            capability_bonus += 20
        elif 'advanced search' in desc_lower:
            capability_bonus += 10  # Less ideal for simple searches
            
        # When-to-use analysis
        if any(phrase in when_lower for phrase in ['simple', 'name', 'description', 'basic']):
            capability_bonus += 20
        elif any(phrase in when_lower for phrase in ['complex', 'criteria', 'advanced']):
            capability_bonus += 5  # Less ideal
            
        score += capability_bonus
        if capability_bonus > 0:
            print(f"🔧 [PERFECT-SCORING] {endpoint_name}: Search capability bonus (+{capability_bonus})")
    
    # STEP 3: INTENT-ENDPOINT NAME MATCHING
    intent_lower = intent.lower()
    name_bonus = 0
    
    if intent_lower == name_lower:
        name_bonus = 50  # Perfect name match
    elif intent_lower in name_lower:
        name_bonus = 30  # Intent in name
    elif name_lower.startswith(intent_lower):
        name_bonus = 35  # Name starts with intent
        
    score += name_bonus
    if name_bonus > 0:
        print(f"📝 [PERFECT-SCORING] {endpoint_name}: Intent-name matching (+{name_bonus})")
    
    # STEP 4: ENTITY MATCHING
    entity_lower = entity_name.lower()
    entity_bonus = 0
    
    if entity_lower in name_lower:
        entity_bonus += 20
    if entity_lower in url_lower:
        entity_bonus += 10
    if entity_lower in desc_lower:
        entity_bonus += 5
        
    score += entity_bonus
    if entity_bonus > 0:
        print(f"🏢 [PERFECT-SCORING] {endpoint_name}: Entity matching (+{entity_bonus})")
    
    # STEP 5: EXAMPLE USES MATCHING
    example_bonus = 0
    for example in example_uses:
        example_lower = example.lower()
        if intent_lower in example_lower:
            example_bonus += 8
        if has_search_params:
            search_terms = ['find', 'search', 'look for', 'get details']
            if any(term in example_lower for term in search_terms):
                example_bonus += 10
                
    score += example_bonus
    if example_bonus > 0:
        print(f"📚 [PERFECT-SCORING] {endpoint_name}: Example matching (+{example_bonus})")
    
    # STEP 6: URL SOPHISTICATION BONUS
    url_bonus = 0
    if '{' in url and '}' in url:
        url_bonus += 5  # Parameterized URLs are more specific
    if url.count('/') >= 3:
        url_bonus += 3  # More specific paths
        
    score += url_bonus
    if url_bonus > 0:
        print(f"🔗 [PERFECT-SCORING] {endpoint_name}: URL sophistication (+{url_bonus})")
    
    final_score = max(0, score)  # Ensure non-negative
    print(f"🏆 [PERFECT-SCORING] {endpoint_name}: FINAL SCORE = {final_score}")
    
    return final_score

from google.adk.tools import FunctionTool, agent_tool as AgentTool
from google.adk.tools.tool_context import ToolContext

# Import shared utilities from common callbacks
from ai_assistant.utils.common_callbacks import (
    invoke_api_tool as base_invoke_api_tool, 
    exit_loop_on_success,
    inject_entity_specific_tools_before_model,
    construct_api_url
)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def extract_path_parameters_from_url(url: str, params_dict: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
    """
    Generic function to extract path parameters from URL and match them with available data.
    
    Args:
        url: URL with path parameters like /api/engagement/partner/by-name/{partnerName}
        params_dict: Dictionary containing potential parameter values
        
    Returns:
        Dictionary of path parameters that can be substituted in the URL
    """
    if not params_dict:
        return {}
    
    # Find all {parameter} patterns in the URL
    path_param_patterns = re.findall(r'\{([^}]+)\}', url)
    
    if not path_param_patterns:
        return {}
    
    path_params = {}
    
    for param_name in path_param_patterns:
        param_value = None
        
        # 1. Direct match in params_dict
        if param_name in params_dict:
            param_value = params_dict[param_name]
        
        # 2. Case variations (camelCase <-> snake_case)
        elif param_name.lower() in params_dict:
            param_value = params_dict[param_name.lower()]
        
        # 3. Snake case to camel case conversion
        else:
            # Convert camelCase to snake_case and check
            snake_case = re.sub(r'([A-Z])', r'_\1', param_name).lower()
            if snake_case in params_dict:
                param_value = params_dict[snake_case]
            
            # Convert snake_case to camelCase and check
            camel_case = re.sub(r'_([a-z])', lambda m: m.group(1).upper(), param_name.lower())
            if camel_case in params_dict:
                param_value = params_dict[camel_case]
        
        # 4. Search in filter structure
        if param_value is None and 'filter' in params_dict and isinstance(params_dict['filter'], list):
            for filter_item in params_dict['filter']:
                if isinstance(filter_item, dict):
                    field = filter_item.get('field', '')
                    value = filter_item.get('value', '')
                    
                    # Match field name to parameter name (with variations)
                    if (field == param_name.lower() or 
                        field == param_name or
                        field.replace('_', '') == param_name.lower().replace('_', '') or
                        field == re.sub(r'([A-Z])', r'_\1', param_name).lower()):
                        
                        param_value = value
                        break
        
        # 5. Common semantic mappings
        if param_value is None:
            semantic_mappings = {
                'partnerName': ['partner_name', 'partner.name', 'name'],
                'partnerId': ['partner_id', 'partner.id', 'id'],
                'contactId': ['contact_id', 'contact.id', 'id'],
                'entityId': ['entity_id', 'id'],
                'id': ['id', 'entity_id', 'partner_id', 'contact_id']
            }
            
            if param_name in semantic_mappings:
                for semantic_key in semantic_mappings[param_name]:
                    if semantic_key in params_dict:
                        param_value = params_dict[semantic_key]
                        break
                    
                    # Also check in filter structure
                    if param_value is None and 'filter' in params_dict:
                        for filter_item in params_dict.get('filter', []):
                            if isinstance(filter_item, dict) and filter_item.get('field') == semantic_key:
                                param_value = filter_item.get('value')
                                break
                    
                    if param_value:
                        break
        
        # Add to path_params if found
        if param_value is not None:
            path_params[param_name] = param_value
    
    return path_params



# External API tools - provide stub implementations


def read_content_from_url(tool_context: ToolContext, url: str, isMultiToolRequest: bool = False, include_json: bool = True, output_format: str = "markdown", title: str = "", description: str = "") -> str:
    """Read content from any URL using the external convert/url API"""
    try:
        from ai_assistant.tools import read_content_from_url as real_reader
        return real_reader(tool_context, url, include_json, output_format, title, description)
    except ImportError:
        return json.dumps({
            "error": "URL content reader service not available in current environment",
            "url": url,
            "endpoint": "https://api.ai.dev.unops.org/v1/convert/url"
        })

def convert_markdown_to_google_doc(tool_context: ToolContext, markdown_content: str, filename: str, isMultiToolRequest: bool = False, metadata: Optional[Dict[str, Any]] = None) -> str:
    """Convert markdown content to Google Doc using external API"""
    try:
        from ai_assistant.tools import convert_markdown_to_google_doc as real_converter
        return real_converter(tool_context, markdown_content, filename, metadata)
    except ImportError:
        return json.dumps({
            "error": "Markdown to Google Doc converter service not available in current environment",
            "filename": filename,
            "endpoint": "https://api.ai.dev.unops.org/v1/convert/markdown-to-google-doc"
        })

# Audio processing functions - provide stub implementations
def transcribe_audio_from_message(audio_file_id: str) -> str:
    """Transcribe audio from message attachment"""
    try:
        from ai_assistant.tools.audio_analysis_tools import transcribe_audio_from_message as real_transcribe
        return real_transcribe(audio_file_id)
    except ImportError:
        return json.dumps({
            "transcript": "",
            "message": "Audio transcription not available in current environment",
            "audio_file_id": audio_file_id
        })

def transcribe_audio_from_artifact(artifact_id: str) -> str:
    """Transcribe audio from artifact"""
    try:
        from ai_assistant.tools.audio_analysis_tools import transcribe_audio_from_artifact as real_transcribe
        return real_transcribe(artifact_id)
    except ImportError:
        return json.dumps({
            "transcript": "",
            "message": "Audio transcription not available in current environment",
            "artifact_id": artifact_id
        })

def transcribe_audio_long_running(audio_file_id: str) -> str:
    """Transcribe long audio file"""
    try:
        from ai_assistant.tools.audio_analysis_tools import transcribe_audio_long_running as real_transcribe
        return real_transcribe(audio_file_id)
    except ImportError:
        return json.dumps({
            "transcript": "",
            "message": "Long audio transcription not available in current environment",
            "audio_file_id": audio_file_id
        })

# UI guidance functions - provide stub implementations
def get_ui_guidance_for_entity(entity_name: str) -> str:
    """Get UI guidance for entity"""
    try:
        from ai_assistant.tools.ui_tools import get_ui_guidance_for_entity as real_guidance
        return real_guidance(entity_name)
    except ImportError:
        return json.dumps({
            "entity": entity_name,
            "guidance": f"UI guidance for {entity_name} not available in current environment",
            "available_actions": []
        })

def get_screen_help(entity_name: str, screen_type: str) -> str:
    """Get screen-specific help"""
    try:
        from ai_assistant.tools.ui_tools import get_screen_help as real_help
        return real_help(entity_name, screen_type)
    except ImportError:
        return json.dumps({
            "entity": entity_name,
            "screen_type": screen_type,
            "help_text": f"Screen help for {entity_name}/{screen_type} not available in current environment"
        })

def get_ui_buttons_for_entity(entity_name: str) -> str:
    """Get UI buttons for entity"""
    try:
        from ai_assistant.tools.ui_tools import get_ui_buttons_for_entity as real_buttons
        return real_buttons(entity_name)
    except ImportError:
        return json.dumps({
            "entity": entity_name,
            "buttons": [],
            "message": f"UI buttons for {entity_name} not available in current environment"
        })

def get_ui_pages_for_entity(entity_name: str) -> str:
    """Get UI pages for entity"""
    try:
        from ai_assistant.tools.ui_tools import get_ui_pages_for_entity as real_pages
        return real_pages(entity_name)
    except ImportError:
        return json.dumps({
            "entity": entity_name,
            "pages": [],
            "message": f"UI pages for {entity_name} not available in current environment"
        })

def get_available_ui_entities() -> str:
    """Get available UI entities"""
    try:
        from ai_assistant.tools.ui_tools import get_available_ui_entities as real_entities
        return real_entities()
    except ImportError:
        return json.dumps({
            "entities": [],
            "message": "UI entities not available in current environment"
        })

def search_ui_by_keyword(keyword: str) -> str:
    """Search UI by keyword"""
    try:
        from ai_assistant.tools.ui_tools import search_ui_by_keyword as real_search
        return real_search(keyword)
    except ImportError:
        return json.dumps({
            "results": [],
            "keyword": keyword,
            "message": "UI search not available in current environment"
        })


# Import the real search agent
try:
    from ai_assistant.sub_agents.search_agent.agent import search_agent
    logging.debug("Successfully imported search_agent")
except ImportError as e:
    logging.warning(f"⚠️ Could not import real search_agent: {e}")
    
    
    # Fallback: provide stub implementation that inherits from BaseAgent
    from google.adk.agents import LlmAgent
    from ai_assistant.utils.api_config_manager import config_manager
    
    def mock_search_function(query: str) -> str:
        """Mock search function for stub agent"""
        import json
        return json.dumps({
            "tool_name": "search_agent",
            "message": f"# Mock Search Results\n\nQuery: {query}\n\nExternal search functionality is available in the production environment.",
            "type": "markdown",
                "sources": [
                    {
                        "title": "Mock Search Result",
                    "url": "https://example.com/mock-result"
                }
            ]
        })
    
    from google.adk.tools import FunctionTool
    
    search_agent = LlmAgent(
        name="search_agent",
        model=config_manager.get_gemini_model(),
        description="External search agent for web and external information retrieval (stub)",
        instruction="You are a mock search agent. Return mock search results for testing purposes.",
        tools=[FunctionTool(func=mock_search_function)],
        output_key="search_results"
    )
    logging.warning("⚠️ Using SearchAgentStub as fallback")


# Enhanced invoke_api_tool that automatically finds endpoints
def invoke_api_tool(entity_name: str, intent: str, params: Optional[dict] = None, isMultiToolRequest: bool = False, tool_context: Optional[ToolContext] = None) -> str:
    """
    Execute API calls using discovered endpoint information.
    
    This tool now works in conjunction with find_entity_endpoint. The LLM should:
    1. First call find_entity_endpoint to discover available endpoints
    2. Then call this tool with the specific endpoint details and parameters
    
    CRITICAL: This tool ALWAYS returns valid JSON format, never plain text or markdown.
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Interaction")
        intent: The intent (e.g., "search", "create", "update", "delete")
        params: Parameters including endpoint details from find_entity_endpoint (optional)
        isMultiToolRequest: True if this is part of a multi-tool workflow (optional)
        tool_context: Tool context for session state (optional)
        
    Returns:
        ALWAYS returns a JSON string with the following structure:
        {
            "success": true/false,
            "data": {...actual API response data...},
            "entity": "entity_name",
            "intent": "intent_name",
            "error": "error message if any",
            "metadata": {...additional info...}
        }
    """
    try:
        # Check if endpoint details are provided in params (decoupled mode)
        endpoint_data = None
        if params and 'endpoint_details' in params:
            endpoint_data = params['endpoint_details']
        else:
            # Backward compatibility: find endpoint if not provided
            params_json = json.dumps(params) if params else "{}"
            endpoint_result = find_entity_endpoint(entity_name, intent, params_json)
            
            try:
                endpoint_data = json.loads(endpoint_result)
            except (json.JSONDecodeError, TypeError):
                error_result = {
                    "tool_name": "invoke_api_tool",
                    "type": "json",
                    "message": {
                        "error": f"Failed to parse endpoint finder result for {entity_name}/{intent}",
                        "raw_result": endpoint_result,
                        "suggestion": "Use find_entity_endpoint first to get endpoint details"
                    }
                }
                
                # Note: State appending now handled by after_tool_callback
                
                
                return json.dumps(error_result)
        
        if not endpoint_data.get("endpoint_found", False):
            error_result = {
                "tool_name": "invoke_api_tool",
                "type": "json",
                "message": {
                    "error": f"No suitable endpoint found for entity '{entity_name}' with intent '{intent}'",
                    "endpoint_finder_result": endpoint_data,
                    "suggestion": "Use find_entity_endpoint to discover available endpoints first"
                }
            }
            
            
            return json.dumps(error_result)
        
        # Step 2: Extract endpoint details
        full_url = endpoint_data.get("full_url")
        method = endpoint_data.get("method", "GET")
        endpoint_params = endpoint_data.get("parameters", {})
        
        if not full_url:
            error_result = {
                "tool_name": "invoke_api_tool",
                "type": "json",
                "message": {
                    "error": f"No URL found in endpoint data for {entity_name}/{intent}",
                    "endpoint_data": endpoint_data
                }
            }
            
            
            return json.dumps(error_result)
        
        
        # Step 3: Prepare the request body and handle search criteria
        request_body = params or {}
        
        # SMART URL CONSTRUCTION: Analyze endpoint structure to determine how to handle search parameters
        endpoint_name = endpoint_data.get("endpoint", {}).get("name", "")
        endpoint_description = endpoint_data.get("endpoint", {}).get("description", "").lower()
        endpoint_url = endpoint_data.get("endpoint", {}).get("url", "")
        endpoint_params = endpoint_data.get("endpoint", {}).get("parameters", {})
        
        # Extract search value from request
        search_value = None
        if 'query' in request_body:
            search_value = request_body.get('query')
        elif 'name' in request_body:
            search_value = request_body.get('name')
        elif 'searchText' in request_body:
            search_value = request_body.get('searchText')
        
        if search_value:
            # 1. Check if URL has path parameters that need replacement
            url_path_params = re.findall(r'\{([^}]+)\}', endpoint_url)
            search_param_replaced = False
            
            for param in url_path_params:
                param_lower = param.lower()
                # If URL has a search-related path parameter, replace it
                if param_lower in ['searchtext', 'query', 'search', 'term', 'name']:
                    full_url = full_url.replace(f"{{{param}}}", search_value)
                    search_param_replaced = True
                    print(f"🔍 [API-TOOL] Replaced URL path parameter {{{param}}} with: {search_value}")
                    # Remove from request body since it's now in URL path
                    request_body.pop('query', None)
                    request_body.pop('name', None)
                    request_body.pop('searchText', None)
                    break
            
            # 2. If no path parameter replacement, check if endpoint expects searchText parameter
            if not search_param_replaced:
                param_names_lower = [p.lower() for p in endpoint_params.keys()]
                
                if 'searchtext' in param_names_lower:
                    # Use searchText parameter
                    request_body['searchText'] = search_value
                    request_body.pop('query', None)
                    request_body.pop('name', None)
                    print(f"🔍 [API-TOOL] Using searchText parameter: {search_value}")
                
                # 3. Check if endpoint expects searchCriteria (advanced search)
                elif 'searchcriteria' in param_names_lower:
                    # Convert simple search to searchCriteria format
                    request_body['searchCriteria'] = [
                        {
                            "field": "name",
                            "operator": "like", 
                            "value": search_value
                        }
                    ]
                    # Remove the simple query/name params as they're now in searchCriteria
                    request_body.pop('query', None)
                    request_body.pop('name', None)
                    request_body.pop('searchText', None)
                    print(f"🔍 [API-TOOL] Converted to searchCriteria format: {request_body['searchCriteria']}")
                
                # 4. Fallback: keep as generic query parameter
                else:
                    # Keep the search parameter as-is for generic endpoints
                    if 'query' not in request_body:
                        request_body['query'] = search_value
                        request_body.pop('name', None)
                        request_body.pop('searchText', None)
                    print(f"🔍 [API-TOOL] Using generic query parameter: {search_value}")
        
        # For GET requests, move body parameters to query parameters (except searchCriteria)
        if method.upper() == "GET" and request_body:
            # searchCriteria should remain in body for POST-like search endpoints
            if 'searchCriteria' not in request_body:
                # Convert to query parameters for simple GET requests
                query_params = []
                for key, value in request_body.items():
                    if value is not None:
                        query_params.append(f"{key}={value}")
                
                if query_params:
                    separator = "&" if "?" in full_url else "?"
                    full_url = f"{full_url}{separator}{'&'.join(query_params)}"
                    request_body = {}  # Clear body for GET request
                    print(f"🔍 [API-TOOL] Converted to GET with query params: {full_url}")
            else:
                # Keep searchCriteria in body even for GET requests (some APIs support this)
                print(f"🔍 [API-TOOL] Keeping searchCriteria in body for search endpoint")
        
        # Step 4: Make the API call using the base invoke_api_tool
        try:
            api_result = base_invoke_api_tool(
                url=full_url,
                method=method,
                body=request_body,
                headers=None,
                tool_context=tool_context
            )
            
            
            # Create result in simple format
            simple_result = {
                "tool_name": "invoke_api_tool",
                "type": "json", 
                "message": api_result
            }
            
            
            return json.dumps(simple_result)
                
        except Exception as api_error:
            
            # Try fallback endpoints if available
            retry_info = endpoint_data.get("retry_info", {})
            fallback_endpoints = retry_info.get("fallback_endpoints", [])
            
            if fallback_endpoints:
                
                for i, fallback in enumerate(fallback_endpoints[:2], 1):  # Try up to 2 fallbacks
                    try:
                        fallback_url = fallback.get("full_url")
                        fallback_method = fallback.get("method", "GET")
                        
                        
                        fallback_result = base_invoke_api_tool(
                            url=fallback_url,
                            method=fallback_method,
                            body=request_body,
                            headers=None,
                            tool_context=tool_context
                        )
                        
                        
                        # Create result in simple format for fallback too
                        simple_result = {
                            "tool_name": "invoke_api_tool",
                            "type": "json",
                            "message": fallback_result
                        }
                        
                        
                        return json.dumps(simple_result)
                            
                    except Exception as fallback_error:
                        continue
            
            # All attempts failed - return simple error format
            error_result = {
                "tool_name": "invoke_api_tool",
                "type": "json",
                "message": {
                    "error": f"All API attempts failed for {entity_name}/{intent}",
                    "primary_error": str(api_error),
                    "fallbacks_attempted": len(fallback_endpoints)
                }
            }
            
            
            return json.dumps(error_result)
        
    except Exception as e:
        import traceback
        traceback.print_exc()
        error_result = {
            "tool_name": "invoke_api_tool",
            "type": "json",
            "message": {
                "error": f"Unexpected error in enhanced API tool: {str(e)}"
            }
        }
        
        
        return json.dumps(error_result)


def find_and_invoke_api_tool(entity_name: str, intent: str, params: str, tool_context: Optional[ToolContext] = None) -> str:
    """
    Combined tool that finds the best endpoint and invokes it in one call.
    This saves multiple tool calls and reduces latency by 2-3 seconds.
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Document")
        intent: The intent (e.g., "search", "create", "update", "delete")  
        params: JSON string of parameters (e.g., '{"query": "ACME"}')
        tool_context: Tool context for session state (optional)
        
    Returns:
        JSON string containing API response in consistent format
    """
    try:
        print(f"🚀 [FIND-AND-INVOKE] Starting combined endpoint finding and API invocation")
        print(f"🚀 [FIND-AND-INVOKE] Entity: {entity_name}, Intent: {intent}, Params: {params}")
        
        # Step 1: Find the best endpoint
        endpoint_result = find_entity_endpoint(entity_name, intent, params, tool_context)
        endpoint_data = json.loads(endpoint_result)
        
        if not endpoint_data.get("endpoint_found"):
            return json.dumps({
                "tool_name": "find_and_invoke_api_tool",
                "message": f"No suitable endpoint found for {entity_name} with intent '{intent}': {endpoint_data.get('error', 'Unknown error')}",
                "type": "json",
                "sources": []
            })
        
        print(f"🎯 [FIND-AND-INVOKE] Selected endpoint: {endpoint_data.get('endpoint', {}).get('name', 'Unknown')}")
        print(f"🎯 [FIND-AND-INVOKE] Score: {endpoint_data.get('score', 'N/A')}")
        
        # Step 2: Parse params for invoke_api_tool
        try:
            params_dict = json.loads(params) if params and params != "{}" else {}
        except (json.JSONDecodeError, TypeError):
            params_dict = {}
        
        # Step 3: Invoke the selected endpoint
        api_result = invoke_api_tool(entity_name, intent, params_dict, False, tool_context)
        api_data = json.loads(api_result)
        
        # Step 4: Enhance the response with endpoint selection details
        enhanced_response = {
            "tool_name": "find_and_invoke_api_tool",
            "message": api_data.get("message", ""),
            "type": api_data.get("type", "json"),
            "sources": api_data.get("sources", []),
            "endpoint_details": {
                "selected_endpoint": endpoint_data.get("endpoint", {}).get("name", "Unknown"),
                "endpoint_url": endpoint_data.get("full_url", ""),
                "method": endpoint_data.get("method", "GET"),
                "score": endpoint_data.get("score", 0),
                "fallback_available": endpoint_data.get("retry_info", {}).get("has_fallbacks", False)
            }
        }
        
        print(f"✅ [FIND-AND-INVOKE] Successfully completed combined operation")
        return json.dumps(enhanced_response)
        
    except Exception as e:
        error_msg = f"Error in combined find and invoke operation: {str(e)}"
        print(f"❌ [FIND-AND-INVOKE] {error_msg}")
        import traceback
        traceback.print_exc()
        return json.dumps({
            "tool_name": "find_and_invoke_api_tool",
            "message": error_msg,
            "type": "json",
            "sources": []
        })


def combined_before_model_callback(callback_context, llm_request=None):
    """Combined callback for entity-specific tools injection"""
    inject_entity_specific_tools_before_model(callback_context, llm_request)
    return None

def find_entity_endpoint(entity_name: str, intent: str, extracted_params: str = "{}", tool_context: Optional[ToolContext] = None) -> str:
    """
    Find the optimal API endpoint for a given entity and intent.
    
    This is now a standalone tool that the LLM can call to discover available endpoints
    before deciding which one to use with invoke_api_tool.
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Document")
        intent: The intent (e.g., "search", "create", "update", "delete")  
        extracted_params: JSON string of parameters (e.g., '{"query": "ACME"}')
        tool_context: Tool context for session state (optional)
        
    Returns:
        JSON string containing endpoint details and available options
    """
    try:
        from ai_assistant.utils.api_config_manager import config_manager
        import json
        
        # Parse extracted parameters
        try:
            params_dict = json.loads(extracted_params) if extracted_params and extracted_params != "{}" else None
        except (json.JSONDecodeError, TypeError):
            params_dict = None
        
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
        from ai_assistant.utils.common_callbacks import construct_api_url
        
        if endpoint_url.startswith('http'):
            full_url = endpoint_url
        else:
            # Use generic path parameter extraction
            path_params = extract_path_parameters_from_url(endpoint_url, params_dict)
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

