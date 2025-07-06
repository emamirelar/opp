import time
import threading
from typing import Dict, Any, Optional, Tuple
import os

class SmartCache:
    """
    Thread-safe in-memory cache with TTL (Time To Live) support.
    Much faster than session-based caching.
    """
    
    def __init__(self, cache_type: str = "", ttl_seconds: int = 300):
        self._cache: Dict[str, Tuple[Any, float]] = {}  # key -> (value, expiry_time)
        self._lock = threading.RLock()
        self.cache_type = cache_type
        self.ttl_seconds = ttl_seconds
        
    def get(self, key: str) -> Optional[Any]:
        """Get value from cache if not expired."""
        with self._lock:
            if key in self._cache:
                value, expiry_time = self._cache[key]
                if time.time() < expiry_time:
                    return value
                else:
                    # Expired, remove it
                    del self._cache[key]
            return None
    
    def set(self, key: str, value: Any, ttl_seconds: int = None) -> bool:
        """Set value in cache with TTL."""
        if ttl_seconds is not None:
            expiry_time = time.time() + ttl_seconds
        else:
            expiry_time = time.time() + self.ttl_seconds
        with self._lock:
            self._cache[key] = (value, expiry_time)
        return True
    
    def delete(self, key: str) -> bool:
        """Delete specific key from cache."""
        with self._lock:
            return self._cache.pop(key, None) is not None
    
    def clear(self) -> None:
        """Clear all cache entries."""
        with self._lock:
            self._cache.clear()
    
    def cleanup_expired(self) -> int:
        """Remove expired entries. Returns number of entries removed."""
        current_time = time.time()
        expired_keys = []
        
        with self._lock:
            for key, (_, expiry_time) in self._cache.items():
                if current_time >= expiry_time:
                    expired_keys.append(key)
            
            for key in expired_keys:
                del self._cache[key]
        
        return len(expired_keys)
    
    def cache_info(self) -> Dict[str, Any]:
        """Get cache statistics."""
        with self._lock:
            total_entries = len(self._cache)
            current_time = time.time()
            valid_entries = sum(1 for _, expiry in self._cache.values() if current_time < expiry)
            expired_entries = total_entries - valid_entries
            
            return {
                "total_entries": total_entries,
                "valid_entries": valid_entries,
                "expired_entries": expired_entries,
                "memory_usage_estimate": f"{len(str(self._cache)) / 1024:.2f} KB"
            }

class EntityAwareCache:
    """
    Advanced cache system with entity awareness and automatic invalidation.
    
    Features:
    - Thread-safe operations
    - TTL-based expiration
    - Entity-specific cache invalidation
    - URL change detection
    - Memory usage tracking
    """
    
    def __init__(self):
        # Initialize with default values, load config lazily
        self.user_cache = SmartCache("user_profile", ttl_seconds=3600)  # Default 1 hour
        self.screen_cache = SmartCache("screen_context", ttl_seconds=7200)  # Default 2 hours
        self.general_cache = SmartCache("general", ttl_seconds=1800)  # Default 30 minutes
        self.current_screen_url = None
        
        # Default cache rules (will be overridden when config is loaded)
        self.entity_change_rules = {
            'Partner': ['screen_context'],
            'Contact': ['screen_context'],
            'Interaction': ['screen_context'],
            'Opportunity': ['screen_context'],
            'User': ['user_profile'],
            'UserData': ['user_profile'],
            'UserPreferences': ['user_profile']
        }
        
        self.last_entity_operations = {}  # Track entity changes
        self.last_url = None
        self._lock = threading.RLock()
        
        # Cache TTL configurations (defaults, will be updated when config loads)
        self.USER_PROFILE_TTL = 3600
        self.SCREEN_CONTEXT_TTL = 7200
        self.GENERAL_TTL = 1800
        
        # Flag to track if config has been loaded
        self._config_loaded = False
    
    def _ensure_config_loaded(self):
        """Ensure configuration is loaded (lazy loading)"""
        if not self._config_loaded:
            try:
                from framework_config import get_config
                framework_cache = get_config().get("cache", {})
                ttl_config = framework_cache.get("ttl", {})
                
                # Update TTL values
                self.USER_PROFILE_TTL = ttl_config.get("user_profile", 3600)
                self.SCREEN_CONTEXT_TTL = ttl_config.get("screen_context", 7200)
                self.GENERAL_TTL = ttl_config.get("general", 1800)
                
                # Recreate caches with correct TTL
                self.user_cache = SmartCache("user_profile", ttl_seconds=self.USER_PROFILE_TTL)
                self.screen_cache = SmartCache("screen_context", ttl_seconds=self.SCREEN_CONTEXT_TTL)
                self.general_cache = SmartCache("general", ttl_seconds=self.GENERAL_TTL)
                
                # Load cache rules from configuration
                self.entity_change_rules = framework_cache.get("rules", self.entity_change_rules)
                print(f"✅ Loaded cache rules from configuration: {list(self.entity_change_rules.keys())}")
                
                self._config_loaded = True
                
            except Exception as e:
                print(f"⚠️ Failed to load cache config, using defaults: {e}")
                # Keep using defaults
    
    def get_user_profile(self, email: str) -> Optional[dict]:
        """Get user profile from cache."""
        self._ensure_config_loaded() # Ensure config is loaded
        result = self.user_cache.get(f"user:{email}")
        if result:
            print(f"✅ Cache HIT: User profile for {email}")
        return result
    
    def set_user_profile(self, email: str, data: dict) -> None:
        """Set user profile in cache with 1 hour TTL."""
        self._ensure_config_loaded() # Ensure config is loaded
        self.user_cache.set(f"user:{email}", data, self.USER_PROFILE_TTL)
        print(f"💾 Cached user profile for {email} (TTL: {self.USER_PROFILE_TTL}s)")
    
    def get_screen_context(self, url: str) -> Optional[dict]:
        """Get screen context from cache."""
        self._ensure_config_loaded() # Ensure config is loaded
        normalized_url = self._normalize_url(url)
        result = self.screen_cache.get(f"screen:{normalized_url}")
        if result:
            print(f"✅ Cache HIT: Screen context for '{normalized_url}'")
        return result
    
    def set_screen_context(self, url: str, data: dict) -> None:
        """Set screen context in cache with 2 hour TTL."""
        self._ensure_config_loaded() # Ensure config is loaded
        normalized_url = self._normalize_url(url)
        self.screen_cache.set(f"screen:{normalized_url}", data, self.SCREEN_CONTEXT_TTL)
        print(f"💾 Cached screen context for '{normalized_url}' (TTL: {self.SCREEN_CONTEXT_TTL}s)")
    
    def invalidate_on_entity_change(self, entity_type: str, operation: str = 'update') -> None:
        """
        Invalidate caches when entities change.
        
        Args:
            entity_type: Type of entity (Partner, Contact, User, etc.)
            operation: Type of operation (create, update, delete)
        """
        self._ensure_config_loaded() # Ensure config is loaded
        with self._lock:
            print(f"🧹 Cache invalidation triggered: {entity_type} {operation}")
            
            caches_to_clear = self.entity_change_rules.get(entity_type, [])
            cleared_caches = []
            
            for cache_type in caches_to_clear:
                if cache_type == 'user_profile':
                    self.user_cache.clear()
                    cleared_caches.append('user_profile')
                elif cache_type == 'screen_context':
                    self.screen_cache.clear()
                    cleared_caches.append('screen_context')
            
            # Track this operation
            self.last_entity_operations[entity_type] = {
                'operation': operation,
                'timestamp': time.time()
            }
            
            print(f"✅ Cleared caches: {cleared_caches}")
    
    def invalidate_on_url_change(self, new_url: str) -> bool:
        """
        Check if URL changed and invalidate relevant cache.
        
        Returns:
            bool: True if URL changed, False if same
        """
        self._ensure_config_loaded() # Ensure config is loaded
        normalized_url = self._normalize_url(new_url)
        
        with self._lock:
            if self.last_url != normalized_url:
                print(f"🔄 URL changed: '{self.last_url}' → '{normalized_url}'")
                self.last_url = normalized_url
                return True
            return False
    
    def check_both_caches_valid(self, email: str, screen_url: str) -> tuple[bool, Optional[dict], Optional[dict]]:
        """
        Check if both user profile and screen context caches are valid for the given parameters.
        
        Args:
            email: User email to check
            screen_url: Screen URL to check
            
        Returns:
            tuple: (both_valid, user_data, screen_data)
        """
        self._ensure_config_loaded() # Ensure config is loaded
        user_data = self.get_user_profile(email)
        screen_data = self.get_screen_context(screen_url)
        
        both_valid = user_data is not None and screen_data is not None
        
        if both_valid:
            print(f"✅ BOTH caches valid for {email} + '{screen_url}'")
        else:
            missing = []
            if not user_data:
                missing.append('user_profile')
            if not screen_data:
                missing.append('screen_context')
            print(f"🔄 Cache status: Missing {', '.join(missing)}")
        
        return both_valid, user_data, screen_data
    
    def _normalize_url(self, url: str) -> str:
        """Normalize URL for consistent caching."""
        if not url:
            return ""
        return url.strip().lower()
    
    def get_cache_stats(self) -> Dict[str, Any]:
        """Get comprehensive cache statistics."""
        self._ensure_config_loaded() # Ensure config is loaded
        return {
            "user_profile_cache": self.user_cache.cache_info(),
            "screen_context_cache": self.screen_cache.cache_info(),
            "general_cache": self.general_cache.cache_info(),
            "last_entity_operations": self.last_entity_operations,
            "last_url": self.last_url,
            "ttl_config": {
                "user_profile_ttl_hours": self.USER_PROFILE_TTL / 3600,
                "screen_context_ttl_hours": self.SCREEN_CONTEXT_TTL / 3600,
                "general_ttl_minutes": self.GENERAL_TTL / 60
            }
        }
    
    def clear_all_caches(self) -> None:
        """Clear all caches."""
        self._ensure_config_loaded() # Ensure config is loaded
        with self._lock:
            self.user_cache.clear()
            self.screen_cache.clear() 
            self.general_cache.clear()
            self.last_entity_operations.clear()
            self.last_url = None
            print("🧹 All caches cleared")
    
    def cleanup_expired_entries(self) -> Dict[str, int]:
        """Clean up expired entries from all caches."""
        self._ensure_config_loaded() # Ensure config is loaded
        return {
            "user_profile_expired": self.user_cache.cleanup_expired(),
            "screen_context_expired": self.screen_cache.cleanup_expired(),
            "general_expired": self.general_cache.cleanup_expired()
        }

    def set_cache(self, key: str, value: Any, ttl_seconds: int = None) -> bool:
        """
        Set a value in the general cache.
        
        Args:
            key: Cache key
            value: Value to cache
            ttl_seconds: Optional TTL override
            
        Returns:
            bool: True if successful
        """
        self._ensure_config_loaded() # Ensure config is loaded
        try:
            if ttl_seconds:
                # Create temporary cache with custom TTL
                temp_cache = SmartCache(f"temp_{key}", ttl_seconds=ttl_seconds)
                return temp_cache.set(key, value)
            else:
                return self.general_cache.set(key, value)
        except Exception as e:
            print(f"❌ Error setting cache {key}: {e}")
            return False
    
    def get_cache(self, key: str) -> Any:
        """
        Get a value from the general cache.
        
        Args:
            key: Cache key
            
        Returns:
            Any: Cached value or None if not found/expired
        """
        self._ensure_config_loaded() # Ensure config is loaded
        try:
            return self.general_cache.get(key)
        except Exception as e:
            print(f"❌ Error getting cache {key}: {e}")
            return None
    
    def delete_cache(self, key: str) -> bool:
        """
        Delete a value from the general cache.
        
        Args:
            key: Cache key
            
        Returns:
            bool: True if successful
        """
        self._ensure_config_loaded() # Ensure config is loaded
        try:
            return self.general_cache.delete(key)
        except Exception as e:
            print(f"❌ Error deleting cache {key}: {e}")
            return False

# Global cache instance (initialized lazily)
entity_cache = None

def get_entity_cache():
    """Get the global entity cache instance (lazy initialization)"""
    global entity_cache
    if entity_cache is None:
        entity_cache = EntityAwareCache()
    return entity_cache

def get_user_email() -> str:
    """Get user email from environment or default."""
    return os.getenv('DEV_EMAIL', 'anushas@unops.org')

# Auto-cleanup function
def auto_cleanup():
    """Automatically cleanup expired entries."""
    global entity_cache
    if entity_cache is None:
        entity_cache = get_entity_cache()
    expired = entity_cache.cleanup_expired_entries()
    total_expired = sum(expired.values())
    if total_expired > 0:
        print(f"🧹 Auto-cleanup removed {total_expired} expired cache entries")
    return expired 