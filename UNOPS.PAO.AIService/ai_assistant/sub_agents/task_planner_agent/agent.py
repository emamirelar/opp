"""
Task Planner Agent

This module defines the task planner agent that uses dynamic loading from tools.json
to detect entities and intents in user queries.
"""

from google.adk.agents import LlmAgent
from google.genai import types
from ai_assistant.utils.api_config_manager import config_manager
from .utils import extract_entity_intent_before_model, dynamic_instruction_callback

task_planner_agent = LlmAgent(
    name="task_planner_agent", 
    description="Smart action planning agent that analyzes user requests and creates step-by-step action plans with entities, intents, and parameters.",
    model=config_manager.get_gemini_model(),
    instruction=dynamic_instruction_callback,  # Use dynamic instruction
    tools=[],  # Explicitly no tools to prevent inheritance
    output_key="action_plan",
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)