"""
Entity Detection Agent

This module defines the entity detection agent that uses dynamic loading from tools.json
to detect entities and intents in user queries.
"""

from google.adk.agents import LlmAgent
from google.genai import types
from .callback import extract_entity_intent_before_model, dynamic_instruction_callback
from ai_assistant.config_manager import config_manager


entity_detection_agent = LlmAgent(
    name="entity_detection_agent", 
    description="Advanced entity detection agent that dynamically loads entity and intent information from tools.json configuration",
    model=config_manager.get_gemini_model(),
    instruction=dynamic_instruction_callback,  # Use dynamic instruction
    output_key="entity_intent_detection",
    before_model_callback=extract_entity_intent_before_model
) 