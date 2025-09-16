"""
Action Log Tools

This module contains tools for the action logging agent to interact
with the database and perform logging operations.
"""

import logging
from typing import Optional, Dict, Any

from ai_assistant.utils.database_manager import db_manager
from ai_assistant.utils.api_config_manager import config_manager

logger = logging.getLogger(__name__)


def is_action_logging_enabled() -> bool:
    """
    Check if action logging is enabled in the configuration.
    
    Returns:
        bool: True if action logging is enabled, False otherwise
    """
    try:
        config = config_manager.framework_config
        action_logging_config = config.get('action_logging', {})
        return action_logging_config.get('enabled', False)
    except Exception as e:
        logger.error(f"❌ Error checking action logging configuration: {e}")
        return False


def is_auto_log_actions_enabled() -> bool:
    """
    Check if auto log actions is enabled in the configuration.
    
    Returns:
        bool: True if auto log actions is enabled, False otherwise
    """
    try:
        config = config_manager.framework_config
        action_logging_config = config.get('action_logging', {})
        return action_logging_config.get('auto_log_actions', False)
    except Exception as e:
        logger.error(f"❌ Error checking auto log actions configuration: {e}")
        return False


def log_ai_action(
    summary: str,
    user_id: int,
    created_by: int,
    entity_type: Optional[str] = None,
    entity_id: Optional[int] = None,
    session_id: Optional[str] = None,
    changes_log: Optional[Dict[str, Any]] = None
) -> Dict[str, Any]:
    """
    Log an AI action to the database if action logging is enabled.
    
    This tool checks the configuration before attempting to log the action.
    If action logging is disabled, it will return a message indicating so
    without performing any database operations.
    
    Args:
        summary: Brief summary of the AI action/interaction
        user_id: ID of the user who performed the action
        created_by: ID of the user who created this log entry
        entity_type: Optional type of entity involved in the action
        entity_id: Optional ID of the specific entity
        session_id: Optional session ID for tracking conversation flow
        changes_log: Optional detailed changes as dictionary
        
    Returns:
        dict: Result of the logging operation with status and message
    """
    logger.info("🎯 AI Action Logging Tool called")
    
    # Check if action logging is enabled
    if not is_action_logging_enabled():
        logger.info("📝 Action logging is disabled in configuration - skipping")
        return {
            "status": "skipped",
            "message": "Action logging is disabled in configuration",
            "logged": False
        }
    
    # Check if auto log actions is enabled
    if not is_auto_log_actions_enabled():
        logger.info("📝 Auto log actions is disabled in configuration - skipping")
        return {
            "status": "skipped", 
            "message": "Auto log actions is disabled in configuration",
            "logged": False
        }
    
    try:
        # Ensure the table exists
        table_created = db_manager.create_action_log_table_if_not_exists()
        if not table_created:
            logger.error("❌ Failed to create action log table")
            return {
                "status": "error",
                "message": "Failed to create action log table",
                "logged": False
            }
        
        # Insert the action log
        success = db_manager.insert_action_log(
            summary=summary,
            user_id=user_id,
            created_by=created_by,
            entity_type=entity_type,
            entity_id=entity_id,
            session_id=session_id,
            changes_log=changes_log
        )
        
        if success:
            logger.info(f"✅ Successfully logged AI action for user {user_id}")
            return {
                "status": "success",
                "message": f"AI action logged successfully for user {user_id}",
                "logged": True
            }
        else:
            logger.error("❌ Failed to insert action log")
            return {
                "status": "error",
                "message": "Failed to insert action log into database",
                "logged": False
            }
            
    except Exception as e:
        logger.error(f"❌ Unexpected error in log_ai_action tool: {e}")
        return {
            "status": "error",
            "message": f"Unexpected error: {str(e)}",
            "logged": False
        }


# Removed: get_recent_user_actions function
# This functionality is handled directly by the generate-suggestions endpoint 
# using db_manager.get_recent_actions_for_user(). The action_log_agent's 
# sole responsibility is logging actions, not retrieving them.