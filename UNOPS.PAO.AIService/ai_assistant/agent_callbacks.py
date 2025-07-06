"""
Agent Callbacks for Smart Cache-Based Execution Control

This module provides callback functions that intelligently gate agent execution
based on cache availability, dramatically improving performance.
"""

import time
from typing import Optional, Dict, Any
from google.adk.agents.callback_context import CallbackContext
from google.adk.models.llm_response import LlmResponse
from google.genai import types
from .cache import entity_cache, get_user_email

def create_mock_llm_response(content: str) -> LlmResponse:
    """
    Create a mock LLM response for cache scenarios.
    
    Args:
        content: The text content for the response
        
    Returns:
        LlmResponse: Mock response that mimics real LLM output
    """
    # Create a mock content part
    text_part = types.Part(text=content)
    
    # Create mock content with the text part
    mock_content = types.Content(
        parts=[text_part],
        role="model"
    )
    
    # Create and return the mock response
    return LlmResponse(
        content=mock_content,
        usage_metadata=None,
        model_version=""
    )

def parse_url_components(url: str) -> Dict[str, Any]:
    """
    Generic URL parsing function to extract entity, id, query params, and remaining parts.
    
    Args:
        url: URL string to parse (e.g., "/partners/123?param=value" or "/partners/1/contacts")
        
    Returns:
        Dict containing parsed components:
        - entity: First non-numeric path segment
        - id: First numeric path segment after entity
        - query_params: Query parameters as dict
        - remaining_url_string: Remaining path parts after entity/id
        - time_captured: Current timestamp
    """
    
    if not url or not url.strip():
        return {
            "entity": None,
            "id": None,
            "query_params": {},
            "remaining_url_string": "",
            "time_captured": time.time()
        }
    
    # Clean and normalize URL
    cleaned_url = url.strip()
    
    # Split URL and query params
    if '?' in cleaned_url:
        path_part, query_part = cleaned_url.split('?', 1)
        # Parse query params
        query_params = {}
        if query_part:
            for param in query_part.split('&'):
                if '=' in param:
                    key, value = param.split('=', 1)
                    query_params[key] = value
    else:
        path_part = cleaned_url
        query_params = {}
    
    # Split path into segments
    path_segments = [segment for segment in path_part.split('/') if segment]
    
    entity = None
    id = None
    remaining_parts = []
    
    # Find entity (first non-numeric segment)
    entity_found = False
    for i, segment in enumerate(path_segments):
        if not segment.isdigit() and not entity_found:
            entity = segment
            entity_found = True
        elif entity_found and segment.isdigit() and id is None:
            id = int(segment)
        elif entity_found and (id is not None or not segment.isdigit()):
            remaining_parts.append(segment)
    
    # Join remaining parts
    remaining_url_string = '/'.join(remaining_parts) if remaining_parts else ""
    
    return {
        "entity": entity,
        "id": id,
        "query_params": query_params,
        "remaining_url_string": remaining_url_string,
        "time_captured": time.time()
    }

def contextual_agent_gate_callback(callback_context: CallbackContext) -> Optional[str]:
    """
    Before agent callback for contextual_agent (ParallelAgent).
    
    This callback:
    1. Parses screen_url to create current_screen_context
    2. Checks cache validity for both user profile and screen context
    3. Decides whether to call agents based on cache validity
    4. Injects cached data if available
    
    Args:
        callback_context: The callback context from Google ADK
        
    Returns:
        Optional[str]: Always returns None to continue normal execution
    """
    
    print("🚪 [GATE] Starting contextual_agent gate callback...")
    
    try:
        # Get user email and current screen URL from state
        user_email = get_user_email()
        current_screen_url = callback_context.state.get('screen_url', '')
        
        print(f"🔍 [GATE] Current state: user={user_email}, screen_url='{current_screen_url}'")
        
        # Parse current screen URL to create current_screen_context
        current_screen_context = parse_url_components(current_screen_url)
        
        print(f"📋 [GATE] Parsed screen context: entity={current_screen_context['entity']}, id={current_screen_context['id']}, remaining={current_screen_context['remaining_url_string']}")
        
        # Store current_screen_context in cache
        entity_cache.set_cache('current_screen_context', current_screen_context)
        
        # Get previous screen context from cache for comparison
        previous_screen_context = entity_cache.get_cache('previous_screen_context')
        
        # Check user profile cache with time tracking
        cached_user_data = entity_cache.get_user_profile(user_email)
        user_profile_time = entity_cache.get_cache('user_profile_time')
        current_time = time.time()
        
        # Determine if user profile cache is valid
        user_cache_valid = (cached_user_data is not None and 
                           user_profile_time is not None and
                           (current_time - user_profile_time) < 3600)  # 1 hour TTL
        
        # Determine if screen context cache is valid
        screen_cache_valid = False
        cached_screen_data = None
        
        if current_screen_context['entity'] is not None:
            # Check if screen context changed
            if previous_screen_context:
                screen_contexts_match = (
                    previous_screen_context.get('entity') == current_screen_context['entity'] and
                    previous_screen_context.get('id') == current_screen_context['id'] and
                    previous_screen_context.get('remaining_url_string') == current_screen_context['remaining_url_string']
                )
                
                if screen_contexts_match:
                    cached_screen_data = entity_cache.get_screen_context(current_screen_url)
                    screen_cache_valid = cached_screen_data is not None
        
        # Log cache status
        if user_cache_valid:
            print(f"✅ [GATE] User profile cache VALID for {user_email}")
        else:
            print(f"❌ [GATE] User profile cache INVALID for {user_email}")
            
        if screen_cache_valid:
            print(f"✅ [GATE] Screen context cache VALID for '{current_screen_url}'")
        else:
            print(f"❌ [GATE] Screen context cache INVALID for '{current_screen_url}'")
        
        # Inject valid cached data into state for agents to use
        if user_cache_valid:
            callback_context.state['cached_user_profile'] = cached_user_data
            print("💾 [GATE] Injected cached user profile into state")
        
        if screen_cache_valid:
            callback_context.state['cached_screen_context'] = cached_screen_data
            print("💾 [GATE] Injected cached screen context into state")
        
        # Set flags for individual agent callbacks
        callback_context.state['user_cache_available'] = user_cache_valid
        callback_context.state['screen_cache_available'] = screen_cache_valid
        callback_context.state['should_call_user_agent'] = not user_cache_valid
        callback_context.state['should_call_screen_agent'] = not screen_cache_valid
        
        if user_cache_valid and screen_cache_valid:
            print("🚀 [GATE] Both caches valid - agents will use cached data")
        elif user_cache_valid:
            print("🔄 [GATE] Only user cache valid - screen agent will fetch fresh data")
        elif screen_cache_valid:
            print("🔄 [GATE] Only screen cache valid - user agent will fetch fresh data")
        else:
            print("🔄 [GATE] No valid caches - both agents will fetch fresh data")
        
        # Always return None to continue with agent execution
        # Individual agents will check state and skip LLM execution if cache available
        return None
        
    except Exception as e:
        print(f"❌ [GATE] Error in contextual_agent gate: {e}")
        callback_context.state['user_cache_available'] = False
        callback_context.state['screen_cache_available'] = False
        callback_context.state['should_call_user_agent'] = True
        callback_context.state['should_call_screen_agent'] = True
        return None  # Continue with agent execution on error

def user_detail_agent_callback(callback_context: CallbackContext, llm_request) -> Optional[LlmResponse]:
    """
    Before model callback for user_detail_agent.
    
    This callback checks if the gate callback injected cached user profile data.
    If yes, skip LLM execution entirely and return cached data.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request about to be sent to the model
        
    Returns:
        Optional[LlmResponse]: Mock response to skip model call, None to continue
    """
    
    print("🔍 [USER] Checking if user profile cache was injected by gate...")
    
    try:
        # Check if gate callback decided this agent should be called
        should_call_user_agent = callback_context.state.get('should_call_user_agent', True)
        
        if not should_call_user_agent:
            print("🚀 [USER] SKIPPING LLM execution - using cached user profile!")
            
            # Get cached data injected by gate
            cached_user_profile = callback_context.state.get('cached_user_profile')
            
            # Set the final result in state for the agent output
            callback_context.state['user_profile'] = cached_user_profile
            
            # Create mock response to skip LLM model execution entirely
            mock_response = create_mock_llm_response("User profile retrieved from cache")
            
            return mock_response
        
        print("🔄 [USER] Proceeding with LLM execution to fetch fresh user profile")
        return None  # Continue with normal LLM execution
        
    except Exception as e:
        print(f"❌ [USER] Error in user_detail_agent callback: {e}")
        return None  # Continue with LLM execution on error

def screen_context_agent_callback(callback_context: CallbackContext, llm_request) -> Optional[LlmResponse]:
    """
    Before model callback for screen_context_agent.
    
    This callback checks if the gate callback injected cached screen context data.
    If yes, skip LLM execution entirely and return cached data.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request about to be sent to the model
        
    Returns:
        Optional[LlmResponse]: Mock response to skip model call, None to continue
    """
    
    print("🔍 [SCREEN] Checking if screen context cache was injected by gate...")
    
    try:
        # Check if gate callback decided this agent should be called
        should_call_screen_agent = callback_context.state.get('should_call_screen_agent', True)
        
        if not should_call_screen_agent:
            print("🚀 [SCREEN] SKIPPING LLM execution - using cached screen context!")
            
            # Get cached data injected by gate
            cached_screen_context = callback_context.state.get('cached_screen_context')
            current_screen_url = callback_context.state.get('screen_url', '')
            
            # Set the final result in state for the agent output
            callback_context.state['screen_context'] = cached_screen_context
            
            # Create mock response to skip LLM model execution entirely
            mock_response = create_mock_llm_response("Screen context retrieved from cache")
            
            return mock_response
        
        print("🔄 [SCREEN] Proceeding with LLM execution to fetch fresh screen context")
        return None  # Continue with normal LLM execution
        
    except Exception as e:
        print(f"❌ [SCREEN] Error in screen_context_agent callback: {e}")
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
        
        if user_profile and isinstance(user_profile, dict):
            user_email = get_user_email()
            current_time = time.time()
            
            # Cache user profile and timestamp
            entity_cache.set_user_profile(user_email, user_profile)
            entity_cache.set_cache('user_profile_time', current_time)
            
            print("✅ [USER] Cached user profile with timestamp successfully")
        
        return None  # Use original response
        
    except Exception as e:
        print(f"❌ [USER] Error caching user profile: {e}")
        return None  # Use original response

def screen_context_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for screen_context_agent.
    
    This callback runs AFTER the model responds and:
    1. Caches the screen context result
    2. Stores current_screen_context as previous_screen_context
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response or None to use original
    """
    
    print("💾 [SCREEN] Caching screen context result and updating previous context...")
    
    try:
        # Check if we have screen context data in state to cache
        screen_context = callback_context.state.get('screen_context')
        screen_url = callback_context.state.get('screen_url', '')
        
        if screen_context and isinstance(screen_context, dict) and screen_url:
            # Cache screen context
            entity_cache.set_screen_context(screen_url, screen_context)
            
            # Get current_screen_context and store as previous_screen_context
            current_screen_context = entity_cache.get_cache('current_screen_context')
            if current_screen_context:
                # Create exact copy with current time
                previous_screen_context = current_screen_context.copy()
                previous_screen_context['time_captured'] = time.time()
                
                entity_cache.set_cache('previous_screen_context', previous_screen_context)
                
                print("✅ [SCREEN] Cached screen context and updated previous context successfully")
            else:
                print("✅ [SCREEN] Cached screen context successfully")
        
        return None  # Use original response
        
    except Exception as e:
        print(f"❌ [SCREEN] Error caching screen context: {e}")
        return None  # Use original response

def api_success_callback(entity_type: str, operation: str, result: dict) -> None:
    """
    Callback to invalidate caches when API operations succeed.
    
    Uses current_screen_context to determine which caches to invalidate.
    
    Args:
        entity_type: Type of entity that was modified (Partner, Contact, etc.)
        operation: Type of operation (create, update, delete)
        result: The API operation result
    """
    
    print(f"🔄 [API] Success callback: {entity_type} {operation}")
    
    try:
        # Get current screen context to determine what to invalidate
        current_screen_context = entity_cache.get_cache('current_screen_context')
        
        if current_screen_context and current_screen_context.get('entity'):
            current_entity = current_screen_context['entity']
            
            # If the modified entity matches the current screen entity, invalidate screen cache
            if current_entity.lower() == entity_type.lower():
                print(f"🔄 [API] Invalidating screen cache - entity {entity_type} matches current screen {current_entity}")
                entity_cache.screen_cache.clear()
                
                # Also clear the screen context tracking
                entity_cache.set_cache('current_screen_context', None)
                entity_cache.set_cache('previous_screen_context', None)
        
        # Always invalidate based on entity type for other screens
        entity_cache.invalidate_on_entity_change(entity_type, operation)
        
        # Log the invalidation for debugging
        print(f"✅ [API] Cache invalidation completed for {entity_type} {operation}")
        
    except Exception as e:
        print(f"❌ [API] Error in success callback: {e}")

def url_change_callback(callback_context: CallbackContext, new_url: str) -> None:
    """
    Callback to handle URL changes.
    
    Args:
        callback_context: The callback context from Google ADK
        new_url: The new URL being navigated to
    """
    
    print(f"🔄 [URL] URL change callback: '{new_url}'")
    
    try:
        # Parse new URL to update current_screen_context
        new_screen_context = parse_url_components(new_url)
        
        # Get previous context for comparison
        previous_screen_context = entity_cache.get_cache('current_screen_context')
        
        # Check if screen context actually changed
        if previous_screen_context:
            contexts_match = (
                previous_screen_context.get('entity') == new_screen_context['entity'] and
                previous_screen_context.get('id') == new_screen_context['id'] and
                previous_screen_context.get('remaining_url_string') == new_screen_context['remaining_url_string']
            )
            
            if not contexts_match:
                print(f"✅ [URL] Screen context changed: {previous_screen_context.get('entity')} -> {new_screen_context['entity']}")
                
                # Update current screen context
                entity_cache.set_cache('current_screen_context', new_screen_context)
                
                # Clear screen cache for new context
                entity_cache.invalidate_on_url_change(new_url)
            else:
                print(f"ℹ️ [URL] Screen context unchanged")
        else:
            # No previous context, store new one
            entity_cache.set_cache('current_screen_context', new_screen_context)
        
    except Exception as e:
        print(f"❌ [URL] Error in URL change callback: {e}")

def get_cache_performance_stats() -> dict:
    """
    Get comprehensive cache performance statistics.
    
    Returns:
        dict: Cache performance metrics
    """
    
    try:
        stats = entity_cache.get_cache_stats()
        
        # Add performance insights
        user_cache_info = stats['user_profile_cache']
        screen_cache_info = stats['screen_context_cache']
        
        performance_insights = {
            "cache_effectiveness": {
                "user_profile_hit_potential": user_cache_info['valid_entries'] > 0,
                "screen_context_hit_potential": screen_cache_info['valid_entries'] > 0,
                "both_caches_ready": (user_cache_info['valid_entries'] > 0 and 
                                    screen_cache_info['valid_entries'] > 0)
            },
            "performance_impact": {
                "expected_response_time": "1-3 seconds" if (user_cache_info['valid_entries'] > 0 and 
                                                          screen_cache_info['valid_entries'] > 0) else "8-15 seconds",
                "cache_coverage": f"{((user_cache_info['valid_entries'] + screen_cache_info['valid_entries']) / 2):.1f}% ready"
            }
        }
        
        stats['performance_insights'] = performance_insights
        return stats
        
    except Exception as e:
        print(f"❌ Error getting cache stats: {e}")
        return {"error": str(e)}

# Utility functions for testing and debugging
def force_cache_refresh(email: str = None, screen_url: str = None) -> dict:
    """
    Force refresh of specific caches (for testing/debugging).
    
    Args:
        email: User email to refresh (None for current user)
        screen_url: Screen URL to refresh (None for all screen contexts)
        
    Returns:
        dict: Refresh results
    """
    
    results = {"refreshed": []}
    
    try:
        if email is None:
            email = get_user_email()
        
        # Clear user profile cache
        entity_cache.user_cache.delete(f"user:{email}")
        results["refreshed"].append(f"user_profile:{email}")
        
        if screen_url:
            # Clear specific screen context
            normalized_url = entity_cache._normalize_url(screen_url)
            entity_cache.screen_cache.delete(f"screen:{normalized_url}")
            results["refreshed"].append(f"screen_context:{normalized_url}")
        else:
            # Clear all screen contexts
            entity_cache.screen_cache.clear()
            results["refreshed"].append("all_screen_contexts")
        
        print(f"🔄 Force refreshed: {results['refreshed']}")
        return results
        
    except Exception as e:
        print(f"❌ Error forcing cache refresh: {e}")
        return {"error": str(e)} 