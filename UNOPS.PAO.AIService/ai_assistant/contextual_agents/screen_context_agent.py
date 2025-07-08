from google.genai import types # For creating response content
from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from google.adk.agents.callback_context import CallbackContext
from google.adk.tools.tool_context import ToolContext
from google.adk.models.llm_response import LlmResponse
from ..agent_callbacks import url_change_callback, screen_context_agent_callback, screen_context_after_model_callback
from typing import Optional
from ai_assistant.config_manager import config_manager

def inject_screen_url_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Callback function that extracts screen_url from state, parses the entity from it,
    and injects explicit instructions telling the agent to use ONLY the detected entity.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request object (optional)
    """
    ctx = callback_context
    print(f"🖥️ [URL-INJECT] inject_screen_url_before_model triggered for {ctx.agent_name}")
    
    # Only apply to screen context agent
    if ctx.agent_name != "screen_context_agent":
        print(f"ℹ️ Skipping URL injection - not screen context agent (current: {ctx.agent_name})")
        return
    
    print("🔧 [URL-INJECT] Parsing entity from screen_url and creating dynamic instruction...")
    
    # Use the llm_request parameter if provided, otherwise try to get it from context
    if not llm_request:
        llm_request = getattr(ctx, 'llm_request', None)
    
    if not llm_request:
        print("⚠️ No LLM request found in context")
        return
    
    # Extract screen_url from state
    screen_url = ctx.state.get("screen_url", "")
    current_url = ctx.state.get("current_url", "")
    page_url = ctx.state.get("page_url", "")
    
    # Use the first available URL
    url_to_use = screen_url or current_url or page_url or ""
    
    print(f"📍 [URL-INJECT] Extracted URL from state: '{url_to_use}'")
    
    # Parse entity and ID from URL
    detected_entity = ""
    detected_id = -1
    detected_query = "list"
    
    if url_to_use:
        normalized_url = url_to_use.strip().lower()
        url_lower = normalized_url.strip('/')
        
        # Handle special UI routes first
        if url_lower in ['notifications', 'notification']:
            detected_entity = 'notification'
            detected_query = "list"
            print(f"📋 [URL-INJECT] Detected special route: notifications")
        elif url_lower in ['dashboard', 'home', '']:
            detected_entity = 'dashboard'
            detected_query = "dashboard"
            print(f"📋 [URL-INJECT] Detected special route: dashboard")
        else:
            # Parse URL like "/partnerships/partners/123" or "/partners/123" or "/contacts" 
            url_parts = normalized_url.strip('/').split('/')
            if len(url_parts) >= 1 and url_parts[-1] and not url_parts[-1].isdigit():
                # Last non-numeric part is the entity
                detected_entity = url_parts[-1].lower()
            elif len(url_parts) >= 2 and url_parts[-2]:
                # Second to last part might be entity if last is numeric
                detected_entity = url_parts[-2].lower()
            
            # Try to extract ID from URL
            for part in reversed(url_parts):
                if part.isdigit():
                    detected_id = int(part)
                    detected_query = "detail"
                    break
            else:
                detected_query = "list"
        
        print(f"📋 [URL-INJECT] Parsed from URL: entity='{detected_entity}', id={detected_id}, query='{detected_query}'")
    else:
        print("📭 [URL-INJECT] No URL found - will use empty context")
    
    # Trigger URL change callback
    url_change_callback(callback_context, url_to_use)
    
    # Create EXPLICIT dynamic instruction that tells agent to ignore user message
    enhanced_instruction = f"""
    **CRITICAL: IGNORE USER MESSAGE CONTENT COMPLETELY**
    
    You are a screen context agent. Your ONLY job is to detect what screen the user is currently viewing based on the URL in the system state.
    
    **CURRENT SCREEN ANALYSIS:**
    - Screen URL from state: "{url_to_use}"
    - Detected Entity: "{detected_entity}"
    - Detected ID: {detected_id}
    - Query Type: "{detected_query}"
    
    **YOUR TASK:**
    Call gather_screen_context() with these EXACT parameters detected from the screen URL:
    - screen_url="{url_to_use}"
    - entity="{detected_entity}" 
    - id={detected_id}
    - query="{detected_query}"
    
    **IMPORTANT RULES:**
    1. DO NOT analyze the user's message content
    2. DO NOT try to understand what the user is asking for
    3. ONLY use the entity detected from the screen URL: "{detected_entity}"
    4. The user could be asking about anything - ignore it completely
    5. Your job is screen context, not request processing
    
    **EXAMPLES OF WHAT TO IGNORE:**
    - If user asks "show me partners" but screen_url is "/notifications" → Use entity="notification"
    - If user asks "get contacts" but screen_url is "/partners/123" → Use entity="partners", id=123
    - If user asks "notifications" but screen_url is "/contacts" → Use entity="contacts"
    
    **EXECUTE NOW:**
    Call gather_screen_context(screen_url="{url_to_use}", entity="{detected_entity}", id={detected_id}, query="{detected_query}")
    
    After calling the function, respond with: "Screen context retrieved for {detected_entity if detected_entity else 'current screen'}"
    """
    
    # Inject the enhanced instruction
    if hasattr(llm_request, 'config') and llm_request.config:
        if hasattr(llm_request.config, 'system_instruction'):
            llm_request.config.system_instruction = enhanced_instruction
            print(f"✅ [URL-INJECT] Injected dynamic instruction with entity='{detected_entity}', id={detected_id}")
        else:
            print("⚠️ No system_instruction found in llm_request.config")
    else:
        print("⚠️ No config found in llm_request")

def gather_screen_context(tool_context: ToolContext, screen_url: str = "", entity: str = "", id: int = -1, query: str = "list") -> dict:
    """
    Intelligently gather screen context using the entity detected from screen_url.
    This function uses the pre-parsed entity from the dynamic instruction, NOT the user's message.
    
    Args:
        tool_context: The tool context from Google ADK
        screen_url: Current screen URL from state (e.g., "/notifications", "/partners/123")
        entity: Entity name (PRE-PARSED from screen_url by callback)
        id: Entity ID (PRE-PARSED from screen_url by callback) 
        query: Query type (PRE-PARSED from screen_url by callback)
    """
    try:
        # Get session state from tool context
        session_state = tool_context.state
        
        # Import here to avoid circular imports
        from ..config_manager import config_manager, API_BASE_URL
        from ..workflow_agent.api_worker.utilities import invoke_api_tool, construct_api_url
        import re
        
        # Normalize screen_url for comparison
        normalized_url = screen_url.strip() if screen_url else ""
        
        print(f"📡 [FUNCTION] Processing screen context with PRE-PARSED parameters:")
        print(f"📍 [FUNCTION] screen_url='{normalized_url}'")
        print(f"📋 [FUNCTION] entity='{entity}' (from URL parsing)")
        print(f"🔢 [FUNCTION] id={id} (from URL parsing)")
        print(f"📝 [FUNCTION] query='{query}' (from URL parsing)")
        print(f"🚫 [FUNCTION] IGNORING user message content completely")
        
        # Handle empty URL case gracefully
        if not normalized_url:
            print("📭 [FUNCTION] No screen URL provided - returning default context")
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
        
        # Use the PRE-PARSED entity from the callback - do NOT re-parse from URL
        # The entity was already detected from screen_url by inject_screen_url_before_model
        final_entity = entity
        final_id = id
        final_query = query
        
        print(f"📋 [FUNCTION] Using PRE-PARSED parameters: entity='{final_entity}', id={final_id}, query='{final_query}'")
        
        # Handle special cases for non-API entities
        if final_entity in ['dashboard', '']:
            print("📭 [FUNCTION] Dashboard/Home route - returning general context")
            dashboard_context = {
                "screen_name": "Dashboard",
                "screen_url": normalized_url,
                "screen_type": "dashboard",
                "screen_data": {
                    "message": "Dashboard screen context",
                    "context_available": True,
                    "view_type": "dashboard",
                    "entity": "dashboard"
                }
            }
            return dashboard_context
        
        if not final_entity:
            print(f"⚠️ [FUNCTION] No entity detected from URL: '{normalized_url}'")
            error_context = {
                "screen_name": "Unknown Page",
                "screen_url": normalized_url,
                "screen_type": "unknown",
                "screen_data": {"error": f"Could not determine entity from URL: '{normalized_url}'"}
            }
            return error_context
        
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
                "screen_url": normalized_url,
                "screen_type": "entity",
                "screen_data": {"entity": final_entity, "error": "Entity not found in configuration"}
            }
            return error_context
        
        # Use the actual entity name from configuration
        actual_entity_name = entity_config.get('entity', final_entity)
        print(f"✅ [FUNCTION] Found entity configuration: {actual_entity_name} for screen '{normalized_url}'")
        
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
                "screen_url": normalized_url,
                "screen_type": "entity", 
                "screen_data": {"entity": actual_entity_name, "entity_id": final_id, "error": "No suitable endpoint found"}
            }
            return error_context
        
        # Construct API URL and prepare parameters
        api_url = construct_api_url(config_manager.framework_config['runtime']['api_base_url'], target_endpoint['url'])
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
            print(f"✅ [FUNCTION] Successfully retrieved {actual_entity_name} data for screen '{normalized_url}'")
            
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
                    "parsed_from_url": normalized_url,
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
                    "parsed_from_url": normalized_url,
                    "detected_entity": final_entity
                }
            
            screen_context = {
                "screen_name": screen_name,
                "screen_url": normalized_url,
                "screen_type": screen_type,
                "screen_data": screen_data
            }
            
            # Store in session state and cache
            try:
                if session_state is not None:
                    session_state['screen_context'] = screen_context
                    
                # Cache the result for future use
                from ..cache import entity_cache
                entity_cache.set_screen_context(normalized_url, screen_context)
                
            except Exception as cache_error:
                print(f"⚠️ Failed to set session state or cache: {cache_error}")
            
            return screen_context
        else:
            error_msg = result.get('error', 'Unknown error')
            print(f"❌ API call failed for screen '{normalized_url}': {error_msg}")
            
            error_context = {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": normalized_url,
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
            "screen_url": screen_url or "unknown", 
            "screen_type": "error",
            "screen_data": {"error": str(e)}
        }

# Define a combined callback that handles both URL injection and cache checking
def combined_screen_context_callback(callback_context: CallbackContext, llm_request) -> Optional[LlmResponse]:
    """
    Combined callback that first injects URL, then checks cache.
    """
    # First, inject the screen URL into the instruction
    inject_screen_url_before_model(callback_context, llm_request)
    
    # Then, check if we can skip LLM execution due to cache
    return screen_context_agent_callback(callback_context, llm_request)

screen_context_agent = Agent(
    name="screen_context_agent",
    model=config_manager.framework_config['runtime']['gemini_model'],
    description="Agent that intelligently gathers screen context by analyzing URLs and fetching real data",
    instruction="""
    You are a background data gathering agent.
    
    Your task: Call gather_screen_context() with the screen URL and return the screen data.

    It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
    You are the first agent to be called and hence the screen context information is ALWAYS necessary to do any such above operations. No exceptions.
    
    The screen URL will be injected into your instruction by the callback.
    Do this immediately without any conversation.
    """,
    tools=[FunctionTool(func=gather_screen_context)],
    output_key="screen_context",
    # Use combined callback that handles both URL injection and cache checking
    before_model_callback=combined_screen_context_callback,
    after_model_callback=screen_context_after_model_callback
) 