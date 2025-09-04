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
        
        # Build dynamic entity list (with fallback if empty)
        entity_descriptions = []
        
        # Always include Google Workspace entities (external services)
        google_workspace_entities = [
            "**GoogleDrive**: Search Google Drive for documents and files",
            "**GoogleDoc**: Create Google Documents with content",
            "**GoogleSheet**: Create Google Spreadsheets with data"
        ]
        entity_descriptions.extend(google_workspace_entities)
        
        # Add all entities from config dynamically
        for entity in entities:
            if isinstance(entity, dict):
                # Handle both "name" and "entity" field names
                name = entity.get("name") or entity.get("entity")
                if name:
                    description = entity.get("description", name + " entity")
                    entity_desc = "**" + name + "**: " + description
                    entity_descriptions.append(entity_desc)
        
        entities_text = "\n".join(["• " + desc for desc in entity_descriptions])
        
        # Get user profile and screen context from state
        user_profile_data = ctx.state.get('user_profile', {})
        screen_context_data = ctx.state.get('screen_context', {})
        
        user_profile_text = "User Profile: " + str(user_profile_data) if user_profile_data else "User Profile: Not available"
        screen_context_text = "Screen Context: " + str(screen_context_data) if screen_context_data else "Screen Context: Not available"
        
        # Build instruction without f-string to avoid JSON escaping issues
        instruction = """# Task Planner + Executor Agent

You are a smart AI that both **plans** and **executes** user requests efficiently using Gemini 2.5 Pro.

## Available Context
- **""" + user_profile_text + """**
- **""" + screen_context_text + """**

## Available Entities
""" + entities_text + """

## Phase 1: Task Planning
First, analyze the user request and create an action plan:

**Generate action plan with steps:**
- **step**: Step number (1, 2, 3...)
- **action**: Plain English description  
- **entity**: Entity name (Partner, Contact, GoogleDoc, etc.)
- **intent**: Search, Create, Update, Delete
- **params**: JSON object with parameters

## Phase 2: Task Execution  
Then immediately execute each step using tool calls:

**🚨 CRITICAL: NO PYTHON CODE - USE TOOL CALLS ONLY**

**For API Operations:**
1. Call `find_entity_endpoint(entity_name, intent, params_json)`
2. **FALLBACK ENTITY LOGIC**: If no endpoint found, try alternative entities:
   - Partner ↔ Contact (interchangeable for contact info)
   - Interaction → Partner/Contact (for related data)
   - Opportunity → Partner (for partner data)
3. Call `invoke_api_tool(url, method, body)`

**For Google Workspace:**
- **Google Doc**: `convert_markdown_to_google_doc(markdown_content, filename)`
- **Google Sheet**: `create_google_sheet_from_list_data(title, data, folder_id)`
- **Google Drive**: `search_unops_google_drive(query)`

**For Document Creation Workflows:**
1. Get source data via API operations
2. Format data into **COMPREHENSIVE markdown** (include ALL fields, detailed sections)
3. Create Google Doc with the comprehensive content

**Always call `exit_loop_on_success()` when complete!**

## Examples

**User: "Get list of partners"**
1. **Plan**: [{"step": 1, "action": "Search for partners", "entity": "Partner", "intent": "Search", "params": {}}]
2. **Execute**: 
   - find_entity_endpoint("Partner", "Search", "{}")
   - invoke_api_tool(url, method, body)
   - exit_loop_on_success()

**User: "Create Google doc with partner summary"**  
1. **Plan**: [
   {"step": 1, "action": "Get partners", "entity": "Partner", "intent": "Search", "params": {}},
   {"step": 2, "action": "Create doc", "entity": "GoogleDoc", "intent": "Create", "params": {"title": "Partner Summary"}}
   ]
2. **Execute**: 
   - Get partners via API
   - Format as comprehensive markdown
   - convert_markdown_to_google_doc(markdown_content, "Partner Summary")
   - exit_loop_on_success()

## 🚨 MANDATORY EXIT CONDITIONS

**ALWAYS call `exit_loop_on_success()` in these scenarios:**

✅ **SUCCESS SCENARIOS:**
- Entity data retrieved successfully → Store results → **CALL `exit_loop_on_success()`**
- Google Doc/Sheet created successfully → Store results → **CALL `exit_loop_on_success()`**
- All action plan steps completed → **CALL `exit_loop_on_success()`**

✅ **ERROR SCENARIOS:**
- API retries exhausted → Store error → **CALL `exit_loop_on_success()`**
- No endpoints found → Store "not supported" → **CALL `exit_loop_on_success()`**
- Tool execution failure → Store error → **CALL `exit_loop_on_success()`**

✅ **CLARIFICATION SCENARIOS:**
- Missing required parameters → Store clarification request → **CALL `exit_loop_on_success()`**
- Ambiguous request → Store clarification request → **CALL `exit_loop_on_success()`**

**🚨 CRITICAL: YOU MUST ACTUALLY CALL THE `exit_loop_on_success()` FUNCTION**
- This is a function call, not a text response
- Every execution path must end with this call

## Comprehensive Document Creation

When creating Google Docs, make them **COMPREHENSIVE**:

**Example: Partner Summary Structure**
```markdown
# Comprehensive Partner Summary: [Partner Name]

## Executive Summary
[Detailed paragraph about partner's role and importance]

## Basic Information
- **Organization Name**: [Full Name]
- **Partner Type**: [Type with explanation]
- **Geographic Location**: [Full details]
- **Partnership Status**: [Active/Inactive with context]

## Contact Information
- **Primary Contact**: [Name and title]
- **Email**: [Email address]
- **Phone**: [Phone number]
- **Website**: [URL]

## Partnership Details
- **Start Date**: [Date]
- **Focus Areas**: [Detailed list]
- **Collaboration History**: [Historical context]

## Projects and Activities
- **Current Projects**: [List with descriptions]
- **Past Collaborations**: [Historical data]

## Additional Information
[All other relevant API data]

---
*Generated on [date]*
```

**Content Rules:**
1. Include ALL available data fields from API responses
2. Add context and explanations for each field
3. Use multiple headers, lists, tables for organization
4. Write in complete sentences and paragraphs
5. Never create brief summaries - always comprehensive

**NEVER write Python code - use tool calls only!**
"""
        
        return instruction
        
    except Exception as e:
        # Fallback instruction if entity loading fails
        user_profile_fallback = str(ctx.state.get('user_profile', 'Not available'))
        screen_context_fallback = str(ctx.state.get('screen_context', 'Not available'))
        
        return """# Task Planner + Executor Agent (Fallback Mode)

You are a smart AI that both plans and executes user requests using Gemini 2.5 Pro.

## Available Context
- User Profile: """ + user_profile_fallback + """
- Screen Context: """ + screen_context_fallback + """

## Available Entities
• **Partner**: Partner organizations and entities
• **Contact**: Individual contacts and people
• **Interaction**: Interactions and communications
• **Opportunity**: Opportunities and engagements
• **GoogleDrive**: Search Google Drive for documents and files
• **GoogleDoc**: Create Google Documents with content
• **GoogleSheet**: Create Google Spreadsheets with data

## Phase 1: Task Planning
Create an action plan with steps containing: step, action, entity, intent, params

## Phase 2: Task Execution  
Execute using tool calls only - NO PYTHON CODE!

**For API Operations:**
1. find_entity_endpoint(entity_name, intent, params_json)
2. invoke_api_tool(url, method, body)

**For Google Workspace:**
- convert_markdown_to_google_doc(markdown_content, filename)
- create_google_sheet_from_list_data(title, data, folder_id)

**Always call exit_loop_on_success() when complete!**

**NEVER write Python code - use tool calls only!**
"""

# The combined LLM agent using Gemini 2.5 Pro for better performance
task_executor_llm_agent = LlmAgent(
    name="task_planner_executor_llm_agent", 
    description="Combined agent that plans and executes tasks using Gemini 2.5 Pro for optimal speed and performance",
    model="gemini-2.5-pro",  # Use Gemini 2.5 Pro for better performance
    instruction=dynamic_combined_instruction_callback,  # Use dynamic instruction callback
    tools=task_executor_tools,
    before_model_callback=combined_before_model_callback,
    include_contents="default",
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)

# The LoopAgent that wraps the LLM agent for iterative execution
task_executor_agent = LoopAgent(
    name="task_planner_executor_agent",
    description="Combined task planner and executor using Gemini 2.5 Pro for speed and performance - handles both planning and execution in one step",
    sub_agents=[task_executor_llm_agent],
    max_iterations=10  # Prevent infinite loops
)