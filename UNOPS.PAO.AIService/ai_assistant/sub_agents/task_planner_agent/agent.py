"""
Task Planner Agent

This module defines the task planner agent that discovers relevant tools
for user queries using the find_relevant_tools function.
"""

from google.adk.agents import LlmAgent
from google.adk.tools import FunctionTool
from ai_assistant.utils.api_config_manager import config_manager

def create_tool_discovery_instruction(ctx):
    """
    Creates focused instruction for tool discovery
    """
    instruction = """# Tool Discovery Agent

You are a specialized agent that discovers the optimal tools for user requests.

## Your Task
Analyze the user's request and call `find_relevant_tools_for_planning` to discover the best tools for execution.

## Instructions
1. **ALWAYS call `find_relevant_tools_for_planning(user_query, max_tools)`**
2. **Use the exact user query** as the first parameter
3. **Set max_tools to 3-5** depending on query complexity
4. **Return the discovered tools** for the execution agent

## Example
User: "Get me the list of engagements of partner VTF"
→ Call: find_relevant_tools_for_planning("Get me the list of engagements of partner VTF", 3)

Always call the tool discovery function first."""
    
    return instruction

# Import the tool function
def find_relevant_tools_for_planning(user_query: str, max_tools: int = 3) -> str:
    """
    Discover relevant tools for the user query
    """
    import json
    
    
    try:
        from ai_assistant.sub_agents.task_executor_agent.dynamic_tool_registry import DynamicToolRegistry
        
        registry = DynamicToolRegistry()
        relevant_tools = registry.find_relevant_tools(user_query, max_tools)
        
        result = {
            "status": "success",
            "relevant_tools": relevant_tools
        }
        
        if result.get("status") == "success":
            relevant_tools = result.get("relevant_tools", [])
            
            return json.dumps({
                "status": "success",
                "relevant_tools": relevant_tools,
                "user_query": user_query
            })
        else:
            return json.dumps({"status": "error", "error": result.get('error', 'Unknown error')})
            
    except Exception as e:
        return json.dumps({"status": "error", "error": str(e)})

task_planner_agent = LlmAgent(
    name="task_planner_agent", 
    description="Discovers relevant tools for user requests",
    model=config_manager.get_gemini_model(),
    instruction=create_tool_discovery_instruction,
    tools=[FunctionTool(func=find_relevant_tools_for_planning)],  # Only the discovery tool
    output_key="relevant_tools",  # Store discovered tools here
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)