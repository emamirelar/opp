"""
Task Executor Agent - Main AI Assistant Agent

An LLM-powered agent that uses intelligent entity detection and intent recognition
by providing entity information in the system instruction for the LLM to analyze.
"""

import logging
from typing import AsyncGenerator
from typing_extensions import override

from google.adk.agents import LlmAgent
from google.adk.agents.invocation_context import InvocationContext
from google.adk.events.event import Event
from google.genai import types
import json

from .utils.api_config_manager import config_manager

# Configure logging
logger = logging.getLogger(__name__)


def get_available_tools() -> str:
    """
    Get available tools information for the instruction.
    
    Returns:
        Formatted string containing tool information
    """
    try:
        # Import here to avoid circular dependencies
        from .sub_agents.task_executor_agent.dynamic_tool_registry import DynamicToolRegistry
        
        registry = DynamicToolRegistry()
        tools = registry.get_available_tools()
        
        tools_section = "## Available Tools\n\n"
        
        for tool in tools:
            name = tool.get('name', 'Unknown')
            description = tool.get('description', 'No description available')
            keywords = tool.get('keywords', [])
            
            tools_section += f"**{name}**: {description}\n"
            if keywords:
                tools_section += f"- Keywords: {', '.join(keywords[:8])}\n"
            tools_section += "\n"
            
        return tools_section
        
    except Exception as e:
        logger.error(f"Error loading tools information: {e}")
        return "## Available Tools\n\nError loading tool information.\n\n"


def get_task_executor_instruction(ctx=None) -> str:
    """
    Generate the system instruction for the task executor agent including entity information.
    
    Args:
        ctx: Optional context parameter (required by ADK)
    
    Returns:
        System instruction string
    """
    try:
        entities = config_manager.get_entities()
        
        entities_section = "## Available Business Entities\n\n"
        
        for entity in entities:
            name = entity.get('entity', 'Unknown')
            description = entity.get('description', 'No description available')
            synonyms = entity.get('synonyms', [])
            
            # Skip Google-related entities
            if 'google' in name.lower():
                continue
            
            entities_section += f"**{name}**: {description}\n"
            if synonyms:
                entities_section += f"- Synonyms: {', '.join(synonyms[:5])}\n"
            entities_section += "\n"
            
    except Exception as e:
        logger.error(f"Error loading entities: {e}")
        entities_section = "## Available Business Entities\n\nError loading entity information.\n\n"
    
    # Get tools information
    tools_section = get_available_tools()
    
    return f"""You are an AI assistant for the UNOPS Partner and Opportunity system. Your role is to help users with data operations, entity management, and system tasks.

Following are the list of business entities you can work with:
{entities_section}

You are presented the following tools>
{tools_section}

## Your Task

Analyze the user message and respond with a JSON object containing the tools to execute in order. The format should be:
Example user query: Get me details of partner XYZ and their contacts and engagements

```json
{{
  "tools_by_order": [
    {{
      "tool_name": "invoke_api_tool",
      "entity": "Partner",
      "intent": "retrieve", 
      "parameters": {{
        "search_terms": ["XYZ"]
      }}
    }},
    {{
      "tool_name": "invoke_api_tool",
      "entity": "Contact",
      "intent": "retrieve",
      "parameters": {{
        "partner_reference": "from_previous_result",
        "search_terms": ["related_to_partner"]
      }}
    }},
    {{
      "tool_name": "invoke_api_tool",
      "entity": "Engagement",
      "intent": "retrieve",
      "parameters": {{
        "partner_reference": "from_previous_result",
        "search_terms": ["related_to_partner"]
      }}
    }}
  ]
}}
```

**NOTE: You do not need to call any tools - just return the JSON structure**

## Instructions

1. **Entity Detection**: Identify which entities from the list above are mentioned in the user's message (including synonyms)
2. **Intent Recognition**: Determine what the user wants to do (retrieve, create, update, delete, search, greeting, help, etc.)
3. **Parameter Extraction**: Extract relevant parameters like IDs, search terms, quoted phrases, etc.
4. **Tool Planning**: Based on the entities, intent, and parameters, determine which tools need to be executed in order. Consider:
   - Use `invoke_api_tool` for any entity data operations (get, create, update, delete) - specify the exact entity and intent
   - Use `search_agent` for web searches, latest information, news, or research - entity can be the subject of search
   - Use `read_content_from_url` when user provides URLs or mentions reading web content
   - **IMPORTANT**: Break down complex requests into multiple sequential steps:
     * If user asks for "contacts and engagements of partner XYZ", create 3 separate tool calls:
       1. First: Get Partner details (to obtain Partner ID)
       2. Second: Get Contacts using the Partner ID
       3. Third: Get Engagements using the Partner ID
   - Each tool call should focus on ONE entity and ONE intent
   - For greetings or help requests, return empty array: `"tools_by_order": []`

## Response Guidelines

- Always respond in valid JSON format with the `tools_by_order` structure
- Each tool entry must specify: `tool_name`, `entity`, `intent`, and `parameters`
- Order tools by execution priority (first tool executes first)
- For greetings or help requests, use empty array: `"tools_by_order": []`
- Same tool can appear multiple times with different entities/intents
- Entity should be the specific business entity being operated on
- Intent should match the operation being performed
- Parameters should contain all relevant data for the tool execution

## Common Patterns:

**"Get contacts of partner XYZ"** → 2 steps:
1. invoke_api_tool + Partner + retrieve (find partner XYZ)
2. invoke_api_tool + Contact + retrieve (get contacts for that partner)

**"Find partner ABC and their engagements"** → 2 steps:
1. invoke_api_tool + Partner + retrieve (find partner ABC)
2. invoke_api_tool + Engagement + retrieve (get engagements for that partner)

**"Get contacts and engagements of partner XYZ"** → 3 steps:
1. invoke_api_tool + Partner + retrieve (find partner XYZ)
2. invoke_api_tool + Contact + retrieve (get contacts for that partner)
3. invoke_api_tool + Engagement + retrieve (get engagements for that partner)

IMPORTANT: Always return a valid JSON object with the exact structure shown above."""


class TaskExecutorAgent(LlmAgent):
    """
    LLM-powered task executor agent that uses intelligent entity detection and intent recognition.
    """
    
    def __init__(self, name: str = "TaskExecutorAgent"):
        """Initialize the task executor agent with LLM capabilities."""
        super().__init__(
            name=name,
            model="gemini-2.5-flash-lite",
            instruction=get_task_executor_instruction,
            after_model_callback=self._process_llm_response
        )
        
        logger.info(f"[{self.name}] LLM-powered task executor agent initialized")
    
    @override
    async def _run_async_impl(self, ctx: InvocationContext) -> AsyncGenerator[Event, None]:
        """Custom implementation with intelligent tool execution logic."""
        logger.info(f"[{self.name}] Starting intelligent TaskExecutor workflow")
        
        # 1. First, run the normal LLM flow to get tool analysis
        logger.info(f"[{self.name}] Running main LLM analysis...")
        async for event in super()._run_async_impl(ctx):
            logger.info(f"[{self.name}] Event from main LLM: {event.model_dump_json(indent=2, exclude_none=True)}")
            yield event
        
        # 2. Check if we have tools to execute
        tools_to_execute = ctx.session.state.get("tools_to_execute", [])
        if not tools_to_execute:
            logger.info(f"[{self.name}] No tools detected - workflow complete")
            return
        
        logger.info(f"[{self.name}] Found {len(tools_to_execute)} tools to execute")
        
        # 3. Process tools intelligently
        execution_results = []
        for i, tool in enumerate(tools_to_execute, 1):
            tool_name = tool.get("tool_name", "Unknown")
            entity = tool.get("entity", "Unknown")
            intent = tool.get("intent", "Unknown")
            parameters = tool.get("parameters", {})
            
            logger.info(f"[{self.name}] Processing tool {i}: {tool_name} for {entity}/{intent}")
            
            if tool_name == "invoke_api_tool":
                # Determine execution strategy
                execution_strategy = self._determine_execution_strategy(entity, intent, parameters, execution_results)
                
                if execution_strategy == "direct":
                    # Simple case - execute directly
                    result = await self._execute_direct_api_call(entity, intent, parameters, ctx)
                    execution_results.append(result)
                    yield self._create_result_event(f"📊 Direct API Result for {entity}", result)
                    
                elif execution_strategy == "llm_assisted":
                    # Complex case - use LLM to help construct the call
                    result = await self._execute_llm_assisted_call(entity, intent, parameters, execution_results, ctx)
                    execution_results.append(result)
                    yield self._create_result_event(f"🤖 LLM-Assisted Result for {entity}", result)
                    
                else:
                    logger.warning(f"[{self.name}] Unknown execution strategy: {execution_strategy}")
            
            else:
                # Handle other tool types (search_agent, read_content_from_url, etc.)
                result = await self._execute_other_tool(tool_name, entity, intent, parameters, ctx)
                execution_results.append(result)
                yield self._create_result_event(f"🔧 Tool Result: {tool_name}", result)
        
        # 4. Provide summary
        summary = self._create_execution_summary(execution_results)
        yield self._create_result_event("✅ Execution Complete", summary)
        
        logger.info(f"[{self.name}] Intelligent workflow finished with {len(execution_results)} results")
    
    def _execute_api_tool(self, entity: str, intent: str, parameters: dict) -> str:
        """
        Execute API tool directly - it already handles endpoint discovery and API calls.
        
        Args:
            entity: The business entity to work with
            intent: The intent/operation to perform
            parameters: Parameters for the operation
            
        Returns:
            JSON string with the API operation result
        """
        try:
            # Use invoke_api_tool directly - it already handles find_entity_endpoint internally
            from .sub_agents.task_executor_agent.utils import invoke_api_tool
            
            logger.info(f"[{self.name}] Executing API tool for {entity}/{intent} with params: {parameters}")
            result = invoke_api_tool(entity, intent, parameters)
            logger.info(f"[{self.name}] API tool completed for {entity}/{intent}")
            
            return result
            
        except Exception as e:
            logger.error(f"[{self.name}] Error executing API tool: {e}")
            error_result = {
                "success": False,
                "error": str(e),
                "entity": entity,
                "intent": intent
            }
            return json.dumps(error_result)
    
    def _create_dynamic_execution_agent(self, tools_to_execute: list) -> LlmAgent:
        """
        Create a dynamic LlmAgent that will handle tool execution.
        
        Args:
            tools_to_execute: List of tools that were detected and need to be executed
            
        Returns:
            A new LlmAgent configured for tool execution
        """
        try:
            # For now, just create a simple hello agent as requested
            dynamic_agent_name = f"DynamicExecutor_{len(tools_to_execute)}tools"
            
            # Simple hello instruction for now
            hello_instruction = f"""Hello! I am a dynamic execution agent created to handle {len(tools_to_execute)} tool(s).

Tools to execute:
{json.dumps(tools_to_execute, indent=2)}

For now, I'm just saying hello as requested. Future implementation will execute these tools.

Please respond with a friendly greeting and confirmation that you're ready to execute the tools."""
            
            # Create the dynamic LlmAgent
            dynamic_agent = LlmAgent(
                name=dynamic_agent_name,
                model="gemini-2.5-flash-lite",
                instruction=hello_instruction,
                # Prevent this agent from transferring further
                disallow_transfer_to_parent=True,
                disallow_transfer_to_peers=True
            )
            
            logger.info(f"[{self.name}] Created dynamic agent: {dynamic_agent.name}")
            logger.info(f"[{self.name}] Dynamic agent will handle {len(tools_to_execute)} tools")
            
            return dynamic_agent
            
        except Exception as e:
            logger.error(f"[{self.name}] Error creating dynamic execution agent: {e}")
            
            # Fallback: create a simple error agent
            fallback_agent = LlmAgent(
                name="ErrorAgent",
                model="gemini-2.5-flash-lite",
                instruction="Hello! There was an error creating the execution agent. Please try again.",
                disallow_transfer_to_parent=True,
                disallow_transfer_to_peers=True
            )
            
            return fallback_agent
    
    def _determine_execution_strategy(self, entity: str, intent: str, parameters: dict, previous_results: list) -> str:
        """
        Determine whether to execute directly or use LLM assistance.
        
        Args:
            entity: The entity being operated on
            intent: The intent/operation 
            parameters: Tool parameters
            previous_results: Results from previous tool executions
            
        Returns:
            "direct" for simple calls, "llm_assisted" for complex calls
        """
        # Check if this is a simple list/get all operation
        simple_operations = {
            "Partner": ["list", "retrieve_all"],
            "Contact": ["list", "retrieve_all"], 
            "Engagement": ["list", "retrieve_all"],
            "Opportunity": ["list", "retrieve_all"]
        }
        
        # Check if parameters suggest dependencies
        has_dependencies = any(key in str(parameters) for key in [
            "from_previous_result", "partner_reference", "contact_reference", 
            "engagement_reference", "{", "}", "placeholder"
        ])
        
        # Check if we need data from previous results
        needs_previous_data = len(previous_results) > 0 and has_dependencies
        
        if intent in simple_operations.get(entity, []) and not has_dependencies:
            logger.info(f"[{self.name}] Strategy: DIRECT - Simple {intent} for {entity}")
            return "direct"
        elif needs_previous_data or has_dependencies:
            logger.info(f"[{self.name}] Strategy: LLM_ASSISTED - Complex {intent} for {entity} with dependencies")
            return "llm_assisted"
        else:
            logger.info(f"[{self.name}] Strategy: DIRECT - Default for {intent} on {entity}")
            return "direct"
    
    async def _execute_direct_api_call(self, entity: str, intent: str, parameters: dict, ctx: InvocationContext) -> dict:
        """Execute a direct API call without LLM assistance."""
        try:
            logger.info(f"[{self.name}] Direct execution: {entity}/{intent}")
            
            # Use the existing invoke_api_tool function
            from .sub_agents.task_executor_agent.utils import invoke_api_tool
            result = invoke_api_tool(entity, intent, parameters)
            
            # Parse result if it's a string
            if isinstance(result, str):
                import json
                try:
                    result = json.loads(result)
                except json.JSONDecodeError:
                    result = {"raw_result": result}
            
            logger.info(f"[{self.name}] Direct execution completed for {entity}/{intent}")
            return {
                "execution_type": "direct",
                "entity": entity,
                "intent": intent,
                "success": True,
                "result": result
            }
            
        except Exception as e:
            logger.error(f"[{self.name}] Direct execution failed: {e}")
            return {
                "execution_type": "direct",
                "entity": entity,
                "intent": intent,
                "success": False,
                "error": str(e)
            }
    
    async def _execute_llm_assisted_call(self, entity: str, intent: str, parameters: dict, previous_results: list, ctx: InvocationContext) -> dict:
        """Execute an LLM-assisted API call that requires data from previous results."""
        try:
            logger.info(f"[{self.name}] LLM-assisted execution: {entity}/{intent}")
            
            # Create dynamic agent for LLM assistance
            assistant_instruction = f"""You are an API parameter assistant. Your job is to help construct API calls using data from previous results.

Current Task:
- Entity: {entity}
- Intent: {intent}  
- Parameters: {json.dumps(parameters, indent=2)}

Previous Results Available:
{json.dumps(previous_results, indent=2)}

Instructions:
1. If parameters contain references like "from_previous_result" or "partner_reference", extract the actual values from previous results
2. If you find an ID or reference in previous results, use it to replace placeholders
3. Return the updated parameters as JSON

Example:
If previous result has {{"result": {{"data": [{{"id": "123", "name": "ABC Corp"}}]}}}}, and parameters need partner_reference, use "123".

Return only the updated parameters as valid JSON."""

            # Create dynamic LLM agent
            assistant_agent = LlmAgent(
                name=f"ParameterAssistant_{entity}",
                model="gemini-2.5-flash-lite",
                instruction=assistant_instruction
            )
            
            # Run the assistant to get updated parameters
            assistant_results = []
            async for event in assistant_agent.run_async(ctx):
                if event.content and event.content.parts:
                    for part in event.content.parts:
                        if part.text:
                            assistant_results.append(part.text)
            
            # Parse assistant response
            updated_params = parameters  # fallback
            if assistant_results:
                try:
                    assistant_response = "".join(assistant_results)
                    # Clean JSON from markdown if needed
                    if "```json" in assistant_response:
                        assistant_response = assistant_response.split("```json")[1].split("```")[0]
                    updated_params = json.loads(assistant_response.strip())
                    logger.info(f"[{self.name}] LLM updated parameters: {updated_params}")
                except Exception as e:
                    logger.warning(f"[{self.name}] Failed to parse LLM response, using original params: {e}")
            
            # Now execute with updated parameters
            from .sub_agents.task_executor_agent.utils import invoke_api_tool
            result = invoke_api_tool(entity, intent, updated_params)
            
            # Parse result if it's a string
            if isinstance(result, str):
                try:
                    result = json.loads(result)
                except json.JSONDecodeError:
                    result = {"raw_result": result}
            
            logger.info(f"[{self.name}] LLM-assisted execution completed for {entity}/{intent}")
            return {
                "execution_type": "llm_assisted",
                "entity": entity,
                "intent": intent,
                "original_parameters": parameters,
                "updated_parameters": updated_params,
                "success": True,
                "result": result
            }
            
        except Exception as e:
            logger.error(f"[{self.name}] LLM-assisted execution failed: {e}")
            return {
                "execution_type": "llm_assisted",
                "entity": entity,
                "intent": intent,
                "success": False,
                "error": str(e)
            }
    
    async def _execute_other_tool(self, tool_name: str, entity: str, intent: str, parameters: dict, ctx: InvocationContext) -> dict:
        """Execute non-API tools like search_agent, read_content_from_url, etc."""
        try:
            logger.info(f"[{self.name}] Executing other tool: {tool_name}")
            
            if tool_name == "search_agent":
                # Use existing search agent
                search_query = parameters.get("search_terms", [entity, intent])
                # TODO: Implement actual search agent execution
                result = {"search_query": search_query, "results": "Search functionality to be implemented"}
                
            elif tool_name == "read_content_from_url":
                url = parameters.get("url", "")
                # TODO: Implement URL content reading
                result = {"url": url, "content": "URL reading functionality to be implemented"}
                
            else:
                result = {"error": f"Unknown tool: {tool_name}"}
            
            return {
                "execution_type": "other_tool",
                "tool_name": tool_name,
                "entity": entity,
                "intent": intent,
                "success": True,
                "result": result
            }
            
        except Exception as e:
            logger.error(f"[{self.name}] Other tool execution failed: {e}")
            return {
                "execution_type": "other_tool",
                "tool_name": tool_name,
                "success": False,
                "error": str(e)
            }
    
    def _create_result_event(self, title: str, result: dict) -> Event:
        """Create an Event from execution results."""
        try:
            result_text = f"{title}\n\n{json.dumps(result, indent=2)}"
            return Event(
                content=types.Content(parts=[types.Part(text=result_text)]),
                author=self.name
            )
        except Exception as e:
            logger.error(f"[{self.name}] Failed to create result event: {e}")
            return Event(
                content=types.Content(parts=[types.Part(text=f"{title}\n\nError creating result display")]),
                author=self.name
            )
    
    def _create_execution_summary(self, results: list) -> dict:
        """Create a summary of all execution results."""
        summary = {
            "total_operations": len(results),
            "successful": len([r for r in results if r.get("success", False)]),
            "failed": len([r for r in results if not r.get("success", False)]),
            "direct_executions": len([r for r in results if r.get("execution_type") == "direct"]),
            "llm_assisted_executions": len([r for r in results if r.get("execution_type") == "llm_assisted"]),
            "other_tools": len([r for r in results if r.get("execution_type") == "other_tool"]),
            "details": results
        }
        return summary
    
    def _process_llm_response(self, callback_context, llm_response):
        """
        Process the LLM response to extract and execute tools.
        
        Args:
            callback_context: The callback context
            llm_response: The LLM response containing the JSON tool plan
            
        Returns:
            The original LLM response (unchanged)
        """
        try:
            # Extract the response content
            if llm_response and llm_response.content and llm_response.content.parts:
                response_text = ""
                for part in llm_response.content.parts:
                    if part.text:
                        response_text += part.text
                
                logger.info(f"[{self.name}] Raw LLM Response: {response_text}")
                
                # Parse the JSON response - handle markdown code blocks
                try:
                    # Remove markdown code block markers if present
                    clean_text = response_text.strip()
                    if clean_text.startswith('```json'):
                        clean_text = clean_text[7:]  # Remove ```json
                    if clean_text.startswith('```'):
                        clean_text = clean_text[3:]   # Remove ```
                    if clean_text.endswith('```'):
                        clean_text = clean_text[:-3]  # Remove trailing ```
                    
                    clean_text = clean_text.strip()
                    
                    tools_plan = json.loads(clean_text)
                    logger.info(f"[{self.name}] Parsed JSON: {tools_plan}")
                    
                    # Extract tools_by_order
                    if "tools_by_order" in tools_plan:
                        tools_to_execute = tools_plan["tools_by_order"]
                        callback_context.state["tools_to_execute"] = tools_to_execute
                        logger.info(f"[{self.name}] Found {len(tools_to_execute)} tools to execute")
                        
                        # If tools are detected, create and store dynamic agent for later use
                        if tools_to_execute:
                            logger.info(f"[{self.name}] Tools detected! Creating dynamic LlmAgent for execution...")
                            
                            # Create a new LlmAgent dynamically
                            dynamic_agent = self._create_dynamic_execution_agent(tools_to_execute)
                            
                            # Store the dynamic agent as a sub-agent for transfer
                            self.sub_agents = [dynamic_agent]
                            
                            # Store the dynamic agent in callback context for potential transfer
                            callback_context.state["dynamic_agent"] = dynamic_agent.name
                            
                            # Instead of modifying the response, let's return None to use the original response
                            # and let the ADK flow handle the transfer naturally
                            logger.info(f"[{self.name}] Dynamic agent {dynamic_agent.name} ready for execution")
                            # Don't modify the response - let it show the original JSON plan
                            # The transfer will happen through ADK's natural flow
                        
                    else:
                        logger.warning(f"[{self.name}] No 'tools_by_order' found in response")
                        
                except json.JSONDecodeError as e:
                    logger.error(f"[{self.name}] Failed to parse JSON: {e}")
                    logger.error(f"[{self.name}] Raw response: {response_text}")
                    
        except Exception as e:
            logger.error(f"[{self.name}] Error processing LLM response: {e}")
        
        # Return the original response unchanged
        return llm_response


# Create the agent instance
task_executor_agent = TaskExecutorAgent(name="TaskExecutorAgent")
