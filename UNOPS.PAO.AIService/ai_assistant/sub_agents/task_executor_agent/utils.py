"""
Task Executor Agent Utilities

This module contains utility functions, tools list, and callbacks for the task executor agent.
Self-contained implementation with all required functions moved here.
"""

import json
import logging
import asyncio
import os
from typing import Dict, List, Any, Optional

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

# --- Capabilities Management ---

def load_capabilities_config() -> Dict[str, Any]:
    """
    Load the capabilities configuration from the framework config.
    
    Returns:
        Dict containing the capabilities configuration
    """
    try:
        # Get the current directory and navigate to config
        current_dir = os.path.dirname(os.path.abspath(__file__))
        config_path = os.path.join(current_dir, "../../../config/framework/capabilities.json")
        
        with open(config_path, 'r') as f:
            capabilities = json.load(f)
            logger.info("✅ Loaded capabilities configuration")
            return capabilities
    except Exception as e:
        logger.warning(f"⚠️ Could not load capabilities config: {e}")
        return {
            "capabilities": {},
            "capability_check": {
                "google_doc_creation": {
                    "available": True,
                    "description": "I can create Google Documents"
                }
            }
        }

def check_capability(capability_name: str) -> str:
    """
    Check if a specific capability is available and return detailed information.
    
    Args:
        capability_name: Name of the capability to check
        
    Returns:
        JSON string with capability information
    """
    try:
        config = load_capabilities_config()
        capability_checks = config.get("capability_check", {})
        
        if capability_name in capability_checks:
            capability_info = capability_checks[capability_name]
            return json.dumps({
                "capability": capability_name,
                "available": capability_info.get("available", False),
                "confidence": capability_info.get("confidence", "unknown"),
                "description": capability_info.get("description", ""),
                "requirements": capability_info.get("requirements", []),
                "optional_parameters": capability_info.get("optional_parameters", []),
                "supported_formats": capability_info.get("supported_formats", []),
                "supported_entities": capability_info.get("supported_entities", [])
            })
        else:
            # Search in general capabilities
            capabilities = config.get("capabilities", {})
            for category, category_info in capabilities.items():
                if category_info.get("available", False):
                    tools = category_info.get("tools", {})
                    for tool_name, tool_info in tools.items():
                        if capability_name.lower() in tool_name.lower() or capability_name.lower() in tool_info.get("description", "").lower():
                            return json.dumps({
                                "capability": capability_name,
                                "available": True,
                                "category": category,
                                "tool": tool_name,
                                "description": tool_info.get("description", ""),
                                "capabilities": tool_info.get("capabilities", []),
                                "examples": tool_info.get("examples", [])
                            })
            
            return json.dumps({
                "capability": capability_name,
                "available": False,
                "message": f"Capability '{capability_name}' not found in available capabilities"
            })
            
    except Exception as e:
        logger.error(f"Error checking capability {capability_name}: {e}")
        return json.dumps({
            "capability": capability_name,
            "available": False,
            "error": str(e)
        })

def list_all_capabilities() -> str:
    """
    List all available capabilities with their descriptions.
    
    Returns:
        JSON string with all available capabilities
    """
    try:
        config = load_capabilities_config()
        capabilities = config.get("capabilities", {})
        capability_checks = config.get("capability_check", {})
        
        result = {
            "summary": {
                "total_categories": len(capabilities),
                "quick_checks": list(capability_checks.keys())
            },
            "categories": {}
        }
        
        for category, category_info in capabilities.items():
            if category_info.get("available", False):
                tools = category_info.get("tools", {})
                result["categories"][category] = {
                    "description": category_info.get("description", ""),
                    "available": category_info.get("available", False),
                    "tools": {
                        tool_name: {
                            "description": tool_info.get("description", ""),
                            "capabilities": tool_info.get("capabilities", []),
                            "examples": tool_info.get("examples", [])[:2]  # Limit examples for brevity
                        }
                        for tool_name, tool_info in tools.items()
                    }
                }
        
        result["quick_capability_checks"] = {
            name: {
                "available": info.get("available", False),
                "description": info.get("description", "")
            }
            for name, info in capability_checks.items()
        }
        
        return json.dumps(result, indent=2)
        
    except Exception as e:
        logger.error(f"Error listing capabilities: {e}")
        return json.dumps({
            "error": str(e),
            "message": "Could not load capabilities list"
        })

# Load capabilities on module import
CAPABILITIES_CONFIG = load_capabilities_config()


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
    logging.info("✅ Successfully imported real search_agent")
except ImportError as e:
    logging.warning(f"⚠️ Could not import real search_agent: {e}")

    
    # Fallback: provide stub implementation
    class SearchAgentStub:
        """Stub implementation for search agent"""
        def __init__(self):
            self.name = "search_agent"
            self.description = "External search agent for web and external information retrieval"
            self.instruction = "Search for external information using web search capabilities"
            self.tools = []  # Empty tools list
            self.model = None  # No model for stub
        
        def execute(self, query: str) -> dict:
            """Execute a search query - returns mock response for testing"""
            return {
                "content": f"Mock search results for query: {query}. External search functionality is available in the production environment.",
                "sources": [
                    {
                        "title": "Mock Search Result",
                        "url": "https://example.com/mock-result",
                        "description": "This is a mock search result for testing purposes"
                    }
                ],
                "message": "Search completed (mock response)"
            }

    search_agent = SearchAgentStub()
    logging.warning("⚠️ Using SearchAgentStub as fallback")


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
            
            # If this is a single-tool request, automatically exit the loop with error
            if not isMultiToolRequest:
                print(f"🏁 [ENHANCED-API-TOOL] Single-tool request (parse error), exiting loop")
                exit_loop_on_success(tool_context)
            
            return error_result
        
        if not endpoint_data.get("endpoint_found", False):
            print(f"❌ [ENHANCED-API-TOOL] No endpoint found for {entity_name}/{intent}")
            error_result = json.dumps({
                "error": f"No suitable endpoint found for entity '{entity_name}' with intent '{intent}'",
                "entity": entity_name,
                "intent": intent,
                "endpoint_finder_result": endpoint_data
            })
            
            # If this is a single-tool request, automatically exit the loop with error
            if not isMultiToolRequest:
                print(f"🏁 [ENHANCED-API-TOOL] Single-tool request (no endpoint), exiting loop")
                exit_loop_on_success(tool_context)
            
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
            
            # If this is a single-tool request, automatically exit the loop with error
            if not isMultiToolRequest:
                print(f"🏁 [ENHANCED-API-TOOL] Single-tool request (no URL), exiting loop")
                exit_loop_on_success(tool_context)
            
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
            
            # If this is a single-tool request, automatically exit the loop
            if not isMultiToolRequest:
                print(f"🏁 [ENHANCED-API-TOOL] Single-tool request complete, exiting loop")
                exit_loop_on_success(tool_context)
            
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
                        
                        # If this is a single-tool request, automatically exit the loop
                        if not isMultiToolRequest:
                            print(f"🏁 [ENHANCED-API-TOOL] Single-tool request complete (via fallback), exiting loop")
                            exit_loop_on_success(tool_context)
                        
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
            
            # If this is a single-tool request, automatically exit the loop with error
            if not isMultiToolRequest:
                print(f"🏁 [ENHANCED-API-TOOL] Single-tool request failed, exiting loop")
                exit_loop_on_success(tool_context)
            
            return error_result
        
    except Exception as e:
        print(f"❌ [ENHANCED-API-TOOL] Unexpected error in {entity_name}/{intent}: {e}")
        import traceback
        traceback.print_exc()
        error_result = json.dumps({
            "error": f"Unexpected error in enhanced API tool: {str(e)}",
            "entity": entity_name,
            "intent": intent
        })
        
        # If this is a single-tool request, automatically exit the loop with error
        if not isMultiToolRequest:
            print(f"🏁 [ENHANCED-API-TOOL] Single-tool request (unexpected error), exiting loop")
            exit_loop_on_success(tool_context)
        
        return error_result


def combined_before_model_callback(callback_context, llm_request=None):
    """Combined callback for entity-specific tools injection"""
    inject_entity_specific_tools_before_model(callback_context, llm_request)
    return None

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
        from ai_assistant.utils.api_config_manager import config_manager
        import json
        
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
        from ai_assistant.utils.common_callbacks import construct_api_url
        
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
        import traceback
        traceback.print_exc()
        return json.dumps({
            "error": f"Failed to find endpoint for {entity_name}/{intent}: {str(e)}",
            "entity": entity_name,
            "intent": intent,
            "endpoint_found": False
        })






def build_task_executor_tools():
    """
    Build the comprehensive tools list dynamically for the task executor agent.
    All tools are always available with fallback implementations.
    
    Returns:
        list: List of FunctionTool and AgentTool objects for the task executor
    """
    tools = [
        # Core API tools (always available)
        FunctionTool(func=invoke_api_tool), 
        FunctionTool(func=exit_loop_on_success)
    ]

    # Add External API tools (always available with stubs)
    tools.extend([
        FunctionTool(func=read_content_from_url),
        FunctionTool(func=convert_markdown_to_google_doc),
    ])
    logging.info("✅ Added External API tools to task executor (with fallback implementations)")

    # Note: Google Doc creation uses convert_markdown_to_google_doc (already added above)
    logging.info("✅ Google Doc creation available via convert_markdown_to_google_doc")

    # Add Speech-to-Text tools (always available with stubs)
    tools.extend([
        FunctionTool(func=transcribe_audio_from_message),
        FunctionTool(func=transcribe_audio_from_artifact),
        FunctionTool(func=transcribe_audio_long_running)
    ])
    logging.info("✅ Added Speech-to-Text tools to task executor (with fallback implementations)")


    # Add UI tools (always available with stubs)
    tools.extend([
        FunctionTool(func=get_ui_guidance_for_entity),
        FunctionTool(func=get_screen_help),
        FunctionTool(func=get_ui_buttons_for_entity),
        FunctionTool(func=get_ui_pages_for_entity),
        FunctionTool(func=get_available_ui_entities),
        FunctionTool(func=search_ui_by_keyword)
    ])
    logging.info("✅ Added UI guidance tools to task executor (with fallback implementations)")

    # Add capability checking tools (always available)
    tools.extend([
        FunctionTool(func=check_capability),
        FunctionTool(func=list_all_capabilities)
    ])
    logging.info("✅ Added capability checking tools to task executor")

    # Add search agent (if it's a proper ADK agent)
    try:
        # Check if search_agent is a proper ADK agent (has required attributes)
        if hasattr(search_agent, 'name') and hasattr(search_agent, 'description') and hasattr(search_agent, 'model'):
            tools.append(AgentTool.AgentTool(agent=search_agent))
            logging.info("✅ Added real search_agent to task executor")
        else:
            logging.warning("⚠️ search_agent is not a proper ADK agent, skipping AgentTool addition")
    except Exception as e:
        logging.error(f"❌ Failed to add search_agent to tools: {e}")


    logging.info(f"🔧 Built task executor tools list with {len(tools)} tools")
    return tools


# Build the tools list
task_executor_tools = build_task_executor_tools()

