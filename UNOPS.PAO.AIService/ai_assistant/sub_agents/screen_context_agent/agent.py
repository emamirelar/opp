from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.utils.common_callbacks import (
    screen_context_after_model_callback,
)
from .utils import parse_screen_url_callback, gather_screen_context

screen_context_agent = Agent(
    name="screen_context_agent",
    model=config_manager.get_gemini_model(),
    description="Agent that processes screen context and fetches entity display names when needed",
    instruction="""
You are a background data gathering agent.
You are only exposed to the tool `gather_screen_context`.

It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
You are the first agent to be called and hence the screen context information is ALWAYS necessary to do any such above operations. No exceptions.

Your task: Call gather_screen_context() to get the current screen context data.

You will use the structured screen_url and user_viewing_panel data from the session state.
The entity and ID information is already provided in a structured format.

Do this immediately without any conversation. You should NOT respond to the user's request or message. You should only ALWAYS return the screen context response in JSON format.
    """,
    tools=[FunctionTool(func=gather_screen_context)],
    output_key="screen_context", 
    before_model_callback=parse_screen_url_callback,
    after_model_callback=screen_context_after_model_callback,
    # Add safety configurations to prevent responses to users
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)