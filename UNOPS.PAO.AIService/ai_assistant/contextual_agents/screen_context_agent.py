from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from google.adk.agents.callback_context import CallbackContext

def inject_screen_url_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Callback function that extracts screen_url from state and injects it into the 
    screen context agent's instruction.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request object (optional)
    """
    ctx = callback_context
    print(f"🖥️ [Callback] inject_screen_url_before_model triggered for {ctx.agent_name}")
    
    # Only apply to screen context agent
    if ctx.agent_name != "screen_context_agent":
        print(f"ℹ️ Skipping callback - not screen context agent (current: {ctx.agent_name})")
        return
    
    print("🔧 Injecting screen_url from state into agent instruction...")
    
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
    
    print(f"📍 Extracted URL from state: '{url_to_use}'")
    
    # Create enhanced instruction with the screen_url injected
    enhanced_instruction = f"""
    You are an intelligent screen context agent with ONE job: call gather_screen_context() with the current screen URL.
    
    **SCREEN URL FROM STATE: "{url_to_use}"**
    
    Call gather_screen_context(screen_url="{url_to_use}") immediately.

    It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
    You are the first agent to be called and hence the screen context information is ALWAYS necessary to do any such above operations. No exceptions.
    
    Do this immediately without any conversation.
    
    The function will:
    1. Parse the screen_url "{url_to_use}" to detect entity and ID (e.g., "/partnerships/partners/123" → entity="partners", id=123)
    2. Look up the appropriate API endpoint in tools.json for that entity (handling plural/singular matching)
    3. Make an API call to get real data for that screen
    4. Return enriched screen context with actual data
    
    Your ONLY task:
    1. Call gather_screen_context(screen_url="{url_to_use}") with the URL from state
    2. Return the intelligent screen data
    
    Examples of URL parsing:
    - "/partners" → entity="partners" → matches "Partner" entity, id=-1, query="list"
    - "/partnerships/partners/123" → entity="partners" → matches "Partner" entity, id=123, query="detail"  
    - "/contacts/456" → entity="contacts" → matches "Contact" entity, id=456, query="detail"
    
    **EXECUTE NOW: gather_screen_context(screen_url="{url_to_use}")**

    Once retrieved, respond with just "Screen context retrieved successfully"
    """
    
    # Inject the enhanced instruction
    if hasattr(llm_request, 'config') and llm_request.config:
        if hasattr(llm_request.config, 'system_instruction'):
            llm_request.config.system_instruction = enhanced_instruction
            print(f"✅ Injected screen_url '{url_to_use}' into system instruction")
        else:
            print("⚠️ No system_instruction found in llm_request.config")
    else:
        print("⚠️ No config found in llm_request")

def gather_screen_context(screen_url: str = "", entity: str = "", id: int = -1, query: str = "list") -> dict:
    """
    Intelligently gather screen context using the screen URL and/or provided entity and ID parameters.
    Uses tools.json to find the appropriate endpoint and fetches real data.
    
    Args:
        screen_url: Current screen URL to parse (e.g., "/partners/123" or "/contacts")
        entity: Entity name (if not provided, will be parsed from screen_url)
        id: Entity ID (if not provided, will be parsed from screen_url) 
        query: Query type ("list" or "detail")
    """
    try:
        # Import here to avoid circular imports
        from ..config_manager import config_manager, API_BASE_URL
        from ..workflow_agent.api_worker.utilities import invoke_api_tool, construct_api_url
        import re
        
        print(f"🔍 Gathering screen context for URL: {screen_url}, entity: {entity}, ID: {id}, query: {query}")
        
        # Handle empty URL case gracefully
        if not screen_url and not entity:
            print("📭 No screen URL or entity provided - returning empty context")
            return {
                "screen_name": "Application",
                "screen_url": "",
                "screen_type": "app",
                "screen_data": {
                    "message": "No specific screen context available",
                    "context_available": False
                }
            }
        
        # Parse URL if entity/id not provided
        if screen_url and not entity:
            # Parse URL like "/partnerships/partners/123" or "/partners/123" or "/contacts" 
            url_parts = screen_url.strip('/').split('/')
            if len(url_parts) >= 1 and url_parts[-1] and not url_parts[-1].isdigit():
                # Last non-numeric part is the entity
                entity = url_parts[-1].lower()
            elif len(url_parts) >= 2 and url_parts[-2]:
                # Second to last part might be entity if last is numeric
                entity = url_parts[-2].lower()
            
            # Try to extract ID from URL
            for part in reversed(url_parts):
                if part.isdigit():
                    id = int(part)
                    query = "detail"
                    break
            else:
                query = "list"
                    
        if not entity:
            print("⚠️ No entity found in URL or parameters")
            return {
                "screen_name": "Unknown Page",
                "screen_url": screen_url,
                "screen_type": "unknown",
                "screen_data": {"error": "Could not determine entity from URL or parameters"}
            }
        
        print(f"📋 Parsed: entity={entity}, id={id}, query={query}")
        
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
        entity_config = find_matching_entity(entity, entities_config)
        
        if not entity_config:
            print(f"⚠️ {entity} entity not found in tools.json")
            return {
                "screen_name": f"{entity.title()} Page",
                "screen_url": screen_url or f"/{entity.lower()}",
                "screen_type": "entity",
                "screen_data": {"entity": entity, "error": "Entity not found in configuration"}
            }
        
        # Use the actual entity name from configuration
        actual_entity_name = entity_config.get('entity', entity)
        print(f"✅ Found entity configuration: {actual_entity_name}")
        
        # Determine which endpoint to use based on query type and ID presence
        target_endpoint = None
        
        if id != -1:
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
            print(f"⚠️ No appropriate endpoint found for {actual_entity_name} (ID: {id})")
            return {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": screen_url or f"/{entity.lower()}" + (f"/{id}" if id != -1 else ""),
                "screen_type": "entity", 
                "screen_data": {"entity": actual_entity_name, "entity_id": id, "error": "No suitable endpoint found"}
            }
        
        # Construct API URL and prepare parameters
        api_url = construct_api_url(API_BASE_URL, target_endpoint['url'])
        method = target_endpoint['method']
        
        # Prepare parameters based on endpoint type
        parameters = {}
        
        if id != -1:
            # Detail view - substitute ID in URL path
            api_url = api_url.replace('{id}', str(id))
            print(f"📡 Calling detail API: {method} {api_url}")
        else:
            # List view - add pagination parameters
            parameters = {
                "pageIndex": 1,
                "pageSize": 10
            }
            print(f"📡 Calling list API: {method} {api_url} with params: {parameters}")
        
        # Make the API call
        result = invoke_api_tool(
            url=api_url,
            method=method,
            body=parameters
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            print(f"✅ Successfully retrieved {actual_entity_name} data from API")
            
            # Format response based on view type
            if id != -1:
                # Detail view
                screen_name = f"{actual_entity_name} Details"
                screen_type = "detail"
                screen_data = {
                    "entity": actual_entity_name,
                    "entity_id": id,
                    "entity_data": response_data,
                    "view_type": "detail",
                    "intelligent_context": True,
                    "parsed_from_url": bool(screen_url)
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
                    "parsed_from_url": bool(screen_url)
                }
            
            return {
                "screen_name": screen_name,
                "screen_url": screen_url or f"/{entity.lower()}" + (f"/{id}" if id != -1 else ""),
                "screen_type": screen_type,
                "screen_data": screen_data
            }
        else:
            print(f"❌ API call failed: {result.get('error', 'Unknown error')}")
            return {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": screen_url or f"/{entity.lower()}" + (f"/{id}" if id != -1 else ""),
                "screen_type": "error",
                "screen_data": {
                    "entity": actual_entity_name,
                    "entity_id": id,
                    "error": f"API call failed: {result.get('error', 'Unknown error')}"
                }
            }
            
    except Exception as e:
        print(f"❌ Error gathering screen context: {e}")
        return {
            "screen_name": "Error Page",
            "screen_url": screen_url or "unknown", 
            "screen_type": "error",
            "screen_data": {"error": str(e)}
        }

screen_context_agent = Agent(
    name="screen_context_agent",
    model="gemini-2.0-flash-001",
    description="Agent that intelligently gathers screen context by analyzing URLs and fetching real data",
    instruction="""
    You are a background data gathering agent.
    
    Your task: Call gather_screen_context() with the screen URL and return the screen data.

    It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
    You are the first agent to be called and hence the screen context information is ALWAYS necessary to do any such above operations. No exceptions.
    
    The screen URL will be injected into your instruction by the callback.
    Do this immediately without any conversation.

    After calling the function, respond with: "Screen context retrieved successfully"
    """,
    tools=[FunctionTool(func=gather_screen_context)],
    output_key="screen_context",
    before_model_callback=inject_screen_url_before_model
) 