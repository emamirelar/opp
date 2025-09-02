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
    description="A streamlined worker agent that processes action plans and formats responses (task planning now handled by user_request_agent)",
    sub_agents=[
        task_executor_agent,       # Step 1: Process action plans and make API calls or any tool calls  
        #action_log_agent,
        response_formatter_agent   # Step 2: Format API results into user-friendly responses
    ],
)