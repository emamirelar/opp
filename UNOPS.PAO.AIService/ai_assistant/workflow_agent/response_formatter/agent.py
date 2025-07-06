"""
Response Formatter Agent

This module defines the response formatter agent that processes API responses
and formats them into structured JSON responses for frontend rendering.
"""

from google.adk.agents import LlmAgent
from google.genai import types
from pydantic import BaseModel
from typing import List, Dict, Any, Optional, Union
from .callback import format_response_before_model, dynamic_response_instruction
from ai_assistant.config_manager import config_manager


class ResponseItem(BaseModel):
    """
    Individual item in the response result array
    """
    type: str  # markdown | card | grid | json | mermaid
    message: Union[str, Dict[str, Any], List[Dict[str, Any]]]
    entity: Optional[str] = None


class SourceItem(BaseModel):
    """
    Source information for responses
    """
    title: str
    url: str
    description: Optional[str] = None


class FormattedResponse(BaseModel):
    """
    Structured response format for frontend rendering
    """
    result: List[ResponseItem]
    sources: Optional[List[SourceItem]] = []
    followUps: List[str] = []

def create_response_agent():    
    """
    Create a response formatter agent
    """
    return LlmAgent(
        name="response_formatter_agent",
        description="Formats API responses into structured JSON responses with appropriate display types for frontend rendering",
        model=config_manager.framework_config['runtime']['gemini_model'],
        instruction=dynamic_response_instruction,  # Use dynamic instruction
        output_key="formatted_response",
        output_schema=FormattedResponse,
        disallow_transfer_to_parent=True,  # Prevent transfer back to parent agents
        before_model_callback=format_response_before_model
    ) 

response_formatter_agent = create_response_agent()