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

class GoogleDocToolWrapper:
    """Lightweight wrapper for Google Docs functionality"""
    def __init__(self):
        self.available = True
        self._doc_tool = None
        self._last_activity = None
    
    async def _get_doc_tool(self):
        """Get or create the Google Doc tool with connection management"""
        import time
        
        try:
            # Check if we need to recreate the tool (avoid stale connections)
            current_time = time.time()
            if (self._doc_tool is None or 
                (self._last_activity and current_time - self._last_activity > 300)):  # 5 minute timeout
                
                logger.info("🔄 Creating fresh Google Doc tool instance")
                
                # Cleanup old tool if exists
                if self._doc_tool:
                    try:
                        await self._doc_tool.cleanup()
                    except Exception:
                        pass
                
                # Create new tool with detailed logging
                try:
                    from ai_assistant.tools import create_google_doc_tool
                    logger.info("🔧 Attempting to create Google Doc tool...")
                    self._doc_tool = await create_google_doc_tool()
                    
                    if self._doc_tool:
                        logger.info("✅ Google Doc tool created successfully")
                    else:
                        logger.error("❌ create_google_doc_tool returned None - check configuration")
                        
                except ImportError as e:
                    logger.error(f"❌ Import error creating Google Doc tool: {e}")
                    return None
                except Exception as e:
                    logger.error(f"❌ Unexpected error creating Google Doc tool: {e}")
                    return None
                
            self._last_activity = current_time
            return self._doc_tool
            
        except Exception as e:
            logger.error(f"❌ Error getting Google Doc tool: {e}")
            return None
    
    async def create_document_from_text(self, tool_context, title: str, content: str, folder_id: str = ""):
        """Create Google Doc from text with proper connection management"""
        try:
            logger.info(f"🔄 Creating document: {title}")
            
            # Get the tool instance
            doc_tool = await self._get_doc_tool()
            if not doc_tool:
                return json.dumps({"error": "Google Doc tool not available"})
            
            # Create the document
            result = await doc_tool.create_document(
                title=title,
                content=content,
                folder_id=folder_id if folder_id else None
            )
            
            # Check for errors in result
            if isinstance(result, dict) and "error" in result:
                logger.error(f"❌ Document creation failed: {result['error']}")
                return json.dumps(result)
            
            logger.info(f"✅ Document created successfully: {result.get('name', title)}")
            return json.dumps(result)
            
        except Exception as e:
            error_msg = f"Failed to create document: {str(e)}"
            logger.error(f"❌ {error_msg}")
            
            # Handle specific error types
            if "WinError 10055" in str(e) or "socket" in str(e).lower():
                # Reset the tool to clear any bad connections
                self._doc_tool = None
                error_msg = "Network connection issue detected. Connection has been reset for next attempt."
            
            return json.dumps({"error": error_msg})


# Initialize tool wrappers (always available)
google_sheet_wrapper = GoogleSheetToolWrapper()
google_doc_wrapper = GoogleDocToolWrapper()
logging.info(f"✅ Google Sheet wrapper initialized in task executor (available: {google_sheet_wrapper.available})")
logging.info(f"✅ Google Doc wrapper initialized in task executor (available: {google_doc_wrapper.available})")

# Google Drive search functions - provide stub implementations
def search_google_drive_knowledge(query: str) -> str:
    """Search Google Drive knowledge base"""
    try:
        from ai_assistant.tools import search_google_drive_knowledge as real_search
        return real_search(query)
    except ImportError:
        return json.dumps({
            "results": [],
            "message": "Google Drive knowledge search not available in current environment",
            "query": query
        })

def search_google_drive_content(search_text: str) -> str:
    """Search Google Drive content"""
    try:
        from ai_assistant.tools import search_google_drive_content as real_search
        return real_search(search_text)
    except ImportError:
        return json.dumps({
            "results": [],
            "message": "Google Drive content search not available in current environment",
            "search_text": search_text
        })

def search_google_drive(query: str) -> str:
    """Search Google Drive files"""
    try:
        from ai_assistant.tools import search_google_drive as real_search
        return real_search(query)
    except ImportError:
        return json.dumps({
            "results": [],
            "message": "Google Drive file search not available in current environment",
            "query": query
        })

def read_google_drive_file(file_id: str) -> str:
    """Read Google Drive file content"""
    try:
        from ai_assistant.tools import read_google_drive_file as real_read
        return real_read(file_id)
    except ImportError:
        return json.dumps({
            "content": "",
            "message": "Google Drive file reading not available in current environment",
            "file_id": file_id
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
    Find the best endpoint for a given entity and intent combination with retry logic
    
    Args:
        entity_name: The entity name (e.g., "Partner", "Contact", "Global")
        intent: The intent (e.g., "search", "create", "update")
        extracted_params: JSON string of extracted parameters from user query (e.g., '{"id": 123, "type": "OrgUnit"}')
        
    Returns:
        JSON string containing endpoint details with attempt information
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
            """Standalone scoring function matching the enhanced algorithm in api_config_manager"""
            score = 0
            endpoint_name = endpoint.get('name', '').lower()
            description = endpoint.get('description', '').lower()
            url = endpoint.get('url', '').lower()
            method = endpoint.get('method', 'GET').upper()
            when_to_use = endpoint.get('when_to_use', '').lower()
            example_uses = endpoint.get('example_uses', [])
            parameters = endpoint.get('parameters', {})
            
            # Intent to HTTP method mapping
            intent_method_mapping = {
                'search': 'GET', 'list': 'GET', 'get': 'GET', 'find': 'GET', 'retrieve': 'GET',
                'create': 'POST', 'add': 'POST', 'new': 'POST', 'insert': 'POST',
                'update': 'PUT', 'modify': 'PUT', 'edit': 'PUT', 'change': 'PUT',
                'delete': 'DELETE', 'remove': 'DELETE', 'destroy': 'DELETE'
            }
            
            target_method = intent_method_mapping.get(intent.lower(), 'GET')
            
            # Must match HTTP method - this is critical
            if method != target_method:
                return 0  # Wrong method = zero score

            # Base scoring for intent matching
            intent_lower = intent.lower()
            if intent_lower == endpoint_name:
                score += 50  # Perfect match
            elif intent_lower in endpoint_name:
                score += 30  # Intent is part of name
            elif any(synonym in endpoint_name for synonym in intent_method_mapping.keys() if intent_method_mapping[synonym] == target_method):
                score += 20  # Related intent word in name
            
            # Intent in description and when_to_use
            intent_keywords = [intent_lower] + [k for k, v in intent_method_mapping.items() if v == target_method]
            for keyword in intent_keywords:
                if keyword in description:
                    score += 15
                if keyword in when_to_use:
                    score += 15
            
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

    # Add Google Drive tools (always available with stubs)
    tools.extend([
        FunctionTool(func=search_google_drive_knowledge),
        FunctionTool(func=search_google_drive),
        FunctionTool(func=read_google_drive_file),
        FunctionTool(func=search_google_drive_content),
    ])
    logging.info("✅ Added Google Drive tools to task executor (with fallback implementations)")

    # Add Google Sheets tools (always available with local wrapper)
    tools.extend([
        FunctionTool(func=create_google_sheet_from_list_data),
        FunctionTool(func=create_google_sheet_with_headers_data)
    ])
    logging.info(f"✅ Added Google Sheets tools to task executor (available: {google_sheet_wrapper.available})")

    # Add Google Docs tools (always available with local wrapper)
    tools.append(FunctionTool(func=create_google_doc_from_text_data))
    logging.info(f"✅ Added Google Docs tools to task executor (available: {google_doc_wrapper.available})")

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

