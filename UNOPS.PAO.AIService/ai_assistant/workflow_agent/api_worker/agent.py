"""
Enhanced API Worker Agent

This module defines the API worker agent that processes detected entities and makes API calls.
Enhanced with patterns from UNOPS.PAO.AgenticAi for better dynamic configuration.
"""

from google.adk.agents import LoopAgent, LlmAgent
from google.adk.tools.base_tool import BaseTool
from .utilities import invoke_api_tool, exit_loop_on_success
from .callback import inject_entity_specific_tools_before_model, advance_entity_callback


def combined_before_model_callback(callback_context, llm_request=None):
    """Combined callback for entity-specific tools injection"""
    inject_entity_specific_tools_before_model(callback_context, llm_request)
    return None


# Enhanced API caller agent with dynamic entity-specific tool injection
api_caller_agent = LlmAgent(
    name="api_caller_agent",
    description="Agent that makes actual API calls using entity-specific endpoints from dynamic configuration",
    model="gemini-2.0-flash-001",
    instruction="""
    🚀 **API CALLER AGENT**
    
    You are responsible for making REAL HTTP API calls based on detected entities and intents.
    
    **CRITICAL INSTRUCTIONS:**
    1. **Parse Input**: Extract entity, intent, and parameters from the detected information
    2. **Find Endpoint**: Use the entity-specific configuration to find the right API endpoint  
    3. **Construct URL**: Build the complete URL with proper base URL and path
    4. **Map Parameters**: Handle parameter mapping (e.g., "top" → "pageSize")
    5. **Make API Call**: Use `invoke_api_tool(url, method, body)` to make the actual HTTP request
    6. **Handle Response**: Process the response and format it appropriately
    7. **Exit Loop**: Call `exit_loop_on_success()` after successful completion
    
    **EXECUTION FLOW:**
    1. Look for entity information in the input
    2. Determine the appropriate endpoint and HTTP method
    3. Build the complete URL and prepare parameters
    4. Call `invoke_api_tool()` to make the HTTP request
    5. Process the response
    6. Call `exit_loop_on_success()` to complete processing
    
    **REMEMBER:** You MUST actually call `invoke_api_tool()` - don't just describe what you would do!
    """,
    tools=[invoke_api_tool, exit_loop_on_success],
    before_model_callback=combined_before_model_callback,
    include_contents="default",
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)

# Enhanced API worker agent (LoopAgent containing the caller)
api_worker_agent = LoopAgent(
    name="api_worker_agent", 
    description="Enhanced API worker that processes detected entities and makes appropriate API calls using dynamic configuration",
    sub_agents=[api_caller_agent],
    max_iterations=10  # Prevent infinite loops
) 