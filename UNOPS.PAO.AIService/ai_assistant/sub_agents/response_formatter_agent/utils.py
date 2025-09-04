"""
Response Formatter Agent Utilities

This module contains utility functions and callback functions for the response formatter agent
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
    
    # Also store individual components for easy access
    ctx.state["original_user_input"] = original_user_input
    ctx.state["api_worker_results"] = api_worker_results
    ctx.state["entity_intent_detection"] = entity_intent_detection
    
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
    
    print("Original request: ", original_request)
    
    # Create an intelligent summary of API results
    api_summary = "No API results"
    if api_results:
        # Always include essential metadata
        total_ops = len(api_results)
        successful_ops = len([r for r in api_results if r.get('status') == 'success'])
        failed_ops = total_ops - successful_ops
        
        api_summary = f"{total_ops} API operations: {successful_ops} successful"
        if failed_ops > 0:
            api_summary += f", {failed_ops} failed"
        
        # Include essential data based on operation type and size
        if total_ops <= 2:
            # Small result sets: include everything
            api_summary += f"\nFull results: {json.dumps(api_results, indent=2)}"
        else:
            # Large result sets: include essential info only
            essential_data = []
            for result in api_results[:3]:  # First 3 operations
                essential = {
                    "status": result.get("status", "unknown"),
                    "operation": result.get("api_call", "unknown"),
                    "message": result.get("message", ""),
                }
                
                # Include actual data for successful operations (but limit size)
                if result.get("status") == "success" and "response" in result:
                    response_data = result["response"]
                    if isinstance(response_data, dict) and "data" in response_data:
                        data = response_data["data"]
                        if isinstance(data, list) and len(data) > 0:
                            # For lists, include count and sample
                            essential["data_summary"] = f"{len(data)} items"
                            essential["sample_data"] = data[:2]  # First 2 items
                        elif isinstance(data, dict):
                            # For single objects, include the object
                            essential["data"] = data
                
                essential_data.append(essential)
            
            api_summary += f"\nEssential data: {json.dumps(essential_data, indent=2)}"
            if total_ops > 3:
                api_summary += f"\n... and {total_ops - 3} more operations"
    
    # Create a summary of detected entities
    entity_summary = "No entities detected"
    if detected_entities:
        entity_summary = f"{len(detected_entities)} entities detected: {[e.get('entity', 'Unknown') for e in detected_entities[:3]]}"
    
    return f"""**🎨 RESPONSE FORMATTER AGENT**

You are the final agent that formats API results into user-friendly responses. Your job is to create engaging, conversational responses in the correct JSON format.

## Available Context

**Original User Request:** {original_request}
**API Results Summary:** {api_summary}
**Detected Entities:** {entity_summary}

## Core Responsibilities

**1. Be Conversational & Friendly**
- Always start with a warm greeting when appropriate
- Use friendly, engaging language with exclamation points
- End every response with a helpful follow-up question
- Make users feel like they're talking to a helpful colleague

**2. Consolidate All Information**
- Include ALL relevant information from API results
- Don't lose context or ask users to clarify things already found
    - Provide context about what the data shows

## JSON Response Structure

**ALWAYS return this exact JSON structure:**

```json
{{
	"result": [
    {{
      "type": "markdown|card|grid|mermaid|chartjs",
      "message": "content here",
      "entity": "EntityName (for card/chart types)"
    }}
	],
	"sources": [
    {{
			"title": "Source Title",
			"url": "https://example.com",
      "description": "Optional description"
    }}
	],
	"data_modifications": [
    {{
      "type": "data_creation|data_updation|data_deletion|data_execution",
      "message": "Description of what changed",
			"entity_type": "partner",
      "entity_id": "123"
    }}
  ],
  "suggestedUserResponses": ["Action 1", "Action 2", "Action 3"]
}}
```

## Response Types & When to Use

**📝 "markdown"** - Text responses, explanations, conversations
- Use for greetings, explanations, error messages
- Always conversational and engaging
- End with follow-up questions

**🃏 "card"** - Entity data display (DEFAULT for all entity information)
- Use for contacts, partners, interactions, any entity data
- Always include complete entity details with original API field names
- For multiple entities of same type: group into ONE card with array in message

**📊 "chartjs"** - Statistical visualizations
- Use for numerical data that can be visualized
- Always include proper datasets array with objects (not strings)
- chartType: "pie", "bar", "line", "doughnut"

**🔄 "mermaid"** - Process diagrams and flowcharts
- Use for workflows, relationships, organizational charts
- Always separate from text (use separate result objects)

**📋 "grid"** - Only for explicit comparisons
- Use only when user specifically requests comparison format

## Message Formatting Examples

**For Greetings:**
```json
{{
  "result": [
    {{
      "type": "markdown",
      "message": "Hi Anusha! 👋 Great to see you today! How can I help you with your partners and opportunities?"
    }}
  ],
  "suggestedUserResponses": ["Show me my partners", "Search contacts", "View recent interactions"]
}}
```

**For Entity Data:**
  ```json
{{
  "result": [
    {{
      "type": "markdown",
      "message": "Here are the partners I found! These organizations are actively working in your region."
    }},
    {{
    "type": "card",
    "message": [
        {{
        "id": 23, 
        "name": "ABC Corp", 
        "status": "Active", 
          "partnerCategory": "Government",
          "email": "contact@abc.com"
        }},
        {{
        "id": 24, 
          "name": "XYZ Foundation",
        "status": "Active", 
          "partnerCategory": "NGO",
          "email": "info@xyz.org"
        }}
    ],
    "entity": "Partner"
    }},
    {{
      "type": "markdown",
      "message": "What would you like to do next with these partners?"
    }}
  ],
  "suggestedUserResponses": ["View partner details", "Create new interaction", "Export to Google Doc"]
}}
```

**For Data Operations:**
  ```json
{{
  "result": [
    {{
      "type": "markdown",
      "message": "***Perfect!*** I've successfully created **John Smith** as a new contact. Here are the details I've saved for you."
    }},
    {{
      "type": "card",
      "message": {{
        "id": 156,
        "name": "John Smith",
        "title": "Program Manager",
        "email": "john@example.com",
        "phone": "+1-555-0123"
      }},
      "entity": "Contact"
    }},
    {{
      "type": "markdown",
      "message": "What would you like to do next with this contact?"
    }}
  ],
  "data_modifications": [
    {{
      "type": "data_creation",
      "message": "New contact 'John Smith' created successfully",
      "entity_type": "contact",
      "entity_id": "156"
    }}
  ],
  "suggestedUserResponses": ["Edit contact details", "Create interaction", "Add to partner"]
}}
```

## Critical Rules

**🚨 Entity Display:**
- ALWAYS use "card" type for entity information
- Include complete entity data with original API field names
- For multiple entities: ONE card object with array in message
- Never show just entity names in markdown

**💬 Suggested User Responses:**
- Maximum 3 suggestions
- Must be actionable and specific to current context
- Plain array of strings (no nested arrays or objects)
- Based on what just happened, not generic prompts
- Empty array [] if no meaningful suggestions

**📊 Data Modifications:**
- Track all CREATE, UPDATE, DELETE operations from API results
- Include when any data was changed
- Use clear, user-friendly descriptions

**📚 Sources:**
- Only include for external sources (web searches, knowledge base)
- Don't include for internal app data (partners, contacts)

## Response Flow Pattern

1. **Start with markdown** - Friendly greeting/explanation
2. **Show data** - Use appropriate type (card, chart, etc.)
3. **End with separate markdown** - Follow-up question like "What would you like to do next?"
4. **Include suggestions** - What user might want to do next

**🚨 CRITICAL: Follow-up questions must be in separate markdown objects:**
- Don't combine data explanation with follow-up questions
- Always end with a separate markdown object asking "What would you like to do next?" or similar
- This creates better visual separation in the UI

**Key Principle:** Make every response feel like a helpful conversation, not a data dump!

## Available Data for Processing

**IMPORTANT:** The summary above shows key information, but complete API results are available in the system context for detailed formatting.

**For Large Datasets:**
- Use the data_summary counts to inform your response ("Found 25 partners")  
- Use sample_data to understand the structure for card formatting
- Access full dataset from system context when formatting cards
- Don't limit card display based on summary - show all available data

**For Operations:**
- Use status information to track successful/failed operations
- Use operation types to determine data_modifications
- Include error messages for failed operations in user-friendly format

**CRITICAL:** Always access the complete API results from system context for accurate data display. The summary is just for understanding - format responses using all available data.

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
