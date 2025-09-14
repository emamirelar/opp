"""
Task Executor Agent

This agent receives discovered tools from the task planner and dynamically
appends them to the LlmRequest using before_model_callback for optimal performance.
"""

from typing import Optional
from google.adk.agents import LlmAgent, LoopAgent
from google.adk.tools import FunctionTool, ToolContext
from google.genai import types
from google.adk.models import LlmResponse
from google.adk.agents.callback_context import CallbackContext
from ai_assistant.utils.api_config_manager import config_manager
import json
import copy

def dynamic_tool_injection_callback(llm_request, callback_context):
    """
    Before model callback that:
    1. Checks if relevant_tools exist in state (from detected_tools call)
    2. If yes: injects those tools and clears state
    3. If no: allows normal execution with current tools
    
    Args:
        llm_request: The LlmRequest object to modify
        callback_context: CallbackContext containing discovered tools in state
    """
    
    try:
        # Check if we have discovered tools in state (from detected_tools call)
        relevant_tools_data = None
        discovered_tools = []
        
        if hasattr(callback_context, 'state') and callback_context.state:
            # Check if relevant_tools is available from the previous iteration's output
            try:
                if hasattr(callback_context.state, 'get'):
                    relevant_tools_data = callback_context.state.get('relevant_tools', None)
                else:
                    relevant_tools_data = getattr(callback_context.state, 'relevant_tools', None)
            except Exception as e:
                relevant_tools_data = None
        
        # If we have discovered tools, inject them and clear the state
        if relevant_tools_data:
            # Parse the discovered tools data
            if isinstance(relevant_tools_data, str):
                try:
                    parsed_data = json.loads(relevant_tools_data)
                    if parsed_data.get('status') == 'success':
                        discovered_tools = parsed_data.get('relevant_tools', [])
                        # Store the parsed data back in state for instruction generation
                        callback_context.state['relevant_tools'] = parsed_data
                except json.JSONDecodeError:
                    pass
            elif isinstance(relevant_tools_data, dict):
                discovered_tools = relevant_tools_data.get('relevant_tools', [])
            elif isinstance(relevant_tools_data, list):
                discovered_tools = relevant_tools_data
            
            if discovered_tools:
                # Import the actual tool functions based on discovered tool names
                # Use a set to track unique tool names to avoid duplicates
                tools_to_add = []
                added_tool_names = set()
                
                for tool_info in discovered_tools:
                    tool_name = tool_info.get('name', '') if isinstance(tool_info, dict) else str(tool_info)
                    
                    # Skip if we've already added this tool
                    if tool_name in added_tool_names:
                        continue
                    
                    # Map tool names to actual function implementations
                    if tool_name == 'find_entity_endpoint':
                        try:
                            from ai_assistant.sub_agents.task_executor_agent.utils import find_entity_endpoint
                            tools_to_add.append(FunctionTool(func=find_entity_endpoint))
                            added_tool_names.add(tool_name)
                        except ImportError:
                            pass
                    
                    elif tool_name == 'invoke_api_tool':
                        try:
                            from ai_assistant.sub_agents.task_executor_agent.utils import find_entity_endpoint
                            tools_to_add.append(FunctionTool(func=find_entity_endpoint))
                            added_tool_names.add(tool_name)
                            from ai_assistant.sub_agents.task_executor_agent.utils import invoke_api_tool
                            tools_to_add.append(FunctionTool(func=invoke_api_tool))
                            added_tool_names.add(tool_name)
                        except ImportError:
                            pass
                    
                    elif tool_name == 'search_agent':
                        try:
                            from ai_assistant.sub_agents.task_executor_agent.utils import search_agent
                            from google.adk.tools import AgentTool
                            if hasattr(search_agent, 'name'):
                                tools_to_add.append(AgentTool(agent=search_agent))
                                added_tool_names.add(tool_name)
                        except (ImportError, Exception):
                            pass
                    
                    elif tool_name == 'read_content_from_url':
                        try:
                            from ai_assistant.sub_agents.task_executor_agent.utils import read_content_from_url
                            tools_to_add.append(FunctionTool(func=read_content_from_url))
                            added_tool_names.add(tool_name)
                        except ImportError:
                            pass
                    
                    elif tool_name == 'convert_markdown_to_google_doc':
                        try:
                            from ai_assistant.sub_agents.task_executor_agent.utils import convert_markdown_to_google_doc
                            tools_to_add.append(FunctionTool(func=convert_markdown_to_google_doc))
                            added_tool_names.add(tool_name)
                        except ImportError:
                            pass
                    
                    elif tool_name == 'detected_tools':
                        try:
                            tools_to_add.append(FunctionTool(func=detected_tools))
                            added_tool_names.add(tool_name)
                        except Exception:
                            pass
                
                # Use LlmRequest.append_tools() to dynamically add the discovered tools
                if tools_to_add:
                    # Validate tools before appending
                    valid_tools = []
                    for tool in tools_to_add:
                        try:
                            # Basic validation - check if tool has required attributes
                            if hasattr(tool, 'name') and hasattr(tool, 'description'):
                                valid_tools.append(tool)
                        except Exception:
                            pass
                    
                    if valid_tools:
                        try:
                            llm_request.append_tools(valid_tools)
                            # Change output_key to execution_result now that tools are injected
                            if hasattr(callback_context, 'agent') and hasattr(callback_context.agent, 'output_key'):
                                callback_context.agent.output_key = "execution_result"
                        except Exception:
                            pass
            
    except Exception:
        pass

def task_executor_after_model_callback(
    callback_context: CallbackContext, llm_response: LlmResponse
) -> Optional[LlmResponse]:
    """
    After model callback for task_executor_llm_agent that processes responses silently.
    Does not return messages to the user - all communication happens through state.
    """
    return None

def create_dynamic_instruction(callback_context):
    """
    Creates dynamic instruction based on current state:
    - First iteration: instructs to call detected_tools() to select tools
    - Subsequent iterations: provides focused instruction for discovered tools
    """
    try:
        # Check if we have discovered tools in state
        relevant_tools_data = None
        if hasattr(callback_context, 'state') and callback_context.state:
            relevant_tools_data = callback_context.state.get('relevant_tools', None)
        
        # If no tools discovered yet, present available tools and ask LLM to choose
        if not relevant_tools_data:
            instruction = """# Task Executor Agent - Tool Selection Phase

You need to select the most appropriate tools for the user's request from this list:

**Only these tools are available (STRICTLY SELECT ONLY THESE TOOLS):**
1. **invoke_api_tool** - Call API endpoints for any entity in order to retrieve data
2. **search_agent** - Search web and knowledge bases for latest news, information, research
3. **read_content_from_url** - Read and extract content from web URLs
4. **convert_markdown_to_google_doc** - Create Google Documents from markdown content

**Your Task:**
Analyze the user's request and call ALWAYS: detected_tools(["tool1", "tool2", "tool3"])

**Examples:**
- For "get partner details" → detected_tools(["invoke_api_tool"])
- For "find partner named ABC Corp" → detected_tools(["invoke_api_tool"])
- For "latest news about AI" → detected_tools(["search_agent"])
- For "create a document with partner data" → detected_tools(["invoke_api_tool", "convert_markdown_to_google_doc"])
- For "read content from https://example.com" → detected_tools(["read_content_from_url"])
- For "get partner details and their engagements" → detected_tools(["invoke_api_tool"])

Choose 1-3 most relevant tools and call detected_tools() with the array."""
            
            # Validate instruction length (Gemini has token limits)
            if len(instruction) > 8000:  # Conservative limit
                instruction = "Select appropriate tools using detected_tools() function.", 

            print(f"🔧 [TASK-EXECUTOR-AGENT] Tool detection Instruction: {instruction}")
            
            return instruction
        
        else:
            # We have discovered tools - create focused execution instruction
            discovered_tools = []
            user_query = ""
            
            # Parse the discovered tools data
            if isinstance(relevant_tools_data, str):
                try:
                    parsed_data = json.loads(relevant_tools_data)
                    discovered_tools = parsed_data.get('relevant_tools', [])
                    user_query = parsed_data.get('user_query', '')
                except json.JSONDecodeError:
                    pass
            elif isinstance(relevant_tools_data, dict):
                discovered_tools = relevant_tools_data.get('relevant_tools', [])
                user_query = relevant_tools_data.get('user_query', '')
            elif isinstance(relevant_tools_data, list):
                discovered_tools = relevant_tools_data
            
            if discovered_tools:
                tools_list = []
                for tool in discovered_tools:
                    if isinstance(tool, dict):
                        tool_name = tool.get('name', 'Unknown')
                        tool_desc = tool.get('description', 'No description')
                        score = tool.get('score', 0)
                        tools_list.append(f"- **{tool_name}** (relevance: {score}%) - {tool_desc}")
                    else:
                        tools_list.append(f"- **{tool}** - Optimal tool for this task")
                
                tools_text = '\n'.join(tools_list)
                
                # Create dynamic examples based on available tools
                tool_names = [tool.get('name', 'Unknown') if isinstance(tool, dict) else str(tool) for tool in discovered_tools]
                examples = []
                
                if 'find_and_invoke_api_tool' in tool_names:
                    examples.append('find_and_invoke_api_tool(entity_name="Partner", intent="search", params="{\"query\": \"ABC\"}")')
                elif 'invoke_api_tool' in tool_names:
                    examples.append('invoke_api_tool(entity_name="Partner", intent="search", params={{"query": "ABC"}})')
                if 'search_agent' in tool_names:
                    examples.append('search_agent(query="latest AI news")')
                if 'read_content_from_url' in tool_names:
                    examples.append('read_content_from_url(url="https://example.com")')
                if 'convert_markdown_to_google_doc' in tool_names:
                    examples.append('convert_markdown_to_google_doc(markdown_content="# Title")')
                
                examples_text = '\n- '.join(examples) if examples else 'Use the available tools appropriately'
                
                instruction = f"""# Task Executor Agent

Execute tools iteratively to answer the user's request: "{user_query}"

Available tools:
{tools_text}

## Execution Strategy:
1. **Start with the most specific tool first** (e.g., if looking for a specific partner, use invoke_api_tool to search)
2. **Use find_entity_endpoint FIRST** to discover the right API endpoint before calling invoke_api_tool
3. **Analyze results** - if you get empty results or no matches, this may be a valid completion
4. **For multi-part requests** (e.g., "partner X AND their engagements"):
   - First find the specific entity (partner X)
   - If entity not found, stop here (cannot proceed to engagements)
   - If entity found, then search for related data (engagements)
5. **Call tools one at a time** and wait for results before proceeding

## Critical Function Call Syntax:
- {examples_text}
- **NEVER use**: call:toolname{{...}} format
- **ALWAYS use**: toolname(parameter="value")

## When to Stop:
- ✅ All requested data retrieved successfully
- ✅ Specific entity not found (valid completion - inform user)
- ❌ Continue if you got partial results but more data was requested

## Example Workflow:
For "Get partner ABC and their engagements":
1. find_entity_endpoint(entity_name="Partner", intent="search", extracted_params='{{"query": "ABC"}}')
2. invoke_api_tool(entity_name="Partner", intent="search", params={{"query": "ABC"}})
3. If partner found → find_entity_endpoint for engagements
4. If partner NOT found → task complete (inform user no partner found)"""
                
                # Validate instruction length (Gemini has token limits)
                if len(instruction) > 8000:  # Conservative limit
                    # Truncate tools list if too long
                    truncated_tools = tools_list[:2]  # Keep only first 2 tools
                    tools_text = '\n'.join(truncated_tools)
                    # Create dynamic examples for truncated version too
                    truncated_examples = []
                    for tool in truncated_tools:
                        if 'invoke_api_tool' in tool:
                            truncated_examples.append('invoke_api_tool(entity_name="Partner", intent="search", params={{"query": "ABC"}})')
                        elif 'search_agent' in tool:
                            truncated_examples.append('search_agent(query="latest AI news")')
                        elif 'read_content_from_url' in tool:
                            truncated_examples.append('read_content_from_url(url="https://example.com")')
                    
                    truncated_examples_text = '\n- '.join(truncated_examples) if truncated_examples else 'Use available tools appropriately'
                    
                    instruction = f"""# Task Executor Agent

Execute tools iteratively to answer the user's request: "{user_query}"

Available tools:
{tools_text}

## Execution Strategy:
1. **Use find_entity_endpoint FIRST** before invoke_api_tool
2. **Call tools one at a time** and analyze results
3. **Stop if specific entity not found** (valid completion)

## Function Call Syntax:
- {truncated_examples_text}
- **NEVER use**: call:toolname{{...}} format"""

                print(f"🔧 [TASK-EXECUTOR-AGENT] Instruction: {instruction}")
                
                return instruction
            
            else:
                fallback_instruction = """# Task Executor Agent

No relevant tools discovered. Use available tools to complete the request."""

                print(f"🔧 [TASK-EXECUTOR-AGENT] Fallback Instruction: {fallback_instruction}")

                return fallback_instruction
        
    except Exception:
        return "Execute the user's request using available tools."

# Import the basic tool for first iteration
from ai_assistant.sub_agents.task_executor_agent.utils import exit_loop_on_success

def add_markdown_message(message: str, tool_context: Optional[ToolContext] = None) -> str:
    """
    Add a markdown message to the execution results in state.
    
    Args:
        message: The markdown message to add
        tool_context: Tool context for accessing state
        
    Returns:
        JSON string with the simple format
    """
    result = {
        "tool": "markdown",
        "type": "markdown",
        "message": message
    }
    
    # Append to state execution_result array
    if tool_context and hasattr(tool_context, 'state'):
        if not hasattr(tool_context.state, 'execution_result') or tool_context.state.execution_result is None:
            tool_context.state.execution_result = []
        tool_context.state.execution_result.append(result)
    
    return json.dumps(result)

def detected_tools(tools_array: list, tool_context: Optional[ToolContext] = None) -> str:
    """
    Set the detected tools in state for dynamic injection.
    
    Args:
        tools_array: List of tool names that the LLM has selected
        tool_context: Tool context for accessing state
        
    Returns:
        JSON string confirming the tools were set
    """
    import json
    
    # Convert tool names to the format expected by dynamic injection
    relevant_tools = []
    for tool_name in tools_array:
        relevant_tools.append({
            "name": tool_name,
            "description": f"Selected tool: {tool_name}",
            "score": 100,  # All selected tools get equal priority
            "parameters": []
        })
    
    result = {
        "status": "success",
        "relevant_tools": relevant_tools,
        "user_query": "User query processed by LLM tool selection"
    }
    
    # Store in state for dynamic injection
    if tool_context and hasattr(tool_context, 'state'):
        tool_context.state['relevant_tools'] = result
    
    return json.dumps(result)


# Create the Task Termination Agent
task_termination_agent = LlmAgent(
    name="task_termination_agent",
    description="Checks if the task is complete and terminates the loop when done",
    model="gemini-2.5-flash-lite",
    instruction="""# Task Termination Agent

You are responsible for checking if the current task has been completed successfully and terminating the loop when appropriate.

## Your Role
1. **Carefully analyze the user's original request** to understand ALL required data
2. **Review the execution results** to see what has been accomplished
3. **Call `exit_loop_on_success()` ONLY when ALL parts of the request are fulfilled**
4. **Do NOT call exit_loop_on_success() if ANY part of the request is missing**

## When to Terminate (ALL conditions must be met)
- ✅ **ALL requested data successfully retrieved** (not just partial data)
- ✅ **User's complete question fully answered** (every part addressed)
- ✅ **No errors in the results** (valid data returned)
- ✅ **No follow-up data needed** (request completely satisfied)

## When NOT to Terminate  
- ❌ **Only tool discovery completed** (still need to execute the actual task)
- ❌ **Partial data retrieved** (e.g., got partners but missing their engagements)
- ❌ **API/System errors occurred** (need to retry or handle errors)
- ❌ **Incomplete data** (missing any part of the user's request)
- ❌ **Related data missing** (e.g., user asked for "partners AND engagements" but only got partners)

## When TO Terminate (Even with No Results)
- ✅ **Specific entity not found and this is needed to proceed the other parts of the request** (e.g., searched for "Partner ABC" but no matches found)
- ✅ **Valid search completed** but returned empty results (this is a valid completion)
- ✅ **User requested non-existent data** and search confirmed it doesn't exist

## Examples of Complete vs Incomplete Tasks

**User Request**: "Get me list of partners and their related engagements"
- ❌ **Incomplete**: Only partners list retrieved → Need engagements too
- ✅ **Complete**: Partners list + their related engagements retrieved

**User Request**: "Show me partner details for ACME Corp"  
- ❌ **Incomplete**: Found partner but API failed to get details → Need to retry
- ✅ **Complete**: Full partner details successfully retrieved

**User Request**: "Get me details of partner The Sunrise Project Australia Limited"
- ✅ **Complete**: Search completed but no partner found with that name → Valid completion, inform user
- ❌ **Incomplete**: Search failed due to API error → Need to retry

## Critical Rules
- **Analyze the user's request for ALL components** (look for "and", "with", "including", etc.)
- **ONLY call `exit_loop_on_success()` when EVERY part is fulfilled**
- **If ANY part is missing, explain what still needs to be done**
- **Be thorough - don't terminate on partial success**""",
    tools=[
        FunctionTool(func=exit_loop_on_success)
    ],
    output_key="termination_check"
)

def after_loop_agent_callback(callback_context):
    """
    After loop callback that logs the results of the task executor agent
    """
    callback_context.state['relevant_tools'] = None
    if callback_context.state["final_response"]:
        # Return Content to *replace* the agent's own output
        return types.Content(
            parts=[types.Part(text=callback_context.state["final_response"])],
            role="model" # Assign model role to the overriding response
        )
    else:
        return None

def after_tool_callback(tool, args, tool_context, tool_response):
    """
    After tool callback that processes tool results and appends them to execution_result state
    """
    try:
        print(f"🔧 [TASK-EXECUTOR-AGENT] Tool: {tool.name}")
        print(f"🔧 [TASK-EXECUTOR-AGENT] Args: {args}")
        print(f"🔧 [TASK-EXECUTOR-AGENT] Tool Context: {tool_context}")
        print(f"🔧 [TASK-EXECUTOR-AGENT] Tool Response: {tool_response}")

        # Only process specific tools that should append to state
        target_tools = ['invoke_api_tool', 'search_agent', 'read_content_from_url', 'convert_markdown_to_google_doc']
        
        if tool.name not in target_tools:
            print(f"🔧 [TASK-EXECUTOR-AGENT] Skipping state append for tool: {tool.name}")
            return None
        
        # Initialize execution_result if it doesn't exist - use proper State mutation
        if 'execution_result' not in tool_context.state or tool_context.state['execution_result'] is None:
            tool_context.state['execution_result'] = []
            print(f"🔧 [TASK-EXECUTOR-AGENT] Initialized execution_result array in state")
        
        # Get current execution_result list
        current_results = tool_context.state.get('execution_result', [])
        print(f"🔧 [TASK-EXECUTOR-AGENT] Current execution_result has {len(current_results)} items")
        
        result_to_append = None
        
        if tool.name == 'invoke_api_tool':
            # Parse the tool response to check for success/error
            try:
                if isinstance(tool_response, str):
                    response_data = json.loads(tool_response)
                else:
                    response_data = tool_response
                
                print(f"🔧 [TASK-EXECUTOR-AGENT] Parsed invoke_api_tool response: {response_data}")
                
                # Check if it's an error response - look at the message structure
                message = response_data.get('message', {})
                
                # If message has an 'error' field, skip appending
                if isinstance(message, dict) and message.get('error'):
                    print(f"🔧 [TASK-EXECUTOR-AGENT] invoke_api_tool returned error: {message.get('error')}, not appending to state")
                    return None
                
                # Check if message has actual API response data
                if isinstance(message, dict) and (
                    message.get('response') or  # API response wrapper
                    message.get('data') or      # Direct data
                    message.get('records') or   # Records array
                    message.get('status') == 'success'  # Success indicator
                ):
                    # Has results - append the entire response_data as is (it's already in correct format)
                    result_to_append = response_data
                    print(f"🔧 [TASK-EXECUTOR-AGENT] invoke_api_tool has API results, will append entire response")
                    
                elif isinstance(message, dict) and not message.get('error'):
                    # Success but no results - append markdown format
                    result_to_append = {
                        "tool_name": tool.name,
                        "type": "markdown",
                        "message": "No results were found"
                    }
                    print(f"🔧 [TASK-EXECUTOR-AGENT] invoke_api_tool success but no results, will append")
                else:
                    print(f"🔧 [TASK-EXECUTOR-AGENT] invoke_api_tool response format not recognized, skipping")
                    print(f"🔧 [TASK-EXECUTOR-AGENT] Message type: {type(message)}, Message: {message}")
                    return None
                
            except (json.JSONDecodeError, TypeError, AttributeError) as e:
                print(f"🔧 [TASK-EXECUTOR-AGENT] Failed to parse invoke_api_tool response: {e}")
                print(f"🔧 [TASK-EXECUTOR-AGENT] Raw response: {tool_response}")
                return None
                
        elif tool.name in ['search_agent', 'read_content_from_url', 'convert_markdown_to_google_doc']:
            # These tools should already return the proper JSON format
            try:
                if isinstance(tool_response, str):
                    response_data = json.loads(tool_response)
                else:
                    response_data = tool_response
                
                # Check if it's already in the correct format
                if isinstance(response_data, dict) and response_data.get('tool_name') == tool.name:
                    # Already in correct format, use directly
                    result_to_append = response_data
                    print(f"🔧 [TASK-EXECUTOR-AGENT] {tool.name} response in correct format, will append")
                else:
                    # Fallback: wrap in standard format
                    result_to_append = {
                        "tool_name": tool.name,
                        "type": "markdown",
                        "message": str(tool_response)
                    }
                    print(f"🔧 [TASK-EXECUTOR-AGENT] {tool.name} response wrapped in standard format, will append")
                    
            except (json.JSONDecodeError, TypeError, AttributeError) as e:
                print(f"🔧 [TASK-EXECUTOR-AGENT] Failed to parse {tool.name} response: {e}")
                # Fallback: append as markdown
                result_to_append = {
                    "tool_name": tool.name,
                    "type": "markdown", 
                    "message": str(tool_response)
                }
                print(f"🔧 [TASK-EXECUTOR-AGENT] {tool.name} fallback markdown format, will append")
        
        # Append the result using proper State mutation
        if result_to_append:
            # Create new list with the appended result and reassign to trigger State tracking
            new_results = current_results.copy()
            new_results.append(result_to_append)
            tool_context.state['execution_result'] = new_results
            print(f"🔧 [TASK-EXECUTOR-AGENT] Successfully appended {tool.name} result to state. New count: {len(new_results)}")
        
        return None
        
    except Exception as e:
        print(f"🔧 [TASK-EXECUTOR-AGENT] Error in after_tool_callback: {e}")
        import traceback
        traceback.print_exc()
        return None


# Create the Task Executor LLM Agent starting with only tool discovery
task_executor_llm_agent = LlmAgent(
    name="task_executor_llm_agent",
    description="Self-discovering executor that finds optimal tools and executes tasks",
    model="gemini-2.5-flash-lite",
    instruction=create_dynamic_instruction,
    tools=[
        FunctionTool(func=detected_tools),  # LLM selects tools from the list
    ],
    before_model_callback=dynamic_tool_injection_callback,  # Dynamically inject discovered tools
    after_model_callback=task_executor_after_model_callback,  # Process responses silently
    output_key="relevant_tools",  # Start with tool discovery output key
    after_tool_callback=after_tool_callback
)        


# The LoopAgent that wraps both the executor and termination agents
task_executor_agent = LoopAgent(
    name="task_executor_agent",
    description="Task Executor Agent with automatic termination checking",
    sub_agents=[
        task_executor_llm_agent,    # Executes the actual task (discovery + execution)
        task_termination_agent      # Checks completion and terminates when done
    ],
    max_iterations=5,  # Safety limit - termination agent should stop it earlier
    #after_agent_callback=after_loop_agent_callback
)