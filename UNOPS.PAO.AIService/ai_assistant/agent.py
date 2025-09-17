"""
Main AI Assistant Agent

This is the entry point for the AI assistant agent hierarchy.
"""

# Import the main task executor agent
from .task_executor_agent import task_executor_agent
from .response_agent import response_agent
from google.adk.agents import SequentialAgent
from google.adk.agents.callback_context import CallbackContext

def before_agent_callback(callback_context: CallbackContext) -> None:
    """Callback to log agent completion time"""
    callback_context.state["tools_to_execute"] = []

# --- Root Agent Definition ---
# This is the entry point for the entire agent hierarchy
root_agent = SequentialAgent(
    name="root_agent",
    description="Root agent that orchestrates the entire agent hierarchy",
    sub_agents=[task_executor_agent, response_agent],
    before_agent_callback=before_agent_callback
)