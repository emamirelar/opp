import json
import os
import re
from urllib.parse import urlparse, parse_qs
from google.adk.agents.callback_context import CallbackContext
from google.adk.tools.tool_context import ToolContext
from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.utils.ui_config_manager import UIConfigManager
ui_config_manager = UIConfigManager()


def get_entity_display_name_tool(entity_type: str, entity_id: str) -> dict:
    """
    Fetches entity display information (name, status, etc.) for a given entity type and ID.
    
    Args:
        entity_type: The type of entity (e.g., 'Partner', 'Contact', 'Interaction')
        entity_id: The ID of the entity to fetch
        
    Returns:
        dict: Entity information including id, name, status, and entity_type
    """
    try:
        # Import here to avoid circular imports
        from ai_assistant.utils.common_callbacks import invoke_api_tool, construct_api_url
        
        # Load entity configuration
        entity_config = config_manager.load_entity_api_config(entity_type)
        
        if not entity_config or 'entities' not in entity_config:
            return {
                "error": f"No configuration found for entity type: {entity_type}",
                "entity_type": entity_type,
                "id": entity_id
            }
        
        # Find the appropriate GET endpoint
        entity_endpoints = entity_config['entities'][0].get('endpoints', [])
        get_endpoint = None
        
        for endpoint in entity_endpoints:
            if endpoint.get('method') == 'GET' and '{id}' in endpoint.get('url', ''):
                get_endpoint = endpoint
                break
        
        if not get_endpoint:
            return {
                "error": f"No GET endpoint found for entity type: {entity_type}",
                "entity_type": entity_type,
                "id": entity_id
            }
        
        # Construct the API URL
        base_url = config_manager.get_api_base_url()
        api_url = construct_api_url(base_url, get_endpoint['url'])
        api_url = api_url.replace('{id}', str(entity_id))
        
        # Make the API call
        result = invoke_api_tool(
            url=api_url,
            method='GET',
            body={},
            tool_context=None
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            
            # Extract common fields
            display_info = {
                "id": entity_id,
                "entity_type": entity_type,
                "name": response_data.get('name', f"{entity_type} {entity_id}"),
                "status": response_data.get('status', 'Unknown'),
                "raw_data": response_data
            }
            
            # Add entity-specific fields
            if entity_type.lower() == 'contact':
                display_info.update({
                    "email": response_data.get('email', ''),
                    "title": response_data.get('title', ''),
                    "department": response_data.get('department', '')
                })
            elif entity_type.lower() == 'partner':
                display_info.update({
                    "shortName": response_data.get('shortName', ''),
                    "website": response_data.get('website', ''),
                    "phone": response_data.get('phone', '')
                })
            elif entity_type.lower() == 'interaction':
                display_info.update({
                    "subject": response_data.get('subject', ''),
                    "interactionType": response_data.get('interactionType', ''),
                    "createdDate": response_data.get('createdDate', '')
                })
            
            return display_info
            
        else:
            return {
                "error": f"Failed to fetch {entity_type} {entity_id}: {result.get('error', 'Unknown error')}",
                "entity_type": entity_type,
                "id": entity_id
            }
            
    except Exception as e:
        return {
            "error": f"Exception while fetching {entity_type} {entity_id}: {str(e)}",
            "entity_type": entity_type,
            "id": entity_id
        }


def parse_screen_url_callback(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Enhanced callback that handles sophisticated screen_url vs user_focus_context logic:
    
    1. When screen_url != user_focus_context and user_focus_context is empty but screen_url is not empty:
       user_focus_context = screen_url
    2. Handle different URL patterns:
       - / means homepage
       - /ai* means fullscreen AI assistant mode
       - anything else means specific page
    3. Extract entity and ID from URLs
    4. Determine focus context and make appropriate API calls
    """
    try:
        session_state = callback_context.state
        
        # Handle Google ADK State object
        print(f"🔍 [CALLBACK] Session state type: {type(session_state)}")
        
        # Extract values from State object
        try:
            if hasattr(session_state, 'get'):
                screen_url_str = session_state.get('screen_url', '') or ''
                user_focus_context_str = session_state.get('user_focus_context', '') or ''
            else:
                screen_url_str = getattr(session_state, 'screen_url', '') or ''
                user_focus_context_str = getattr(session_state, 'user_focus_context', '') or ''
            print(f"🔍 [CALLBACK] Successfully extracted URLs")
        except Exception as e:
            print(f"❌ [CALLBACK] Error extracting URLs: {e}")
            screen_url_str = ''
            user_focus_context_str = ''
        
        print(f"🎯 [SCREEN] Processing URLs: screen_url='{screen_url_str}', user_focus_context='{user_focus_context_str}'")
        
        # Apply conditional logic for user_focus_context
        original_user_focus = user_focus_context_str
        if screen_url_str != user_focus_context_str and not user_focus_context_str and screen_url_str:
            user_focus_context_str = screen_url_str
            print(f"🔄 [SCREEN] Applied condition: user_focus_context set to '{user_focus_context_str}' (was empty)")
        
        # Determine focus relationship
        focus_relationship = "same" if screen_url_str == user_focus_context_str else "different"
        print(f"🎯 [SCREEN] Focus relationship: {focus_relationship}")
        
        # Initialize enhanced screen context
        enhanced_screen_context = {
            "original_screen_url": screen_url_str,
            "original_user_focus_context": original_user_focus,
            "resolved_user_focus_context": user_focus_context_str,
            "focus_relationship": focus_relationship,
            "screen_analysis": _analyze_enhanced_url(screen_url_str),
            "focus_analysis": _analyze_enhanced_url(user_focus_context_str),
            "primary_context": None,
            "secondary_context": None,
            "recommendations": []
        }
        
        # Determine primary and secondary contexts
        if focus_relationship == "same":
            enhanced_screen_context["primary_context"] = enhanced_screen_context["screen_analysis"]
            enhanced_screen_context["recommendations"].append("User is focusing on this context - prioritize this entity in responses")
        else:
            enhanced_screen_context["primary_context"] = enhanced_screen_context["focus_analysis"]
            enhanced_screen_context["secondary_context"] = enhanced_screen_context["screen_analysis"]
            enhanced_screen_context["recommendations"].append("User has separate focus - ask about current focus entity when relevant")
        
        # Fetch entity details for contexts that have entities
        if enhanced_screen_context["primary_context"]["entity_in_focus"] and enhanced_screen_context["primary_context"]["entity_id_in_focus"]:
            entity_details = get_entity_display_name_tool(
                enhanced_screen_context["primary_context"]["entity_in_focus"],
                enhanced_screen_context["primary_context"]["entity_id_in_focus"]
            )
            enhanced_screen_context["primary_context"]["entity_details"] = entity_details
            
        if enhanced_screen_context["secondary_context"] and enhanced_screen_context["secondary_context"]["entity_in_focus"] and enhanced_screen_context["secondary_context"]["entity_id_in_focus"]:
            entity_details = get_entity_display_name_tool(
                enhanced_screen_context["secondary_context"]["entity_in_focus"],
                enhanced_screen_context["secondary_context"]["entity_id_in_focus"]
            )
            enhanced_screen_context["secondary_context"]["entity_details"] = entity_details
        
        # Store enhanced context in state
        callback_context.state['enhanced_screen_context'] = enhanced_screen_context
        
        # Also maintain backward compatibility with basic_screen_context
        basic_context = enhanced_screen_context["primary_context"].copy()
        callback_context.state['basic_screen_context'] = basic_context
        
        print(f"✅ [SCREEN] Enhanced context processed successfully")
        
    except Exception as e:
        print(f"❌ [SCREEN] Error in enhanced parse_screen_url_callback: {e}")
        # Extract URLs for error context
        try:
            error_screen_url = getattr(session_state, 'screen_url', '') or (session_state.get('screen_url', '') if hasattr(session_state, 'get') else '')
            error_user_focus = getattr(session_state, 'user_focus_context', '') or (session_state.get('user_focus_context', '') if hasattr(session_state, 'get') else '')
        except:
            error_screen_url = 'unknown'
            error_user_focus = 'unknown'
            
        error_context = {
            "original_screen_url": error_screen_url,
            "original_user_focus_context": error_user_focus,
            "error": str(e),
            "screen_type": "error"
        }
        callback_context.state['enhanced_screen_context'] = error_context
        callback_context.state['basic_screen_context'] = error_context


def _analyze_enhanced_url(url_str: str) -> dict:
    """
    Enhanced URL analysis that handles the sophisticated patterns:
    - / means homepage
    - /ai* means fullscreen AI assistant mode  
    - anything else means specific page
    - Extract entity and ID from URLs with query params
    """
    analysis = {
        "current_url": url_str,
        "parsed_path": "",
        "query_params": {},
        "screen_type": "unknown",
        "entity_in_focus": None,
        "entity_id_in_focus": None,
        "screen_metadata": {},
        "ui_schema_file": None,
        "url_pattern": "unknown"
    }
    
    if not url_str:
        analysis.update({
            "screen_type": "homepage",
            "url_pattern": "empty"
        })
        return analysis
    
    # Handle root homepage
    if url_str == '/' or url_str.strip() == '':
        analysis.update({
            "screen_type": "homepage", 
            "url_pattern": "homepage"
        })
        return analysis
    
    # Parse URL for query params and path
    if not url_str.startswith('http'):
        url_str_for_parsing = f"https://dummy.com{url_str if url_str.startswith('/') else '/' + url_str}"
    else:
        url_str_for_parsing = url_str
    
    parsed_url = urlparse(url_str_for_parsing)
    path = parsed_url.path
    query_params = parse_qs(parsed_url.query)
    
    analysis["parsed_path"] = path
    analysis["query_params"] = {k: v[0] if len(v) == 1 else v for k, v in query_params.items()}
    
    # Check for AI assistant mode
    if path.lower().startswith('/ai') or 'ai' in path.lower():
        analysis.update({
            "screen_type": "ai_assistant_mode",
            "url_pattern": "ai_fullscreen"
        })
        return analysis
    
    # Try UI config manager first
    ui_match = ui_config_manager.find_matching_page_by_url(url_str)
    if ui_match:
        analysis.update({
            "screen_type": ui_match.get("screen_type", "unknown"),
            "entity_in_focus": ui_match.get("entity_name"),
            "entity_id_in_focus": ui_match.get("entity_id"),
            "screen_metadata": ui_match.get("screen_metadata", {}),
            "ui_schema_file": ui_match.get("ui_schema_file"),
            "url_pattern": "ui_config_match"
        })
        return analysis
    
    # Fallback to path analysis
    path_parts = [part for part in path.strip('/').split('/') if part]
    
    if not path_parts:
        analysis.update({
            "screen_type": "homepage",
            "url_pattern": "root_path"
        })
        return analysis
    
    # Enhanced entity pattern matching - prioritize the LAST entity pattern found
    entity_patterns = {
        'partners': 'Partner',
        'contacts': 'Contact', 
        'interactions': 'Interaction',
        'aiprompts': 'AiPrompt',
        'partnerships': 'Partner',
        'partner-tree': 'PartnerTree',
        'partnertree': 'PartnerTree'
    }
    
    # Look for entity patterns, but prioritize the last one found
    found_entity = None
    found_entity_index = -1
    
    for i, part in enumerate(path_parts):
        if part.lower() in entity_patterns:
            found_entity = entity_patterns[part.lower()]
            found_entity_index = i
    
    if found_entity and found_entity_index >= 0:
        analysis["entity_in_focus"] = found_entity
        
        # Look for ID in the part immediately after the entity
        if found_entity_index + 1 < len(path_parts):
            next_part = path_parts[found_entity_index + 1]
            if _is_likely_id(next_part):
                analysis["entity_id_in_focus"] = next_part
                analysis["screen_type"] = "entity_detail_page"
                analysis["url_pattern"] = "entity_detail"
                return analysis
        
        analysis["screen_type"] = "entity_list_page"
        analysis["url_pattern"] = "entity_list"
        return analysis
    
    # Check for numeric ID at end of any path (fallback)
    if path_parts and _is_likely_id(path_parts[-1]):
        # Try to infer entity from earlier parts
        if len(path_parts) >= 2:
            potential_entity = path_parts[-2].lower()
            if potential_entity in entity_patterns:
                analysis.update({
                    "entity_in_focus": entity_patterns[potential_entity],
                    "entity_id_in_focus": path_parts[-1],
                    "screen_type": "entity_detail_page",
                    "url_pattern": "inferred_entity_detail"
                })
                return analysis
    
    # Form or special pages
    if any(keyword in path.lower() for keyword in ['create', 'edit', 'new', 'form']):
        analysis.update({
            "screen_type": "form_page",
            "url_pattern": "form"
        })
        return analysis
    
    # Dashboard pages
    if 'dashboard' in path.lower():
        analysis.update({
            "screen_type": "dashboard_overview",
            "url_pattern": "dashboard"
        })
        return analysis
    
    analysis.update({
        "screen_type": "specific_page",
        "url_pattern": "generic_page"
    })
    return analysis


def _is_likely_id(value: str) -> bool:
    """Check if a string value is likely an entity ID"""
    if not value:
        return False
    
    # Numeric ID
    if value.isdigit():
        return True
    
    # GUID pattern
    if re.match(r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$', value):
        return True
    
    # Long alphanumeric (likely ID)
    if len(value) > 10 and re.match(r'^[a-zA-Z0-9]+$', value):
        return True
    
    return False


def _analyze_path_fallback(path: str) -> tuple:
    """
    Fallback path analysis when ui_config_manager doesn't find a match.
    This is the original logic moved to a fallback function.
    
    Returns:
        tuple: (screen_type, entity_in_focus, entity_id_in_focus)
    """
    if not path or path == '/':
        return ("homepage", None, None)
    
    # Remove leading/trailing slashes and split
    path_parts = [part for part in path.strip('/').split('/') if part]
    
    if not path_parts:
        return ("homepage", None, None)
    
    # AI-specific pages
    if any(ai_keyword in path.lower() for ai_keyword in ['ai', 'assistant', 'chat']):
        return ("ai_specific_page", None, None)
    
    # Dashboard pages
    if 'dashboard' in path.lower():
        return ("dashboard_overview", None, None)
    
    # Entity patterns
    entity_patterns = {
        'partners': 'Partner',
        'contacts': 'Contact', 
        'interactions': 'Interaction',
        'aiprompts': 'AiPrompt',
        'partnerships': 'Partner'  # For /partnerships/partners
    }
    
    # Check for entity patterns
    for i, part in enumerate(path_parts):
        if part.lower() in entity_patterns:
            entity_name = entity_patterns[part.lower()]
            
            # Check if there's an ID after the entity
            if i + 1 < len(path_parts):
                next_part = path_parts[i + 1]
                # Check if it looks like an ID (numeric or GUID-like)
                if (next_part.isdigit() or 
                    re.match(r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$', next_part) or
                    len(next_part) > 10):  # Probably an ID
                    return ("entity_detail_page", entity_name, next_part)
            
            # It's an entity list/management page
            return ("entity_list_page", entity_name, None)
    
    # Form patterns
    if any(form_keyword in path.lower() for form_keyword in ['create', 'edit', 'new', 'form']):
        return ("form_page", None, None)
    
    return ("unknown", None, None)


def _get_screen_metadata_fallback(path: str, entity_name: str) -> tuple:
    """
    Fallback method to get screen metadata when ui_config_manager doesn't find a match.
    This uses the original direct file reading approach.
    
    Returns:
        tuple: (screen_metadata_dict, ui_schema_filename)
    """
    try:
        ui_dir = os.path.join(os.path.dirname(__file__), "../../../config/tools/ui")
        
        # Find the appropriate UI schema file
        ui_schema_file = None
        screen_metadata = {}
        
        # Priority 1: Direct entity match
        if entity_name:
            entity_file = f"{entity_name.lower()}-ui.json"
            entity_file_path = os.path.join(ui_dir, entity_file)
            if os.path.exists(entity_file_path):
                ui_schema_file = entity_file
                screen_metadata = _load_ui_schema_metadata_fallback(entity_file_path, path)
        
        # Priority 2: Path-based matching
        if not ui_schema_file:
            for filename in os.listdir(ui_dir):
                if filename.endswith('-ui.json'):
                    file_key = filename.replace('-ui.json', '')
                    if file_key.lower() in path.lower():
                        ui_schema_file = filename
                        file_path = os.path.join(ui_dir, filename)
                        screen_metadata = _load_ui_schema_metadata_fallback(file_path, path)
                        break
        
        return (screen_metadata, ui_schema_file)
        
    except Exception as e:
        print(f"⚠️ [SCREEN] Error loading UI schema in fallback: {e}")
        return ({}, None)


def _load_ui_schema_metadata_fallback(file_path: str, current_path: str) -> dict:
    """
    Fallback method to load and extract relevant metadata from UI schema file.
    This is the original logic moved to a fallback function.
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            ui_schema = json.load(f)
        
        # Find the matching page
        pages = ui_schema.get('pages', [])
        matching_page = None
        
        for page in pages:
            page_route = page.get('route', '')
            if page_route and (page_route in current_path or current_path in page_route):
                matching_page = page
                break
        
        # If no exact match, use the first page or general info
        if not matching_page and pages:
            matching_page = pages[0]
        
        # Build screen metadata
        metadata = {
            "title": ui_schema.get('entity', 'Page'),
            "type": "unknown",
            "entity_type_singular": ui_schema.get('entity', '').lower() if ui_schema.get('entity') else None,
            "description": ui_schema.get('description', ''),
            "available_actions": []
        }
        
        if matching_page:
            metadata.update({
                "title": matching_page.get('name', ui_schema.get('entity', 'Page')),
                "description": matching_page.get('description', ui_schema.get('description', '')),
                "available_actions": matching_page.get('capabilities', [])
            })
            
            # Determine type based on route and capabilities
            route = matching_page.get('route', '')
            if any(detail_keyword in route for detail_keyword in ['{id}', '/details', '/view']):
                metadata["type"] = "entity_detail_page"
            elif any(list_keyword in route for list_keyword in ['/list', 's/', 'manage']):
                metadata["type"] = "entity_list_page"
            else:
                metadata["type"] = "general_page"
        
        return metadata
        
    except Exception as e:
        print(f"⚠️ [SCREEN] Error parsing UI schema file {file_path} in fallback: {e}")
        return {}


def gather_screen_context(tool_context: ToolContext) -> dict:
    """
    Intelligently gather screen context using structured data from session state.
    Uses screen_url object and user_viewing_panel for entity and ID information.
    
    Args:
        tool_context: The tool context from Google ADK containing session state
    """
    try:
        # Get session state from tool context
        session_state = tool_context.state
        
        # Handle Google ADK State object
        print(f"🔍 [FUNCTION] Session state type: {type(session_state)}")
        
        # Extract data from Google ADK State object
        screen_url = ''
        user_focus_context = ''
        user_viewing_panel = {}
        
        try:
            # Google ADK State objects typically have dict-like access
            if hasattr(session_state, 'get'):
                screen_url = session_state.get('screen_url', '') or ''
                user_focus_context = session_state.get('user_focus_context', '') or ''
                user_viewing_panel = session_state.get('user_viewing_panel', {}) or {}
                print(f"🔍 [FUNCTION] Accessed via get() method")
            else:
                # Fallback: try attribute access
                screen_url = getattr(session_state, 'screen_url', '') or ''
                user_focus_context = getattr(session_state, 'user_focus_context', '') or ''
                user_viewing_panel = getattr(session_state, 'user_viewing_panel', {}) or {}
                print(f"🔍 [FUNCTION] Accessed via attributes")
        except Exception as e:
            print(f"❌ [FUNCTION] Error accessing session state: {e}")
            # Return minimal error context for homepage
            if hasattr(session_state, '__str__') and 'screen_url' in str(session_state):
                return {
                    "screen_name": "Dashboard",
                    "screen_url": "/",
                    "screen_type": "homepage",
                    "screen_data": {"message": "Fallback homepage context"}
                }
            else:
                return {
                    "screen_name": "Error Page",
                    "screen_url": "unknown",
                    "screen_type": "error", 
                    "screen_data": {"error": f"Cannot access session state: {e}"}
                }
        
        # Import here to avoid circular imports
        from ai_assistant.utils.common_callbacks import invoke_api_tool, construct_api_url
        
        # Debug: Print extracted values
        print(f"🔍 [FUNCTION] Extracted screen_url: '{screen_url}'")
        print(f"🔍 [FUNCTION] Extracted user_focus_context: '{user_focus_context}'")
        print(f"🔍 [FUNCTION] Extracted user_viewing_panel: {user_viewing_panel}")
        
        print(f"🔍 [FUNCTION] Parsing URLs directly:")
        print(f"🔍 [FUNCTION] screen_url: '{screen_url}'")
        print(f"🔍 [FUNCTION] user_focus_context: '{user_focus_context}'")
        
        # Determine which URL to use for parsing
        url_to_parse = user_focus_context if user_focus_context else screen_url
        print(f"🔍 [FUNCTION] Using URL for parsing: '{url_to_parse}'")
        
        # Parse the URL to extract entity and ID
        # Expected URL patterns:
        # "/" -> homepage
        # "/partnerships/partners" -> entity: partners, id: None
        # "/partnerships/partners/123" -> entity: partners, id: 123
        # "/partnerships/contacts/456" -> entity: contacts, id: 456
        # "/ai" -> entity: None (special case)
        
        entity = ''
        entity_id = None
        section = ''
        
        if url_to_parse and url_to_parse != '/':
            # Remove leading slash and split by '/'
            path_parts = url_to_parse.strip('/').split('/')
            print(f"🔍 [FUNCTION] URL path parts: {path_parts}")
            
            if len(path_parts) >= 1 and path_parts[0]:
                first_part = path_parts[0]
                
                # Handle special routes
                if first_part == 'ai':
                    print("🤖 [FUNCTION] AI route detected - no entity context needed")
                    entity = ''
                elif len(path_parts) >= 2 and path_parts[1]:
                    # Normal entity routes like /partnerships/partners
                    entity = path_parts[1]
                    print(f"🔍 [FUNCTION] Detected entity: {entity}")
                    
                    if len(path_parts) >= 3 and path_parts[2]:
                        # Third part might be entity ID
                        potential_id = path_parts[2]
                        if potential_id.isdigit():
                            entity_id = int(potential_id)
                            print(f"🔍 [FUNCTION] Detected numeric entity_id: {entity_id}")
                        elif potential_id not in ['new', 'edit', 'list']:  # Skip action words
                            entity_id = potential_id
                            print(f"🔍 [FUNCTION] Detected string entity_id: {entity_id}")
                            
                    if len(path_parts) >= 4:
                        # Fourth part might be section/action
                        section = path_parts[3]
                        print(f"🔍 [FUNCTION] Detected section: {section}")
                else:
                    # If only one part, it might be the entity directly
                    entity = first_part
                    print(f"🔍 [FUNCTION] Single part entity: {entity}")
        
        # Fallback: check user_viewing_panel if no entity found from URL
        if not entity and user_viewing_panel:
            entity = user_viewing_panel.get('entity', '')
            entity_id = user_viewing_panel.get('entity_id', None)
            if entity_id and isinstance(entity_id, str) and entity_id.isdigit():
                entity_id = int(entity_id)
        
        print(f"📡 [FUNCTION] Processing screen context with structured data:")
        print(f"📋 [FUNCTION] entity='{entity}'")
        print(f"🔢 [FUNCTION] entity_id={entity_id}")
        print(f"📄 [FUNCTION] section='{section}'")
        
        # Handle empty entity case gracefully - no context needed
        if not entity:
            print("📭 [FUNCTION] No entity found - checking if this is homepage")
            
            # Check if this is homepage/dashboard
            if url_to_parse == '/' or url_to_parse == '' or not url_to_parse:
                print("🏠 [FUNCTION] Detected homepage - returning appropriate context")
                homepage_context = {
                    "screen_name": "Dashboard",
                    "screen_url": "/",
                    "screen_type": "homepage", 
                    "screen_data": {
                        "message": "Welcome to the homepage",
                        "context_available": True,
                        "view_type": "homepage"
                    }
                }
                return homepage_context
            else:
                print("📭 [FUNCTION] No entity found in non-homepage context - returning empty context")
                empty_context = {
                    "screen_name": "Application",
                    "screen_url": url_to_parse,
                    "screen_type": "app",
                    "screen_data": {
                        "message": "No specific screen context available",
                        "context_available": False
                    }
                }
                return empty_context
        
        final_entity = entity
        final_id = entity_id
        final_query = "detail" if final_id else "list"
        
        print(f"📋 [FUNCTION] Using structured data: entity='{final_entity}', id={final_id}, query='{final_query}'")
        
        # Create a display URL for logging and context
        display_url = f"/{final_entity}"
        if final_id:
            display_url += f"/{final_id}"
        if section:
            display_url += f"/{section}"
        
        # Handle special cases for non-API entities
        if final_entity in ['dashboard', ''] or url_to_parse in ['/', '']:
            print("📭 [FUNCTION] Dashboard/Home route - returning general context")
            dashboard_context = {
                "screen_name": "Dashboard",
                "screen_url": "/" if url_to_parse in ['/', ''] else display_url,
                "screen_type": "dashboard",
                "screen_data": {
                    "message": "Dashboard screen context",
                    "context_available": True,
                    "view_type": "dashboard",
                    "entity": "dashboard"
                }
            }
            return dashboard_context
        
        # Load entities configuration from tools.json
        entities_config = config_manager.get_entities()
        
        # Enhanced entity matching to handle plural/singular and case differences
        def find_matching_entity(target_entity: str, entities_list):
            target_lower = target_entity.lower()
            
            for ent in entities_list:
                config_entity = ent.get('entity', '')
                config_entity_lower = config_entity.lower()
                
                # Direct match
                if config_entity_lower == target_lower:
                    return ent
                
                # Singular vs plural matching
                # Remove 's' from target and compare
                if target_lower.endswith('s') and config_entity_lower == target_lower[:-1]:
                    return ent
                
                # Add 's' to config entity and compare  
                if config_entity_lower + 's' == target_lower:
                    return ent
                
                # Check synonyms
                synonyms = ent.get('synonyms', [])
                for synonym in synonyms:
                    if synonym.lower() == target_lower:
                        return ent
                    # Check plural/singular for synonyms too
                    if target_lower.endswith('s') and synonym.lower() == target_lower[:-1]:
                        return ent
                    if synonym.lower() + 's' == target_lower:
                        return ent
            
            return None
        
        # Find the entity configuration with enhanced matching
        entity_config = find_matching_entity(final_entity, entities_config)
        original_search_entity = final_entity  # Keep track of what we were originally looking for
        
        # If no entity found, try fallback search in Values entity for org units and other common lookups
        if not entity_config:
            print(f"⚠️ [FUNCTION] {final_entity} entity not found, trying fallback to Values...")
            
            # Define fallback mappings for common requests that should use Values entity
            values_fallback_keywords = [
                'org', 'orgunit', 'organizationunit', 'organizationunits', 'organization',
                'department', 'departments', 'unit', 'units', 'hierarchy',
                'currency', 'currencies', 'country', 'countries', 'eligible', 'lookup', 'lookups'
            ]
            
            # Check if the entity matches any Values fallback keywords
            final_entity_lower = final_entity.lower()
            should_use_values = any(keyword in final_entity_lower for keyword in values_fallback_keywords)
            
            if should_use_values:
                # Try to find Values entity
                values_entity = find_matching_entity('values', entities_config)
                if values_entity:
                    print(f"✅ [FUNCTION] Using Values entity as fallback for '{final_entity}'")
                    entity_config = values_entity
                    final_entity = 'values'  # Update the entity name for endpoint search
        
        if not entity_config:
            print(f"⚠️ [FUNCTION] {final_entity} entity not found in tools.json and no fallback available")
            error_context = {
                "screen_name": f"{final_entity.title()} Page",
                "screen_url": display_url,
                "screen_type": "entity",
                "screen_data": {"entity": final_entity, "error": "Entity not found in configuration"}
            }
            return error_context
        
        # Use the actual entity name from configuration
        actual_entity_name = entity_config.get('entity', final_entity)
        print(f"✅ [FUNCTION] Found entity configuration: {actual_entity_name} for entity '{final_entity}'")
        
        # Determine which endpoint to use based on query type and ID presence
        target_endpoint = None
        
        if final_id:
            # Detail view - look for GetEntityById endpoints
            for endpoint in entity_config.get('endpoints', []):
                endpoint_name = endpoint.get('name', '').lower()
                if 'get' in endpoint_name and 'byid' in endpoint_name:
                    target_endpoint = endpoint
                    break
        else:
            # List view - look for GetEntities endpoints
            # Special handling for org units and other Values entity requests
            original_entity_lower = original_search_entity.lower() if original_search_entity != final_entity else None
            
            # Priority search for specific endpoints
            for endpoint in entity_config.get('endpoints', []):
                endpoint_name = endpoint.get('name', '').lower()
                endpoint_url = endpoint.get('url', '').lower()
                
                # Special case: if looking for org units, prioritize GetOrganizationUnits
                if original_entity_lower and any(term in original_entity_lower for term in ['org', 'unit', 'organization']):
                    if 'organization' in endpoint_name and 'unit' in endpoint_name:
                        target_endpoint = endpoint
                        print(f"🎯 [FUNCTION] Found specific org units endpoint: {endpoint_name}")
                        break
                
                # General pattern matching for list endpoints
                if ('get' in endpoint_name and 
                    ('s' in endpoint_name[-1:] or 'list' in endpoint_name) and
                    'byid' not in endpoint_name):
                    target_endpoint = endpoint
                    # Don't break here - continue looking for more specific matches
            
            # If still no target found, try any GET endpoint without ID parameter
            if not target_endpoint:
                for endpoint in entity_config.get('endpoints', []):
                    endpoint_name = endpoint.get('name', '').lower()
                    endpoint_url = endpoint.get('url', '').lower()
                    if 'get' in endpoint_name and '{id}' not in endpoint_url:
                        target_endpoint = endpoint
                        break
        
        if not target_endpoint:
            print(f"⚠️ [FUNCTION] No appropriate endpoint found for {actual_entity_name} (ID: {final_id})")
            error_context = {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": display_url,
                "screen_type": "entity", 
                "screen_data": {"entity": actual_entity_name, "entity_id": final_id, "error": "No suitable endpoint found"}
            }
            return error_context
        
        # Construct API URL and prepare parameters
        api_url = construct_api_url(config_manager.get_api_base_url(), target_endpoint['url'])
        method = target_endpoint['method']
        
        # Prepare parameters based on endpoint type
        parameters = {}
        
        if final_id:
            # Detail view - substitute ID in URL path
            api_url = api_url.replace('{id}', str(final_id))
            print(f"🌐 [FUNCTION] Calling detail API for screen: {method} {api_url}")
        else:
            # List view - add pagination parameters
            parameters = {
                "pageIndex": 1,
                "pageSize": 10
            }
            print(f"🌐 [FUNCTION] Calling list API for screen: {method} {api_url} with params: {parameters}")
        
        # Make the API call
        result = invoke_api_tool(
            url=api_url,
            method=method,
            body=parameters,
            tool_context=tool_context
        )
        
        if result.get('status') == 'success':
            response_data = result.get('response', {})
            print(f"✅ [FUNCTION] Successfully retrieved {actual_entity_name} data for entity '{final_entity}'")
            
            # Format response based on view type
            if final_id:
                # Detail view
                screen_name = f"{actual_entity_name} Details"
                screen_type = "detail"
                screen_data = {
                    "entity": actual_entity_name,
                    "entity_id": final_id,
                    "entity_data": response_data,
                    "view_type": "detail",
                    "intelligent_context": True,
                    "parsed_from_state": display_url,
                    "detected_entity": final_entity
                }
            else:
                # List view
                screen_name = f"{actual_entity_name} List"
                screen_type = "list"
                data_items = response_data.get('data', [])
                total_count = response_data.get('totalCount', len(data_items))
                screen_data = {
                    "entity": actual_entity_name,
                    "view_type": "list",
                    "total_count": total_count,
                    "current_page": 1,
                    "page_size": len(data_items),
                    "items": data_items,
                    "intelligent_context": True,
                    "parsed_from_state": display_url,
                    "detected_entity": final_entity
                }
            
            screen_context = {
                "screen_name": screen_name,
                "screen_url": display_url,
                "screen_type": screen_type,
                "screen_data": screen_data
            }
            
            # Store in session state
            try:
                if session_state is not None:
                    session_state['screen_context'] = screen_context
                    print(f"💾 Stored screen context in session state")
                
            except Exception as cache_error:
                print(f"⚠️ Failed to set session state: {cache_error}")
            
            return screen_context
        else:
            error_msg = result.get('error', 'Unknown error')
            print(f"❌ API call failed for entity '{final_entity}': {error_msg}")
            
            error_context = {
                "screen_name": f"{actual_entity_name} Page",
                "screen_url": display_url,
                "screen_type": "error",
                "screen_data": {
                    "entity": actual_entity_name,
                    "entity_id": final_id,
                    "error": f"API call failed: {error_msg}"
                }
            }
            return error_context
            
    except Exception as e:
        print(f"❌ Error gathering screen context: {e}")
        return {
            "screen_name": "Error Page",
            "screen_url": "unknown", 
            "screen_type": "error",
            "screen_data": {"error": str(e)}
        }

