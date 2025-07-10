from google.genai import types # For creating response content
from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from google.adk.agents.callback_context import CallbackContext
from google.adk.tools.tool_context import ToolContext
from google.adk.models.llm_response import LlmResponse
from ..agent_callbacks import screen_context_agent_callback, screen_context_after_model_callback
from typing import Optional
from ai_assistant.config_manager import config_manager


def gather_screen_context(tool_context: ToolContext) -> dict:
    """
    Intelligently gather screen context using structured data from session state.
    Uses screen_url object and user_viewing_panel for entity and ID information.
    
    Args:
        tool_context: The tool context from Google ADK containing session state
    """
    try:
        # Get session state from tool context
        session_state = tool_context.state
        
        # Import here to avoid circular imports
        from ..config_manager import config_manager, API_BASE_URL
        from ..workflow_agent.api_worker.utilities import invoke_api_tool, construct_api_url
        
        # Extract entity and ID from structured state data
        screen_url_obj = session_state.get('screen_url', {})
        user_viewing_panel = session_state.get('user_viewing_panel', {})
        
        # First priority: screen_url object
        entity = screen_url_obj.get('entity', '')
        entity_id = screen_url_obj.get('id', None)
        section = screen_url_obj.get('section', '')
        
        # Second priority: user_viewing_panel if screen_url is empty
        if not entity and user_viewing_panel:
            entity = user_viewing_panel.get('entity', '')
            entity_id = user_viewing_panel.get('entity_id', None)
            # Convert string ID to int if needed
            if entity_id and isinstance(entity_id, str) and entity_id.isdigit():
                entity_id = int(entity_id)
        
        print(f"📡 [FUNCTION] Processing screen context with structured data:")
        print(f"📋 [FUNCTION] entity='{entity}'")
        print(f"🔢 [FUNCTION] entity_id={entity_id}")
        print(f"📄 [FUNCTION] section='{section}'")
        
        # Handle empty entity case gracefully - no context needed
        if not entity:
            print("📭 [FUNCTION] No entity found in screen_url or user_viewing_panel - returning empty context")
            empty_context = {
                "screen_name": "Application",
                "screen_url": "",
                "screen_type": "app",
                "screen_data": {
                    "message": "No specific screen context available",
                    "context_available": False
                }
            }
            return empty_context
        
        final_entity = entity
        final_id = entity_id
        final_query = "detail" if final_id else "list"
        
        print(f"📋 [FUNCTION] Using structured data: entity='{final_entity}', id={final_id}, query='{final_query}'")
        
        # Create a display URL for logging and context
        display_url = f"/{final_entity}"
        if final_id:
            display_url += f"/{final_id}"
        if section:
            display_url += f"/{section}"
        
        # Handle special cases for non-API entities
        if final_entity in ['dashboard', '']:
            print("📭 [FUNCTION] Dashboard/Home route - returning general context")
            dashboard_context = {
                "screen_name": "Dashboard",
                "screen_url": display_url,
                "screen_type": "dashboard",
                "screen_data": {
                    "message": "Dashboard screen context",
                    "context_available": True,
                    "view_type": "dashboard",
                    "entity": "dashboard"
                }
            }
            return dashboard_context
        
        # Load entities configuration from tools.json
        entities_config = config_manager.get_entities()
        
        # Enhanced entity matching to handle plural/singular and case differences
        def find_matching_entity(target_entity: str, entities_list):
            target_lower = target_entity.lower()
            
            for ent in entities_list:
                config_entity = ent.get('entity', '')
                config_entity_lower = config_entity.lower()
                
                # Direct match
                if config_entity_lower == target_lower:
                    return ent
                
                # Singular vs plural matching
                # Remove 's' from target and compare
                if target_lower.endswith('s') and config_entity_lower == target_lower[:-1]:
                    return ent
                
                # Add 's' to config entity and compare  
                if config_entity_lower + 's' == target_lower:
                    return ent
                
                # Check synonyms
                synonyms = ent.get('synonyms', [])
                for synonym in synonyms:
                    if synonym.lower() == target_lower:
                        return ent
                    # Check plural/singular for synonyms too
                    if target_lower.endswith('s') and synonym.lower() == target_lower[:-1]:
                        return ent
                    if synonym.lower() + 's' == target_lower:
                        return ent
            
            return None
        
        # Find the entity configuration with enhanced matching
        entity_config = find_matching_entity(final_entity, entities_config)
        
        if not entity_config:
            print(f"⚠️ [FUNCTION] {final_entity} entity not found in tools.json")
            error_context = {
                "screen_name": f"{final_entity.title()} Page",
                "screen_url": display_url,
                "screen_type": "entity",
                "screen_data": {"entity": final_entity, "error": "Entity not found in configuration"}
            }
            return error_context
        
        # Use the actual entity name from configuration
        actual_entity_name = entity_config.get('entity', final_entity)
        print(f"✅ [FUNCTION] Found entity configuration: {actual_entity_name} for entity '{final_entity}'")
        
        # Determine which endpoint to use based on query type and ID presence
        target_endpoint = None
        
        if final_id != -1:
            # Detail view - look for GetEntityById endpoints
            for endpoint in entity_config.get('endpoints', []):
                endpoint_name = endpoint.get('name', '').lower()
                if 'get' in endpoint_name and 'byid' in endpoint_name:
                    target_endpoint = endpoint
                    break
        else:
            # List view - look for GetEntities endpoints  
            for endpoint in entity_config.get('endpoints', []):
                endpoint_name = endpoint.get('name', '').lower()
                if ('get' in endpoint_name and 
                    ('s' in endpoint_name[-1:] or 'list' in endpoint_name) and
                    'byid' not in endpoint_name):
                    target_endpoint = endpoint
                    break
        
        if not target_endpoint:
            print(f"⚠️ [FUNCTION] No appropriate endpoint found for {actual_entity_name} (ID: {final_id})")
            error_context = {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": display_url,
                "screen_type": "entity", 
                "screen_data": {"entity": actual_entity_name, "entity_id": final_id, "error": "No suitable endpoint found"}
            }
            return error_context
        
        # Construct API URL and prepare parameters
        api_url = construct_api_url(config_manager.get_api_base_url(), target_endpoint['url'])
        method = target_endpoint['method']
        
        # Prepare parameters based on endpoint type
        parameters = {}
        
        if final_id != -1:
            # Detail view - substitute ID in URL path
            api_url = api_url.replace('{id}', str(final_id))
            print(f"🌐 [FUNCTION] Calling detail API for screen: {method} {api_url}")
        else:
            # List view - add pagination parameters
            parameters = {
                "pageIndex": 1,
                "pageSize": 10
            }
            print(f"🌐 [FUNCTION] Calling list API for screen: {method} {api_url} with params: {parameters}")
        
        # Make the API call
        result = invoke_api_tool(
            url=api_url,
            method=method,
            body=parameters,
            tool_context=tool_context
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            print(f"✅ [FUNCTION] Successfully retrieved {actual_entity_name} data for entity '{final_entity}'")
            
            # Format response based on view type
            if final_id != -1:
                # Detail view
                screen_name = f"{actual_entity_name} Details"
                screen_type = "detail"
                screen_data = {
                    "entity": actual_entity_name,
                    "entity_id": final_id,
                    "entity_data": response_data,
                    "view_type": "detail",
                    "intelligent_context": True,
                    "parsed_from_state": display_url,
                    "detected_entity": final_entity
                }
            else:
                # List view
                screen_name = f"{actual_entity_name} List"
                screen_type = "list"
                data_items = response_data.get('data', [])
                total_count = response_data.get('totalCount', len(data_items))
                screen_data = {
                    "entity": actual_entity_name,
                    "view_type": "list",
                    "total_count": total_count,
                    "current_page": 1,
                    "page_size": len(data_items),
                    "items": data_items,
                    "intelligent_context": True,
                    "parsed_from_state": display_url,
                    "detected_entity": final_entity
                }
            
            screen_context = {
                "screen_name": screen_name,
                "screen_url": display_url,
                "screen_type": screen_type,
                "screen_data": screen_data
            }
            
            # Store in session state and cache
            try:
                if session_state is not None:
                    session_state['screen_context'] = screen_context
                    
                # Cache the result by entity+id combination for future use
                from ..cache import entity_cache
                cache_key = f"{final_entity}:{final_id}" if final_id else f"{final_entity}:list"
                entity_cache.set_screen_context(cache_key, screen_context)
                print(f"💾 Cached screen context with key: {cache_key}")
                
            except Exception as cache_error:
                print(f"⚠️ Failed to set session state or cache: {cache_error}")
            
            return screen_context
        else:
            error_msg = result.get('error', 'Unknown error')
            print(f"❌ API call failed for entity '{final_entity}': {error_msg}")
            
            error_context = {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": display_url,
                "screen_type": "error",
                "screen_data": {
                    "entity": actual_entity_name,
                    "entity_id": final_id,
                    "error": f"API call failed: {error_msg}"
                }
            }
            return error_context
            
    except Exception as e:
        print(f"❌ Error gathering screen context: {e}")
        return {
            "screen_name": "Error Page",
            "screen_url": "unknown", 
            "screen_type": "error",
            "screen_data": {"error": str(e)}
        }

screen_context_agent = Agent(
    name="screen_context_agent",
    model=config_manager.get_gemini_model(),
    description="Agent that gathers screen context using structured data from session state",
    instruction="""
    You are a background data gathering agent.
    
    Your task: Call gather_screen_context() to get the current screen context data.

    You will use the structured screen_url and user_viewing_panel data from the session state.
    The entity and ID information is already provided in a structured format.
    
    Do this immediately without any conversation. You should not respond to the user's request. You should only ALWAYS return the screen context response in JSON format.
    """,
    tools=[FunctionTool(func=gather_screen_context)],
    output_key="screen_context",
    before_model_callback=screen_context_agent_callback,
    after_model_callback=screen_context_after_model_callback
) 