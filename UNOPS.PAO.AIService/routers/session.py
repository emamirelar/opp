"""
Session Router

This module contains session-related API endpoints for the AI Assistant.
Provides clean endpoints for session management with the .NET backend.
"""

import logging
from datetime import datetime, timezone
from fastapi import APIRouter, HTTPException, Query
from fastapi.responses import JSONResponse
from google.adk.sessions import DatabaseSessionService

from ai_assistant.utils.config import get_config, get_database_url, get_application_name

logger = logging.getLogger(__name__)


# Create router
router = APIRouter()

@router.get("/configuration")
async def get_session_configuration():
    """
    Get session configuration including app_name and other session-related settings.
    This endpoint is called by the .NET backend to get the correct configuration.
    """
    try:
        config = get_config()
        branding_config = config.get('branding', {})
        
        session_config = {
            "app_name": get_application_name(),
            "application_name": branding_config.get('application_name', 'AI Agent'),
            "project_name": branding_config.get('project_name', 'AI Agent'),
            "organization": branding_config.get('organization', 'UNOPS'),
            "environment": config.get('environment', 'local'),
            "version": "1.0.0"
        }
        
        return JSONResponse(content=session_config)
        
    except Exception as e:
        logger.error(f"❌ Error getting session configuration: {e}")
        raise HTTPException(status_code=500, detail=f"Failed to get session configuration: {str(e)}")

@router.get("/get-user-sessions")
async def get_user_sessions(
    app_name: str = Query(None, description="Application name"),
    user_id: str = Query(..., description="User ID to retrieve sessions for")
):
    """
    Get all sessions for a specific user from the ADK session service.
    This endpoint is called by the .NET backend to retrieve user sessions.
    """
    try:
        # Use default app_name if not provided
        if not app_name:
            app_name = get_application_name()
        
        # Create session service
        db_url = get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        
        # Get all sessions for the user
        sessions_response = await session_service.list_sessions(app_name=app_name, user_id=user_id)
        
        # Extract sessions from the response object
        sessions = sessions_response.sessions if hasattr(sessions_response, 'sessions') else []
        
        if not sessions:
            return []
        
        # Convert sessions to a serializable format with data from ADK session store
        session_list = []
        for session in sessions:
            # Handle different ADK Session object attributes
            session_id = getattr(session, 'id', getattr(session, 'session_id', getattr(session, 'sessionId', None)))
            
            # Get session state data
            session_state = getattr(session, 'state', {}) or {}
            if not isinstance(session_state, dict):
                session_state = {}
            
            # Get timestamp from ADK session
            updated_at = getattr(session, 'last_update_time', None)
            
            # Convert timestamp to numeric (Unix timestamp)
            last_updated_timestamp = None
            
            if updated_at:
                try:
                    if hasattr(updated_at, 'timestamp'):
                        # If it's a datetime object, get timestamp
                        last_updated_timestamp = updated_at.timestamp()
                    elif isinstance(updated_at, (int, float)):
                        # If it's already numeric, use as-is
                        last_updated_timestamp = float(updated_at)
                    else:
                        # Try to parse as datetime string and convert to timestamp
                        import dateutil.parser
                        dt = dateutil.parser.parse(str(updated_at))
                        last_updated_timestamp = dt.timestamp()
                except:
                    last_updated_timestamp = None
            
            # Use current time as fallback for timestamps
            current_timestamp = datetime.now(timezone.utc).timestamp()
            
            session_data = {
                "id": session_id,
                "userId": int(user_id),
                "title": session_state.get('title', 'New Chat'),
                "status": "Active",
                "starred": session_state.get('starred', False),
                "archived": session_state.get('archived', False),
                "aiGenerateTitle": session_state.get('aiGenerateTitle', True),
                "userEmail": session_state.get('user_email', ''),
                "lastUpdated": last_updated_timestamp or current_timestamp
            }
            
            session_list.append(session_data)
        
        return session_list
        
    except Exception as e:
        logger.error(f"❌ Error retrieving user sessions: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Failed to retrieve user sessions: {str(e)}")



@router.get("/session-with-chats")
async def get_session_with_chats(
    app_name: str = Query(..., description="Application name"),
    user_id: str = Query(..., description="User ID"),
    session_id: str = Query(..., description="Session ID to retrieve chats from")
):
    """
    Get a specific session with its chat history from the ADK session service.
    This endpoint is called by the .NET backend to retrieve session details with conversation history.
    """
    try:
        # Create session service
        db_url = get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        
        # Get the specific session with error handling for serialization issues
        session = None
        session_load_error = None
        try:
            session = await session_service.get_session(app_name=app_name, user_id=user_id, session_id=session_id)
        except Exception as e:
            session_load_error = e
            logger.warning(f"⚠️ Failed to load session {session_id} due to serialization error: {e}")
            logger.warning("⚠️ Attempting to recover session state only...")
            
            # Try to recover at least the session state from database directly
            try:
                async with session_service.database_session_factory() as db_session:
                    from google.adk.sessions.database_session_service import StorageSession
                    storage_session = await db_session.get(StorageSession, (app_name, user_id, session_id))
                    if storage_session:
                        # Create a minimal session-like object with just the state
                        class MinimalSession:
                            def __init__(self, state, session_id):
                                self.state = state
                                self.id = session_id
                                self.events = []  # Events couldn't be loaded
                        session = MinimalSession(storage_session.state or {}, session_id)
                        logger.info(f"✅ Recovered session state for {session_id}")
            except Exception as recover_error:
                logger.error(f"❌ Could not recover session state: {recover_error}")
        
        if not session:
            raise HTTPException(status_code=404, detail=f"Session {session_id} not found")
        
        # Get conversation history from session events - serialize properly
        conversation_history = []
        if session and hasattr(session, 'events') and session.events:
            # Try to serialize the ADK events properly
            import json
            try:
                # First, try to convert each event to a dict if it has model_dump method
                serialized_events = []
                for event in session.events:
                    try:
                        event_data = event.model_dump_json(exclude_none=True, by_alias=True)
                        event_data = json.loads(event_data)
                        serialized_events.append(event_data)
                    except Exception as event_error:
                        logger.warning(f"Failed to serialize individual event: {event_error}")
                        serialized_events.append({"error": f"Failed to serialize event: {str(event_error)}"})
                
                conversation_history = serialized_events
            except Exception as e:
                logger.error(f"Failed to serialize events: {e}")
                # Fallback: return empty array
                conversation_history = []
        
        # Handle different ADK Session object attributes
        session_id_attr = getattr(session, 'id', getattr(session, 'session_id', getattr(session, 'sessionId', session_id)))
        updated_at_attr = getattr(session, 'updated_at', getattr(session, 'updatedAt', getattr(session, 'last_updated', None)))
        
        # Provide default timestamp if none available (C# expects non-nullable DateTime)
        default_timestamp = datetime.now(timezone.utc)
        timestamp_iso = updated_at_attr.isoformat() if updated_at_attr else default_timestamp.isoformat()
        
        # Get title from session state if available
        session_title = "New Chat"  # Default title
        if hasattr(session, 'state') and session.state and isinstance(session.state, dict):
            session_title = session.state.get('title', 'New Chat')
                
        # Format the response to match expected C# SessionWithChats structure
        session_with_chats = {
            "session": {
                "id": session_id_attr,
                "userId": int(user_id),
                "status": "Active",
                "lastUpdated": timestamp_iso,
                "title": session_title,
                "starred": False,
                "archived": False,
                "aiGenerateTitle": True
            },
            "chatMessages": conversation_history  # Changed from "chats" to match C# property name
        }
                
        return session_with_chats
        
    except HTTPException:
        # Re-raise HTTP exceptions (like 404) as-is
        raise
    except Exception as e:
        logger.error(f"❌ Error retrieving session with chats: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Failed to retrieve session with chats: {str(e)}")



@router.post("/update-session-metadata")
async def update_session_metadata(
    session_id: str = Query(..., description="Session ID to update"),
    user_id: str = Query(..., description="User ID"),
    starred: bool = Query(None, description="Star status"),
    archived: bool = Query(None, description="Archive status")
):
    """
    Update session metadata (starred, archived) in the session state.
    This endpoint is called by the .NET backend to update session metadata.
    """
    try:
        # Validate inputs
        if not session_id or session_id.strip() == "":
            raise HTTPException(status_code=400, detail="Session ID is required")
        
        # Prepare state updates
        state_updates = {}
        if starred is not None:
            state_updates['starred'] = starred
        if archived is not None:
            state_updates['archived'] = archived
        
        if not state_updates:
            raise HTTPException(status_code=400, detail="At least one metadata field must be provided")
        
        # Create session service
        db_url = get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        
        # Update session state
        from ai_assistant.utils.session_management import update_session_state_in_database
        await update_session_state_in_database(
            session_service=session_service,
            app_name="ai_assistant",
            user_id=user_id,
            session_id=session_id,
            state_updates=state_updates
        )
        
        return {
            "session_id": session_id,
            "updates": state_updates,
            "status": "success"
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"❌ Error updating session metadata: {e}")
        raise HTTPException(status_code=500, detail=f"Failed to update session metadata: {str(e)}")



@router.post("/update-session-title")
async def update_session_title(
    session_id: str = Query(..., description="Session ID to update title for"),
    user_id: str = Query(..., description="User ID"),
    title: str = Query(..., description="New title for the session")
):
    """
    Update the title of an existing session in the session state.
    This endpoint is called by the .NET backend to update session titles.
    """
    try:
        # Validate inputs
        if not session_id or session_id.strip() == "":
            raise HTTPException(status_code=400, detail="Session ID is required")
        if not title or title.strip() == "":
            raise HTTPException(status_code=400, detail="Title is required")
        
        # Create session service
        db_url = get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        
        # Update session state with new title
        from ai_assistant.utils.session_management import update_session_state_in_database
        await update_session_state_in_database(
            session_service=session_service,
            app_name="ai_assistant",
            user_id=user_id,
            session_id=session_id,
            state_updates={"title": title.strip()}
        )
        
        return {
            "session_id": session_id,
            "title": title.strip(),
            "status": "success"
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"❌ Error updating session title: {e}")
        raise HTTPException(status_code=500, detail=f"Failed to update session title: {str(e)}")
