"""
Session Management Utilities

This module contains session creation, retrieval, and state management logic
moved from main.py for better organization.
"""

import logging
import uuid
from typing import Dict, Any
from google.adk.sessions import DatabaseSessionService

logger = logging.getLogger(__name__)


async def update_session_state_in_database(
    session_service: DatabaseSessionService, 
    app_name: str, 
    user_id: str, 
    session_id: str, 
    state_updates: Dict[str, Any]
) -> None:
    """
    Update session state directly in the database for existing sessions.
    
    Args:
        session_service: The database session service instance
        app_name: Application name
        user_id: User ID
        session_id: Session ID
        state_updates: Dictionary of state updates to apply
    """
    try:
        # Access the database session factory from the service
        with session_service.database_session_factory() as db_session:
            # Import the StorageSession model from the session service module
            from google.adk.sessions.database_session_service import StorageSession
            
            # Get the existing session
            storage_session = db_session.get(StorageSession, (app_name, user_id, session_id))
            
            if storage_session:
                # Update the state with new data
                if not storage_session.state:
                    storage_session.state = {}
                
                storage_session.state.update(state_updates)
                
                # Commit the changes
                db_session.commit()
                logger.info("💾 Successfully updated session state in database")
            else:
                logger.warning("⚠️ Session not found in database for update")
                
    except Exception as e:
        logger.error(f"❌ Error updating session state in database: {e}")
        raise


async def get_or_create_session(
    session_service: DatabaseSessionService,
    app_name: str,
    user_id: str,
    session_id: str,
    initial_state: Dict[str, Any]
):
    """
    Get an existing session or create a new one with proper state management.
    
    Args:
        session_service: Database session service
        app_name: Application name
        user_id: User ID
        session_id: Session ID (can be empty for new sessions)
        initial_state: Initial state to set for the session
        
    Returns:
        tuple: (session, actual_session_id, is_new_session)
    """
    # Handle session creation vs retrieval
    original_session_id = session_id
    is_new_session_request = not original_session_id or original_session_id.strip() == ""
    
    if is_new_session_request:
        actual_session_id = str(uuid.uuid4())
        logger.info(f"🆔 Empty session_id provided - generating new session: {actual_session_id}")
    else:
        actual_session_id = original_session_id
        logger.info(f"🆔 Using provided session_id: {actual_session_id}")

    # Handle session creation vs retrieval
    if is_new_session_request:
        logger.info("🆕 Creating new session (empty session_id provided)...")
        session = await session_service.create_session(
            app_name=app_name,
            user_id=user_id,
            session_id=actual_session_id,
            state=initial_state
        )
        logger.info("✅ New session created with initial state")
        return session, actual_session_id, True
    else:
        logger.info(f"🔍 Getting existing session for app: {app_name}, user: {user_id}, session: {actual_session_id}")
        session = await session_service.get_session(
            app_name=app_name,
            user_id=user_id,
            session_id=actual_session_id
        )

        if not session:
            logger.info("🆕 Session not found - creating new session...")
            session = await session_service.create_session(
                app_name=app_name,
                user_id=user_id,
                session_id=actual_session_id,
                state=initial_state
            )
            logger.info("✅ New session created with initial state")
            return session, actual_session_id, True
        else:
            logger.info("📋 Found existing session")
            
            if not hasattr(session, 'state') or session.state is None:
                session.state = {}
            
            # Update the in-memory session state with new data
            session.state.update(initial_state)
            logger.info("✅ Updated existing session in-memory with current request data")
            
            # Update the session state in database for future requests
            try:
                await update_session_state_in_database(
                    session_service, 
                    app_name, 
                    user_id, 
                    actual_session_id, 
                    initial_state
                )
                logger.info("✅ Persisted state updates to database for future requests")
            except Exception as db_update_error:
                logger.warning(f"⚠️ Failed to persist to database (will work for current request): {db_update_error}")
                logger.info("🔄 In-memory state is still available for current request")

            return session, actual_session_id, False


def parse_request_state(state_input: Any) -> Dict[str, Any]:
    """
    Parse state from request - can be either a JSON string or a dict.
    
    Args:
        state_input: State input from request (string or dict)
        
    Returns:
        dict: Parsed state dictionary
    """
    if state_input:
        if isinstance(state_input, str):
            # State is a JSON string, try to parse it
            if state_input.strip():  # Only parse non-empty strings
                try:
                    import json
                    parsed_state = json.loads(state_input)
                    logger.info(f"📄 Parsed JSON state: {parsed_state}")
                    return parsed_state
                except json.JSONDecodeError as e:
                    logger.warning(f"⚠️ Failed to parse state JSON: {e}")
                    logger.info(f"🔍 Raw state value: {state_input}")
                    return {}
            else:
                logger.info("📄 Empty state string, using empty dict")
                return {}
        elif isinstance(state_input, dict):
            # State is already a dict, use it directly
            logger.info(f"📊 Using dict state: {state_input}")
            return state_input
        else:
            logger.warning(f"⚠️ Unexpected state type: {type(state_input)}")
            return {}
    else:
        logger.info("📄 No state provided, using empty dict")
        return {} 