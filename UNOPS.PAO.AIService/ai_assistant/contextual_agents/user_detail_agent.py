from google.adk.agents import Agent
from google.adk.tools import FunctionTool

def get_user_profile() -> dict:
    """
    Get current user's profile information from the API.
    Uses the GetUserContext endpoint from tools.json via invoke_api_tool.
    """
    try:
        # Import here to avoid circular imports
        from ..config_manager import config_manager, API_BASE_URL
        from ..workflow_agent.api_worker.utilities import invoke_api_tool, construct_api_url
        import os
        
        print("🔍 Getting user profile via tools.json configuration...")
        
        # Load entities configuration from tools.json
        entities_config = config_manager.get_entities()
        
        # Find the UserData entity configuration
        profile_entity = None
        for entity in entities_config:
            if entity.get('entity', '').lower() == 'userdata':
                profile_entity = entity
                break
        
        if not profile_entity:
            print("⚠️ UserData entity not found in tools.json - using fallback")
            return {}
        
        # Find the GetUserContext endpoint
        get_profile_endpoint = None
        for endpoint in profile_entity.get('endpoints', []):
            if endpoint.get('name') == 'GetUserContext':
                get_profile_endpoint = endpoint
                break
        
        if not get_profile_endpoint:
            print("⚠️ GetUserContext endpoint not found in tools.json - using fallback")
            return {}
        
        # GetUserContext requires email parameter - get from environment or use dev email
        user_email = os.getenv('DEV_EMAIL', 'anushas@unops.org')
        
        # Construct the full API URL using the utilities
        api_url = construct_api_url(API_BASE_URL, get_profile_endpoint['url'])
        method = get_profile_endpoint['method']
        
        # Prepare parameters with required email
        parameters = {"email": user_email}
        
        print(f"📡 Calling API: {method} {api_url} with email: {user_email}")
        
        # Use the invoke_api_tool to make the API call with proper headers
        result = invoke_api_tool(
            url=api_url,
            method=method,
            body=parameters
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            print(f"✅ Successfully retrieved user profile from API")

            user_profile = response_data
            
            return user_profile
        else:
            print(f"❌ API call failed: {result.get('error', 'Unknown error')}")
            return {}
            
    except Exception as e:
        print(f"❌ Error getting user profile: {e}")
        return {}

user_detail_agent = Agent(
    name="user_detail_agent",
    model="gemini-2.0-flash-001",
    description="Agent that gathers user details from the API",
    instruction="""
    You are a background data gathering agent.
    
    Your task: Call get_user_profile() and return the user data.

    It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
    You are the first agent to be called and hence the user information is ALWAYS necessary to do any such above operations. No exceptions.
    
    Do this immediately without any conversation.

    After calling the function, respond with: "User profile retrieved successfully"
    """,
    tools=[FunctionTool(func=get_user_profile)],
    output_key="user_profile"
) 