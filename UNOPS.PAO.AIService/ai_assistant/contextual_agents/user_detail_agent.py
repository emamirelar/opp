from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from google.adk.tools.tool_context import ToolContext
from ..agent_callbacks import user_detail_agent_callback, user_detail_after_model_callback
from ai_assistant.config_manager import config_manager
import json
import os

def get_user_profile(tool_context: ToolContext) -> dict:
    """
    Get user profile from the API using the user's email from session state.
    
    Args:
        tool_context: Tool context containing session state
        
    Returns:
        dict: User profile data
    """
    try:
        # Get user email from state with proper fallback chain
        user_email = None
        if tool_context and hasattr(tool_context, 'state') and tool_context.state:
            # First priority: user_email from new state format
            user_email = tool_context.state.get('user_email')
            if user_email:
                print(f"📧 Using user_email from session state: {user_email}")
            else:
                # Second priority: header_email from IAP headers (fallback)
                user_email = tool_context.state.get('header_email')
                if user_email:
                    print(f"📧 Using header_email from IAP headers: {user_email}")
        
        # Final fallback to environment variable
        if not user_email:
            user_email = os.getenv('DEV_EMAIL', 'anushas@unops.org')
            print(f"📧 Using DEV_EMAIL fallback: {user_email}")

        # Check cache FIRST before making API call
        from ..cache import get_entity_cache
        entity_cache = get_entity_cache()
        
        cached_profile = entity_cache.get_user_profile(user_email)
        if cached_profile:
            print(f"✅ Cache HIT: Using cached user profile for {user_email}")
            return cached_profile
        
        # Load tools configuration
        tools_config_path = os.path.join(os.path.dirname(__file__), '..', '..', 'config', 'tools.json')
        with open(tools_config_path, 'r') as f:
            tools_config = json.load(f)

        print("About to get profile endpoint")
        
        # Find the GetUserContext endpoint
        get_profile_endpoint = None
        for entity in tools_config.get('entities', []):
            for endpoint in entity.get('endpoints', []):
                if endpoint.get('name') == 'GetUserContext':
                    get_profile_endpoint = endpoint
                    break
        
        if not get_profile_endpoint:
            print("⚠️ GetUserContext endpoint not found in tools.json")
            return {}
        
        # Import here to avoid circular imports
        from ai_assistant.config_manager import config_manager, API_BASE_URL
        from ..workflow_agent.api_worker.utilities import invoke_api_tool, construct_api_url
        
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
            body=parameters,
            tool_context=tool_context
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            print(f"✅ [FUNCTION] Successfully retrieved user profile from API")

            user_profile = response_data
            
            # Store in session state and cache
            try:
                if tool_context and hasattr(tool_context, 'state') and tool_context.state is not None:
                    #tool_context.state['user_profile'] = user_profile
                    print(f"✅ [FUNCTION] Set user_profile in tool_context.state")
                else:
                    print(f"⚠️ [FUNCTION] Could not set user_profile - no valid tool_context.state")
                    
                # Cache the result for future use
                from ..cache import get_entity_cache
                entity_cache = get_entity_cache()
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
    model=config_manager.get_gemini_model(),
    description="Agent that gathers user details from the API",
    instruction="""
    You are a background data gathering agent.
    
    Your task: Call get_user_profile() and return the user data.

    It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
    You are the first agent to be called and hence the user information is ALWAYS necessary to do any such above operations. No exceptions.
    
    Do this immediately without any conversation. 
    
    NOTE: You should not respond to the user's request. You should only ALWAYS return the user profile response in JSON format.
    """,
    tools=[FunctionTool(func=get_user_profile)],
    output_key="user_profile",
    before_model_callback=user_detail_agent_callback,
    after_model_callback=user_detail_after_model_callback
) 