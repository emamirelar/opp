"""
Response Formatter Callback

This module contains callback functions for the response formatter agent
that processes API worker results and formats them into user-friendly responses.
"""

from typing import Optional, Dict, Any
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
import json


def format_response_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Callback function that prepares API response data for formatting into
    user-friendly natural language responses.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request object (optional)
    """
    ctx = callback_context
    print(f"🎨 [Callback] format_response_before_model triggered for {ctx.agent_name}")
    
    # Only apply to response formatter agent
    if ctx.agent_name != "response_formatter_agent":
        print(f"ℹ️ Skipping callback - not response formatter agent (current: {ctx.agent_name})")
        return
    
    print("🎨 Processing API responses for user-friendly formatting...")
    
    # Use the llm_request parameter if provided, otherwise try to get it from context
    if not llm_request:
        llm_request = getattr(ctx, 'llm_request', None)
    
    if not llm_request:
        print("⚠️ No LLM request found in context")
        return
    
    # Get API worker results and original user request
    api_worker_results = ctx.state.get("api_worker_results", [])
    original_user_input = ctx.state.get("original_user_input", "")
    entity_intent_detection = ctx.state.get("entity_intent_detection", [])
    
    # Prepare context data for response formatting
    response_context = {
        "original_request": original_user_input,
        "detected_entities": entity_intent_detection,
        "api_results": api_worker_results,
        "total_operations": len(api_worker_results) if api_worker_results else 0
    }
    
    # Store in state for prompt to access
    ctx.state["response_context"] = response_context
    
    print(f"✅ Response context prepared - {len(api_worker_results)} API results to format")
    
    return None


def dynamic_response_instruction(callback_context: CallbackContext, llm_request=None) -> str:
    """
    Dynamic instruction callback that builds response formatting instructions
    for structured JSON output based on the API results and user context.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request object (optional)
        
    Returns:
        str: The dynamic instruction for structured response formatting
    """
    ctx = callback_context
    print(f"🎨 [Callback] dynamic_response_instruction for {ctx.agent_name}")
    
    # Get response context
    response_context = ctx.state.get("response_context", {})
    original_request = response_context.get("original_request", "")
    api_results = response_context.get("api_results", [])
    detected_entities = response_context.get("detected_entities", [])
    
    # Build context information for the prompt
    context_info = f"""
**ORIGINAL USER REQUEST:**
"{original_request}"

**DETECTED ENTITIES AND INTENTS:**
{json.dumps(detected_entities, indent=2) if detected_entities else "No entities detected"}

**API OPERATION RESULTS:**
{json.dumps(api_results, indent=2) if api_results else "No API results available"}
"""
    
    return f"""**🎨 STRUCTURED RESPONSE FORMATTER AGENT**

You must convert API results into a structured JSON response format for frontend rendering.

**CONTEXT INFORMATION:**
{context_info}

**🎯 OUTPUT FORMAT - RETURN EXACTLY THIS STRUCTURE:**

```json
{{
  "prefixMessage": "Brief intro message to show before the result",
  "result": "The actual data (object, array, or string)",
  "type": "Display type: grid | markdown | json | mermaid | card (default: markdown)",
  "postfixMessage": "Optional message to show after the result", 
  "gems": ["Take this action", "Try another option", "Get help with this"]
}}
```

**📊 TYPE SELECTION RULES:**

**Use "grid" when:**
- Multiple contacts/partners/interactions in search results
- List operations with tabular data
- Any array of objects that should be displayed in a table

**Use "card" when:**
- Single contact/partner/interaction details  
- Individual record information
- One specific entity result

**Use "markdown" when:**
- Text explanations or descriptions
- Error messages
- General information that needs formatting

**Use "json" when:**
- Raw data display is preferred
- Complex nested structures
- Debug or technical information

**🎯 RESPONSE EXAMPLES:**

**Multiple Contacts Search:**
```json
{{
  "prefixMessage": "Here are the top 5 contacts in your system",
  "result": [
    {{
      "id": 123,
      "firstName": "John",
      "lastName": "Smith", 
      "title": "Program Manager",
      "email": "john.smith@unicef.org",
      "phone": "+1-555-0123",
      "organization": "UNICEF"
    }},
    {{
      "id": 124,
      "firstName": "Jane", 
      "lastName": "Doe",
      "title": "Director",
      "email": "jane.doe@who.int", 
      "phone": "+1-555-0456",
      "organization": "WHO"
    }}
  ],
  "type": "grid",
  "postfixMessage": "Do you need any more details about these contacts?",
  "gems": ["Show these contact details", "Export this to Google Sheets", "Summarize about these contacts"]
}}
```

**Single Contact Details:**
```json
{{
  "prefixMessage": "Here are the details for Madeline",
  "result": {{
    "id": 125,
    "firstName": "Madeline",
    "lastName": "Johnson", 
    "title": "Technical Advisor",
    "email": "madeline.johnson@undp.org",
    "phone": "+1-555-0789",
    "organization": "UNDP",
    "department": "Technology",
    "location": "New York"
  }},
  "type": "card", 
  "postfixMessage": "Would you like to update any of this information?",
  "gems": ["Edit this contact", "Summarize about this contact", "Export this to Google Docs"]
}}
```

**Create Success (No Redundancy):**
```json
{{
  "prefixMessage": "Contact created successfully",
  "result": {{
    "id": 126,
    "firstName": "John",
    "lastName": "Smith",
    "title": "Program Manager", 
    "email": "john.smith@unicef.org",
    "organization": "UNICEF"
  }},
  "type": "card",
  "postfixMessage": null,
  "gems": ["Edit these contact details", "Summarize about this contact", "Export this to Google Sheets"]
}}
```

**Update Success (No Redundancy):**
```json
{{
  "prefixMessage": null,
  "result": "John Smith's title has been changed to 'Senior Program Manager'",
  "type": "markdown",
  "postfixMessage": null,
  "gems": ["View this contact's details", "Summarize about this contact", "Export this to Google Docs"]
}}
```

**Error Response (No Redundancy - Option 1):**
```json
{{
  "prefixMessage": null,
  "result": "You don't have permission to create contacts. Your role allows read-only access to contact information.",
  "type": "markdown", 
  "postfixMessage": null,
  "gems": ["Search contacts", "View my permissions", "Contact support (larsj@unops.org)"]
}}
```

**Error Response (No Redundancy - Option 2):**
```json
{{
  "prefixMessage": "Permission denied",
  "result": "Your role allows read-only access to contact information.",
  "type": "markdown", 
  "postfixMessage": null,
  "gems": ["Search contacts", "View my permissions", "Contact support (larsj@unops.org)"]
}}
```

**API Error (500 Error Example):**
```json
{{
  "prefixMessage": null,
  "result": "The API returned a 500 error. Please try again later.",
  "type": "markdown",
  "postfixMessage": null,
  "gems": ["Try this again", "Contact support (larsj@unops.org)", "Check server status"]
}}
```

**Missing Information:**
```json
{{
  "prefixMessage": "Missing required information",
  "result": {{
    "missingFields": ["firstName", "lastName", "email", "title"],
    "message": "Please provide the following required fields to create the contact"
  }},
  "type": "json",
  "postfixMessage": null,
  "gems": ["Try this again with complete info", "View requirements", "Cancel this action"]
}}
```

**🚨 CRITICAL RULES:**

1. **ALWAYS return valid JSON** - No explanatory text outside the JSON structure
2. **Choose appropriate type** based on the data and context
3. **Include meaningful gems** - 1-3 relevant next actions for the user
4. **AVOID REDUNDANCY** - Don't repeat the same information in prefixMessage, result, and postfixMessage
5. **Handle all scenarios** - Success, errors, empty results, permissions
6. **Preserve data structure** - Don't lose important information from API results
7. **Be user-focused** - Think about what the user wants to do next

**🚨 REDUNDANCY PREVENTION RULES:**

**For Errors:**
- **Option 1**: Use `result` for the main error message, set `prefixMessage` and `postfixMessage` to `null`
- **Option 2**: Use `prefixMessage` for brief context, `result` for details, `postfixMessage` to `null`
- **NEVER repeat the same error message across multiple fields**

**For Success:**
- Use `prefixMessage` for brief success context
- Use `result` for the actual data or detailed success message
- Use `postfixMessage` for additional guidance only if it adds new value
- **Don't repeat the same success information**

**🎯 GEM GENERATION GUIDELINES:**

**For Search Results:**
- "View these details", "Export this data", "Export this to Google Sheets", "Export this to Google Docs", "Summarize about these [items]", "Refine this search", "Create new [entity]"

**For Single Items:**  
- "Edit this [entity]", "Delete this [entity]", "View related data", "Summarize about this [entity]", "Export this to Google Sheets", "Export this to Google Docs", "Create similar item"

**For Create Operations:**
- "View this created item", "Create another", "Edit these details", "Summarize about this item", "Export this to Google Sheets", "Share this with team"

**For Update Operations:**
- "View this updated item", "Make more changes", "Undo this change", "View this item's history"

**For Errors:**
- "Try this again", "Contact support (larsj@unops.org)", "View help about this", "Search instead"

**ANALYZE THE CONTEXT ABOVE AND GENERATE THE APPROPRIATE STRUCTURED JSON RESPONSE NOW.**"""


def extract_key_data(api_result: Dict[str, Any]) -> Dict[str, Any]:
    """
    Extract key information from API results for response formatting.
    
    Args:
        api_result: Single API result object
        
    Returns:
        Dict containing extracted key data
    """
    if not api_result or not isinstance(api_result, dict):
        return {}
    
    # Extract response data
    response_data = api_result.get("response", {})
    if isinstance(response_data, dict) and "data" in response_data:
        data = response_data["data"]
    else:
        data = response_data
    
    # Handle different data types
    extracted = {
        "status": api_result.get("status", "unknown"),
        "api_call": api_result.get("api_call", ""),
        "message": api_result.get("message", ""),
        "data": data
    }
    
    return extracted


def format_contact_data(contact: Dict[str, Any]) -> str:
    """
    Format a single contact into a user-friendly string.
    
    Args:
        contact: Contact data dictionary
        
    Returns:
        str: Formatted contact information
    """
    if not contact:
        return ""
    
    name_parts = []
    if contact.get("firstName"):
        name_parts.append(contact["firstName"])
    if contact.get("lastName"):
        name_parts.append(contact["lastName"])
    
    name = " ".join(name_parts) if name_parts else contact.get("name", "Unknown")
    title = contact.get("title", "")
    organization = contact.get("organizationName", "")
    email = contact.get("email", "")
    phone = contact.get("phone", "")
    
    # Build formatted string
    result = f"**{name}**"
    
    if title and organization:
        result += f" - {title} at {organization}"
    elif title:
        result += f" - {title}"
    elif organization:
        result += f" - {organization}"
    
    details = []
    if email:
        details.append(f"📧 {email}")
    if phone:
        details.append(f"📱 {phone}")
    
    if details:
        result += f"\n  {' | '.join(details)}"
    
    return result


def format_partner_data(partner: Dict[str, Any]) -> str:
    """
    Format a single partner into a user-friendly string.
    
    Args:
        partner: Partner data dictionary
        
    Returns:
        str: Formatted partner information
    """
    if not partner:
        return ""
    
    name = partner.get("name", partner.get("organizationName", "Unknown"))
    partner_type = partner.get("partnerType", "")
    website = partner.get("website", "")
    description = partner.get("description", "")
    
    result = f"**{name}**"
    
    if partner_type:
        result += f" ({partner_type})"
    
    details = []
    if website:
        details.append(f"🌐 {website}")
    if description:
        details.append(f"📝 {description}")
    
    if details:
        result += f"\n  {' | '.join(details)}"
    
    return result 