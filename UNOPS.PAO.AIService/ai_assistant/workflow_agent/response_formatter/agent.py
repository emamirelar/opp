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


class FormattedResponse(BaseModel):
    """
    Structured response format for frontend rendering
    """
    prefixMessage: Optional[str] = None
    result: Optional[Union[Dict[str, Any], List[Dict[str, Any]], str]] = None
    type: str = "markdown"  # grid | markdown | json | mermaid | card
    postfixMessage: Optional[str] = None
    gems: List[str] = []


response_formatter_agent = LlmAgent(
    name="response_formatter_agent",
    description="Formats API responses into structured JSON responses with appropriate display types for frontend rendering",
    model="gemini-2.0-flash-001",
    instruction=dynamic_response_instruction,  # Use dynamic instruction
    output_key="formatted_response",
    output_schema=FormattedResponse,
    disallow_transfer_to_parent=True,  # Prevent transfer back to parent agents
    before_model_callback=format_response_before_model
) 