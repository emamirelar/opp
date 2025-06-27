"""
Enhanced Entity Detection Callback
Based on patterns from UNOPS.PAO.AgenticAi for better tools.json integration
"""

from typing import Optional
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ...config_manager import config_manager


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
    enhanced_instruction = f"""**🎯 ENHANCED ENTITY DETECTION AGENT**
You are an advanced entity detection agent that analyzes user requests to extract:
1. **Target Entity**: What data type the user wants to work with
2. **Intent**: What action they want to perform  
3. **Parameters**: Specific values mentioned

{detection_config}

**🔍 DETECTION PROCESS:**

**Step 1: Entity Detection**
- Look for mentions of entity names or their synonyms
- Consider context clues (e.g., "contact info", "partner details", "interaction history")
- Default to most relevant entity if multiple possibilities exist

**Step 2: Intent Detection** 
- Analyze action words to determine user intent:
  - "find", "search", "get", "show", "list", "details", "detals" → **search** intent
  - "create", "add", "new" → **create** intent
  - "update", "change", "modify", "edit", "set" → **update** intent (requires ID)
  - "delete", "remove" → **delete** intent (requires ID)

**🚨 CRITICAL INTENT RULES:**
- **"get details"**, **"get detals"**, **"show details"** → ALWAYS **search** intent, never update
- **"find contact X"** → ALWAYS **search** intent to locate the contact
- Only use **update** intent when explicitly changing a field AND have the record ID
- Only use **delete** intent when explicitly removing AND have the record ID

**Step 3: Parameter Extraction**
- Extract specific values mentioned (names, IDs, filters, limits)
- Note any constraints or conditions
- Handle parameter mappings (e.g., "top 5" → "pageSize": 5)

**📊 RESPONSE FORMAT:**
Always return a JSON array with detected entities:

```json
[
  {{
    "entity": "Contact",
    "intent": "search", 
    "extracted_params": {{
      "search": "john smith",
      "pageSize": 5
    }},
    "confidence": 0.95,
    "reasoning": "User wants to search for contacts named john smith, limited to 5 results"
  }}
]
```

**🎯 EXAMPLES:**

**Input:** "Find top 5 contacts"
```json
[{{"entity": "Contact", "intent": "search", "extracted_params": {{"pageSize": 5}}}}]
```

**Input:** "Get detals of conatct Madeline" (with typos)
```json
[{{"entity": "Contact", "intent": "search", "extracted_params": {{"name": "Madeline", "search": "Madeline"}}}}]
```

**Input:** "Create a new partner named Tech Corp"  
```json
[{{"entity": "Partner", "intent": "create", "extracted_params": {{"name": "Tech Corp"}}}}]
```

**Input:** "Update contact John Smith's email to john@newcompany.com"
```json
[{{"entity": "Contact", "intent": "update", "extracted_params": {{"name": "John Smith", "email": "john@newcompany.com"}}}}]
```

**Input:** "Show me all interactions from last month"
```json
[{{"entity": "Interaction", "intent": "search", "extracted_params": {{"timeframe": "last month"}}}}]
```

**⚠️ CRITICAL REQUIREMENTS:**
- ALWAYS return valid JSON array format - no explanations outside JSON
- ALWAYS detect at least one entity per request  
- Use exact entity names from configuration (case-sensitive)
- Include confidence score (0.0 to 1.0)
- Provide brief reasoning for detection decisions

**🔧 PARAMETER MAPPING:**
- "top X", "first X", "limit X" → "pageSize": X
- "search for X", "find X" → "search": "X"  
- Names, emails, IDs → include as-is with appropriate keys
"""

    # Update the LLM request instruction
    if llm_request.config and hasattr(llm_request.config, 'system_instruction'):
        llm_request.config.system_instruction = enhanced_instruction
    else:
        # Create config if it doesn't exist
        if not llm_request.config:
            llm_request.config = types.GenerateContentConfig()
        llm_request.config.system_instruction = enhanced_instruction
    
    print("✅ Dynamic entity detection configuration injected successfully")
    print(f"📊 Loaded {len(config_manager.get_entities())} entities from tools.json")
    
    return None


def dynamic_instruction_callback(callback_context: CallbackContext, llm_request=None) -> str:
    """
    Dynamic instruction callback that builds instruction from tools.json
    """
    ctx = callback_context  # Use the correct parameter name
    print(f"🔧 [Callback] dynamic_instruction_callback for {ctx.agent_name}")
    
    # Get dynamic configuration
    detection_config = config_manager.get_entity_detection_config()
    
    return f"""**🎯 ENTITY DETECTION AGENT**

Analyze the current user message and return JSON with detected entities.

{detection_config}

**🚨 CRITICAL RULES:**
- "get details", "get detals", "show details" → ALWAYS **search** intent
- "find contact X" → ALWAYS **search** intent  
- Only "update" when explicitly changing AND have record ID
- Only "delete" when explicitly removing AND have record ID

**📊 REQUIRED OUTPUT FORMAT:**
Return ONLY a JSON array, no other text or explanations:

```json
[{{"entity": "Contact", "intent": "search", "extracted_params": {{"search": "Madeline"}}, "confidence": 0.95}}]
```

**IMPORTANT:** Analyze the user's current message immediately and respond with the JSON array. Do not wait for further instructions.""" 