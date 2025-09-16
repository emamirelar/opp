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
from typing import List, Dict, Any

from .utils.api_config_manager import config_manager

# Configure logging
logger = logging.getLogger(__name__)

def _load_all_tools(self) -> List[Dict[str, Any]]:
    """
    Load all available tools with their metadata
    
    Returns:
        List of tool dictionaries with name, description, and scoring info
    """
    tools = [
        {
            "name": "api_tool_agent",
            "description": "Automatically finds and calls API endpoints for any entity and intent",
            "keywords": ["api", "call", "endpoint", "data", "fetch", "get", "create", "update", "delete", 
                        "list", "retrieve", "entity"],
            "score_weight": 100
        },
        {
            "name": "web_search_agent", 
            "description": "Search web and knowledge bases for latest news, information, and research",
            "keywords": ["search", "web", "google", "find", "information", "lookup", "query", "research", 
                        "news", "latest", "current", "recent", "today", "trending", "updates", "breaking"],
            "score_weight": 90
        },
        {
            "name": "read_content_from_url",
            "description": "Read and extract content from web URLs",
            "keywords": ["url", "web", "content", "read", "extract", "webpage", "link", "fetch"],
            "score_weight": 70
        }
    ]
    
    return tools

def get_available_tools() -> str:
    """
    Get available tools information for the instruction.
    
    Returns:
        Formatted string containing tool information
    """
    try:
        tools = _load_all_tools()
        
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
Example 1 - API queries: "Get me details of partner XYZ and their contacts and engagements"
```json
{{
  "tools_by_order": [
    {{
      "tool_name": "invoke_api_tool",
      "entity": "Partner",
      "intent": "retrieve", 
      "parameters": {{
        "search_terms": ["XYZ"],
        "searchText": "XYZ"
      }}
    }},
    {{
      "tool_name": "invoke_api_tool",
      "entity": "Contact",
      "intent": "retrieve",
      "parameters": {{
        "reference": "from_previous_result",
        "search_terms": ["related_to_partner"]
      }}
    }},
    {{
      "tool_name": "invoke_api_tool",
      "entity": "Engagement",
      "intent": "retrieve",
      "parameters": {{
        "reference": "from_previous_result",
        "search_terms": ["related_to_partner"]
      }}
    }}
  ]
}}
```

Example 2 - Web search: "Tell me about VTF UN Voluntary Trust Fund for Assistance in Mine Action"
```json
{{
  "tools_by_order": [
    {{
      "tool_name": "web_search_agent",
      "entity": "Partner",
      "intent": "search",
      "parameters": {{
        "query": "Search for VTF UN Voluntary Trust Fund for Assistance in Mine Action official information activities and their related information"
      }}
    }}
  ]
}}
```

Example 3 - URL content reading: "Read this document: https://example.com/report.pdf"
```json
{{
  "tools_by_order": [
    {{
      "tool_name": "read_content_from_url",
      "entity": "Document",
      "intent": "read",
      "parameters": {{
        "url": "https://example.com/report.pdf",
        "title": "Report PDF",
        "description": "User-provided document to read and analyze"
      }}
    }}
  ]
}}
```

**IMPORTANT: You do NOT call tools or transfer to other agents. ONLY return the JSON structure.**

## Instructions

1. **Entity Detection**: Identify which entities from the list above are mentioned in the user's message (including synonyms)
2. **Intent Recognition**: Determine what the user wants to do (retrieve, create, update, delete, search, greeting, help, etc.)
3. **Parameter Extraction**: Extract relevant parameters like IDs, search terms, quoted phrases, etc.
4. **Tool Planning**: Based on the entities, intent, and parameters, determine which tools need to be executed in order. Consider:
   - Use `invoke_api_tool` for any entity data operations (get, create, update, delete) - specify the exact entity and intent
   - Use `web_search_agent` for web searches, latest information, news, or research:
     * Provide an optimized `query` parameter with the full search phrase
     * Include relevant `search_terms` array with key terms for backup
     * Make queries specific and complete (e.g., "organization name official website activities" instead of just "organization")
     * When the tool is web_search_agent, add a *query* parameter with the full search phrase
   - Use `read_content_from_url` when user provides URLs or mentions reading web content:
     * Include the full URL in parameters.url
     * Provide meaningful title and description parameters when possible
     * Can handle PDF, DOC, web pages, and other document types
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

CRITICAL RULES:
1. ONLY return valid JSON - never transfer to any agent or send any json_payload functions. 
2. NEVER call tools yourself - just plan them in the JSON structure  
3. ALWAYS use the exact JSON format with "tools_by_order" array
4. For greetings/help, return empty tools_by_order: []

ONLY VALID RESPONSE: JSON object with tools_by_order structure."""


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
        
        logger.info(f"[{self.name}] Found {len(tools_to_execute)} tools to execute")
        
        # 3. Process tools intelligently
        execution_results = []
        for i, tool in enumerate(tools_to_execute, 1):
            tool_name = tool.get("tool_name", "Unknown")
            entity_raw = tool.get("entity", "Unknown")
            intent_raw = tool.get("intent", "Unknown")
            parameters = tool.get("parameters", {})
            
            # Ensure entity and intent are strings
            entity = str(entity_raw) if entity_raw else "Unknown"
            intent = str(intent_raw) if intent_raw else "Unknown"
            
            logger.info(f"[{self.name}] Processing tool {i}: {tool_name} for {entity}/{intent} with params: {parameters}")
            
            if tool_name == "invoke_api_tool":
                # Pre-process: Convert retrieve with no search terms to list BEFORE strategy determination
                if intent == "retrieve" and (not parameters or not parameters.get("search_terms") or len(parameters.get("search_terms", [])) == 0):
                    logger.info(f"[{self.name}] PRE-PROCESSING: Converting retrieve with no search terms to list operation")
                    intent = "list"
                    parameters = {}  # Clear for list operations
                
                # Determine execution strategy
                execution_strategy = self._determine_execution_strategy(entity, intent, parameters, execution_results)
                logger.info(f"Execution strategy: {execution_strategy}")
                
                if execution_strategy == "direct":
                    # Check if URL can be constructed properly - if not, switch to LLM assisted
                    from .utils.task_execution_utils import find_entity_endpoint
                    import json
                    import re
                    
                    # Test URL construction
                    test_params = json.dumps(parameters) if parameters else "{}"
                    endpoint_result = find_entity_endpoint(entity, intent, test_params)
                    endpoint_data = json.loads(endpoint_result)
                    
                    # Check if URL has unresolved placeholders
                    test_url = endpoint_data.get('full_url', '')
                    unresolved_placeholders = re.findall(r'\{([^}]+)\}', test_url)
                    
                    if unresolved_placeholders:
                        logger.info(f"[{self.name}] ⚠️ STRATEGY CHANGE: URL has unresolved placeholders: {unresolved_placeholders}")
                        logger.info(f"[{self.name}] 🔄 SWITCHING: Direct → LLM-Assisted to resolve: {test_url}")
                        execution_strategy = "llm_assisted"
                    else:
                        logger.info(f"[{self.name}] ✅ URL construction verified: {test_url}")
                
                if execution_strategy == "direct":
                    # Simple case - execute directly
                    logger.info(f"[{self.name}] ✅ EXECUTING: Direct API call for {entity}/{intent}")
                    logger.info(f"[{self.name}] 🔄 FLOW: Task Executor → invoke_api_tool → HTTP Request")
                    result = await self._execute_direct_api_call(entity, intent, parameters, ctx)
                    execution_results.append(result)
                    yield self._create_result_event(f"📊 Direct API Result for {entity}", result)
                    
                elif execution_strategy == "llm_assisted":
                    # Complex case - use LLM to help construct the call
                    logger.info(f"[{self.name}] 🧠 EXECUTING: LLM-Assisted API call for {entity}/{intent}")
                    logger.info(f"[{self.name}] 🔄 FLOW: Task Executor → LLM Parameter Assistant → Extract IDs → invoke_api_tool → HTTP Request")
                    result = await self._execute_llm_assisted_call(entity, intent, parameters, execution_results, ctx)
                    execution_results.append(result)
                    yield self._create_result_event(f"🤖 LLM-Assisted Result for {entity}", result)
                    
                else:
                    logger.warning(f"[{self.name}] Unknown execution strategy: {execution_strategy}")
            
            else:
                # Handle other tool types (web_search_agent, read_content_from_url, etc.)
                result = await self._execute_other_tool(tool_name, entity, intent, parameters, ctx)
                execution_results.append(result)
                yield self._create_result_event(f"🔧 Tool Result: {tool_name}", result)
        
        # 4. Provide summary and store in state for next agent
        summary = self._create_execution_summary(execution_results)
        ctx.session.state["execution_results"] = summary
        ctx.session.state["execution_details"] = execution_results
        
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
            from .utils.task_execution_utils import invoke_api_tool
            
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
        # Dynamically determine simple operations based on available entities
        try:
            entities = config_manager.get_entities()
            simple_operations = {}
            for entity_config in entities:
                entity_name = entity_config.get('entity', '')
                if entity_name and 'google' not in entity_name.lower():
                    # All entities support basic operations
                    simple_operations[entity_name] = ["list", "retrieve_all", "retrieve"]
            logger.info(f"[{self.name}] Dynamic simple operations: {list(simple_operations.keys())}")
        except Exception as e:
            logger.warning(f"[{self.name}] Failed to load entities, using fallback operations: {e}")
            # Fallback to basic operations
            simple_operations = {
                "Partner": ["list", "retrieve_all", "retrieve"],
                "Contact": ["list", "retrieve_all", "retrieve"], 
                "Engagement": ["list", "retrieve_all", "retrieve"],
                "Opportunity": ["list", "retrieve_all", "retrieve"]
            }
        
        # Check if parameters suggest dependencies
        has_dependencies = any(key in str(parameters) for key in [
            "from_previous_result", "reference", "placeholder"
        ])

        if has_dependencies:
            matching_keys = [key for key in ["from_previous_result", "reference", "{", "}", "placeholder"] if key in str(parameters)]
            logger.info(f"[{self.name}]   Matching dependency keys: {matching_keys}")
        
        # ENHANCED: Check if we need data from previous results
        needs_previous_data = len(previous_results) > 0 and (
            has_dependencies or 
            # If parameters suggest this is a related/dependent call (but only if there are previous results)
            any(keyword in str(parameters).lower() for keyword in [
                "related_to", "related", "associated", "belonging_to"
            ])
        )
        
        if len(previous_results) == 0:
            needs_previous_data = False
        
        # Special case: if it's a retrieve with empty parameters, treat as list all
        is_list_all_request = (intent == "retrieve" and 
                              (not parameters or 
                               not parameters.get("search_terms") or 
                               len(parameters.get("search_terms", [])) == 0 or
                               parameters.get("search_terms") == []))
        
        # Decision logic with clear reasoning
        if intent in simple_operations.get(entity, []) and not has_dependencies and not needs_previous_data:
            if is_list_all_request:
                strategy = "direct"
                reason = f"List all {entity} with no search terms - simple operation"
                logger.info(f"[{self.name}] ✅ STRATEGY: DIRECT")
                logger.info(f"[{self.name}] 💡 REASON: {reason}")
                return strategy
            elif intent in ["list", "retrieve_all"]:
                strategy = "direct"
                reason = f"Simple {intent} operation for {entity} - no dependencies"
                logger.info(f"[{self.name}] ✅ STRATEGY: DIRECT")
                logger.info(f"[{self.name}] 💡 REASON: {reason}")
                return strategy
            else:
                strategy = "direct"
                reason = f"Simple {intent} for {entity} without dependencies or previous data needs"
                logger.info(f"[{self.name}] ✅ STRATEGY: DIRECT")
                logger.info(f"[{self.name}] 💡 REASON: {reason}")
                return strategy
        elif needs_previous_data or has_dependencies:
            strategy = "llm_assisted"
            reasons = []
            if has_dependencies:
                reasons.append("explicit dependency markers found")
            if len(previous_results) > 0 and entity != "Partner":
                reasons.append(f"dependent {entity} call after previous results")
            if any(keyword in str(parameters).lower() for keyword in ["related_to", "related", "associated", "belonging_to"]):
                reasons.append("relationship keywords detected")
            
            reason = f"Complex {intent} for {entity} - " + " + ".join(reasons)
            logger.info(f"[{self.name}] 🧠 STRATEGY: LLM_ASSISTED")
            logger.info(f"[{self.name}] 💡 REASON: {reason}")
            logger.info(f"[{self.name}] 🔗 WILL EXTRACT: IDs from previous results to construct proper URL parameters")
            return strategy
        else:
            strategy = "direct"
            reason = f"Default direct execution for {intent} on {entity}"
            logger.info(f"[{self.name}] ✅ STRATEGY: DIRECT")
            logger.info(f"[{self.name}] 💡 REASON: {reason}")
            return strategy
    
    async def _execute_direct_api_call(self, entity: str, intent: str, parameters: dict, ctx: InvocationContext) -> dict:
        """Execute a direct API call without LLM assistance."""
        try:
            logger.info(f"[{self.name}] 🚀 DIRECT EXECUTION STARTED: {entity}/{intent}")
            logger.info(f"[{self.name}] 📋 No LLM assistance needed - using parameters as-is")
            
            # Always ensure default values for pagination parameters
            if not parameters:
                parameters = {}
            
            # Set default pagination values if not already present
            if "pageIndex" not in parameters:
                parameters["pageIndex"] = 1
            if "pageSize" not in parameters:
                parameters["pageSize"] = 5
            if "orderBy" not in parameters:
                parameters["orderBy"] = "name"
            if "ascending" not in parameters:
                parameters["ascending"] = True

            if "search_terms" in parameters:
                parameters["searchText"] = parameters["search_terms"][0] if len(parameters["search_terms"]) == 1 else ' '.join(parameters["search_terms"])
                
            logger.info(f"[{self.name}] Using parameters with defaults: {parameters}")
            
            # Use the existing invoke_api_tool function
            from .utils.task_execution_utils import invoke_api_tool
            result = invoke_api_tool(entity, intent, parameters)
            
            # Parse result if it's a string
            if isinstance(result, str):
                import json
                try:
                    result = json.loads(result)
                except json.JSONDecodeError:
                    result = {"raw_result": result}
            
            logger.info(f"[{self.name}] Direct execution completed for {entity}/{intent}")
            
            # Prepare the final result
            final_result = {
                "execution_type": "direct",
                "entity": entity,
                "intent": intent,
                "success": True,
                "result": result
            }
            
            return final_result
            
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
        """Execute an LLM-assisted API call using a dedicated API URL Constructor agent."""
        try:
            import json  # Import json at the very beginning
            from .utils.task_execution_utils import find_entity_endpoint
            logger.info(f"[{self.name}] 🧠 LLM-ASSISTED EXECUTION STARTED: {entity}/{intent}")
            # STEP 1: Find the endpoint BEFORE creating the agent
            logger.info(f"[{self.name}] 🔍 Finding endpoint for {entity}/{intent}...")
            endpoint_info = find_entity_endpoint(entity, intent, parameters)
            logger.info(f"[{self.name}] 📍 Endpoint discovery result: {json.dumps(endpoint_info, indent=2)}")
            
            # STEP 2: Create Constructor Agent instruction with endpoint info included
            logger.info(f"[{self.name}] 🏗️ Creating API URL Constructor Agent with pre-discovered endpoint info")
            
            # Create API URL Constructor Agent instruction with angle brackets to avoid context variables
            endpoint_info_safe = json.dumps(endpoint_info, indent=2).replace("{", "<").replace("}", ">")
            previous_results_safe = json.dumps(previous_results, indent=2).replace("{", "<").replace("}", ">")
            parameters_safe = json.dumps(parameters, indent=2).replace("{", "<").replace("}", ">")
            
            constructor_instruction = f"""You are an API URL Constructor Agent. Your job is to analyze previous API results and construct the proper API call for dependent operations.

CURRENT TASK:
- Entity: {entity}
- Intent: {intent}
- Original Parameters: {parameters_safe}
- Endpoint Info: {endpoint_info_safe}

PREVIOUS RESULTS:
{previous_results_safe}


YOUR MISSION:
1. Find the most appropriate data and extract the parameters required for the current api to run. 
2.For example, the user would be asking to get the engagements of a partner, so we need to get the partner id from the previous results and use it as the partnerId parameter for the engagement endpoint.
3. Use your knowledge and smartness to correctly determine the parameters needed for the "Endpoint Info" to work properly.


REQUIRED OUTPUT FORMAT (example):
{{
  "partnerId": <Id of the closest relevant partner from the previous results>,
  "pageIndex": 1,
  "pageSize": 5,
  "orderBy": "name",
  "ascending": true
}}

CRITICAL RULES:
- Never leave any placeholder unreplaced in the endpoint info. pageIndex, pageSize, 
Return ONLY the JSON object, no explanations"""

            # Import the tools for the constructor agent
            from .utils.task_execution_utils import invoke_api_tool, find_entity_endpoint
            from google.adk.tools import FunctionTool
            
            # Create the API URL Constructor Agent with both tools
            constructor_agent = LlmAgent(
                name=f"param_constructor_{entity}",
                description="Extract partner ID and construct API parameters.",
                model="gemini-2.5-flash-lite",
                instruction=constructor_instruction
            )
            
            logger.info(f"[{self.name}] ✅ API URL Constructor Agent created successfully")
            logger.info(f"[{self.name}] 🏃 Running API Constructor to analyze and execute API call...")
            
            # Run the constructor agent - it will analyze and return JSON parameters
            constructor_results = []
            
            async for event in constructor_agent.run_async(ctx):
                logger.info(f"[{self.name}] 📨 Constructor event type: {type(event)}")
                
                # Capture text responses (should be JSON parameters)
                if hasattr(event, 'content') and event.content and event.content.parts:
                    for part in event.content.parts:
                        if part.text:
                            constructor_results.append(part.text)
                            logger.info(f"[{self.name}] 📝 Constructor JSON response: {part.text}")
            
            logger.info(f"[{self.name}] ✅ Constructor Agent completed with {len(constructor_results)} responses")
            
            # Extract and parse the JSON parameters from constructor response
            constructed_parameters = None
            if constructor_results:
                try:
                    last_response = constructor_results[-1].strip()
                    logger.info(f"[{self.name}] 🔍 Parsing constructor response as JSON...")
                    
                    # Handle case where response might be wrapped in markdown code blocks
                    if last_response.startswith('```') and last_response.endswith('```'):
                        # Extract JSON from code block
                        lines = last_response.split('\n')
                        json_content = '\n'.join(lines[1:-1])  # Remove first and last line
                        constructed_parameters = json.loads(json_content)
                    else:
                        constructed_parameters = json.loads(last_response)
                    
                    logger.info(f"[{self.name}] ✅ Successfully parsed parameters: {json.dumps(constructed_parameters, indent=2)}")
                    
                except json.JSONDecodeError as e:
                    logger.error(f"[{self.name}] ❌ Failed to parse constructor response as JSON: {e}")
                    logger.error(f"[{self.name}] Raw response: {last_response}")
                except Exception as e:
                    logger.error(f"[{self.name}] ❌ Unexpected error parsing JSON: {e}")
                    logger.error(f"[{self.name}] Raw response: {last_response if 'last_response' in locals() else 'N/A'}")
            
            # Now WE call invoke_api_tool with the constructed parameters
            if constructed_parameters:
                logger.info(f"[{self.name}] 🚀 Calling invoke_api_tool with constructed parameters...")
                from .utils.task_execution_utils import invoke_api_tool
                
                api_result = invoke_api_tool(entity, intent, constructed_parameters)
                logger.info(f"[{self.name}] ✅ API call completed")
                
                final_result = {
                    "execution_type": "llm_assisted",
                    "entity": entity,
                    "intent": intent,
                    "original_parameters": parameters,
                    "constructed_parameters": constructed_parameters,
                    "constructor_response": constructor_results,
                    "success": True,
                    "result": api_result
                }
            else:
                logger.error(f"[{self.name}] ❌ Could not extract parameters from constructor response")
                final_result = {
                    "execution_type": "llm_assisted",
                    "entity": entity,
                    "intent": intent,
                    "original_parameters": parameters,
                    "constructor_response": constructor_results,
                    "success": False,
                    "error": "Failed to parse parameters from constructor response"
                }
                
            
            return final_result
            
        except Exception as e:
            logger.error(f"[{self.name}] ❌ LLM-assisted execution failed: {e}")
            return {
                "execution_type": "llm_assisted",
                "entity": entity,
                "intent": intent,
                "success": False,
                "error": str(e)
            }
    
    async def _execute_other_tool(self, tool_name: str, entity: str, intent: str, parameters: dict, ctx: InvocationContext) -> dict:
        """Execute non-API tools like web_search_agent, read_content_from_url, etc."""
        try:
            import json
            logger.info(f"[{self.name}] 🔧 Executing other tool: {tool_name}")
            
            if tool_name == "web_search_agent":
                # Create a simple search agent directly here
                from google.adk.agents import LlmAgent
                from google.adk.tools import google_search
                instruction = f"""You are an intelligent web search agent. You MUST use the google_search tool to search: {parameters["query"]}
                
                Get a very comprehensive search result with the most relevant information."""
                web_search_agent = LlmAgent(
                    name="web_search_agent",
                    model="gemini-2.5-flash-lite",
                    description="Agent that searches the internet for information",
                    instruction=instruction,
                    # google_search is a pre-built tool which allows the agent to perform Google searches.
                    tools=[google_search]
                )
                
                # Execute the web search agent
                search_results = []
                
                async for event in web_search_agent.run_async(ctx):
                    logger.info(f"[{self.name}] 📨 Search agent event type: {type(event)}")
                    
                    # Capture text responses (final results)
                    if hasattr(event, 'content') and event.content and event.content.parts:
                        for part in event.content.parts:
                            if part.text:
                                search_results.append(part.text)
                                logger.info(f"[{self.name}] 📝 Search agent text: {part.text[:200]}...")
                    
                    # Also capture tool call responses (google_search results)
                    if hasattr(event, 'tool_call_response'):
                        tool_response = event.tool_call_response
                        logger.info(f"[{self.name}] 🔧 Search tool response: {str(tool_response)[:300]}...")
                        # Let the agent process this, don't add to search_results directly
                
                logger.info(f"[{self.name}] ✅ Simple Search Agent completed with {len(search_results)} responses")
                
                # Try to parse the search result as JSON, fallback to text
                search_result_data = None
                if search_results:
                    try:
                        last_response = search_results[-1].strip()
                        # Handle markdown code blocks
                        if last_response.startswith('```') and last_response.endswith('```'):
                            lines = last_response.split('\n')
                            json_content = '\n'.join(lines[1:-1])
                            search_result_data = json.loads(json_content)
                        else:
                            search_result_data = json.loads(last_response)
                        logger.info(f"[{self.name}] ✅ Successfully parsed search results as JSON")
                    except json.JSONDecodeError:
                        logger.info(f"[{self.name}] 📝 Using search results as text (not JSON)")
                        search_result_data = {"content": last_response, "sources": []}
                
                result = {
                    "tool_name": "web_search_agent",
                    "search_query": parameters["query"],
                    "search_response": search_result_data or {"content": "No search results", "sources": []},
                    "type": "json"
                }
                
            elif tool_name == "read_content_from_url":
                from ai_assistant.tools.google_drive_utils import read_content_from_url
                
                url = parameters.get("url", "")
                title = parameters.get("title", "")
                description = parameters.get("description", "")
                
                logger.info(f"[{self.name}] 📄 Reading content from URL: {url}")
                
                if not url:
                    result = {
                        "tool_name": "read_content_from_url",
                        "error": "No URL provided",
                        "type": "error"
                    }
                else:
                    try:
                        # Get tool context from session state or create one
                        tool_context = getattr(ctx.session.state, 'tool_context', None)
                        if not tool_context:
                            # Create a tool context using the current invocation context
                            from google.adk.tools import ToolContext
                            tool_context = ToolContext(invocation_context=ctx)
                        
                        # Call the actual URL content reader
                        content_result = read_content_from_url(
                            tool_context=tool_context,
                            url=url,
                            title=title,
                            description=description
                        )
                        
                        # Parse the result which should be a JSON string
                        try:
                            result = json.loads(content_result)
                            
                            # Check if the result contains an error
                            if result.get("error") or result.get("type") == "error":
                                logger.error(f"[{self.name}] ❌ URL reading failed: {result.get('error', 'Unknown error')}")
                            else:
                                logger.info(f"[{self.name}] ✅ Successfully read content from URL: {url}")
                                
                        except json.JSONDecodeError:
                            result = {
                                "tool_name": "read_content_from_url",
                                "message": content_result,
                                "type": "text",
                                "url": url
                            }
                            
                    except Exception as e:
                        logger.error(f"[{self.name}] ❌ Error reading URL content: {str(e)}")
                        result = {
                            "tool_name": "read_content_from_url",
                            "error": f"Failed to read URL content: {str(e)}",
                            "url": url,
                            "type": "error"
                        }
                
            else:
                result = {"error": f"Unknown tool: {tool_name}"}
            
            # Prepare the final result
            final_result = {
                "execution_type": "other_tool",
                "tool_name": tool_name,
                "entity": entity,
                "intent": intent,
                "success": True,
                "result": result
            }
            
            
            
            return final_result
            
        except Exception as e:
            logger.error(f"[{self.name}] ❌ Other tool execution failed: {e}")
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
                        
                        # Tools detected - they will be executed directly in _run_async_impl
                        if tools_to_execute:
                            logger.info(f"[{self.name}] Tools detected! Will execute directly in workflow (no agent transfer needed)")
                            # Just store the tools for execution - no dynamic agent creation
                        
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
