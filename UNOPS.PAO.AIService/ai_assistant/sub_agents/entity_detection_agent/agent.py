"""
Entity Detection Agent

This module defines the entity detection agent that uses dynamic loading from tools.json
to detect entities and intents in user queries.
"""

from google.adk.agents import LlmAgent
from google.genai import types
from ai_assistant.utils.api_config_manager import config_manager
from .utils import extract_entity_intent_before_model, dynamic_instruction_callback

entity_detection_agent = LlmAgent(
    name="entity_detection_agent", 
    description="Background agent that analyzes user requests to detect entities, intents, and visualization requirements using dynamic JSON configuration",
    model=config_manager.get_gemini_model(),
    instruction=dynamic_instruction_callback,  # Use dynamic instruction
    tools=[],  # Explicitly no tools to prevent inheritance
    output_key="action_plan",
    # NOTE: Removed before_model_callback to prevent conflict with dynamic instruction
    # The dynamic_instruction_callback now handles all context injection
    # Add safety configurations to prevent responses to users
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)