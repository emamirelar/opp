"""
Main Workflow Agent

This module defines the main workflow agent that orchestrates various sub-agents
for processing user requests in a sequential manner.
"""

from google.adk.agents import SequentialAgent
from .entity_detection import entity_detection_agent
from .api_worker import api_worker_agent
from .response_formatter import response_formatter_agent


workflow_agent = SequentialAgent(
    name="workflow_agent",
    description="A comprehensive workflow agent that processes user requests through entity detection, API operations, and response formatting",
    sub_agents=[
        entity_detection_agent,    # Step 1: Detect entities and intents from user input
        api_worker_agent,          # Step 2: Process detected entities and make API calls
        response_formatter_agent   # Step 3: Format API results into user-friendly responses
    ],
)