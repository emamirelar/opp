"""
Workflow Agent Utilities

This module contains utility functions for workflow coordination and management.
"""

def log_workflow_start():
    """Log when workflow agent starts"""
    print("🔄 [WORKFLOW] Starting sequential workflow processing...")

def log_workflow_step(step_name: str, step_number: int):
    """Log workflow step progression"""
    print(f"📋 [WORKFLOW] Step {step_number}: {step_name}")

def log_workflow_complete():
    """Log when workflow agent completes"""
    print("✅ [WORKFLOW] Workflow processing complete")

def validate_workflow_output(output: dict) -> bool:
    """
    Validate that workflow output has required structure
    
    Args:
        output: The workflow output to validate
        
    Returns:
        bool: True if valid, False otherwise
    """
    try:
        # Check for basic structure
        if not isinstance(output, dict):
            return False
            
        # Add any specific validation logic here
        return True
        
    except Exception as e:
        print(f"❌ [WORKFLOW] Error validating output: {e}")
        return False

def format_workflow_error(error: str) -> dict:
    """
    Format workflow errors consistently
    
    Args:
        error: Error message
        
    Returns:
        dict: Formatted error response
    """
    return {
        "error": True,
        "message": error,
        "workflow_stage": "error",
        "timestamp": None  # Could add timestamp if needed
    }