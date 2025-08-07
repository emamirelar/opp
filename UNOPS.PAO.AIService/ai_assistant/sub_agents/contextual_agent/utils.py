"""
Contextual Agent Utilities

This module contains utility functions for contextual coordination and management.
"""

def log_contextual_start():
    """Log when contextual agent starts"""
    print("🔍 [CONTEXTUAL] Starting parallel context gathering...")

def log_contextual_complete():
    """Log when contextual agent completes"""
    print("✅ [CONTEXTUAL] Context gathering complete")

def validate_contextual_output(output: dict) -> bool:
    """
    Validate that contextual output has required structure
    
    Args:
        output: The contextual output to validate
        
    Returns:
        bool: True if valid, False otherwise
    """
    try:
        # Check for basic structure
        if not isinstance(output, dict):
            return False
            
        # Could add validation for user_profile and screen_context keys
        return True
        
    except Exception as e:
        print(f"❌ [CONTEXTUAL] Error validating output: {e}")
        return False

def format_contextual_error(error: str) -> dict:
    """
    Format contextual errors consistently
    
    Args:
        error: Error message
        
    Returns:
        dict: Formatted error response
    """
    return {
        "error": True,
        "message": error,
        "contextual_stage": "error",
        "timestamp": None
    }