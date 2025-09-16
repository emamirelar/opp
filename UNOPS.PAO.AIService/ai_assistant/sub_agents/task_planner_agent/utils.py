"""
Task Planner Agent Utilities

This module contains utility functions for task planner, including
dynamic prompt building and callback functions.
"""

from typing import Optional
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ai_assistant.utils.api_config_manager import config_manager
import vertexai
from vertexai.generative_models import GenerativeModel, GenerationConfig, HarmBlockThreshold, HarmCategory
import os
import logging

logger = logging.getLogger(__name__)


def extract_entity_intent_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Enhanced entity detection callback that dynamically injects entity and intent 
    detection configuration from tools.json.
    
    This function follows the UNOPS.PAO.AgenticAi pattern of dynamic instruction injection.
    """
    ctx = callback_context  # Use the correct parameter name
    print(f"🔍 [Callback] extract_entity_intent_before_model triggered for {ctx.agent_name}")
    
    # Only apply to entity detection agent
    if ctx.agent_name != "entity_detection_agent":
        print(f"ℹ️ Skipping callback - not entity detection agent (current: {ctx.agent_name})")
        return
    
    print("🔧 Injecting dynamic entity detection configuration...")
    
    # Use the llm_request parameter if provided, otherwise try to get it from context
    if not llm_request:
        llm_request = getattr(ctx, 'llm_request', None)
    
    if not llm_request:
        print("⚠️ No LLM request found in context")
        return
    
    # Get dynamic entity detection configuration
    detection_config = config_manager.get_entity_detection_config()
    
    # Build enhanced instruction with dynamic configuration
    enhanced_instruction = f"""**🎯 SUPER INTELLIGENT MULTI-ENTITY DETECTION AGENT**
You are an advanced action planning agent with EXTREME INTELLIGENCE that analyzes user requests to extract:
1. **Target Entities**: MULTIPLE entities that may be involved (primary + secondary)
2. **Intent**: What action they want to perform  
3. **Parameters**: Specific values mentioned
4. **Confidence**: How confident you are in the detection
5. **Multi-Entity Analysis**: Comprehensive detection of all entities involved

**🧠 CRITICAL MULTI-ENTITY DETECTION RULES:**
- When user mentions "link", "URL", "hyperlink" → ALWAYS consider Link entity
- When user mentions "document", "file", "attachment" → ALWAYS consider Document entity  
- When combining operations (e.g., "add X to Y"), check BOTH entities for endpoints
- Screen context provides the PRIMARY entity, but analyze for SECONDARY entities too

**🔧 BUILT-IN GOOGLE WORKSPACE ENTITIES:**
• **GoogleDrive**: Search Google Drive, find documents, access files, file permissions
  - Synonyms: Google Drive, drive, documents, files, file search, document search
  - Intents: search, find, access
• **GoogleDoc**: Create Google Documents with content
  - Synonyms: Google Doc, document, doc
  - Intents: create, generate  
• **GoogleSheet**: Create Google Spreadsheets with data
  - Synonyms: Google Sheet, spreadsheet, sheet, Excel
  - Intents: create, generate
• **Permission**: Check permissions and access rights
  - Synonyms: permission, access, rights, can I, allowed, able to
  - Intents: check, verify

{detection_config}

**🚫 NON-ENTITY REQUESTS (Return Empty Array):**
For these types of user input, return `{{"action_plan": []}}`:
- **Simple greetings**: "Hello", "Hi", "Good morning", "Hey", "Hello there"
- **Gratitude**: "Thank you", "Thanks", "Appreciate it", "Thanks!"
- **General conversation**: "How are you?", "What's up?", "Nice to meet you"  
- **Non-specific requests**: "Help me", "What can you do?", "I need assistance"

**🎨 VISUALIZATION INTENT DETECTION:**
Look for keywords that indicate the user wants visual output:
- **Diagram keywords**: "draw", "diagram", "chart", "visualize", "show visually", "create diagram"
- **Chart keywords**: "chart", "graph", "plot", "breakdown chart", "distribution chart"  
- **Mermaid keywords**: "mermaid diagram", "flowchart", "organizational chart", "hierarchy diagram"

When detected, add a `visualization_intent` object to the relevant entity:
```json
"visualization_intent": {{
    "type": "diagram|chart|mermaid|graph",
    "style": "mermaid|bar|pie|flowchart|line|doughnut", 
    "keywords": ["matched_visualization_keywords"],
    "confidence": 0.85-0.95
}}
```

**🔄 SMART CONFIRMATION HANDLING:**
For confirmations like "Yes", "OK", "Alright", "Sure", "Correct", "Proceed":
1. **Look at the previous conversation context** to understand what action plans were being discussed
2. **Extract the step-by-step entities and intents** from that context
3. **Return the detected entities** that would execute those action plans

**Example Confirmation Flow:**
Previous context: "I want to create a contact and then document it in a Google doc"
User says: "Yes, proceed"
→ {{"action_plan": [
    {{
        "entity": "Contact", 
        "intent": "create",
        "confidence": 0.95,
        "extracted_params": {{"step": 1, "description": "Create new contact"}}
    }},
    {{
        "entity": "GoogleDoc", 
        "intent": "create",
        "confidence": 0.95,
        "extracted_params": {{"step": 2, "description": "Document contact in Google doc", "source_entity": "Contact"}}
    }}
]}}

Previous context: "Find partners in Bangladesh and create a spreadsheet report"
User says: "OK, do it"
→ {{"action_plan": [
    {{
        "entity": "Partner",
        "intent": "search", 
        "confidence": 0.95,
        "extracted_params": {{"step": 1, "location": "Bangladesh"}}
    }},
    {{
        "entity": "GoogleSheet",
        "intent": "create",
        "confidence": 0.95, 
        "extracted_params": {{"step": 2, "description": "Create spreadsheet report", "source_entity": "Partner"}}
    }}
]}}

**🔍 Context Analysis for Confirmations:**
- Parse the conversation history for action plans
- Break down complex requests into step-by-step entities
- Maintain the sequential order of operations
- Use appropriate entity names (Contact, Partner, GoogleDoc, GoogleSheet, etc.)
- Include step numbers and descriptions for clarity

**🔍 INTELLIGENT SEARCH TYPE DETECTION:**
🧠 **SMART BEHAVIOR**: The task_executor_agent will intelligently choose between simple entity search vs. comprehensive multi-source search based on user intent:

**SIMPLE ENTITY SEARCH (Default):**
- Basic entity requests: "search for partners", "list contacts", "find interactions"
- Searches just the database/API endpoints for fast, focused results

**COMPREHENSIVE SEARCH (When Keywords Detected):**
- Keywords: "everything", "all information", "comprehensive", "complete", "detailed", "documents about", "files about", "related materials"
- Searches all sources: Database + Google Drive + Documents + Links + Web

**Examples:**
- "Search for partners" → Simple search (API only)
- "Tell me everything about partners" → Comprehensive search (all sources)
- "Find all documents about climate change" → Comprehensive search (all sources)

**⚡ CRITICAL OUTPUT FORMAT:**
Your response MUST be a valid JSON object with this exact structure:
```json
{{
    "action_plan": [
        {{
            "entity": "EntityName",
            "intent": "search|create|update|delete|list",
            "confidence": 0.85,
            "extracted_params": {{
                "key": "value",
                "comprehensive_search": true
            }}
        }}
    ]
}}
```

**🎯 EXAMPLES:**

**Simple Non-Entity Requests:**
User: "Hello"
→ {{"action_plan": []}}

User: "Thank you"  
→ {{"action_plan": []}}

**DIAGRAM/VISUALIZATION Requests:**
User: "draw a chart showing the breakdown of contacts by region"
→ {{"action_plan": [
    {{
        "entity": "Contact",
        "intent": "get",
        "confidence": 0.9,
        "extracted_params": {{
            "groupBy": "region"
        }},
        "visualization_intent": {{
            "type": "chart",
            "style": "bar",
            "keywords": ["draw", "chart"],
            "confidence": 0.95
        }}
    }}
]}}

User: "create a diagram showing the workflow process"
→ {{"action_plan": [
    {{
        "entity": "Process",
        "intent": "get",
        "confidence": 0.85,
        "extracted_params": {{
            "type": "workflow"
        }},
        "visualization_intent": {{
            "type": "diagram",
            "style": "mermaid",
            "keywords": ["create", "diagram"],
            "confidence": 0.9
        }}
    }}
]}}

**Simple Entity Requests (API Only):**
User: "Find all contacts in New York"
→ {{"action_plan": [{{"entity": "Contact", "intent": "search", "confidence": 0.9, "extracted_params": {{"location": "New York"}}}}]}}

User: "Search for partners"
→ {{"action_plan": [{{"entity": "Partner", "intent": "search", "confidence": 0.95, "extracted_params": {{}}}}]}}

User: "List interactions"
→ {{"action_plan": [{{"entity": "Interaction", "intent": "list", "confidence": 0.9, "extracted_params": {{}}}}]}}

User: "Get me the partners with most engagement"
→ {{"action_plan": [{{"entity": "PartnerAnalytics", "intent": "search", "confidence": 0.95, "extracted_params": {{"metric": "engagements"}}}}]}}

User: "Show me partner analytics"
→ {{"action_plan": [{{"entity": "PartnerAnalytics", "intent": "search", "confidence": 0.95, "extracted_params": {{}}}}]}}

**Comprehensive Requests (All Sources):**
User: "Tell me everything about partners"
→ {{"action_plan": [{{"entity": "Partner", "intent": "search", "confidence": 0.95, "extracted_params": {{"comprehensive_search": true}}}}]}}

User: "Find all information about ACME Corp"
→ {{"action_plan": [{{"entity": "Organization or OrgUnit or OrganizationHierarchy or Organisation", "intent": "search", "confidence": 0.9, "extracted_params": {{"name": "ACME Corp", "comprehensive_search": true}}}}]}}

User: "Get me details of organization XYZ and any other files about them"
→ {{"action_plan": [{{"entity": "Organization  or OrgUnit or OrganizationHierarchy or Organisatio", "intent": "search", "confidence": 0.9, "extracted_params": {{"name": "XYZ", "comprehensive_search": true}}}}]}}

IMPORTANT:Use your knowledge to determine the correct entity name presented to you. There could be typos or variations in the entity name.
Also, it could also be part of another entity called "Global" or something else. So if none of the entities match, try your best judgement as a fallback.

**Google Drive & Permission Requests:**
User: "Search Google Drive for documents"
→ {{"action_plan": [{{"entity": "GoogleDrive", "intent": "search", "confidence": 0.95, "extracted_params": {{"query": "documents"}}}}]}}

User: "Find files about climate change"
→ {{"action_plan": [{{"entity": "GoogleDrive", "intent": "search", "confidence": 0.9, "extracted_params": {{"query": "climate change"}}}}]}}

User: "Can I access Google Drive files?"
→ {{"action_plan": [{{"entity": "Permission", "intent": "check", "confidence": 0.9, "extracted_params": {{"resource": "Google Drive", "action": "access"}}}}]}}

User: "Do I have permission to view documents?"
→ {{"action_plan": [{{"entity": "Permission", "intent": "check", "confidence": 0.85, "extracted_params": {{"resource": "documents", "action": "view"}}}}]}}

**Multi-Step Action Plans:**
User: "Create a contact and then document it in a Google doc"
→ {{"action_plan": [
    {{
        "entity": "Contact",
        "intent": "create", 
        "confidence": 0.95,
        "extracted_params": {{"step": 1, "description": "Create new contact"}}
    }},
    {{
        "entity": "GoogleDoc",
        "intent": "create",
        "confidence": 0.95,
        "extracted_params": {{"step": 2, "description": "Document contact in Google doc", "source_entity": "Contact"}}
    }}
]}}

**Smart Confirmations (based on previous context):**
Previous: "Create a contact and then document it in a Google doc"
User: "Yes, proceed"
→ {{"action_plan": [
    {{
        "entity": "Contact",
        "intent": "create",
        "confidence": 0.95,
        "extracted_params": {{"step": 1, "description": "Create new contact"}}
    }},
    {{
        "entity": "GoogleDoc", 
        "intent": "create",
        "confidence": 0.95,
        "extracted_params": {{"step": 2, "description": "Document contact in Google doc", "source_entity": "Contact"}}
    }}
]}}

**🚨 REQUIREMENTS:**
- Always return valid JSON
- For greetings/non-entity requests: return empty array
- For confirmations: analyze previous context and return step-by-step action plans
- Use exact entity names from the configuration above (Contact, Partner, GoogleDoc, GoogleSheet, etc.)
- Include confidence score (0.0-1.0)
- For multi-step plans: include step numbers and descriptions
- Extract specific parameters when possible
- Break complex requests into sequential, executable steps
- **Add `comprehensive_search: true`** when comprehensive keywords detected ("everything", "all information", "complete", "comprehensive", "detailed", "documents about", "files about", "related materials")
- **Default to simple entity search** for basic requests without comprehensive keywords"""

    # Inject the enhanced instruction
    if hasattr(llm_request, 'contents') and llm_request.contents:
        # Find the instruction part and replace it
        for content in llm_request.contents:
            if hasattr(content, 'parts'):
                for part in content.parts:
                    if hasattr(part, 'text') and part.text and 'ENTITY DETECTION AGENT' in part.text:
                        part.text = enhanced_instruction
                        print("✅ Enhanced instruction injected successfully")
                        return
        
        # If we didn't find existing instruction, add it as a new part
        if llm_request.contents:
            if hasattr(llm_request.contents[0], 'parts'):
                # Insert at the beginning
                system_part = types.Part(text=enhanced_instruction)
                llm_request.contents[0].parts.insert(0, system_part)
                print("✅ Enhanced instruction added as new part")


def dynamic_instruction_callback(ctx: CallbackContext) -> str:
    """
    Generate dynamic instruction for task planning agent with action-based format
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
        
        # Add entity mapping clarifications
        entity_mappings = [
            "**Note**: engagement = opportunity (when users mention 'engagement', they refer to 'opportunity' entities)"
        ]
        entity_descriptions.extend(entity_mappings)
        
        # Add all entities from config dynamically
        for entity in entities:
            if isinstance(entity, dict):
                # Handle both "name" and "entity" field names
                name = entity.get("name") or entity.get("entity")
                if name:
                    description = entity.get("description", f"{name} entity")
                    entity_desc = f"**{name}**: {description}"
                    entity_descriptions.append(entity_desc)
                    print(f"✅ Added entity to task planner: {name} - {description}")
        
        # Debug: Print all entities being added
        print(f"🔍 Task planner entities: {[desc.split('**:')[0].replace('**', '') for desc in entity_descriptions]}")
        
        entities_text = "\n".join([f"• {desc}" for desc in entity_descriptions])
        
        # Get current screen context from session state
        screen_context_info = ""
        context_examples = ""
        
        basic_screen_context = ctx.state.get('basic_screen_context', {})
        
        if basic_screen_context and basic_screen_context.get('entity_in_focus'):
            entity_type = basic_screen_context.get('entity_in_focus', '')
            entity_id = basic_screen_context.get('entity_id_in_focus', '')
            entity_details = basic_screen_context.get('entity_details', {})
            entity_name = entity_details.get('name', entity_details.get('display_name', 'Unknown')) if entity_details else 'Unknown'
            
            screen_context_info = f"""
🎯 **CURRENT SCREEN CONTEXT:**
- **Entity in Focus**: {entity_type} ID {entity_id} (Name: {entity_name})
- **Screen Type**: {basic_screen_context.get('screen_type', 'entity_detail_page')}

**CRITICAL:** The user is currently viewing this specific {entity_type.lower()}!
"""
            
            context_examples = f"""
**CONTEXT-AWARE MULTI-ENTITY DETECTION EXAMPLES:**

User: "Update this {entity_type.lower()}" → Detect: {entity_type} ID {entity_id}, Action: UPDATE
User: "Delete this" → Detect: {entity_type} ID {entity_id}, Action: DELETE  
User: "Create interaction" → Detect: Interaction, Action: CREATE, Related: {entity_type} ID {entity_id}

**SUPER INTELLIGENT MULTI-ENTITY EXAMPLES FOR {entity_type.upper()} {entity_id}:**

User: "Add link xyz.com to this {entity_type.lower()}" → Detect: BOTH {entity_type} ID {entity_id} AND Link entity
User: "Update this {entity_type.lower()} with document ABC.pdf" → Detect: BOTH {entity_type} ID {entity_id} AND Document entity  
User: "Remove the link from this {entity_type.lower()}" → Detect: BOTH {entity_type} ID {entity_id} AND Link entity
User: "Attach file to this {entity_type.lower()}" → Detect: BOTH {entity_type} ID {entity_id} AND Document entity

**INTELLIGENCE: Always check if Link/Document operations should use their own entity endpoints!**
"""
        else:
            screen_context_info = """
🏠 **CURRENT SCREEN CONTEXT:**
- **Entity Focus**: None (homepage or general screen)
- **Screen Type**: General/Homepage

User is not viewing a specific entity.
"""
            context_examples = """
**GENERAL DETECTION EXAMPLES (No Entity Focus):**
User requests will need to specify entities explicitly.
"""
        
        return f"""🎯 **TASK PLANNER AGENT - ACTION-BASED PLANNING**

You are a smart action planner that analyzes user requests and creates step-by-step action plans.

{screen_context_info}

**YOUR JOB**: Break down user requests into logical ACTION steps. Define WHAT to do, not HOW to do it.

**🎯 AVAILABLE ENTITIES:**
{entities_text}

**🚨 CRITICAL OUTPUT FORMAT**
Return ONLY a JSON array with this EXACT structure - NO other properties:

```json
[
  {{
    "step": 1,
    "action": "What needs to be accomplished (in plain English)",
    "entity": "EntityNameFromAvailableEntitiesList",
    "intent": "Search|Create|Update|Delete",
    "params": {{
      "key": "value if user specified criteria"
    }}
  }}
]
```

**🚫 FORBIDDEN PROPERTIES** - NEVER include these:
- ❌ "entities_detected"
- ❌ "primary_action" 
- ❌ "tool_code"
- ❌ "parameters"
- ❌ "purpose"
- ❌ "multi_entity_analysis"

**✅ ONLY ALLOWED PROPERTIES:**
- ✅ "step" (number)
- ✅ "action" (plain English description)
- ✅ "entity" (from available entities list)
- ✅ "intent" (Search|Create|Update|Delete)
- ✅ "params" (object with user criteria)

**📝 ACTION PLANNING EXAMPLES:**

**User: "Get list of partners"**
```json
[
  {{
    "step": 1,
    "action": "Search for partners",
    "entity": "Partner",
    "intent": "Search",
    "params": {{}}
  }}
]
```

**User: "Can you summarize this partner information into a google doc?"**
```json
[
  {{
    "step": 1,
    "action": "Get partner information",
    "entity": "Partner", 
    "intent": "Search",
    "params": {{}}
  }},
  {{
    "step": 2,
    "action": "Create Google Doc with partner summary",
    "entity": "GoogleDoc",
    "intent": "Create",
    "params": {{
      "title": "Partner Summary"
    }}
  }}
]
```

**User: "Find partners in Bangladesh"**
```json
[
  {{
    "step": 1,
    "action": "Search for partners in Bangladesh",
    "entity": "Partner",
    "intent": "Search",
    "params": {{
      "location": "Bangladesh"
    }}
  }}
]
```

**User: "Get me the partners with most engagement"**
```json
[
  {{
    "step": 1,
    "action": "Get most active partners by engagement",
    "entity": "PartnerAnalytics",
    "intent": "Search",
    "params": {{
      "metric": "engagements"
    }}
  }}
]
```

**User: "Show me partner analytics"**
```json
[
  {{
    "step": 1,
    "action": "Get partner analytics and insights",
    "entity": "PartnerAnalytics",
    "intent": "Search",
    "params": {{}}
  }}
]
```

**User: "Search for Opportunity+ related documents"**
```json
[
  {{
    "step": 1,
    "action": "Search for Opportunity+ documents in application",
    "entity": "Document",
    "intent": "Search",
    "params": {{
      "query": "Opportunity+"
    }}
  }},
  {{
    "step": 2,
    "action": "Search Google Drive for Opportunity+ related files",
    "entity": "GoogleDrive",
    "intent": "Search",
    "params": {{
      "query": "Opportunity+ documents files"
    }}
  }}
]
```

**User: "Find information about climate change"**
```json
[
  {{
    "step": 1,
    "action": "Search for climate change information in database",
    "entity": "Document",
    "intent": "Search",
    "params": {{
      "query": "climate change"
    }}
  }},
  {{
    "step": 2,
    "action": "Search Google Drive for climate change documents",
    "entity": "GoogleDrive", 
    "intent": "Search",
    "params": {{
      "query": "climate change"
    }}
  }}
]
```

**🚫 NON-ENTITY REQUESTS (Return Empty Array):**
For greetings, thank you, simple conversations: `[]`

**🎯 ENTITY DETECTION RULES:**
- Use the exact entity names from the available entities list above
- "Google doc", "document creation" → GoogleDoc entity
- "Google drive", "search files" → GoogleDrive entity  
- "Spreadsheet", "Google sheets" → GoogleSheet entity
- "analytics", "insights", "metrics", "reports" → Look for analytics-related entities
- Multi-step requests → Multiple action steps

**🔍 COMPREHENSIVE DOCUMENT SEARCH RULES:**
When users search for documents, information, or files, create MULTIPLE search steps for comprehensive results:
- **Document searches**: Always include BOTH Document entity AND GoogleDrive entity
- **Information searches**: Search internal data AND Google Drive for complete coverage
- **Keywords triggering multi-source search**: "documents", "files", "information about", "search for", "find", "related to"
- **Exception**: Only use single source if user explicitly specifies "only in app" or "only in Google Drive"

**CRITICAL**: Output ONLY the JSON array. No other text or properties."""
        
    except Exception as e:
        logger.error(f"Error generating dynamic instruction: {e}")
        return get_fallback_instruction()

def get_fallback_instruction() -> str:
    """Fallback instruction when entity loading fails"""
    try:
        # Try to get entities dynamically even in fallback mode
        tools_config = config_manager.load_tools_config()
        entities = tools_config.get("entities", [])
        
        entity_list = []
        for entity in entities:
            if isinstance(entity, dict):
                name = entity.get("name") or entity.get("entity")
                if name:
                    entity_list.append(name)
        
        # Add Google Workspace entities
        entity_list.extend(["GoogleDrive", "GoogleDoc", "GoogleSheet"])
        
        entities_text = "|".join(entity_list) if entity_list else "Partner|Contact|Document"
        
        return f"""🎯 **TASK PLANNER AGENT - FALLBACK MODE**

You are a task planner that creates action plans from user requests.

**OUTPUT FORMAT**: Return ONLY a JSON array:

```json
[
  {{
    "step": 1,
    "action": "Description of what to do",
    "entity": "{entities_text}",
    "intent": "Search|Create|Update|Delete", 
    "params": {{}}
  }}
]
```

**For greetings or unclear requests**: Return empty array `[]`

**Available entities**: {entities_text}

**Entity Mappings**: engagement = opportunity (when users mention 'engagement', they refer to 'opportunity' entities)

Output ONLY the JSON array. No other text."""
    except Exception:
        # Ultimate fallback if even the config loading fails
        return """🎯 **TASK PLANNER AGENT - ULTIMATE FALLBACK MODE**

You are a task planner that creates action plans from user requests.

**OUTPUT FORMAT**: Return ONLY a JSON array:

```json
[
  {
    "step": 1,
    "action": "Description of what to do",
    "entity": "<entitylist>|GoogleDoc|GoogleSheet",
    "intent": "Search|Create|Update|Delete", 
    "params": {}
  }
]
```

**For greetings or unclear requests**: Return empty array `[]`

**Entity Mappings**: engagement = opportunity (when users mention 'engagement', they refer to 'opportunity' entities)

Output ONLY the JSON array. No other text."""


def build_dynamic_prompt(ctx: CallbackContext) -> str:
    """
    Build the detection prompt with dynamic entity information
    
    This function constructs a comprehensive prompt for entity detection using
    entity information dynamically loaded from tools.json configuration.
    
    Args:
        ctx: The callback context containing entity information
        
    Returns:
        str: The dynamically built prompt string
    """
    entity_info = ctx.state.get("entity_info", {})
    
    # Build entities summary
    entities_summary = ""
    entities_list = entity_info.get("entities_list", [])
    descriptions = entity_info.get("entity_descriptions", {})
    
    for entity in entities_list:
        desc = descriptions.get(entity, "")
        entities_summary += f"- **{entity}**: {desc}\n"
    
    # Build synonyms summary  
    synonyms_summary = ""
    synonyms = entity_info.get("entity_synonyms", {})
    
    for entity, entity_synonyms in synonyms.items():
        if entity_synonyms:
            synonyms_summary += f"- **{entity}**: {', '.join(entity_synonyms)}\n"
    
    # Dynamic prompt template
    prompt = f"""**ADVANCED ENTITY & INTENT DETECTION AGENT**

You are an intelligent entity and intent detection agent that analyzes user requests to extract:
1. **Target Entity**: What data type the user wants to work with
2. **Intent**: What action they want to perform  
3. **Parameters**: Specific values mentioned
4. **Confidence**: How confident you are in the detection

**Available Entities:**
{entities_summary}

**Entity Synonyms:**
{synonyms_summary}

**Intent Categories:**
- **search/find/get/list**: Retrieve or find data
- **create/add/new**: Create new records
- **update/modify/edit**: Update existing records  
- **delete/remove**: Delete records

**Output Format:**
Return a JSON object with detected entities, intents, and extracted parameters.

Example:
{{
    "action_plan": [
        {{
            "entity": "Partner",
            "intent": "search", 
            "confidence": 0.9,
            "extracted_params": {{
                "location": "Bangladesh"
            }}
        }}
    ]
}}"""
    
    return prompt


def log_entity_detection_start():
    """Log when entity detection starts"""
    print("🔍 [ENTITY] Starting entity detection...")

def log_entity_detection_complete(entities: list):
    """Log when entity detection completes"""
    if entities:
        entity_names = [e.get('entity', 'Unknown') for e in entities]
        print(f"✅ [ENTITY] Detected entities: {', '.join(entity_names)}")
    else:
        print("⚠️ [ENTITY] No entities detected")