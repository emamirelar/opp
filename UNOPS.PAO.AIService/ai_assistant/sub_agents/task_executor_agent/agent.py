"""
Task Planner + Executor Agent (Combined)

This module defines the combined task planner and executor agent that both plans 
and executes user requests using Gemini 2.5 Pro for better speed and performance.
"""

from google.adk.agents import LlmAgent, LoopAgent
from ai_assistant.utils.api_config_manager import config_manager

# Import the tools and callback from utils
from .utils import task_executor_tools, combined_before_model_callback

def dynamic_combined_instruction_callback(ctx):
    """
    Generate dynamic instruction for combined task planner + executor agent
    """
    try:
        # Load tools config to get available entities
        tools_config = config_manager.load_tools_config()
        entities = tools_config.get("entities", [])
        
        # Build dynamic entity list with descriptions and synonyms
        entity_descriptions = []
        
        # Always include Google Workspace entities (external services)
        google_workspace_entities = [
            "**GoogleDoc**: Create Google Documents with content"
        ]
        entity_descriptions.extend(google_workspace_entities)
        
        # Load entity configurations from config manager to get descriptions and synonyms
        try:
            available_entities = config_manager.get_available_entities()
            
            for entity_name in available_entities:
                try:
                    # Load the full entity configuration
                    entity_config = config_manager.load_entity_api_config(entity_name)
                    if entity_config:
                        description = entity_config.get("description", f"{entity_name} entity")
                        synonyms = entity_config.get("synonyms", [])
                        
                        # Format: **EntityName**: Description (Synonyms: synonym1, synonym2)
                        entity_desc = f"**{entity_name}**: {description}"
                        if synonyms:
                            synonyms_text = ", ".join(synonyms)
                            entity_desc += f" (Synonyms: {synonyms_text})"
                        
                        entity_descriptions.append(entity_desc)
                except Exception as e:
                    # Fallback for individual entity loading errors
                    entity_descriptions.append(f"**{entity_name}**: {entity_name} entity")
                    
        except Exception as e:
            # Fallback if config manager is not available
            for entity in entities:
                if isinstance(entity, dict):
                    # Handle both "name" and "entity" field names
                    name = entity.get("name") or entity.get("entity")
                    if name:
                        description = entity.get("description", name + " entity")
                        entity_desc = f"**{name}**: {description}"
                        entity_descriptions.append(entity_desc)
        
        entities_text = "\n".join(["• " + desc for desc in entity_descriptions])
        
        # Get user profile and screen context from state
        user_profile_data = ctx.state.get('user_profile', {})
        screen_context_data = ctx.state.get('screen_context', {})
        
        user_profile_text = "User Profile: " + str(user_profile_data) if user_profile_data else "User Profile: Not available"
        screen_context_text = "Screen Context: " + str(screen_context_data) if screen_context_data else "Screen Context: Not available"
        
        # Build simplified instruction
        instruction = """# Task Planner and Executor Agent

You break down user requests into tasks and execute them step by step using tools only.

## Process
1. **Create action_plan** → 2. **Execute tasks** → 3. **Exit when complete**

## Available Context
- """ + user_profile_text + """
- """ + screen_context_text + """

## Available Entities
""" + entities_text + """

## Step 1: Create Action Plan
Analyze the request and generate steps:
- **step**: Step number (1, 2, 3...)
- **action**: Plain English description  
- **entity**: Entity name (Partner, Contact, GoogleDoc, etc.)
- **intent**: Search, Create, Update, Delete
- **params**: JSON object with parameters

## Step 2: Execute with Tools

**invoke_api_tool** - Automatically finds and calls API endpoints
PARAMETERS: entity_name, intent, params, isMultiToolRequest

**search_agent** - Performs web/Google searches  
PARAMETERS: query, isMultiToolRequest

**read_content_from_url** - Reads content from any URL
PARAMETERS: url, isMultiToolRequest

**convert_markdown_to_google_doc** - Creates Google Documents
PARAMETERS: markdown_content, filename, isMultiToolRequest

**exit_loop_on_success** - Terminates the agent loop

**isMultiToolRequest**: Set to `true` when user's request involves multiple tools, `false` for single tool requests.

## Examples

**"Get partners"** → `invoke_api_tool("Partner", "Search", {}, false)` → `exit_loop_on_success()`

**"Create partner doc"** → `invoke_api_tool("Partner", "Search", {}, true)` → `convert_markdown_to_google_doc(content, "Partners", false)` → `exit_loop_on_success()`

## Exit Rules
- **Success/Error/Missing Info** → Always call `exit_loop_on_success()`

**CRITICAL**: Every execution path must end with `exit_loop_on_success()`.

## Final Response Format
Return results in this simple JSON structure:

```json
{
  "results": [
    {
      "tool": "tool_name",
      "type": "json|markdown|text",
      "content": "raw_output_from_tool"
    }
  ]
}
```

## Output Type Rules
**CRITICAL OUTPUT RULES:**
- **invoke_api_tool results**: Always type "json" with raw API data (arrays/objects)
- **search_agent results**: Always type "markdown" 
- **convert_markdown_to_google_doc**: Always type "text" with success message
- **read_content_from_url**: Type "markdown" or "text" based on content
- If there is a need to ask a followup question to the user, include it as another markdown object in the results array.

**DO NOT convert API data to markdown - keep as raw JSON.**
"""
        
        return instruction
        
    except Exception as e:
        # Fallback instruction if entity loading fails
        user_profile_fallback = str(ctx.state.get('user_profile', 'Not available'))
        screen_context_fallback = str(ctx.state.get('screen_context', 'Not available'))
        
        return """# Task Planner and Executor Agent (Fallback)

You break down user requests into tasks and execute them step by step using tools only.

## Process
1. **Create action_plan** → 2. **Execute tasks** → 3. **Exit when complete**

## Available Context
- User Profile: """ + user_profile_fallback + """
- Screen Context: """ + screen_context_fallback + """

## Available Entities
• **Partner**: Partner organizations
• **Contact**: Individual contacts
• **Interaction**: Communications
• **Opportunity**: Engagements
• **GoogleDoc**: Create documents

## Step 1: Create Action Plan
Analyze the request and generate steps:
- **step**: Step number (1, 2, 3...)
- **action**: Plain English description  
- **entity**: Entity name (Partner, Contact, GoogleDoc, etc.)
- **intent**: Search, Create, Update, Delete
- **params**: JSON object with parameters

## Step 2: Execute with Tools

**invoke_api_tool** - Automatically finds and calls API endpoints
PARAMETERS: entity_name, intent, params, isMultiToolRequest

**search_agent** - Performs web/Google searches  
PARAMETERS: query, isMultiToolRequest

**read_content_from_url** - Reads content from any URL
PARAMETERS: url, isMultiToolRequest

**convert_markdown_to_google_doc** - Creates Google Documents
PARAMETERS: markdown_content, filename, isMultiToolRequest

**exit_loop_on_success** - Terminates the agent loop

**isMultiToolRequest**: Set to `true` when user's request involves multiple tools, `false` for single tool requests.

## Examples

**"Get partners"** → `invoke_api_tool("Partner", "Search", {}, false)` → Auto-exits

**"Create partner doc"** → `invoke_api_tool("Partner", "Search", {}, true)` → `convert_markdown_to_google_doc(content, "Partners", false)` → Auto-exits

## Exit Rules
- **Success/Error/Missing Info** → Always call `exit_loop_on_success()` or auto-exit for single tools

**CRITICAL**: Every execution path must end with `exit_loop_on_success()` or auto-exit.

## Final Response Format
Return results in this JSON structure:
```json
{
  "results": [
    {
      "tool": "tool_name",
      "type": "json|markdown|text",
      "content": "raw_output_from_tool"
    }
  ]
}
```

## Output Type Rules
**CRITICAL OUTPUT RULES:**
- **invoke_api_tool results**: Always type "json" with raw API data (arrays/objects)
- **search_agent results**: Always type "markdown" 
- **convert_markdown_to_google_doc**: Always type "text" with success message
- **read_content_from_url**: Type "markdown" or "text" based on content

**DO NOT convert API data to markdown - keep as raw JSON for frontend to render as cards.**

Example for "list partners":
```json
{
  "results": [
    {
      "tool": "invoke_api_tool",
      "type": "json", 
      "content": [{"id": 1, "name": "UNICEF", "status": "Active"}, {"id": 2, "name": "WHO", "status": "Active"}]
    }
  ]
}
```
"""

# The combined LLM agent using Gemini 2.5 Pro for better performance
task_executor_llm_agent = LlmAgent(
    name="task_planner_executor_llm_agent", 
    description="Task Planner and Executor Agent",
    model="gemini-2.5-flash-lite",  # Use Gemini 2.5 Pro for better performance
    instruction=dynamic_combined_instruction_callback,  # Use dynamic instruction callback
    tools=task_executor_tools,
    before_model_callback=combined_before_model_callback,
    include_contents="default",
    output_key="final_result"
)

# The LoopAgent that wraps the LLM agent for iterative execution
task_executor_agent = LoopAgent(
    name="task_planner_executor_agent",
    description="Task Planner and Executor Agent",
    sub_agents=[task_executor_llm_agent],
    max_iterations=10  # Prevent infinite loops
)