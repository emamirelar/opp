"""
Response Formatter Agent

This module defines the response formatter agent that processes API responses
and formats them into structured JSON responses for frontend rendering.
"""

from google.adk.agents import LlmAgent
from google.genai import types
from pydantic import BaseModel
from typing import List, Dict, Any, Optional, Union
from .utils import format_response_before_model, dynamic_response_instruction
from ai_assistant.utils.common_callbacks import response_formatter_after_model_callback
from ai_assistant.utils.api_config_manager import config_manager


class ResponseItem(BaseModel):
    """
    Individual item in the response result array
    
    For 'card' type with multiple entities of the same type:
    - message should be List[Dict[str, Any]] (array of entity objects)
    - entity should specify the entity type (e.g., "Partner", "Contact")
    
    For 'card' type with single entity:
    - message can be Dict[str, Any] (single entity object) 
    - entity should specify the entity type
    
    For 'chartjs' type:
    - message should be Dict[str, Any] with Chart.js configuration
    - chartType should specify chart type (pie, bar, line, etc.)
    - entity should specify what the chart represents
    
    For 'mermaid' type:
    - message should be str with mermaid diagram code
    - entity should specify what the diagram represents
    """
    type: str  # markdown | card | grid | json | mermaid | chartjs
    message: Union[str, Dict[str, Any], List[Dict[str, Any]]]
    entity: Optional[str] = None
    chartType: Optional[str] = None  # For chartjs type: pie, bar, line, doughnut, radar, etc.


class SourceItem(BaseModel):
    """
    Source information for responses
    """
    title: str
    url: str
    description: Optional[str] = None


class DataModificationItem(BaseModel):
    """
    Individual data modification/action performed during the interaction
    """
    type: str  # data_updation | data_creation | data_deletion | data_execution
    message: str  # Description of what was modified/created/deleted
    entity_type: Optional[str] = None  # Type of entity modified (e.g., "project", "partner")
    entity_id: Optional[str] = None    # ID of the entity if applicable


class FormattedResponse(BaseModel):
    """
    Structured response format for frontend rendering
    """
    result: List[ResponseItem]
    sources: Optional[List[SourceItem]] = []
    data_modifications: List[DataModificationItem] = []  # Track all data changes
    suggestedUserResponses: List[str] = []

def create_response_agent():    
    """
    Create a response formatter agent
    """
    return LlmAgent(
        name="response_formatter_agent",
        description="Formats API responses into structured JSON responses with appropriate display types for frontend rendering",
        model=config_manager.get_gemini_model(),
        instruction=dynamic_response_instruction,  # Use dynamic instruction
        tools=[],  # Explicitly no tools to prevent inheritance
        output_key="formatted_response",
        output_schema=FormattedResponse,
        disallow_transfer_to_parent=True,  # Prevent transfer back to parent agents
        before_model_callback=format_response_before_model,
        after_model_callback=response_formatter_after_model_callback
    ) 

response_formatter_agent = create_response_agent()
