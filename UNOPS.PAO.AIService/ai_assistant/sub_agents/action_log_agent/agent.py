"""
Action Log Agent

This agent is responsible for logging AI interactions and actions
when the feature is enabled in the configuration.
"""

import logging
from google.adk.agents import Agent
from google.adk.tools import FunctionTool
from ai_assistant.utils.api_config_manager import config_manager
from .utils import log_ai_action

logger = logging.getLogger(__name__)

# Define the action log agent
action_log_agent = Agent(
    name="action_log_agent",
    description="""
    Specialized agent for logging AI interactions.
    
    Your SOLE responsibility is to analyze the user interaction and AI response, then create a concise summary and log it.
    
    INSTRUCTIONS:
    1. Analyze the user's request and the AI's response/actions
    2. Generate a concise summary (maximum 400 characters) that captures:
       - What the user asked for
       - What action was taken or information provided
       - Key entities or outcomes involved
    3. Call the "log_ai_action" tool with your generated summary
    
    Your summary should be informative, concise, and focus on the most essential information.
    
    Example good summaries:
    - "User requested Afghanistan projects. Found 15 infrastructure projects, displayed top 5 recent ones including Water Supply Initiative."
    - "User asked about budget status. Retrieved Q3 budget data showing 85% utilization across 12 active projects."
    - "User searched for expired contracts. Found 8 contracts expiring in next 30 days, flagged for renewal."
    
    Always call the log_ai_action tool with the summary you generate.
    """,
    tools=[FunctionTool(func=log_ai_action)],
    model=config_manager.get_gemini_model()
)