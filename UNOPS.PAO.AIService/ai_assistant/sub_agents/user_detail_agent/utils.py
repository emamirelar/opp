import json
import os
from typing import Optional
from google.adk.tools.tool_context import ToolContext
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ai_assistant.utils.api_config_manager import config_manager

# Import from the new common callbacks structure
from ai_assistant.utils.common_callbacks import (
    invoke_api_tool,
    construct_api_url
)

# Import LlmResponse from the correct module
LlmResponse = types.GenerateContentResponse

def get_user_email_from_session(session_state: dict) -> str:
    """
    Extract user email from session state
    
    Args:
        session_state: Session state dictionary
        
    Returns:
        str: User email or empty string if not found
    """
    if not session_state:
        return ""
    
    # Try various possible keys where user email might be stored
    possible_keys = ['user_email', 'email', 'userEmail', 'user_id', 'userId']
    
    for key in possible_keys:
        if key in session_state and session_state[key]:
            return session_state[key]
    
    return ""

def get_user_profile(tool_context: ToolContext) -> dict:
    """
    Get user profile from the API using the user's email from session state.
    
    Args:
        tool_context: The tool context from Google ADK containing session state
    
    Returns:
        dict: User profile data in JSON format
    """
    try:
        # Get session state from tool context
        session_state = tool_context.state
        
        # Get user email from session state
        user_email = get_user_email_from_session(session_state)

        # Development mode override
        if os.getenv('CURRENT_ENV') == 'dev':
            dev_email = os.getenv('DEV_EMAIL', 'anushas@unops.org')
            print(f"🔧 [DEV MODE] Using development email: {dev_email}")
            user_email = dev_email

        if not user_email:
            return {"error": "No user email found in session state"}
        
        # Enhanced approach: Try configuration first, then use robust dynamic discovery
        found_endpoint = None
        endpoint_name = None
        entity_name = None
        
        # Step 1: Try to get configured endpoint (if available)
        try:
            profile_config = config_manager.get_user_profile_config()
            if profile_config:
                endpoint_name = profile_config["endpoint_name"]  # "GetCurrentUserData"
                entity_name = profile_config["entity"]          # "UserProfile"
                print(f"🎯 [DEBUG] Configuration available - looking for endpoint '{endpoint_name}' in entity '{entity_name}'")
                
                # Try userprofile-tools.json first  
                try:
                    print(f"📁 [DEBUG] Step 1a: Trying {entity_name.lower()}-tools.json")
                    entity_config = config_manager.load_entity_api_config(entity_name)
                    
                    # Find the endpoint by name
                    for entity in entity_config.get('entities', []):
                        for endpoint in entity.get('endpoints', []):
                            if endpoint.get('name') == endpoint_name:
                                found_endpoint = endpoint
                                found_endpoint['from_config_lookup'] = True  # Mark for retry logic
                                print(f"✅ [DEBUG] Found endpoint in {entity_name.lower()}-tools.json")
                                break
                        if found_endpoint:
                            break
                            
                except Exception as e:
                    print(f"⚠️ [DEBUG] Could not load {entity_name.lower()}-tools.json: {e}")
                
                # Fallback to tools.json if not found
                if not found_endpoint:
                    print(f"📁 [DEBUG] Step 1b: Fallback to tools.json")
                    try:
                        tools_config = config_manager.load_tools_config()
                        
                        # Find UserProfile entity and the endpoint
                        for entity in tools_config.get('entities', []):
                            if entity.get('entity') == entity_name:
                                for endpoint in entity.get('endpoints', []):
                                    if endpoint.get('name') == endpoint_name:
                                        found_endpoint = endpoint
                                        found_endpoint['from_config_lookup'] = True  # Mark for retry logic
                                        print(f"✅ [DEBUG] Found endpoint in tools.json under {entity_name} entity")
                                        break
                                if found_endpoint:
                                    break
                                    
                    except Exception as e:
                        print(f"❌ [DEBUG] Could not load tools.json: {e}")
            else:
                print(f"⚠️ [DEBUG] No user profile configuration available - will use dynamic discovery")
                
        except Exception as e:
            print(f"⚠️ [DEBUG] Could not load user profile configuration: {e} - will use dynamic discovery")
        
        # Define scoring function for user profile endpoints (used in multiple places)
        def score_endpoint_for_user_profile(endpoint: dict, entity_name: str) -> int:
                """Score endpoint for user profile suitability"""
                score = 0
                endpoint_name = endpoint.get('name', '').lower()
                description = endpoint.get('description', '').lower()
                url = endpoint.get('url', '').lower()
                method = endpoint.get('method', 'GET').upper()
                when_to_use = endpoint.get('when_to_use', '').lower()
                
                # Must be GET method for user profile retrieval
                if method != 'GET':
                    return 0
                
                # Score based on user profile keywords
                user_profile_keywords = [
                    ('current', 15), ('user', 10), ('profile', 10), ('context', 8), 
                    ('info', 5), ('data', 5), ('detail', 5), ('me', 8), ('self', 8)
                ]
                
                for keyword, points in user_profile_keywords:
                    if keyword in endpoint_name:
                        score += points + 5  # Bonus for being in name
                    elif keyword in description:
                        score += points
                    elif keyword in when_to_use:
                        score += points
                    elif keyword in url:
                        score += points // 2
                
                # Prefer endpoints that take email as parameter
                parameters = endpoint.get('parameters', {})
                if 'email' in parameters or 'userEmail' in parameters:
                    score += 20
                
                # Penalty for endpoints that seem like lists or searches
                list_indicators = ['list', 'search', 'all', 'many', 'multiple']
                if any(indicator in endpoint_name for indicator in list_indicators):
                    score -= 10
                
                return max(0, score)
        
        # Step 2: Use dynamic endpoint discovery (like find_entity_endpoint) if needed
        if not found_endpoint:
            print(f"🔍 [DEBUG] Using dynamic endpoint discovery (robust fallback)...")
            
            # Try multiple user-related entity names 
            user_entities = ["UserProfile", "UserData", "UserInfo", "User", "Profile", "UserContext"]
            
            best_endpoint = None
            best_score = 0
            
            # Try each entity type
            for entity_candidate in user_entities:
                print(f"🔍 [DEBUG] Step 2a: Checking entity '{entity_candidate}'")
                try:
                    all_endpoints = config_manager.get_entity_api_endpoints(entity_candidate)
                    if not all_endpoints:
                        continue
                    
                    # Score all endpoints for this entity
                    for endpoint in all_endpoints:
                        score = score_endpoint_for_user_profile(endpoint, entity_candidate)
                        if score > best_score:
                            best_score = score
                            best_endpoint = endpoint
                            print(f"✅ [DEBUG] Better endpoint found: '{endpoint.get('name')}' in {entity_candidate} (score: {score})")
                            
                except Exception as e:
                    print(f"⚠️ [DEBUG] Could not check entity {entity_candidate}: {e}")
                    continue
            
            if best_endpoint and best_score >= 10:  # Minimum threshold
                found_endpoint = best_endpoint
                print(f"🎯 [DEBUG] Selected best endpoint: '{found_endpoint.get('name')}' (score: {best_score})")
            
            # Step 2b: Final fallback - comprehensive search across all entities
            if not found_endpoint:
                print(f"🔍 [DEBUG] Step 2b: Comprehensive search across all entities...")
                try:
                    tools_config = config_manager.load_tools_config()
                    
                    for entity in tools_config.get('entities', []):
                        entity_name_check = entity.get('entity', '')
                        for endpoint in entity.get('endpoints', []):
                            score = score_endpoint_for_user_profile(endpoint, entity_name_check)
                            if score > best_score and score >= 15:  # Higher threshold for cross-entity search
                                best_score = score
                                best_endpoint = endpoint
                                print(f"✅ [DEBUG] Cross-entity endpoint found: '{endpoint.get('name')}' in {entity_name_check} (score: {score})")
                    
                    if best_endpoint:
                        found_endpoint = best_endpoint
                        
                except Exception as e:
                    print(f"⚠️ [DEBUG] Comprehensive search failed: {e}")
            
            if not found_endpoint:
                print(f"❌ [DEBUG] All endpoint discovery methods failed")
                return {"error": "No suitable user profile endpoint found. Tried: configuration-based lookup, dynamic entity scoring, and comprehensive cross-entity search. Please check API configuration."}
          
        # Step 3: Prepare retry candidates (for robust API calling)
        retry_candidates = [found_endpoint]
        
        # If we found multiple good endpoints during discovery, prepare them as fallbacks
        if not found_endpoint.get('from_config_lookup', False):  # Only for dynamic discovery
            try:
                # Find additional fallback endpoints
                print(f"🔄 [DEBUG] Finding additional fallback endpoints...")
                for entity_candidate in ["UserProfile", "UserData", "UserInfo", "User", "Profile"]:
                    try:
                        all_endpoints = config_manager.get_entity_api_endpoints(entity_candidate)
                        if not all_endpoints:
                            continue
                        
                        for endpoint in all_endpoints:
                            if endpoint != found_endpoint:  # Don't duplicate primary endpoint
                                score = score_endpoint_for_user_profile(endpoint, entity_candidate)
                                if score >= 8:  # Lower threshold for fallbacks
                                    retry_candidates.append(endpoint)
                                    if len(retry_candidates) >= 3:  # Limit to 3 total attempts
                                        break
                        
                        if len(retry_candidates) >= 3:
                            break
                            
                    except Exception:
                        continue
                        
                print(f"🎯 [DEBUG] Prepared {len(retry_candidates)} endpoint(s) for retry")
                        
            except Exception as e:
                print(f"⚠️ [DEBUG] Could not prepare fallback endpoints: {e}")
        
        # Step 4: Try endpoints with retry logic (like task_executor_agent)
        base_url = config_manager.get_api_base_url()
        last_error = None
        
        for attempt, endpoint in enumerate(retry_candidates, 1):
            try:
                api_url = construct_api_url(base_url, endpoint['url'])
                method = endpoint['method']
                
                # Determine parameter name for user email
                endpoint_params = endpoint.get('parameters', {})
                if 'email' in endpoint_params:
                    parameters = {"email": user_email}
                elif 'userEmail' in endpoint_params:
                    parameters = {"userEmail": user_email}
                else:
                    # Default to 'email'
                    parameters = {"email": user_email}
                    
                print(f"🌐 [DEBUG] Attempt {attempt}/{len(retry_candidates)}: {method} {api_url}")
                print(f"📧 [DEBUG] Parameters: {parameters}")
                
                # Make API call to get user profile
                result = invoke_api_tool(
                    url=api_url,
                    method=method,
                    body=parameters,
                    tool_context=tool_context
                )
                
                if result.get('status') == 'success':
                    # Success! Return response data
                    response_data = result.get('response', {})
                    # Store in tool context state and cache
                    if hasattr(tool_context, 'state') and tool_context.state is not None:
                        tool_context.state['user_profile'] = response_data
                    
                    print(f"✅ [DEBUG] Successfully retrieved user profile with endpoint: {endpoint.get('name')} (attempt {attempt})")
                    return response_data
                else:
                    # This endpoint failed, try next one
                    error_msg = result.get('error', 'Unknown error')
                    print(f"❌ [DEBUG] Attempt {attempt} failed: {error_msg}")
                    last_error = error_msg
                    continue
                    
            except Exception as e:
                print(f"❌ [DEBUG] Attempt {attempt} exception: {str(e)}")
                last_error = str(e)
                continue
        
        # All endpoints failed
        print(f"❌ [DEBUG] All {len(retry_candidates)} endpoint(s) failed")
        return {"error": f"Failed to get user profile after {len(retry_candidates)} attempts. Last error: {last_error}"}
            
    except Exception as e:
        # Return dict instead of JSON string to prevent agent confusion
        return {"error": f"Exception getting user profile: {str(e)}"}

def user_detail_agent_callback(callback_context: CallbackContext, llm_request) -> Optional[LlmResponse]:
    """
    Before model callback for user_detail_agent.
    
    This callback checks cache first and returns cached data if available.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request about to be sent to the model
        
    Returns:
        Optional[LlmResponse]: LlmResponse with cached JSON to skip model call, None to continue
    """
    
    print("🔍 [USER] Checking user profile cache...")
    
    try:
        # Get user email with proper fallback chain
        user_email = None
        if callback_context.state:
            # First priority: user_email from new state format
            user_email = callback_context.state.get('user_email')
            if not user_email:
                # Second priority: header_email from IAP headers (fallback)
                user_email = callback_context.state.get('header_email')
        
        # Final fallback to environment variable
        if not user_email:
            user_email = os.getenv('DEV_EMAIL', 'anushas@unops.org')
        
        print(f"📧 [USER] Using email: {user_email}")
        
        # Check cache for user profile
        from ai_assistant.utils.cache import get_entity_cache
        entity_cache = get_entity_cache()
        
        cached_profile = entity_cache.get_user_profile(user_email)
        if cached_profile:
            print(f"✅ [USER] Cache HIT: Returning cached user profile for {user_email}")
            
            # Set the result in state for other agents
            callback_context.state['user_profile'] = cached_profile
            
            # Return LlmResponse with JSON content to skip model execution
            import json
            json_response = json.dumps(cached_profile, indent=2)
            
            return LlmResponse(
                content=types.Content(
                    role="model",
                    parts=[types.Part(text=json_response)],
                )
            )
        
        print("🔄 [USER] Cache MISS: Proceeding with LLM execution to fetch fresh user profile")
        return None  # Continue with normal LLM execution
        
    except Exception as e:
        print(f"❌ [USER] Error in user_detail_agent callback: {e}")
        return None  # Continue with LLM execution on error 

def user_detail_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for user_detail_agent.
    
    This callback runs AFTER the model responds and caches the result with timestamp.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response or None to use original
    """
    
    print("💾 [USER] Caching user profile result with timestamp...")
    
    try:
        # Check if we have user profile data in state to cache
        user_profile = callback_context.state.get('user_profile')
        
        print(f"🔍 [USER-CACHE] State keys: {list(callback_context.state.keys())}")
        print(f"🔍 [USER-CACHE] user_profile in state: {user_profile is not None}")
        print(f"🔍 [USER-CACHE] user_profile type: {type(user_profile)}")
        
        if user_profile and isinstance(user_profile, dict):
            # Get user email using same logic as user_detail_agent_callback
            user_email = None
            if callback_context.state:
                # First priority: user_email from new state format
                user_email = callback_context.state.get('user_email')
                if not user_email:
                    # Second priority: header_email from IAP headers (fallback)
                    user_email = callback_context.state.get('header_email')
            
            # Final fallback to environment variable
            if not user_email:
                user_email = os.getenv('DEV_EMAIL', 'anushas@unops.org')
            
            print(f"📧 [USER-CACHE] Using email for caching: {user_email}")
            
            # Cache user profile and timestamp
            from ai_assistant.utils.cache import get_entity_cache
            entity_cache = get_entity_cache()
            entity_cache.set_user_profile(user_email, user_profile)
            
            print("✅ [USER] Cached user profile successfully")
        else:
            print("⚠️ [USER] No user_profile found in state or invalid format")
        
        return None  # Use original response
        
    except Exception as e:
        print(f"❌ [USER] Error caching user profile: {e}")
        return None  # Use original response            