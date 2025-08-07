from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from ai_assistant.utils.api_config_manager import config_manager
from .utils import get_user_profile, user_detail_after_model_callback, user_detail_agent_callback

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
    after_model_callback=user_detail_after_model_callback,
    # Add safety configurations to prevent infinite loops
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)