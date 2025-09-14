"""
User Request Agent

This module defines the user request agent that handles user requests and routes them
to appropriate workflows or provides direct responses.
"""

import json
from typing import Optional
from google.adk.agents import LlmAgent
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ..task_executor_agent.agent import task_executor_agent
from ai_assistant.utils.api_config_manager import config_manager
from .utils import enforce_json_format_callback, handle_audio_artifacts_before_model


# Instruction with mixed static config values and state placeholders
def get_user_request_instruction_with_state(ctx=None) -> str:
    """
    Generate instruction for the user request agent with cleaner, tool-based approach
    """
    try:
        application_name = config_manager.get_application_name()
    except Exception:
        application_name = "Opportunity+"
    
    # Extract context data if available
    user_profile_data = "Not available"
    screen_context_data = "Not available"
    
    if ctx and hasattr(ctx, 'state'):
        state = ctx.state
        
        user_profile = state.get('user_profile', {})
        if user_profile:
            user_profile_data = json.dumps(user_profile, indent=2)
            
        screen_context = state.get('screen_context', {})
        if screen_context:
            screen_context_data = json.dumps(screen_context, indent=2)
    
    # Get available entities dynamically
    try:
        tools_config = config_manager.load_tools_config()
        entities = tools_config.get("entities", [])
        entity_names = []
        
        # Add entities from config
        for entity in entities:
            if isinstance(entity, dict):
                name = entity.get("name") or entity.get("entity")
                if name and name not in ["GoogleDrive", "GoogleDoc", "GoogleSheet", "Permission"]:
                    entity_names.append(name)
        
        entities_text = ", ".join(entity_names) if entity_names else "Partner, Contact, Interaction, Opportunity, User, UserProfile"
        
    except Exception:
        entities_text = "No entities available"
    
    instruction = f"""You are the main AI assistant for {application_name}.

**Available Context Data**

**User Profile:**
{user_profile_data}

**Screen Context:**
{screen_context_data}

## Request Analysis

Analyze the user's request and decide:

**HANDLE DIRECTLY** (return JSON with "result" key):
- Greetings: "Hi", "Hello", "Good morning", etc.
- Confirmations: "Yes", "No", "Okay", "Thanks", etc.
- Simple conversational responses
- Basic pleasantries

**REDIRECT TO task_executor_agent** :
- Data operations (search, create, update, delete)
- File operations and Google Drive requests
- Multi-step workflows
- Any request requiring tools or external data
- Complex questions needing business logic

## Response Formats

## Final Response Format
Return results in this simple JSON structure:

```json
{{
  "results": [
    {{
      "tool": "tool_name",
      "type": "json|markdown|text|mermaid",
      "content": "raw_output_from_tool"
    }}
  ]
}}
```

## Output Type Rules
**CRITICAL OUTPUT RULES:**
- **invoke_api_tool results**: Always type "json" with raw API data (arrays/objects)
- **search_agent results**: Always type "markdown" 
- **convert_markdown_to_google_doc**: Always type "text" with success message
- **read_content_from_url**: Type "markdown" or "text" based on content
- If there is a need to ask a followup question to the user, include it as another markdown object in the results array.

**DO NOT convert API data to markdown - keep as raw JSON.**

## Critical Instructions

- **BE VERY SELECTIVE** - Only handle simple greetings and confirmations directly
- **When in doubt, redirect to task_executor_agent**
- **Use exact format**: ALWAYS RESULT IN JSON FORMAT
- **NO other output formats**
- **Personalize responses** using user profile data

**NEVER use print() or function calls - return exact formats only!**
"""
    print(f"🔧 [USER-REQUEST-AGENT] Instruction: {instruction}")
    return instruction

def get_global_instruction(ctx=None) -> str:
    """
    Comprehensive global instruction containing ALL capabilities, personality,
    and behavioral guidelines. This is inherited by ALL sub-agents.
    Note: ctx parameter is required by Google ADK but not used here since we use static config
    """
    try:
        application_name = config_manager.get_application_name()
        project_name = config_manager.get_project_name()
    except Exception:
        application_name = "Opportunity+"
        project_name = "UNOPS"
    
    # Get available entities dynamically
    try:
        tools_config = config_manager.load_tools_config()
        entities = tools_config.get("entities", [])
        entity_names = []
        for entity in entities:
            if isinstance(entity, dict):
                name = entity.get("name") or entity.get("entity")
                if name and name not in ["GoogleDrive", "GoogleDoc", "GoogleSheet", "Permission"]:
                    entity_names.append(name)
        
        if entity_names:
            entities_text = f"**Available Business Entities**: {', '.join(entity_names)}"
        else:
            entities_text = "**Available Business Entities**: Partner, Contact, Interaction, Opportunity, User, UserProfile"
    except Exception:
        entities_text = "**Available Business Entities**: Partner, Contact, Interaction, Opportunity, User, UserProfile"
    
    return f"""# 🤖 AI Assistant Global Instructions

You are an AI assistant for {project_name}'s {application_name}. 
You have extensive tools and capabilities available. Always attempt operations and provide real value to users.

**This global instruction applies to ALL agents in the workflow to ensure consistent behavior and capabilities.**
"""

user_request_agent = LlmAgent(
    name="user_request_agent",
    model="gemini-2.5-flash-lite",
    description="Main AI assistant that handles simple requests directly or redirects to task_executor_agent",
    instruction=get_user_request_instruction_with_state,
    global_instruction=get_global_instruction,
    sub_agents=[task_executor_agent],  # Direct connection with tool discovery in after_model_callback
    before_model_callback=handle_audio_artifacts_before_model,
    output_key="detected_tools"
)
