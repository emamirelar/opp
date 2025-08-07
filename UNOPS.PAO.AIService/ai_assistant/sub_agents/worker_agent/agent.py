"""
Main Workflow Agent

This module defines the main workflow agent that orchestrates various sub-agents
for processing user requests in a sequential manner.
"""

from google.adk.agents import SequentialAgent
from ..task_executor_agent import task_executor_agent  
from ..response_formatter_agent import response_formatter_agent
from .utils import log_workflow_start, log_workflow_complete
from ..entity_detection_agent import entity_detection_agent
from ..action_log_agent import action_log_agent

worker_agent = SequentialAgent(
    name="worker_agent",
    description="A comprehensive worker agent that processes user requests through API operations or any tool calls, and response formatting",
    sub_agents=[
        entity_detection_agent,
        task_executor_agent,       # Step 1: Process detected entities and make API calls or any tool calls  
        #action_log_agent,
        response_formatter_agent   # Step 2: Format API results into user-friendly responses
    ],
)