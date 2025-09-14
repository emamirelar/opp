"""
Cache Management Utilities

This module contains cache management functions for the AI assistant
moved from agent.py for better organization.
"""

import logging
from ai_assistant.utils.cache import entity_cache, auto_cleanup
from ai_assistant.utils.common_callbacks import (
    get_cache_performance_stats,
    force_cache_refresh,
    api_success_callback
)

def get_cache_stats() -> str:
    """Get detailed cache performance statistics"""
    try:
        auto_cleanup()
        stats = get_cache_performance_stats()
        if stats.get('error'):
            return f"❌ Error getting cache stats: {stats['error']}"
        
        user_cache = stats['user_profile_cache']
        screen_cache = stats['screen_context_cache']
        performance = stats['performance_insights']
        ttl_config = stats['ttl_config']
        
        formatted_stats = f"""📊 **Cache Performance Statistics**

**User Profile Cache:**
- Valid entries: {user_cache['valid_entries']}
- Expired entries: {user_cache['expired_entries']}
- Memory usage: {user_cache['memory_usage_estimate']}

**Screen Context Cache:**
- Valid entries: {screen_cache['valid_entries']}
- Expired entries: {screen_cache['expired_entries']}
- Memory usage: {screen_cache['memory_usage_estimate']}

**Performance Impact:**
- Expected response time: {performance['performance_impact']['expected_response_time']}
- Cache effectiveness: {performance['cache_effectiveness']}

**TTL Configuration:**
- User profile: {ttl_config['user_profile_ttl_hours']} hours
- Screen context: {ttl_config['screen_context_ttl_hours']} hours

**Current Status:**
- Both caches ready: {'✅' if performance['cache_effectiveness']['both_caches_ready'] else '❌'}
- Last URL: {stats.get('last_url', 'None')}
"""
        return formatted_stats
    except Exception as e:
        return f"❌ Error getting cache stats: {e}"

def clear_cache(cache_type: str = "all") -> str:
    """Clear cache based on type (all, user, or screen)"""
    try:
        if cache_type.lower() == "all":
            entity_cache.clear_all_caches()
            return "✅ All caches cleared successfully"
        elif cache_type.lower() == "user":
            entity_cache.user_cache.clear()
            return "✅ User profile cache cleared successfully"
        elif cache_type.lower() == "screen":
            entity_cache.screen_cache.clear()
            return "✅ Screen context cache cleared successfully"
        else:
            return f"❌ Invalid cache type '{cache_type}'. Use 'all', 'user', or 'screen'."
    except Exception as e:
        return f"❌ Error clearing cache: {e}"

def refresh_cache(target: str = "all") -> str:
    """Force refresh of cache based on target"""
    try:
        if target.lower() == "all":
            result = force_cache_refresh()
        elif target.lower() == "user":
            result = force_cache_refresh(email=None, screen_url=None)
        elif target.lower() == "screen":
            result = force_cache_refresh(email=None, screen_url="")
        else:
            return f"❌ Invalid target '{target}'. Use 'all', 'user', or 'screen'."
        
        if result.get('error'):
            return f"❌ Error refreshing cache: {result['error']}"
        
        refreshed = result.get('refreshed', [])
        return f"✅ Cache refresh completed: {', '.join(refreshed)}"
    except Exception as e:
        return f"❌ Error refreshing cache: {e}"

def notify_api_success(entity_type: str, operation: str, result: dict) -> None:
    """Cache management hook for API operations"""
    try:
        api_success_callback(entity_type, operation, result)
    except Exception as e:
        logging.error(f"Error notifying cache system: {e}") 