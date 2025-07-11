"""
Agent Callbacks for Smart Cache-Based Execution Control

This module provides callback functions that intelligently gate agent execution
based on cache availability, dramatically improving performance.
"""

import json
import os
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
        from ..cache import get_entity_cache
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

def screen_context_agent_callback(callback_context: CallbackContext, llm_request) -> Optional[LlmResponse]:
    """
    Before model callback for screen_context_agent.
    
    This callback checks cache using entity+id combination and returns cached data if available.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request about to be sent to the model
        
    Returns:
        Optional[LlmResponse]: LlmResponse with cached JSON to skip model call, None to continue
    """
    
    print("🔍 [SCREEN] Checking screen context cache...")
    
    try:
        # Extract entity and ID from structured state data
        screen_url_obj = callback_context.state.get('screen_url', {})
        user_viewing_panel = callback_context.state.get('user_viewing_panel', {})
        
        # First priority: screen_url object
        entity = screen_url_obj.get('entity', '')
        entity_id = screen_url_obj.get('id', None)
        
        # Second priority: user_viewing_panel if screen_url is empty
        if not entity and user_viewing_panel:
            entity = user_viewing_panel.get('entity', '')
            entity_id = user_viewing_panel.get('entity_id', None)
            # Convert string ID to int if needed
            if entity_id and isinstance(entity_id, str) and entity_id.isdigit():
                entity_id = int(entity_id)
        
        # Handle empty entity case - no context needed, return empty response
        if not entity:
            print("📭 [SCREEN] No entity found - returning empty context")
            empty_context = {
                "screen_name": "Application",
                "screen_url": "",
                "screen_type": "app",
                "screen_data": {
                    "message": "No specific screen context available",
                    "context_available": False
                }
            }
            
            # Set in state and return LlmResponse
            callback_context.state['screen_context'] = empty_context
            
            import json
            json_response = json.dumps(empty_context, indent=2)
            
            return LlmResponse(
                content=types.Content(
                    role="model",
                    parts=[types.Part(text=json_response)],
                )
            )
        
        print(f"📋 [SCREEN] Looking for cache with entity: {entity}, id: {entity_id}")
        
        # Check cache using entity+id combination
        from ..cache import get_entity_cache
        entity_cache = get_entity_cache()
        
        cache_key = f"{entity}:{entity_id}" if entity_id else f"{entity}:list"
        cached_context = entity_cache.get_screen_context(cache_key)
        
        if cached_context:
            print(f"✅ [SCREEN] Cache HIT: Returning cached screen context for {cache_key}")
            
            # Set the result in state for other agents
            callback_context.state['screen_context'] = cached_context
            
            # Return LlmResponse with JSON content to skip model execution
            import json
            json_response = json.dumps(cached_context, indent=2)
            
            return LlmResponse(
                content=types.Content(
                    role="model",
                    parts=[types.Part(text=json_response)],
                )
            )
        
        print(f"🔄 [SCREEN] Cache MISS for {cache_key}: Proceeding with LLM execution to fetch fresh screen context")
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
            from ..cache import get_entity_cache
            entity_cache = get_entity_cache()
            entity_cache.set_user_profile(user_email, user_profile)
            
            print("✅ [USER] Cached user profile successfully")
        else:
            print("⚠️ [USER] No user_profile found in state or invalid format")
        
        return None  # Use original response
        
    except Exception as e:
        print(f"❌ [USER] Error caching user profile: {e}")
        return None  # Use original response</thinking>

def screen_context_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for screen_context_agent.
    
    This callback runs AFTER the model responds and caches the screen context result.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response or None to use original
    """
    
    print("💾 [SCREEN] Caching screen context result...")
    
    try:
        # Check if we have screen context data in state to cache
        screen_context = callback_context.state.get('screen_context')
        
        # Extract entity and ID from structured state data to build cache key
        screen_url_obj = callback_context.state.get('screen_url', {})
        user_viewing_panel = callback_context.state.get('user_viewing_panel', {})
        
        # First priority: screen_url object
        entity = screen_url_obj.get('entity', '')
        entity_id = screen_url_obj.get('id', None)
            
        # Second priority: user_viewing_panel if screen_url is empty
        if not entity and user_viewing_panel:
            entity = user_viewing_panel.get('entity', '')
            entity_id = user_viewing_panel.get('entity_id', None)
            # Convert string ID to int if needed
            if entity_id and isinstance(entity_id, str) and entity_id.isdigit():
                entity_id = int(entity_id)
        
        if screen_context and isinstance(screen_context, dict) and entity:
            # Cache screen context using entity+id key
            cache_key = f"{entity}:{entity_id}" if entity_id else f"{entity}:list"
            entity_cache.set_screen_context(cache_key, screen_context)
                
            print(f"✅ [SCREEN] Cached screen context successfully for {cache_key}")
        
        return None  # Use original response
        
    except Exception as e:
        print(f"❌ [SCREEN] Error caching screen context: {e}")
        return None  # Use original response

def api_success_callback(entity_type: str, operation: str, result: dict) -> None:
    """
    Callback to invalidate caches when API operations succeed.
    
    Args:
        entity_type: Type of entity that was modified (Partner, Contact, etc.)
        operation: Type of operation (create, update, delete)
        result: The API operation result
    """
    
    print(f"🔄 [API] Success callback: {entity_type} {operation}")
    
    try:
        # Invalidate screen cache for the modified entity type
        print(f"🔄 [API] Invalidating screen cache for entity type: {entity_type}")
        entity_cache.screen_cache.clear()
        
        # Use entity cache invalidation method
        entity_cache.invalidate_on_entity_change(entity_type, operation)
        
        # Log the invalidation for debugging
        print(f"✅ [API] Cache invalidation completed for {entity_type} {operation}")
        
    except Exception as e:
        print(f"❌ [API] Error in success callback: {e}")



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

def clean_json_response(text: str) -> str:
    """
    Clean a response text to extract valid JSON.
    
    This function removes:
    - Markdown code blocks (```json, ```)
    - Extra explanatory text before/after JSON
    - Whitespace and formatting issues
    
    Args:
        text: Raw text response from LLM
        
    Returns:
        str: Cleaned JSON text
    """
    
    # Remove markdown code blocks
    cleaned = text.strip()
    
    # Remove ```json and ``` markers
    if cleaned.startswith('```json'):
        cleaned = cleaned[7:]  # Remove ```json
    elif cleaned.startswith('```'):
        cleaned = cleaned[3:]   # Remove ```
    
    if cleaned.endswith('```'):
        cleaned = cleaned[:-3]  # Remove trailing ```
    
    # Find JSON array or object boundaries
    json_start = -1
    json_end = -1
    
    # Look for JSON array start
    for i, char in enumerate(cleaned):
        if char == '[':
            json_start = i
            break
        elif char == '{':
            json_start = i
            break
    
    # Look for JSON array/object end (find the matching closing bracket)
    if json_start != -1:
        bracket_count = 0
        start_char = cleaned[json_start]
        end_char = ']' if start_char == '[' else '}'
        
        for i in range(json_start, len(cleaned)):
            if cleaned[i] == start_char:
                bracket_count += 1
            elif cleaned[i] == end_char:
                bracket_count -= 1
                if bracket_count == 0:
                    json_end = i
                    break
    
    # Extract JSON portion
    if json_start != -1 and json_end != -1:
        cleaned = cleaned[json_start:json_end + 1]
    
    # Final cleanup
    cleaned = cleaned.strip()
    
    print(f"🧹 [CLEAN] Original length: {len(text)}, Cleaned length: {len(cleaned)}")
    print(f"🔍 [CLEAN] Cleaned preview: {cleaned[:100]}...")
    
    return cleaned

def user_request_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for user_request_agent.
    
    This callback cleans JSON responses and ensures proper formatting.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response with cleaned JSON or None to use original
    """
    
    print("🧹 [USER-REQUEST] Cleaning user request agent response...")
    
    try:
        # Get the raw text response
        if not llm_response.content or not llm_response.content.parts:
            print("⚠️ [USER-REQUEST] No content found in response")
            return None
        
        raw_text = llm_response.content.parts[0].text
        print(f"🔍 [USER-REQUEST] Raw response length: {len(raw_text)} characters")
        
        # Check if the response looks like JSON
        if not (raw_text.strip().startswith('{') or raw_text.strip().startswith('[')):
            print("ℹ️ [USER-REQUEST] Response doesn't appear to be JSON, skipping cleanup")
            return None
        
        # Clean the response by removing markdown code blocks and extra text
        cleaned_text = clean_json_response(raw_text)
        
        # Validate that it's proper JSON
        try:
            json.loads(cleaned_text)
            print("✅ [USER-REQUEST] JSON validation passed")
        except json.JSONDecodeError as e:
            print(f"❌ [USER-REQUEST] JSON validation failed: {e}")
            print(f"🔍 [USER-REQUEST] Problematic text: {cleaned_text[:200]}...")
            return None  # Use original response if cleaning failed
        
        # Return modified response with cleaned content
        return LlmResponse(
            content=types.Content(
                role="model",
                parts=[types.Part(text=cleaned_text)],
            )
        )
        
    except Exception as e:
        print(f"❌ [USER-REQUEST] Error cleaning response: {e}")
        return None  # Use original response on error

def response_formatter_after_model_callback(callback_context: CallbackContext, llm_response: LlmResponse) -> Optional[LlmResponse]:
    """
    After model callback for response_formatter_agent.
    
    This callback cleans JSON responses and ensures proper formatting without markdown blocks.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_response: The actual response from the LLM model
        
    Returns:
        Optional[LlmResponse]: Modified response with cleaned JSON or None to use original
    """
    
    print("🧹 [RESPONSE-FORMATTER] Cleaning response formatter agent response...")
    
    try:
        # Get the raw text response
        if not llm_response.content or not llm_response.content.parts:
            print("⚠️ [RESPONSE-FORMATTER] No content found in response")
            return None
        
        raw_text = llm_response.content.parts[0].text
        print(f"🔍 [RESPONSE-FORMATTER] Raw response length: {len(raw_text)} characters")
        
        # Clean the response by removing markdown code blocks and extra text
        cleaned_text = clean_json_response(raw_text)
        
        # Validate that it's proper JSON
        try:
            parsed_json = json.loads(cleaned_text)
            print("✅ [RESPONSE-FORMATTER] JSON validation passed")
        except json.JSONDecodeError as e:
            print(f"❌ [RESPONSE-FORMATTER] JSON validation failed: {e}")
            print(f"🔍 [RESPONSE-FORMATTER] Problematic text: {cleaned_text[:200]}...")
            return None  # Use original response if cleaning failed
        
        # Store the cleaned result in state for other agents
        callback_context.state['formatted_response'] = cleaned_text
        
        # Return modified response with cleaned content
        return LlmResponse(
            content=types.Content(
                role="model",
                parts=[types.Part(text=cleaned_text)],
            )
        )
        
    except Exception as e:
        print(f"❌ [RESPONSE-FORMATTER] Error cleaning response: {e}")
        return None  # Use original response on error 