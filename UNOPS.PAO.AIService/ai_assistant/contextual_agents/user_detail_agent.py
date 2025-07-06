from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from google.adk.tools.tool_context import ToolContext
from ..agent_callbacks import user_detail_agent_callback
from ai_assistant.config_manager import config_manager

def get_user_profile(tool_context: ToolContext) -> dict:
    """
    Get current user's profile information from the API.
    This function only runs when cache is not available.
    """
    try:
        # Get session state from tool context  
        session_state = tool_context.state
        
        # Import here to avoid circular imports
        from ai_assistant.config_manager import config_manager, API_BASE_URL
        from ..workflow_agent.api_worker.utilities import invoke_api_tool, construct_api_url
        import os
        
        print("📡 [FUNCTION] Fetching fresh user profile from API...")
        
        # Load entities configuration from tools.json
        entities_config = config_manager.get_entities()
        
        # Find the UserData entity configuration
        profile_entity = None
        for entity in entities_config:
            if entity.get('entity', '').lower() == 'userdata':
                profile_entity = entity
                break
        
        if not profile_entity:
            print("⚠️ UserData entity not found in tools.json")
            return {}
        
        # Find the GetUserContext endpoint
        get_profile_endpoint = None
        for endpoint in profile_entity.get('endpoints', []):
            if endpoint.get('name') == 'GetUserContext':
                get_profile_endpoint = endpoint
                break
        
        if not get_profile_endpoint:
            print("⚠️ GetUserContext endpoint not found in tools.json")
            return {}
        
        # GetUserContext requires email parameter - get from environment or use dev email
        user_email = os.getenv('DEV_EMAIL', 'anushas@unops.org')
        
        # Construct the full API URL using the utilities
        api_url = construct_api_url(API_BASE_URL, get_profile_endpoint['url'])
        method = get_profile_endpoint['method']
        
        # Prepare parameters with required email
        parameters = {"email": user_email}
        
        print(f"🌐 [FUNCTION] Calling API: {method} {api_url} with email: {user_email}")
        
        # Use the invoke_api_tool to make the API call with proper headers
        result = invoke_api_tool(
            url=api_url,
            method=method,
            body=parameters
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            print(f"✅ [FUNCTION] Successfully retrieved user profile from API")

            user_profile = response_data
            
            # Store in session state and cache
            try:
                if session_state is not None:
                    session_state['user_profile'] = user_profile
                    
                # Cache the result for future use
                from ..cache import entity_cache
                entity_cache.set_user_profile(user_email, user_profile)
                
            except Exception as cache_error:
                print(f"⚠️ Failed to set session state or cache: {cache_error}")
            
            return user_profile
        else:
            error_msg = result.get('error', 'Unknown error')
            print(f"❌ API call failed: {error_msg}")
            return {}
            
    except Exception as e:
        print(f"❌ Error getting user profile: {e}")
        return {}

user_detail_agent = Agent(
    name="user_detail_agent",
    model=config_manager.framework_config['runtime']['gemini_model'],
    description="Agent that gathers user details from the API",
    instruction="""
    You are a background data gathering agent.
    
    Your task: Call get_user_profile() and return the user data.

    It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
    You are the first agent to be called and hence the user information is ALWAYS necessary to do any such above operations. No exceptions.
    
    Do this immediately without any conversation.
    """,
    tools=[FunctionTool(func=get_user_profile)],
    output_key="user_profile",
    before_model_callback=user_detail_agent_callback
) 