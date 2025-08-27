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
    invoke_api_tool, 
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

# Define lightweight wrapper classes locally to avoid import issues
class GoogleSheetToolWrapper:
    """Lightweight wrapper for Google Sheets functionality"""
    def __init__(self):
        self.available = True
    
    async def create_spreadsheet_from_list(self, tool_context, title: str, data, folder_id: str = ""):
        """Create spreadsheet from list data"""
        try:
            # Use the local function
            return create_google_sheet_from_list_data(title, json.dumps(data), folder_id)
        except Exception as e:
            return json.dumps({"error": f"Failed to create spreadsheet: {str(e)}"})
    
    async def create_spreadsheet_with_headers(self, tool_context, title: str, headers, data, folder_id: str = ""):
        """Create spreadsheet with headers"""
        try:
            # Use the local function
            return create_google_sheet_with_headers_data(title, json.dumps(headers), json.dumps(data), folder_id)
        except Exception as e:
            return json.dumps({"error": f"Failed to create spreadsheet with headers: {str(e)}"})




# Initialize tool wrappers (always available)
google_sheet_wrapper = GoogleSheetToolWrapper()
logging.info(f"✅ Google Sheet wrapper initialized in task executor (available: {google_sheet_wrapper.available})")

# External API tools - provide stub implementations

def search_external_drive_service(tool_context: ToolContext, query: str, external_endpoint_url: str, auth_headers: Optional[Dict[str, str]] = None) -> str:
    """Search using external service for file IDs, then read content from Google Drive"""
    try:
        from ai_assistant.tools import search_external_drive_service as real_search
        return real_search(tool_context, query, external_endpoint_url, auth_headers)
    except ImportError:
        return json.dumps({
            "error": "External drive search service not available in current environment",
            "query": query,
            "endpoint": external_endpoint_url
        })

def search_unops_google_drive(tool_context: ToolContext, query: str, auth_headers: Optional[Dict[str, str]] = None) -> str:
    """Convenience function for UNOPS external Google Drive search service"""
    try:
        from ai_assistant.tools import search_unops_google_drive as real_search
        return real_search(tool_context, query, auth_headers)
    except ImportError:
        return json.dumps({
            "error": "UNOPS external drive search service not available in current environment",
            "query": query,
            "endpoint": "https://api.ai.dev.unops.org/v1/tools/google-drive/search"
        })

def read_content_from_url(tool_context: ToolContext, url: str, include_json: bool = True, output_format: str = "markdown", title: str = "", description: str = "") -> str:
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

def convert_markdown_to_google_doc(tool_context: ToolContext, markdown_content: str, filename: str, metadata: Optional[Dict[str, Any]] = None) -> str:
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

# Cache management functions - provide stub implementations
def get_cache_stats() -> str:
    """Get cache statistics"""
    try:
        from ai_assistant.utils.common_callbacks import get_cache_performance_stats
        return get_cache_performance_stats()
    except ImportError:
        return json.dumps({
            "cache_stats": {},
            "message": "Cache statistics not available in current environment"
        })

def clear_cache() -> str:
    """Clear cache"""
    try:
        from ai_assistant.utils.cache import entity_cache
        entity_cache.clear()
        return json.dumps({"message": "Cache cleared successfully"})
    except ImportError:
        return json.dumps({
            "message": "Cache clearing not available in current environment"
        })

def refresh_cache() -> str:
    """Refresh cache"""
    try:
        from ai_assistant.utils.common_callbacks import force_cache_refresh
        return force_cache_refresh()
    except ImportError:
        return json.dumps({
            "message": "Cache refresh not available in current environment"
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


def combined_before_model_callback(callback_context, llm_request=None):
    """Combined callback for entity-specific tools injection"""
    inject_entity_specific_tools_before_model(callback_context, llm_request)
    return None


# Task Executor-specific function definitions
def create_google_doc_from_text_data(title: str, content: str, folder_id: str = "") -> str:
    """Create a Google Doc from text data - simplified interface for task executor"""
    # Tool is currently not functional - return silent response
    logger.info(f"ℹ️ Google Doc creation requested for '{title}' but tool is currently not functional")
    return json.dumps({
        "message": "Google Docs creation tool is currently not functional. The request has been acknowledged but no document was created.",
        "title": title,
        "status": "tool_not_functional"
    })


def create_google_sheet_from_list_data(title: str, data: str, folder_id: str = "") -> str:
    """Create a Google Sheet from JSON list data - simplified interface for task executor"""
    # Tool is currently not functional - return silent response
    logger.info(f"ℹ️ Google Sheet creation requested for '{title}' but tool is currently not functional")
    return json.dumps({
        "message": "Google Sheets creation tool is currently not functional. The request has been acknowledged but no spreadsheet was created.",
        "title": title,
        "status": "tool_not_functional"
    })


def create_google_sheet_with_headers_data(title: str, headers: str, data: str, folder_id: str = "") -> str:
    """Create a Google Sheet with headers and data - simplified interface for task executor"""
    # Tool is currently not functional - return silent response
    logger.info(f"ℹ️ Google Sheet with headers creation requested for '{title}' but tool is currently not functional")
    return json.dumps({
        "message": "Google Sheets creation tool is currently not functional. The request has been acknowledged but no spreadsheet was created.",
        "title": title,
        "status": "tool_not_functional"
    })


def get_entity_api_tools_config(entity_name: str) -> str:
    """
    Get entity-specific tools configuration from config_manager
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Global")
        
    Returns:
        JSON string containing the entity-specific tools configuration
    """
    try:
        from ai_assistant.utils.api_config_manager import config_manager
        
        # Load entity-specific configuration
        entity_config = config_manager.load_entity_api_config(entity_name)
        
        # Get formatted summary for the task executor
        tools_summary = config_manager.get_entity_api_tools(entity_name)
        
        # Return both the raw config and formatted summary
        result = {
            "entity": entity_name,
            "tools_summary": tools_summary,
            "raw_config": entity_config,
            "base_url": config_manager.get_api_base_url(),
            "available_entities": config_manager.get_available_entities()
        }
        
        return json.dumps(result, indent=2)
        
    except Exception as e:
        logging.error(f"Error loading entity tools config for {entity_name}: {e}")
        return json.dumps({
            "error": f"Failed to load entity tools config for {entity_name}: {str(e)}",
            "entity": entity_name
        })


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
        
        def score_endpoint_for_intent_standalone(endpoint: dict, intent: str, entity_name: str, extracted_params: dict = None) -> int:
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
        full_url = f"{base_url}{best_endpoint.get('url', '')}" if not best_endpoint.get('url', '').startswith('http') else best_endpoint.get('url', '')
        
        # Prepare retry information for the agent
        fallback_endpoints = []
        if len(ranked_candidates) > 1:
            for i, candidate in enumerate(ranked_candidates[1:3], 1):  # Next 2 best alternatives
                ep = candidate['endpoint']
                fallback_url = f"{base_url}{ep.get('url', '')}" if not ep.get('url', '').startswith('http') else ep.get('url', '')
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


def get_entity_search_metadata(entity_name: str) -> str:
    """
    Get search metadata for an entity including available fields, operators, and examples
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Interaction")
        
    Returns:
        JSON string containing search metadata for advanced search queries
    """
    try:
        from ai_assistant.utils.api_config_manager import config_manager
        
        # Get search metadata (with graceful fallback)
        search_metadata = config_manager.get_entity_search_metadata(entity_name)
        
        if not search_metadata:
            print(f"ℹ️ No searchMetadata for {entity_name} - this is normal for entities without advanced search")
            return json.dumps({
                "entity": entity_name,
                "searchMetadata": {},
                "message": f"No advanced search metadata available for entity '{entity_name}'. This entity may only support basic text search.",
                "guidance": {
                    "recommended_approach": "Use simple text search (searchText parameter)",
                    "basic_operators": ["like", "is", "not", "contains", "startsWith", "endsWith"],
                    "note": "Advanced nested field searches may not be supported"
                }
            })
        
        # Format the response
        result = {
            "entity": entity_name,
            "searchMetadata": search_metadata,
            "guidance": {
                "use_advanced_search_when": [
                    "Searching related entities (e.g., 'interactions with UNICEF partners')",
                    "Field-specific searches (e.g., 'partners where status is Active')",
                    "Multiple criteria with operators"
                ],
                "use_simple_text_when": [
                    "Simple keyword searches (e.g., 'project', 'meeting notes')",
                    "General text searches across fields",
                    "Single search terms"
                ],
                "mandatory_requirements": [
                    "searchCriteria is REQUIRED when advancedSearch=true",
                    "Each criteria object MUST include a 'description' field",
                    "Use only fields from directFields and nestedFields",
                    "Use only operators from the operators list"
                ]
            }
        }
        
        return json.dumps(result, indent=2)
        
    except Exception as e:
        logging.error(f"Error getting search metadata for {entity_name}: {e}")
        return json.dumps({
            "entity": entity_name,
            "error": f"Failed to get search metadata for {entity_name}: {str(e)}"
        })


def get_entity_search_examples(entity_name: str) -> str:
    """
    Get example search criteria for an entity with proper formatting
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Interaction")
        
    Returns:
        JSON string containing formatted search examples
    """
    try:
        from ai_assistant.utils.api_config_manager import config_manager
        
        # Get search examples (with graceful fallback)
        examples = config_manager.get_entity_search_examples(entity_name)
        fields = config_manager.get_entity_search_fields(entity_name)
        operators = config_manager.get_entity_search_operators(entity_name)
        
        # Check if we have any meaningful data
        has_search_metadata = (examples or 
                             fields.get("directFields") or 
                             fields.get("nestedFields") or 
                             len(operators) > 6)  # More than basic fallback operators
        
        if has_search_metadata:
            result = {
                "entity": entity_name,
                "exampleCriteria": examples,
                "availableFields": fields,
                "availableOperators": operators,
                "hasAdvancedSearch": True,
                "formattingGuide": {
                    "simple_example": {
                        "field": "name",
                        "operator": "like",
                        "value": "UNICEF",
                        "description": "Find entities with UNICEF in name"
                    },
                    "multiple_criteria_example": [
                        {
                            "field": "partner.name",
                            "operator": "like",
                            "value": "UNICEF",
                            "description": "Find entities related to UNICEF partners"
                        },
                        {
                            "field": "status",
                            "operator": "is",
                            "value": "Active",
                            "logicalOperator": "AND",
                            "description": "Must be active status"
                        }
                    ]
                }
            }
        else:
            # Provide basic guidance when no advanced search metadata is available
            print(f"ℹ️ No advanced search metadata for {entity_name} - providing basic search guidance")
            result = {
                "entity": entity_name,
                "exampleCriteria": [],
                "availableFields": {"directFields": [], "nestedFields": {}},
                "availableOperators": operators,  # Basic fallback operators
                "hasAdvancedSearch": False,
                "message": f"No advanced search examples available for entity '{entity_name}'",
                "recommendation": {
                    "preferred_approach": "Use simple text search with searchText parameter",
                    "basic_search_example": {
                        "searchText": "UNICEF",
                        "advancedSearch": False,
                        "description": "Simple text search across entity fields"
                    },
                    "note": "This entity may not support complex nested field searches"
                }
            }
        
        return json.dumps(result, indent=2)
        
    except Exception as e:
        logging.error(f"Error getting search examples for {entity_name}: {e}")
        return json.dumps({
            "entity": entity_name,
            "error": f"Failed to get search examples for {entity_name}: {str(e)}"
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
        FunctionTool(func=exit_loop_on_success),
        # Entity-specific configuration tools
        FunctionTool(func=get_entity_api_tools_config),
        FunctionTool(func=find_entity_endpoint),
        # Search metadata tools for advanced search
        FunctionTool(func=get_entity_search_metadata),
        FunctionTool(func=get_entity_search_examples)
    ]

    # Add External API tools (always available with stubs)
    tools.extend([
        FunctionTool(func=search_unops_google_drive),
        FunctionTool(func=search_external_drive_service),
        FunctionTool(func=read_content_from_url),
        FunctionTool(func=convert_markdown_to_google_doc),
    ])
    logging.info("✅ Added External API tools to task executor (with fallback implementations)")

    # Add Google Sheets tools (always available with local wrapper)
    tools.extend([
        FunctionTool(func=create_google_sheet_from_list_data),
        FunctionTool(func=create_google_sheet_with_headers_data)
    ])
    logging.info(f"✅ Added Google Sheets tools to task executor (available: {google_sheet_wrapper.available})")

    # Note: Google Doc creation uses convert_markdown_to_google_doc (already added above)
    logging.info("✅ Google Doc creation available via convert_markdown_to_google_doc")

    # Add Speech-to-Text tools (always available with stubs)
    tools.extend([
        FunctionTool(func=transcribe_audio_from_message),
        FunctionTool(func=transcribe_audio_from_artifact),
        FunctionTool(func=transcribe_audio_long_running)
    ])
    logging.info("✅ Added Speech-to-Text tools to task executor (with fallback implementations)")

    # Add cache management tools (always available with stubs)
    tools.extend([
        FunctionTool(func=get_cache_stats),
        FunctionTool(func=clear_cache),
        FunctionTool(func=refresh_cache),
    ])
    logging.info("✅ Added cache management tools to task executor (with fallback implementations)")

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

