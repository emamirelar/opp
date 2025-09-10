"""
Main Workflow Agent

This module defines the main workflow agent that orchestrates various sub-agents
for processing user requests in a sequential manner.
"""

from google.adk.agents import SequentialAgent
from ..task_executor_agent import task_executor_agent  
from ..response_formatter_agent import response_formatter_agent
from .utils import log_workflow_start, log_workflow_complete
from ..action_log_agent import action_log_agent

worker_agent = SequentialAgent(
    name="worker_agent",
    description="Streamlined workflow agent with combined task planning and execution for improved speed",
    sub_agents=[
        #task_executor_agent,       # Combined task planner + executor (using Gemini 2.5 Pro)
        action_log_agent,
        #response_formatter_agent   # Format API results into user-friendly responses
    ],
)