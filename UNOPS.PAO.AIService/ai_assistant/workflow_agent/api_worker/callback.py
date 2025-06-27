"""
Callback Functions for API Worker Agent

This module contains callback functions used by the API worker agent
to process detected entities and prepare API calls based on tools.json configuration.
"""

import json
import os
import re
from typing import Optional
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ...config_manager import config_manager, API_BASE_URL


def prepare_api_worker_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Prepare API worker before model execution by loading entity detection results
    and injecting entity-specific configuration from tools.json.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request object (optional)
    """
    print("🚨 DEBUG: prepare_api_worker_before_model called!")
    print(f"🚨 DEBUG: llm_request is {'None' if llm_request is None else 'available'}")
    print(f"🚨 DEBUG: llm_request.config is {'None' if llm_request is None or llm_request.config is None else 'available'}")
    
    ctx = callback_context
    
    # Check if this is the first time running (initialization)
    api_worker_state = ctx.state.get("api_worker_state")
    
    if api_worker_state is None:
        # First time - initialize the API worker
        print("🔧 API Worker: Initializing for API operations...")
        
        # Get entity detection results from previous agent
        entity_detection_result = ctx.state.get("entity_intent_detection", None)
        print(f"🔍 DEBUG: Entity detection result: {entity_detection_result}")
        
        # Convert single entity object to array for processing
        if entity_detection_result:
            if isinstance(entity_detection_result, dict):
                # Single entity object - convert to array
                entity_detections = [entity_detection_result]
                print(f"📊 Found single entity detection: {entity_detection_result.get('entity', 'Unknown')}")
                print(f"🔍 DEBUG: Entity '{entity_detection_result.get('entity')}' with intent '{entity_detection_result.get('intent')}'")
            elif isinstance(entity_detection_result, list):
                # Already an array - use as is
                entity_detections = entity_detection_result
                print(f"📊 Found {len(entity_detections)} detected entities to process")
            else:
                # Invalid format
                entity_detections = []
                print(f"⚠️ WARNING: Invalid entity detection format: {type(entity_detection_result)}")
        else:
            entity_detections = []
            print("⚠️ WARNING: No entity detections found!")
            print(f"🔍 DEBUG: Available state keys: {list(ctx.state.keys())}")
        
        # Load API base URL from environment or config
        api_base_url = os.getenv('API_BASE_URL', 'https://localhost:44426')
        
        # Load entities configuration from tools.json
        entities_config = config_manager.get_entities()
        
        # Handle empty entity detections
        if not entity_detections:
            print("❌ CRITICAL ERROR: No entity detections found!")
            api_worker_state = {
                "detected_entities": [],
                "current_entity_index": 0,
                "api_base_url": api_base_url,
                "entities_config": entities_config,
                "processed_results": [],
                "total_entities": 0,
                "processing_complete": True,
                "iteration_count": 0,
                "error": "No entity detections found from previous agent"
            }
            ctx.state["api_worker_state"] = api_worker_state
            ctx.state["current_entity"] = None
            return
        
        # Prepare API worker state
        api_worker_state = {
            "detected_entities": entity_detections,
            "current_entity_index": 0,
            "api_base_url": api_base_url,
            "entities_config": entities_config,
            "processed_results": [],
            "total_entities": len(entity_detections),
            "processing_complete": False,
            "iteration_count": 0
        }
        
        # Store in context state
        ctx.state["api_worker_state"] = api_worker_state
        ctx.state["current_entity"] = entity_detections[0] if entity_detections else None
        ctx.state["entity_intent_detection"] = entity_detection_result
        
        print(f"✅ API Worker: Initialized to process {len(entity_detections)} entities")
        print(f"🌐 API Base URL: {api_base_url}")
    else:
        # Subsequent iterations - check if we should advance or exit
        api_worker_state["iteration_count"] = api_worker_state.get("iteration_count", 0) + 1
        current_index = api_worker_state.get("current_entity_index", 0)
        detected_entities = api_worker_state.get("detected_entities", [])
        
        print(f"🔄 API Worker: Iteration {api_worker_state['iteration_count']} - Current index: {current_index}/{len(detected_entities)}")
        
        # Check if we've processed all entities
        if current_index >= len(detected_entities):
            print("✅ All entities have been processed - setting current_entity to None")
            ctx.state["current_entity"] = None
            api_worker_state["processing_complete"] = True
            return
        
        # Set current entity for this iteration (no advancement yet - will advance after processing)
        current_entity = detected_entities[current_index]
        ctx.state["current_entity"] = current_entity
        print(f"📋 Processing entity {current_index + 1}/{len(detected_entities)}: {current_entity.get('entity', 'Unknown')} - {current_entity.get('intent', 'Unknown')}")
        
        # Advance the index for next iteration (this entity is about to be processed)
        api_worker_state["current_entity_index"] = current_index + 1
    
    # **NEW: Inject entity-specific configuration into the LLM request**
    if llm_request and llm_request.config:
        current_entity = ctx.state.get("current_entity")
        print(f"🔧 About to inject configuration for: {current_entity.get('entity', 'Unknown') if current_entity else 'None'}")
        if current_entity:
            inject_entity_configuration(llm_request, current_entity, api_worker_state)
        else:
            print("⚠️ No current_entity found - configuration injection skipped!")
    else:
        print("⚠️ llm_request not available - configuration injection skipped!")


def inject_entity_configuration(llm_request, current_entity: dict, api_worker_state: dict):
    """
    Inject entity-specific configuration from tools.json into the LLM request
    
    Args:
        llm_request: The LLM request object
        current_entity: Current entity being processed
        api_worker_state: API worker state containing tools configuration
    """
    try:
        entity_name = current_entity.get("entity", "").lower()
        intent = current_entity.get("intent", "").lower()
        extracted_params = current_entity.get("extracted_params", {})
        api_base_url = api_worker_state.get("api_base_url", "")
        entities_config = api_worker_state.get("entities_config", [])
        
        print(f"🔧 Injecting configuration for entity: {entity_name}")
        print(f"🔍 Available entities: {[e.get('entity', 'Unknown') for e in entities_config]}")
        print(f"🌐 Using API base URL: {api_base_url}")
        
        # Find the matching entity configuration in tools.json
        entity_config = None
        for entity in entities_config:
            # Check if entity name matches or is in synonyms
            entity_names = [entity['entity'].lower()] + [syn.lower() for syn in entity.get('synonyms', [])]
            if entity_name in entity_names:
                entity_config = entity
                print(f"✅ Found matching entity configuration: {entity['entity']}")
                break
        
        if not entity_config:
            print(f"⚠️ No entity configuration found for: {entity_name}")
            return
        
        # Format entity configuration for injection
        entity_config_text = f"""
**Entity:** {entity_config['entity']}
**Description:** {entity_config['description']}
**Synonyms:** {', '.join(entity_config.get('synonyms', []))}
**Mandatory Fields:** {', '.join(entity_config.get('mandatoryFields', []))}

**Available Endpoints:**
"""
        
        # Add each endpoint with full details
        from .utilities import construct_api_url
        for endpoint in entity_config.get('endpoints', []):
            endpoint_full_url = construct_api_url(api_base_url, endpoint['url'])
            entity_config_text += f"""
---
**{endpoint['name']}**
- **URL:** {endpoint_full_url}
- **Method:** {endpoint['method']}
- **Description:** {endpoint['description']}
- **When to use:** {endpoint.get('when_to_use', 'Not specified')}

**Parameters:**
"""
            for param_name, param_info in endpoint.get('parameters', {}).items():
                required_text = "REQUIRED" if param_info.get('required', False) else "Optional"
                param_description = param_info.get('description', 'No description')
                
                # Add possible values if they exist
                possible_values = param_info.get('possible_values', [])
                if possible_values:
                    possible_values_text = ", ".join([f'"{val}"' for val in possible_values])
                    param_description += f" | **Possible Values:** {possible_values_text} - Choose the closest matching value"
                
                entity_config_text += f"  - **{param_name}** ({param_info.get('type', 'unknown')}, {required_text}): {param_description}\n"
            
            # Add example uses
            if endpoint.get('example_uses'):
                entity_config_text += f"\n**Example uses:**\n"
                for example in endpoint['example_uses']:
                    entity_config_text += f"  - \"{example}\"\n"
        
        # Create dynamic context information
        context_info = f"""
**CURRENT OPERATION CONTEXT:**

**Detected Entity:** {entity_name}
**Intent:** {intent}  
**Extracted Parameters:** {json.dumps(extracted_params, indent=2)}

**🚨 MANDATORY TOOL USAGE:**
- **ONLY USE THE TOOLS PROVIDED TO YOU**: invoke_api_tool, exit_loop_on_success, advance_entity_index
- **DO NOT USE ANY GOOGLE APIS OR EXTERNAL SEARCH TOOLS**
- **DO NOT USE customsearch, googleapis.com, or any other external APIs**
- **ONLY CALL ENDPOINTS FROM THE CONFIGURATION ABOVE**

**MANDATORY WORKFLOW:**
1. **MAKE API CALL** using invoke_api_tool with appropriate endpoint from above
2. **EXIT LOOP** immediately after successful API call using exit_loop_on_success()
3. **RETURN RESULT** with grade="pass" and detailed response

**🚨 YOU MUST CALL invoke_api_tool() - THIS IS REQUIRED**
**🚨 YOU MUST CALL exit_loop_on_success() AFTER SUCCESSFUL API CALLS**
**🚨 DO NOT USE ANY OTHER TOOLS OR APIS - ONLY USE THE PROVIDED TOOLS**

**CRITICAL:** The agent MUST make actual API calls using invoke_api_tool. Do not use Google Search, Custom Search, or any external APIs.

**FOR THE CURRENT REQUEST:**
- Entity: {entity_name}
- Intent: {intent}
- Choose the appropriate endpoint from the configuration above
- Use invoke_api_tool with the full URL from the endpoint configuration
"""
        
        # Create a comprehensive system instruction directly injecting all tools.json data
        system_instruction = f"""
**🚨 API WORKER AGENT - DYNAMIC TOOLS CONFIGURATION INJECTED 🚨**

You are responsible for executing API calls based on detected entities and intents using the EXACT configuration from tools.json.

**🌐 API BASE URL:** {api_base_url}

**📋 CURRENT ENTITY TO PROCESS:**
- **Entity:** {entity_name}
- **Intent:** {intent} 
- **Parameters:** {json.dumps(extracted_params, indent=2)}

**🔧 ENTITY CONFIGURATION FROM TOOLS.JSON:**
{entity_config_text}

**🚨 MANDATORY INSTRUCTIONS:**
1. **ONLY USE THE ENDPOINTS LISTED ABOVE** - these are from your tools.json configuration
2. **USE THE EXACT URLs** - combine {api_base_url} + endpoint URL from above
3. **MATCH INTENT TO ENDPOINT** - use the "when_to_use" and "example_uses" guidance
4. **CALL invoke_api_tool()** - this is mandatory for every entity
5. **CALL exit_loop_on_success()** - this is mandatory after successful API calls

**🎯 WORKFLOW FOR CURRENT ENTITY:**
1. **Select Endpoint**: Choose appropriate endpoint from the configuration above based on intent "{intent}"
2. **Build URL**: Combine {api_base_url} + endpoint path
3. **Prepare Parameters**: Use extracted_params: {json.dumps(extracted_params)}
   - **IMPORTANT**: For parameters with **Possible Values**, choose the closest matching value from the list
   - **Match Logic**: Use semantic similarity to select the most appropriate value from possible_values
4. **Call API**: invoke_api_tool(url=full_url, method=endpoint_method, body=parameters)
5. **Exit Loop**: exit_loop_on_success() if API call succeeds

**🚨 CRITICAL RULES:**
- **DO NOT USE GOOGLE APIS** - only use the endpoints configured above
- **DO NOT USE example.com URLs** - use the real {api_base_url} URLs
- **DO NOT SKIP API CALLS** - every entity must result in an API call
- **MUST CALL exit_loop_on_success()** after successful API operations
- **FOR PARAMETERS WITH POSSIBLE VALUES**: Always choose from the provided list, never use custom values
- **VALUE MATCHING**: Use closest semantic match when selecting from possible_values

**🔍 DEBUGGING INFO:**
- Base URL: {api_base_url}
- Entity: {entity_name}
- Intent: {intent}
- Available endpoints: {len(entity_config.get('endpoints', []))}
- Configuration loaded from tools.json: ✅
"""
        
        # Set the complete system instruction
        llm_request.config.system_instruction = system_instruction
        
        print(f"✅ Injected entity-specific configuration for: {entity_config['entity']}")
        print(f"📋 Available endpoints: {[ep['name'] for ep in entity_config.get('endpoints', [])]}")
        
        # Use construct_api_url to avoid double slash issues
        from .utilities import construct_api_url
        sample_endpoint_url = entity_config.get('endpoints', [{}])[0].get('url', 'N/A')
        if sample_endpoint_url != 'N/A':
            sample_full_url = construct_api_url(api_base_url, sample_endpoint_url)
        else:
            sample_full_url = 'N/A'
        print(f"🔍 DEBUG - Sample endpoint URL: {sample_full_url}")
        print(f"🔍 DEBUG - System instruction starts with: {llm_request.config.system_instruction[:200]}...")
        
    except Exception as e:
        print(f"❌ Error injecting entity configuration: {e}")
        import traceback
        print(f"📋 Traceback: {traceback.format_exc()}")


def get_current_entity_for_processing(callback_context: CallbackContext) -> dict:
    """
    Get the current entity that needs to be processed
    
    Args:
        callback_context: The callback context
        
    Returns:
        dict: Current entity detection result or empty dict if none
    """
    api_worker_state = callback_context.state.get("api_worker_state", {})
    detected_entities = api_worker_state.get("detected_entities", [])
    current_index = api_worker_state.get("current_entity_index", 0)
    
    if current_index < len(detected_entities):
        return detected_entities[current_index]
    else:
        return {}


def find_appropriate_endpoint(entity_name: str, intent: str, entity_endpoints: dict) -> dict:
    """
    Find the appropriate API endpoint for the given entity and intent
    
    Args:
        entity_name: Name of the entity (e.g., "Partner", "Contact")
        intent: Detected intent (e.g., "search", "create", "update", "delete")
        entity_endpoints: Dictionary of entity endpoints from tools.json
        
    Returns:
        dict: Endpoint configuration or empty dict if not found
    """
    endpoints = entity_endpoints.get(entity_name, [])
    
    print(f"🔍 Finding endpoint for {entity_name} - {intent}")
    print(f"📋 Available endpoints: {[ep.get('name', 'Unknown') for ep in endpoints]}")
    
    # Map intents to HTTP methods and endpoint name patterns
    intent_mapping = {
        "search": {"methods": ["GET"], "patterns": ["Get", "List", "Search", "Contacts", "Partners", "Interactions"]},
        "create": {"methods": ["POST"], "patterns": ["Create", "Add", "New"]},
        "update": {"methods": ["PUT", "PATCH"], "patterns": ["Update", "Modify", "Edit"]},
        "delete": {"methods": ["DELETE"], "patterns": ["Delete", "Remove"]},
        "similarity_search": {"methods": ["GET"], "patterns": ["Similarity", "Similar"]}
    }
    
    intent_config = intent_mapping.get(intent, {"methods": ["GET"], "patterns": ["Get"]})
    target_methods = intent_config["methods"]
    name_patterns = intent_config["patterns"]
    
    print(f"🎯 Looking for methods: {target_methods}, patterns: {name_patterns}")
    
    # Find matching endpoint
    for endpoint in endpoints:
        endpoint_method = endpoint.get("method", "GET").upper()
        endpoint_name = endpoint.get("name", "")
        
        print(f"   Checking: {endpoint_name} ({endpoint_method})")
        
        # Check if method matches
        if endpoint_method in target_methods:
            # Check if name pattern matches
            if any(pattern.lower() in endpoint_name.lower() for pattern in name_patterns):
                print(f"✅ Found matching endpoint: {endpoint_name}")
                return endpoint
    
    # Fallback: find any endpoint with matching method
    print("🔄 Trying fallback - method match only")
    for endpoint in endpoints:
        endpoint_method = endpoint.get("method", "GET").upper()
        endpoint_name = endpoint.get("name", "")
        if endpoint_method in target_methods:
            print(f"✅ Found fallback endpoint: {endpoint_name}")
            return endpoint
    
    # Last resort: return first endpoint
    if endpoints:
        endpoint = endpoints[0]
        print(f"⚠️ Using first available endpoint: {endpoint.get('name', 'Unknown')}")
        return endpoint
    else:
        print(f"❌ No endpoints found for {entity_name}")
        return {}


def record_api_result(callback_context: CallbackContext, entity: dict, endpoint: dict, result: dict) -> None:
    """
    Record the result of an API operation
    
    Args:
        callback_context: The callback context
        entity: The entity that was processed
        endpoint: The endpoint that was called
        result: The API call result
    """
    api_worker_state = callback_context.state.get("api_worker_state", {})
    processed_results = api_worker_state.get("processed_results", [])
    
    result_record = {
        "entity": entity.get("entity", ""),
        "intent": entity.get("intent", ""),
        "endpoint_name": endpoint.get("name", ""),
        "api_call": result.get("api_call", ""),
        "status": result.get("status", "unknown"),
        "status_code": result.get("status_code"),
        "success": result.get("status") == "success",
        "error": result.get("error") if result.get("status") == "error" else None,
        "response_data": result.get("response")
    }
    
    processed_results.append(result_record)
    api_worker_state["processed_results"] = processed_results
    
    print(f"📊 Recorded API result: {result_record['entity']} - {result_record['status']}")


def get_processing_summary(callback_context: CallbackContext) -> dict:
    """
    Get a summary of all processed API operations
    
    Args:
        callback_context: The callback context
        
    Returns:
        dict: Summary of processing results
    """
    api_worker_state = callback_context.state.get("api_worker_state", {})
    processed_results = api_worker_state.get("processed_results", [])
    
    successful_calls = [r for r in processed_results if r.get("success")]
    failed_calls = [r for r in processed_results if not r.get("success")]
    
    summary = {
        "total_entities": len(processed_results),
        "successful_calls": len(successful_calls),
        "failed_calls": len(failed_calls),
        "success_rate": len(successful_calls) / len(processed_results) if processed_results else 0,
        "results": processed_results
    }
    
    return summary


def inject_entity_specific_tools_before_model(
    callback_context: CallbackContext, 
    llm_request
) -> Optional[types.Content]:
    """
    Before model callback that injects entity-specific API tools information.
    This dynamically loads only the tools relevant to the current entity being processed.
    
    Based on UNOPS.PAO.AgenticAi implementation pattern.
    """
    
    # Get the agent name to check if this is the api_caller_agent
    agent_name = callback_context.agent_name
    
    if agent_name != "api_caller_agent":
        print(f"ℹ️ Skipping entity-specific tools injection - not api_caller_agent (current: {agent_name})")
        return None  # Only apply to api_caller_agent
    
    print(f"🔧 [Callback] Injecting entity-specific API tools for {agent_name}")
    
    # Try to extract entity from context - look for previous agent output
    detected_entity = None
    detected_entities = []
    
    # Check if there's context from entity intent detection agent
    if hasattr(callback_context, 'context') and callback_context.context:
        context_data = callback_context.context
        print(f"🔍 Found context data: {type(context_data)}")
        
        # Look for entity information from previous agent
        if isinstance(context_data, dict):
            detected_entity = context_data.get('entity')
            if not detected_entity:
                # Try other possible keys
                detected_entity = context_data.get('detected_entity')
            if not detected_entity:
                # Try to find in nested structures
                for key, value in context_data.items():
                    if isinstance(value, dict) and 'entity' in value:
                        detected_entity = value['entity']
                        break
                    elif isinstance(value, list):
                        # Handle array of detected entities
                        for item in value:
                            if isinstance(item, dict) and 'entity' in item:
                                detected_entities.append(item)
        
        # If we found an array of entities, use the first one for now
        if detected_entities:
            detected_entity = detected_entities[0].get('entity')
            print(f"🎯 Found {len(detected_entities)} detected entities, using first: {detected_entity}")
    
    # Check current state for detected entities
    if not detected_entity and hasattr(callback_context, 'state') and callback_context.state:
        current_entities = callback_context.state.get('current_entities', [])
        current_entity_index = callback_context.state.get('current_entity_index', 0)
        
        if current_entities and current_entity_index < len(current_entities):
            current_entity_data = current_entities[current_entity_index]
            if isinstance(current_entity_data, dict):
                detected_entity = current_entity_data.get('entity')
                print(f"🔄 Using entity from state: {detected_entity} (index {current_entity_index})")
    
    # If no entity detected from context, try to parse from the messages
    if not detected_entity and llm_request and llm_request.contents:
        for content in llm_request.contents:
            if hasattr(content, 'text') and content.text:
                text = content.text.lower()
                # Look for entity patterns in the text
                if 'entity=' in text or 'entity":' in text:
                    # Extract entity from entity=value pattern
                    match = re.search(r'entity["\']?\s*[:=]\s*["\']?([^"\'\\s,}]+)["\']?', text)
                    if match:
                        detected_entity = match.group(1)
                        print(f"🔍 Extracted entity from text: {detected_entity}")
                        break
    
    print(f"🎯 Final detected entity: {detected_entity}")
    
    # Get entity-specific tools or fall back to all tools
    if detected_entity:
        api_summary = config_manager.get_entity_specific_tools(detected_entity)
        print(f"✅ Loaded {detected_entity}-specific tools")
    else:
        # Fall back to all tools if no entity detected
        api_summary = config_manager.get_api_endpoints_summary()
        print("⚠️ No entity detected - loading all tools as fallback")
    
    # Build enhanced instruction with API information
    enhanced_instruction = f"""
🛠️ **API CALLER AGENT - ENHANCED WITH DYNAMIC CONFIGURATION**

You are responsible for executing real HTTP API calls based on detected entities and intents.

**BASE URL:** {API_BASE_URL}

{api_summary}

**🔧 YOUR RESPONSIBILITIES:**

1. **Endpoint Selection**: Choose the correct API endpoint based on entity + intent
2. **URL Construction**: Build complete URLs with proper path parameters  
3. **Parameter Mapping**: Use endpoint configuration to format parameters correctly
4. **HTTP Execution**: Make real requests using `invoke_api_tool(url, method, body)`
5. **Error Handling**: Process responses and handle errors appropriately

**EXECUTION PROCESS:**

**Step 1: Parse Input**
- Review entity, intent, and extracted_params from previous agent
- Example: entity="Contact", intent="search", params={{"search": "john", "pageSize": 5}}

**Step 2: Map to Endpoint**
- Match entity to service (Contact/Partner/Interaction)
- Match intent to HTTP method:
  - create → POST endpoints (CreateContact, CreatePartner)
  - search/list → GET endpoints (GetContacts, GetPartners)
  - update → PUT endpoints (UpdateContact)
  - delete → DELETE endpoints

**Step 3: Construct URL**
- Combine base_url + endpoint_path
- Replace path parameters: `/contact/{{id}}` → `/contact/123`
- Examples:
  - GET {API_BASE_URL}/api/contact (list all)
  - GET {API_BASE_URL}/api/contact/123 (get specific)
  - POST {API_BASE_URL}/api/contact (create new)

**Step 4: Prepare Parameters**
- **GET requests**: Parameters go in query string (search, pageSize, offset)
- **POST/PUT requests**: Parameters go in request body (name, email, phone)
- **Path parameters**: Replace {{id}} placeholders in URL path with actual values
- **Required vs Optional**: Include required params, add optional when available
- **Possible Values**: For parameters with possible_values, choose the closest semantic match from the provided list
- **Value Selection**: Use intent and context to select the most appropriate value from possible_values

**Step 5: Execute & Format Response**
```python
result = invoke_api_tool(
    url="{API_BASE_URL}/api/contact",
    method="GET", 
    body={{"search": "john", "pageSize": 5}}
)
```

**PARAMETER HANDLING EXAMPLES:**

**Search Contacts by Name:**
- Input: entity="Contact", intent="search", params={{"name": "Madeline", "search": "Madeline"}}
- URL: GET {API_BASE_URL}/api/contact
- Body: {{"search": "Madeline"}}
- Response: Show found contacts for user to identify the correct one

**Using Possible Values Example:**
- Parameter: entityType with possible_values: ["retrieve_partner_information", "retrieve_contact_information", "retrieve_interaction_information"]
- User asks: "Summarize about this contact"
- Choose: "retrieve_contact_information" (closest match for contact summary)
- User asks: "Generate partner summary"
- Choose: "retrieve_partner_information" (closest match for partner summary)

**Search Contacts with Limit:**
- Input: entity="Contact", intent="search", params={{"search": "tech", "pageSize": 10}}
- URL: GET {API_BASE_URL}/api/contact
- Body: {{"search": "tech", "pageSize": 10, "offset": 0}}

**Create Partner:**
- Input: entity="Partner", intent="create", params={{"name": "Tech Corp", "email": "info@tech.com"}}
- URL: POST {API_BASE_URL}/api/partner  
- Body: {{"name": "Tech Corp", "email": "info@tech.com"}}

**Update Contact (Search-First Workflow with Loop Exit):**
- Input: entity="Contact", intent="update", params={{"name": "John", "email": "new@email.com"}}
- Step 1: GET {API_BASE_URL}/api/contact with {{"search": "John"}} to find the contact
- Step 2: Show results: "Found: John Smith (ID: 123), John Doe (ID: 456). Please provide the ID to update."
- Step 3: Call `exit_loop_on_success()` to stop loop and wait for user input
- Step 4: User provides ID in next message
- Step 5: New conversation turn - PUT {API_BASE_URL}/api/contact/123 with {{"email": "new@email.com"}}

**Search Contact (Simple Workflow with Loop Exit):**
- Input: entity="Contact", intent="search", params={{"name": "Madeline"}}
- Step 1: GET {API_BASE_URL}/api/contact with {{"search": "Madeline"}}
- Step 2: Show results to user
- Step 3: Call `exit_loop_on_success()` to complete the request

**Get Specific Contact by ID:**
- Input: entity="Contact", intent="search", params={{"id": "123"}}
- URL: GET {API_BASE_URL}/api/contact/123
- Body: {{}} (empty for GET with path parameter)

**ERROR HANDLING:**
- Connection errors: "API server may be unavailable"
- 400 errors: "Missing required parameters" or "Invalid data format"
- 404 errors: "Resource not found"  
- 500 errors: "Server error occurred"
- Always provide helpful context for users

**🚨 CRITICAL WORKFLOW FOR UPDATE/DELETE OPERATIONS:**

**For UPDATE or DELETE intents:**
1. **SEARCH FIRST**: Use GET endpoint to find the entity by name/search criteria
2. **SHOW RESULTS**: Present found records to user for confirmation
3. **ASK FOR CONFIRMATION**: "Which record would you like to update/delete? Please provide the ID."
4. **EXIT LOOP**: Call `exit_loop_on_success()` immediately after asking for confirmation
5. **WAIT FOR USER**: User will provide the ID in their next message
6. **THEN PROCEED**: In the next conversation turn, use PUT/DELETE endpoint with the provided ID

**For SEARCH intents:**
- Directly call the appropriate GET endpoint with search parameters
- Show the results to the user
- Call `exit_loop_on_success()` after showing results

**🚨 CRITICAL LOOP EXIT RULES:**
- **After ANY API call**: Call `exit_loop_on_success()`
- **After asking user a question**: Call `exit_loop_on_success()`
- **After showing search results**: Call `exit_loop_on_success()`
- **When waiting for user input**: Call `exit_loop_on_success()`
- **NEVER continue the loop when waiting for user response**

**CRITICAL INSTRUCTIONS:**
- You MUST call `invoke_api_tool()` to make the actual HTTP request
- Do NOT just describe what you would do - ACTUALLY DO IT
- For update/delete without ID: FIRST search, then ask for confirmation and ID
- **ALWAYS call `exit_loop_on_success()` after completing your task**
- **If waiting for user input/confirmation, call `exit_loop_on_success()` to let user respond**
- **If you ask a question to the user, immediately call `exit_loop_on_success()`**

**RESPONSE FORMAT:**
Always return structured information after the API call:
```json
{{
    "api_call": "GET {API_BASE_URL}/api/contact",
    "parameters": {{"search": "john", "pageSize": 5}},
    "response": {{"data": [...], "total": 3}},
    "status": "success",
    "status_code": 200,
    "message": "Found 3 contacts matching 'john'"
}}
```

Remember: You are making REAL HTTP requests to actual endpoints. The configuration above shows you exactly which endpoints are available and how to use them.
"""
    
    # Update the LLM request instruction
    if llm_request.config and llm_request.config.system_instruction:
        llm_request.config.system_instruction = enhanced_instruction
    else:
        # Create config if it doesn't exist
        if not llm_request.config:
            llm_request.config = types.GenerateContentConfig()
        llm_request.config.system_instruction = enhanced_instruction
    
    print("✅ Entity-specific API tools information injected successfully")
    
    # Don't return content - just modify the request
    return None


def advance_entity_callback(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Callback to advance to the next entity in processing queue
    """
    print(f"🔄 [Callback] advance_entity_callback triggered for {callback_context.agent_name}")
    
    # Get current entities and index from state
    current_entities = callback_context.state.get('current_entities', [])
    current_entity_index = callback_context.state.get('current_entity_index', 0)
    
    print(f"📊 Current state: {len(current_entities)} entities, index {current_entity_index}")
    
    # Advance to next entity
    next_index = current_entity_index + 1
    
    if next_index < len(current_entities):
        callback_context.state['current_entity_index'] = next_index
        next_entity = current_entities[next_index]
        print(f"➡️ Advanced to entity {next_index}: {next_entity.get('entity', 'Unknown')}")
    else:
        print("🏁 No more entities to process")
        callback_context.state['all_entities_processed'] = True
    
    return None 