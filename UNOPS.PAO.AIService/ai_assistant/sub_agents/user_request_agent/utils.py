"""
User Request Agent Utilities

This module contains utility functions for the user request agent,
including request classification and routing logic.
"""

import re
import json
import logging
from typing import Dict, List, Any, Optional
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ai_assistant.utils.api_config_manager import config_manager

logger = logging.getLogger(__name__)


def enforce_json_format_callback(callback_context: CallbackContext, llm_response: types.GenerateContentResponse) -> Optional[types.GenerateContentResponse]:
    """
    After model callback for UserRequestAgent to ensure the response is valid JSON.
    If the response is not valid JSON or if it's a tool call, it handles it appropriately.

    Args:
        callback_context: The callback context from Google ADK
        llm_response: The raw response object from the LLM

    Returns:
        Optional[types.GenerateContentResponse]: Modified response or None to use original
    """
    logger.info("Starting after_model_callback for UserRequestAgent: Enforcing JSON format.")

    llm_output_text = ""
    has_function_call = False

    try:
        if llm_response and llm_response.candidates:
            first_candidate = llm_response.candidates[0]
            if first_candidate.content and first_candidate.content.parts:
                for part in first_candidate.content.parts:
                    if hasattr(part, 'text') and part.text:
                        llm_output_text += part.text
                    elif hasattr(part, 'function_call') and part.function_call:
                        has_function_call = True
                        logger.info("LLM response contains a function call. Skipping text extraction and JSON validation.")
                        # If a function call is detected, we typically don't
                        # want to modify the response as it will be handled
                        # by the agent's tool execution logic.
                        return None # Return None to use the original response for tool calls

        if has_function_call:
            return None # Already handled above, but double-check

        if not llm_output_text:
            raise ValueError("Empty text content from LLM response (or only non-text parts).")

        # Try to parse as JSON first
        try:
            # Check if JSON is embedded in markdown code block
            if "```json" in llm_output_text:
                json_start = llm_output_text.find("```json") + len("```json")
                json_end = llm_output_text.rfind("```")
                # Handle cases where ```json might be at the end, or no closing ```
                if json_end == -1 or json_end < json_start:
                    json_string = llm_output_text[json_start:].strip()
                else:
                    json_string = llm_output_text[json_start:json_end].strip()
                parsed_json_output = json.loads(json_string)
            else:
                parsed_json_output = json.loads(llm_output_text)
            
            # Validate basic structure
            if isinstance(parsed_json_output, dict) and "result" in parsed_json_output:
                # Ensure followUps is present and limited to 3 items
                if "followUps" not in parsed_json_output:
                    parsed_json_output["followUps"] = []
                elif not isinstance(parsed_json_output["followUps"], list):
                    logger.warning("followUps is not a list, forcing to empty list.")
                    parsed_json_output["followUps"] = []
                elif len(parsed_json_output["followUps"]) > 3:
                    parsed_json_output["followUps"] = parsed_json_output["followUps"][:3]
                
                # Return modified response with cleaned JSON
                return types.GenerateContentResponse(
                    candidates=[
                        types.Candidate(
                            content=types.Content(
                                role="model",
                                parts=[types.Part(text=json.dumps(parsed_json_output, indent=2))]
                            )
                        )
                    ]
                )
            else:
                # If it parsed as JSON but didn't have "result", wrap it as markdown
                logger.warning(f"Parsed JSON but missing 'result' key. Wrapping as markdown. Content: {parsed_json_output}")
                raise ValueError("JSON missing required 'result' field (or invalid structure for markdown wrapping).")
                
        except (json.JSONDecodeError, ValueError) as json_err:
            # If it's not valid JSON or doesn't have the right structure,
            # wrap it in proper JSON format with type markdown
            logger.info(f"Response is not valid JSON or structured incorrectly ({json_err}). Wrapping in markdown format. Original Text: '{llm_output_text.strip()[:100]}...'")
            
            wrapped_response = {
                "result": [
                    {
                        "type": "markdown",
                        "message": llm_output_text.strip()
                    }
                ],
                "followUps": []
            }
            
            # Return modified response with wrapped JSON
            return types.GenerateContentResponse(
                candidates=[
                    types.Candidate(
                        content=types.Content(
                            role="model",
                            parts=[types.Part(text=json.dumps(wrapped_response, indent=2))]
                        )
                    )
                ]
            )

    except Exception as e:
        logger.error(f"An unexpected error occurred in after_model_callback: {e}", exc_info=True)
        # Fallback to a safe default response
        default_response = {
            "result": [
                {
                    "type": "markdown",
                    "message": "I'm sorry, an unexpected issue occurred. Could you please try asking again?"
                }
            ],
            "followUps": []
        }
        
        # Return default response
        return types.GenerateContentResponse(
            candidates=[
                types.Candidate(
                    content=types.Content(
                        role="model",
                        parts=[types.Part(text=json.dumps(default_response, indent=2))]
                    )
                )
            ]
        )


def classify_request_type(user_input: str) -> str:
    """
    Classify the type of user request to help with routing decisions.
    
    Args:
        user_input: The user's input text
        
    Returns:
        str: The classified request type
    """
    if not user_input:
        return "unknown"
    
    user_input_lower = user_input.lower().strip()
    
    # Greeting patterns
    greeting_patterns = [
        r'\b(hi|hello|hey|good morning|good afternoon|good evening)\b',
        r'\b(how are you|what\'s up|greetings)\b',
        r'^(hi|hello|hey)[\s!.]*$'
    ]
    
    if any(re.search(pattern, user_input_lower) for pattern in greeting_patterns):
        return "greeting"
    
    # Gratitude patterns
    gratitude_patterns = [
        r'\b(thank you|thanks|thank u|thx)\b',
        r'\b(appreciate|grateful)\b'
    ]
    
    if any(re.search(pattern, user_input_lower) for pattern in gratitude_patterns):
        return "gratitude"
    
    # Knowledge/explanation requests
    knowledge_patterns = [
        r'^(what is|what are|how do|how to|explain|define)',
        r'\b(tell me about|help me understand|can you explain)\b'
    ]
    
    if any(re.search(pattern, user_input_lower) for pattern in knowledge_patterns):
        return "knowledge_request"
    
    # Data operation requests
    data_patterns = [
        r'\b(show me|get me|find|search|list|display)\b',
        r'\b(create|add|update|edit|delete|remove)\b',
        r'\b(business|entities|data|records)\b'
    ]
    
    if any(re.search(pattern, user_input_lower) for pattern in data_patterns):
        return "data_operation"
    
    # Document/file operations
    document_patterns = [
        r'\b(create document|create doc|make document)\b',
        r'\b(documents|files|reports|export)\b',
        r'\b(google doc|google sheet|spreadsheet)\b'
    ]
    
    if any(re.search(pattern, user_input_lower) for pattern in document_patterns):
        return "document_operation"
    
    # Cache management
    cache_patterns = [
        r'\b(cache stats|clear cache|refresh cache)\b',
        r'\b(performance|stats|metrics)\b'
    ]
    
    if any(re.search(pattern, user_input_lower) for pattern in cache_patterns):
        return "cache_management"
    
    return "general"


def should_delegate_to_workflow(request_type: str, user_input: str) -> bool:
    """
    Determine if the request should be delegated to worker_agent.
    
    Args:
        request_type: The classified request type
        user_input: The original user input
        
    Returns:
        bool: True if should delegate to workflow, False if can handle directly
    """
    # Always delegate data operations and document operations
    if request_type in ["data_operation", "document_operation"]:
        return True
    
    # Delegate complex requests or anything involving specific entities
    entity_mentions = [
        "partner", "contact", "interaction", "opportunity", 
        "organization", "user", "project", "document"
    ]
    
    if any(entity in user_input.lower() for entity in entity_mentions):
        # If it's just a knowledge request about entities, might handle directly
        if request_type == "knowledge_request":
            return False
        return True
    
    # Don't delegate simple greetings, gratitude, or knowledge requests
    if request_type in ["greeting", "gratitude", "knowledge_request"]:
        return False
    
    # Delegate cache management (it has specific tools)
    if request_type == "cache_management":
        return True
    
    # Default to delegation for anything else
    return True


def get_team_entities() -> List[str]:
    """Get available entity names from the team's configuration - no predefined priorities"""
    try:
        # Get all available entities without any artificial prioritization
        all_entities = config_manager.get_available_entities()
        
        # Filter out system/admin entities but don't impose priority order
        user_entities = []
        system_entities = {'configuration', 'system', 'permission', 'values', 'systemadmin', 'usermanagement', 'userprofile'}
        
        for entity in all_entities:
            if entity.lower() not in system_entities:
                user_entities.append(entity.lower())
        
        # Return all user entities - let the agent decide which to mention and how
        return user_entities if user_entities else ["your business data"]
    except Exception:
        return ["your business data"]

def get_dynamic_greeting(user_name: Optional[str] = None) -> str:
    """
    Generate a basic greeting with application name - let agent decide entity context.
    
    Args:
        user_name: Optional user name for personalization
        
    Returns:
        str: A simple greeting with application name, letting agent decide content contextually
    """
    # Get application name from branding config
    application_name = config_manager.get_application_name()
    
    # Keep greeting simple - let agent decide what entities/capabilities to mention
    if user_name:
        return f"Hi {user_name}! 👋 Welcome to {application_name}! How can I help you today?"
    else:
        return f"Hi there! 👋 Welcome to {application_name}! How can I help you today?"



def get_friendly_greeting(user_name: Optional[str] = None) -> str:
    """
    Generate a friendly greeting message.
    
    Args:
        user_name: Optional user name for personalization
        
    Returns:
        str: A friendly greeting message
    """
    # Use the new dynamic greeting
    return get_dynamic_greeting(user_name)


def get_gratitude_response(user_name: Optional[str] = None) -> str:
    """
    Generate a response to user gratitude.
    
    Args:
        user_name: Optional user name for personalization
        
    Returns:
        str: A friendly response to gratitude
    """
    if user_name:
        return f"You're very welcome, {user_name}! I'm always happy to help. If you need anything else, just let me know!"
    else:
        return "You're very welcome! I'm always happy to help. If you need anything else, just let me know!"


def get_suggested_followups(request_type: str, context: Dict[str, Any] = None) -> List[str]:
    """
    Get relevant follow-up suggestions based on request type and context.
    
    Args:
        request_type: The classified request type
        context: Optional context information
        
    Returns:
        List[str]: List of suggested follow-up actions (max 3)
    """
    context = context or {}
    
    if request_type == "greeting":
        # Let the agent decide suggestions contextually based on available entities
        # The agent will generate appropriate suggestions dynamically
        return []
    
    elif request_type == "data_operation":
        # Context-specific suggestions based on what was just done
        screen_context = context.get("screen_context", {})
        entity_type = screen_context.get("entity_in_focus")
        
        # Let agent decide suggestions contextually based on actual entity type
        if entity_type:
            return []  # Agent will generate contextual suggestions
        else:
                    return []  # Agent will generate contextual suggestions
    
    elif request_type == "document_operation":
        return [
            "Create another document",
            "Export data to spreadsheet",
            "Search existing documents"
        ]
    
    elif request_type == "knowledge_request":
        return [
            "Search for more information",
            "View related entities",
            "Create summary document"
        ]
    
    # Return empty list if no meaningful suggestions
    return []


def format_response(message: str, response_type: str = "markdown", sources: List[Dict] = None, followups: List[str] = None) -> Dict[str, Any]:
    """
    Format the response in the expected JSON structure.
    
    Args:
        message: The response message
        response_type: Type of response (markdown, mermaid, etc.)
        sources: Optional list of sources
        followups: Optional list of follow-up suggestions
        
    Returns:
        Dict: Formatted response structure
    """
    response = {
        "result": [
            {
                "type": response_type,
                "message": message
            }
        ]
    }
    
    if sources:
        response["sources"] = sources
    
    if followups:
        response["followUps"] = followups[:3]  # Limit to max 3
    else:
        response["followUps"] = []
    
    return response

# --- Proper Before Model Callback for File Processing ---
def handle_audio_artifacts_before_model(callback_context: CallbackContext, llm_request) -> None:
    """
    Before model callback to handle audio file processing and artifact saving.
    This is the correct ADK way to process incoming files before LLM processing.
    """
    try:
        callback_context.state["execution_result"] = []
        # Check if there are audio files in the uploaded files metadata
        uploaded_files_metadata = callback_context.state.get('uploaded_files_metadata', [])
        audio_files_metadata = callback_context.state.get('audio_files_metadata', [])
        
        if not uploaded_files_metadata and not audio_files_metadata:
            return
        
        files_info_for_context = []
        
        # Process the uploaded files metadata to set up STT tools access
        for file_meta in uploaded_files_metadata:
            filename = file_meta.get('filename', 'unknown_file')
            mime_type = file_meta.get('mime_type', 'application/octet-stream')
            is_audio = file_meta.get('is_audio', mime_type.startswith('audio/'))
            
            file_info = {
                'filename': filename,
                'mime_type': mime_type,
                'is_audio': is_audio,
                'size': file_meta.get('size', 0)
            }
            files_info_for_context.append(file_info)
            
            # Set flags for STT tools to know what files are available
            if is_audio:
                callback_context.state['has_audio_files'] = True
                audio_files = callback_context.state.get('available_audio_files', [])
                audio_files.append(file_info)
                callback_context.state['available_audio_files'] = audio_files
        
        # Store all file info in session state for tools to access
        if files_info_for_context:
            callback_context.state['uploaded_artifacts'] = files_info_for_context
        else:
            callback_context.state['has_audio_files'] = False
            
    except Exception as e:
        logging.error(f"Error in audio file processing callback: {e}")


def handle_delegation_after_model(callback_context: CallbackContext, llm_response):
    """
    After model callback to handle delegation to worker_agent when action_plan is generated.
    """
    import json
    from google.genai import types
    
    try:
        if not llm_response or not llm_response.candidates:
            return None
            
        # Extract the response text
        response_text = ""
        for candidate in llm_response.candidates:
            if candidate.content and candidate.content.parts:
                for part in candidate.content.parts:
                    if hasattr(part, 'text') and part.text:
                        response_text += part.text
        
        if not response_text:
            return None
        
        # Try to parse as JSON
        try:
            response_json = json.loads(response_text)
            
            # Check if this is an action_plan response (delegation case)
            if isinstance(response_json, dict) and "action_plan" in response_json:
                action_plan = response_json["action_plan"]
                
                
                # Store the action plan in state for worker_agent to use
                # The task_executor_agent expects {{action_plan}} to be a JSON string
                action_plan_json = json.dumps(action_plan, indent=2)
                
                callback_context.state["action_plan"] = action_plan_json  # JSON string for template substitution
                callback_context.state["action_plan_raw"] = action_plan  # Raw object for programmatic use
                callback_context.state["entity_intent_detection"] = action_plan
                
                
                # Return the action_plan directly to trigger delegation via output_key
                return {"action_plan": action_plan}
                
        except json.JSONDecodeError:
            # Not JSON, probably a direct response
            pass
            
    except Exception as e:
        logging.error(f"Error in delegation callback: {e}")
    
    return None
