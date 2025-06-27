"""
Main Workflow Agent

This module defines the main workflow agent that orchestrates various sub-agents
for processing user requests in a sequential manner.
"""

from google.adk.agents import SequentialAgent
from .entity_detection import entity_detection_agent
from .api_worker import api_worker_agent


workflow_agent = SequentialAgent(
    name="workflow_agent",
    description="A comprehensive workflow agent that processes user requests through entity detection and API operations",
    sub_agents=[
        entity_detection_agent,  # Step 1: Detect entities and intents from user input
        api_worker_agent         # Step 2: Process detected entities and make API calls
    ],
)