"""
Session Management Utilities

This module contains session creation, retrieval, and state management logic
moved from main.py for better organization.
"""

import logging
import json
import base64
import uuid
from datetime import datetime, timezone
from typing import Dict, Any
from google.adk.sessions import DatabaseSessionService

from ai_assistant.utils.config import get_config, get_gemini_adhoc_model

logger = logging.getLogger(__name__)

def safe_convert_to_string(data):
    """
    Safely convert any data type to a string that can be serialized to JSON.
    Handles bytes, binary data, and other non-serializable types.
    """
    if data is None:
        return ""
    
    if isinstance(data, str):
        return data
    
    if isinstance(data, bytes):
        try:
            # Try to decode as UTF-8 first
            return data.decode('utf-8')
        except UnicodeDecodeError:
            # If UTF-8 fails, encode as base64
            return base64.b64encode(data).decode('ascii')
    
    # For any other type, convert to string
    try:
        return str(data)
    except Exception:
        return ""

def detect_mime_type_from_data(data, filename=""):
    """
    Detect MIME type from data content or filename.
    Provides sensible defaults for common file types.
    """
    if not data:
        return "application/octet-stream"
    
    # Try to detect from filename first
    if filename:
        filename_lower = filename.lower()
        if filename_lower.endswith(('.jpg', '.jpeg')):
            return "image/jpeg"
        elif filename_lower.endswith('.png'):
            return "image/png"
        elif filename_lower.endswith('.gif'):
            return "image/gif"
        elif filename_lower.endswith('.webp'):
            return "image/webp"
        elif filename_lower.endswith(('.mp3', '.wav', '.ogg')):
            return "audio/mpeg"
        elif filename_lower.endswith('.pdf'):
            return "application/pdf"
        elif filename_lower.endswith(('.txt', '.text')):
            return "text/plain"
        elif filename_lower.endswith(('.doc', '.docx')):
            return "application/msword"
        elif filename_lower.endswith(('.xls', '.xlsx')):
            return "application/vnd.ms-excel"
    
    # Try to detect from data content
    if isinstance(data, str):
        # Check if it's base64 encoded
        if data.startswith('data:'):
            # Data URL format: data:mime/type;base64,data
            mime_part = data.split(',')[0]
            if ';' in mime_part:
                return mime_part.split(';')[0].replace('data:', '')
        elif len(data) > 100:  # Likely base64 encoded binary data
            return "application/octet-stream"
        else:
            return "text/plain"
    
    elif isinstance(data, bytes):
        # Check for common file signatures
        if len(data) >= 4:
            if data[:4] == b'\x89PNG':
                return "image/png"
            elif data[:2] == b'\xff\xd8':
                return "image/jpeg"
            elif data[:4] == b'GIF8':
                return "image/gif"
            elif data[:4] == b'RIFF' and data[8:12] == b'WEBP':
                return "image/webp"
            elif data[:4] == b'%PDF':
                return "application/pdf"
            elif data[:3] == b'ID3':
                return "audio/mpeg"
        
        return "application/octet-stream"
    
    return "application/octet-stream"

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
                    logger.info(f"📄 Parsed JSON state with keys: {list(parsed_state.keys())}")
                    return parsed_state
                except json.JSONDecodeError as e:
                    logger.warning(f"⚠️ Failed to parse state JSON: {e}")
                    return {}
            else:
                logger.info("📄 Empty state string, using empty dict")
                return {}
        elif isinstance(state_input, dict):
            # State is already a dict, use it directly
            logger.info(f"📊 Using dict state with keys: {list(state_input.keys())}")
            return state_input
        else:
            logger.warning(f"⚠️ Unexpected state type: {type(state_input)}")
            return {}
    else:
        logger.info("📄 No state provided, using empty dict")
        return {}

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
        logger.info(f"💾 Updating session state for {app_name}/{user_id}/{session_id}")
        logger.info(f"💾 State updates: {list(state_updates.keys())}")
        
        # Access the database session factory from the service (async in ADK 1.3.0+)
        async with session_service.database_session_factory() as db_session:
            # Use the same schema classes as the service (v0 or v1 depending on DB)
            schema = session_service._get_schema_classes()
            StorageSession = schema.StorageSession

            # Get the existing session (use await for async session)
            storage_session = await db_session.get(StorageSession, (app_name, user_id, session_id))
            
            if storage_session:
                # Update the state with new data
                if not storage_session.state:
                    storage_session.state = {}
                
                # Log the state update details
                old_state_keys = set(storage_session.state.keys()) if storage_session.state else set()
                storage_session.state.update(state_updates)
                new_state_keys = set(storage_session.state.keys())
                added_keys = new_state_keys - old_state_keys
                updated_keys = old_state_keys.intersection(new_state_keys)
                
                logger.info(f"💾 State update summary: +{len(added_keys)} new, ~{len(updated_keys)} updated")
                
                # Commit the changes (use await for async session)
                await db_session.commit()
                logger.info("✅ Successfully updated session state in database")
            else:
                logger.error(f"❌ Session {session_id} not found in database for state update")
                raise Exception(f"Session {session_id} not found in database")
                
    except Exception as e:
        logger.error(f"❌ Error updating session state in database: {e}")
        logger.error(f"❌ Session: {app_name}/{user_id}/{session_id}")
        logger.error(f"❌ State updates: {state_updates}")
        raise

async def _recover_session_state_from_db(
    session_service: DatabaseSessionService,
    app_name: str,
    user_id: str,
    session_id: str
) -> Dict[str, Any]:
    """
    Attempt to recover session STATE directly from database, bypassing event deserialization.
    This is useful when get_session() fails due to corrupted event data.
    
    Args:
        session_service: The database session service instance
        app_name: Application name
        user_id: User ID
        session_id: Session ID
        
    Returns:
        dict: The recovered session state, or empty dict if recovery fails
    """
    try:
        async with session_service.database_session_factory() as db_session:
            # Use the same schema classes as the service (v0 or v1 depending on DB)
            schema = session_service._get_schema_classes()
            StorageSession = schema.StorageSession

            # Get the raw storage session (this bypasses event deserialization)
            storage_session = await db_session.get(StorageSession, (app_name, user_id, session_id))
            
            if storage_session and storage_session.state:
                logger.info(f"🔧 Recovered session state from database: {list(storage_session.state.keys())}")
                return dict(storage_session.state)
            else:
                logger.info(f"🔧 No state found in database for session {session_id}")
                return {}
                
    except Exception as e:
        logger.error(f"❌ Error recovering session state from database: {e}")
        return {}


async def get_or_create_session(
    session_service: DatabaseSessionService,
    app_name: str,
    user_id: str,
    session_id: str,
    initial_state: Dict[str, Any],
    user_prompt: str = None
):
    """
    Get an existing session or create a new one with proper state management.
    
    Args:
        session_service: Database session service
        app_name: Application name
        user_id: User ID
        session_id: Session ID (can be empty for new sessions)
        initial_state: Initial state to set for the session
        user_prompt: Optional user prompt to use as default title for new sessions
        
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
        
        # Generate a default title for new sessions
        if user_prompt and user_prompt.strip():
            # Use user prompt as title, truncated if too long
            default_title = user_prompt.strip()[:200] + "..." if len(user_prompt.strip()) > 200 else user_prompt.strip()
        else:
            default_title = "New Chat"
        
        # Prepare enhanced initial state with session metadata
        initial_state_with_title = initial_state.copy()
        initial_state_with_title.update({
            'title': default_title,
            'starred': False,
            'archived': False,
            'aiGenerateTitle': True,
            'status': 'Active'
        })
        
        session = await session_service.create_session(
            app_name=app_name,
            user_id=user_id,
            session_id=actual_session_id,
            state=initial_state_with_title
        )
        logger.info(f"✅ New session created with initial state including title: {default_title}")
        return session, actual_session_id, True
    else:
        logger.info(f"🔍 Getting existing session for app: {app_name}, user: {user_id}, session: {actual_session_id}")
        
        session = None
        session_load_error = None
        
        try:
            session = await session_service.get_session(
                app_name=app_name,
                user_id=user_id,
                session_id=actual_session_id
            )
        except Exception as e:
            # Handle corrupted session data (e.g., Transcription validation errors from Gemini responses)
            session_load_error = e
            logger.warning(f"⚠️ Failed to load session {actual_session_id}: {e}")
            logger.warning("⚠️ Attempting to recover session state from database...")
            session = None
            
            # Try to recover at least the session STATE from the database directly
            # This preserves uploaded_files_metadata and other state even if events are corrupted
            try:
                recovered_state = await _recover_session_state_from_db(
                    session_service, app_name, user_id, actual_session_id
                )
                if recovered_state:
                    # Merge recovered state into initial_state so it's preserved
                    initial_state.update(recovered_state)
                    logger.info(f"✅ Recovered session state with keys: {list(recovered_state.keys())}")
            except Exception as recover_error:
                logger.warning(f"⚠️ Could not recover session state: {recover_error}")

        if not session:
            if session_load_error:
                # CRITICAL: Don't generate new session ID - this loses all conversation history!
                # Instead, try to recover the session by recreating it with the SAME ID
                logger.warning(f"🔄 Session {actual_session_id} failed to load due to: {session_load_error}")
                logger.warning("🔄 Attempting to recover session with same ID (conversation history may be partial)")
                
                # Try to delete the corrupted session first, then recreate
                try:
                    await session_service.delete_session(
                        app_name=app_name,
                        user_id=user_id,
                        session_id=actual_session_id
                    )
                    logger.info(f"🗑️ Deleted corrupted session {actual_session_id}")
                except Exception as delete_error:
                    logger.warning(f"⚠️ Could not delete corrupted session: {delete_error}")
                
                # Keep the SAME session ID so frontend maintains continuity
                # The user's conversation will continue with the same session ID
                # even though some events may have been lost
                logger.warning(f"⚠️ IMPORTANT: Session {actual_session_id} events may have been lost due to serialization error")
                logger.warning(f"⚠️ Original error: {session_load_error}")
            else:
                logger.info("🆕 Session not found - creating new session...")
            
            # Generate a default title for new sessions
            if user_prompt and user_prompt.strip():
                # Use user prompt as title, truncated if too long
                default_title = user_prompt.strip()[:200] + "..." if len(user_prompt.strip()) > 200 else user_prompt.strip()
            else:
                default_title = "New Chat"
            
            # Prepare enhanced initial state with session metadata
            initial_state_with_title = initial_state.copy()
            initial_state_with_title.update({
                'title': default_title,
                'starred': False,
                'archived': False,
                'aiGenerateTitle': True,
                'status': 'Active'
            })
            
            session = await session_service.create_session(
                app_name=app_name,
                user_id=user_id,
                session_id=actual_session_id,
                state=initial_state_with_title
            )
            
            if session_load_error:
                logger.warning(f"✅ Session {actual_session_id} recovered (events may be partial)")
            else:
                logger.info(f"✅ New session created with initial state including title: {default_title}")
            
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
                logger.error(f"❌ CRITICAL: Failed to persist session state to database: {db_update_error}")
                logger.error(f"❌ Session {actual_session_id} state may be lost on next request")
                # Don't silently ignore this - it's critical for session continuity
                raise Exception(f"Session state persistence failed: {db_update_error}")

            return session, actual_session_id, False

def call_gemini_direct(prompt: str, model_name: str = "gemini-1.5-flash", max_tokens: int = 100, temperature: float = 0.7) -> str:
    """
    Make a direct call to Gemini via Google GenAI SDK.
    
    Args:
        prompt (str): The prompt to send to Gemini
        model_name (str): The Gemini model to use (default: "gemini-1.5-flash")
        max_tokens (int): Maximum output tokens (default: 100)
        temperature (float): Temperature for generation (0.0 to 1.0, default: 0.7)
    
    Returns:
        str: The generated response from Gemini
        
    Raises:
        Exception: If there's an error with the Gemini call
    """
    try:
        # Get configuration from config manager
        config = get_config()
        google_cloud_config = config.get('google_cloud', {})
        
        # Get project ID and location from config
        project_id = google_cloud_config.get('project')
        location = google_cloud_config.get('location')

        
        if not project_id:
            raise ValueError("Google Cloud Project ID not found in configuration or environment variables")
        
        logger.info(f"🔧 Initializing Google GenAI Client for project: {project_id}, location: {location}")
        
        # Initialize Google GenAI Client
        from google.genai import Client, types
        
        # Create client with Vertex AI backend
        with Client(vertexai=True, project=project_id, location=location) as client:
            logger.info(f"🤖 Sending prompt to Gemini model: {model_name}")
            
            response = client.models.generate_content(
                model=model_name,
                contents=prompt,
                config=types.GenerateContentConfig(
                    temperature=temperature,
                    max_output_tokens=max_tokens,
                    top_p=0.8,
                    top_k=40,
                    safety_settings=[
                        types.SafetySetting(
                            category=types.HarmCategory.HARM_CATEGORY_HARASSMENT,
                            threshold=types.HarmBlockThreshold.BLOCK_NONE
                        ),
                        types.SafetySetting(
                            category=types.HarmCategory.HARM_CATEGORY_HATE_SPEECH,
                            threshold=types.HarmBlockThreshold.BLOCK_NONE
                        ),
                        types.SafetySetting(
                            category=types.HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
                            threshold=types.HarmBlockThreshold.BLOCK_NONE
                        ),
                        types.SafetySetting(
                            category=types.HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
                            threshold=types.HarmBlockThreshold.BLOCK_NONE
                        )
                    ]
                )
            )

            print(response)
            
            # Extract the generated text
            if response.text:
                generated_text = response.text.strip()
                logger.info(f"✅ Gemini response generated successfully: {generated_text[:50]}...")
                return generated_text
            else:
                # Handle cases where no valid response was generated
                print(f"🔍 DEBUG: Response object details:")
                print(f"  - Response type: {type(response)}")
                print(f"  - Response attributes: {dir(response)}")
                
                error_msg = "No valid response could be generated from Gemini"
                logger.warning(f"⚠️ {error_msg}")
                print(f"🔍 DEBUG: No text in response")
                print(f"  - Response: {response}")
                raise Exception(error_msg)
                
    except Exception as e:
        logger.error(f"❌ Error calling Gemini: {str(e)}")
        raise Exception(f"Gemini API call failed: {str(e)}")

def generate_conversation_title(formatted_conversation: str) -> str:
    """
    Generate a concise title for a conversation using Gemini.
    
    Args:
        formatted_conversation (str): The formatted conversation text
        
    Returns:
        str: A concise title (3-5 words) for the conversation
    """
    try:
        # Create the prompt for title generation
        title_prompt = f"""
Please generate a concise and descriptive title for the following conversation, limited to 3-5 words. The title should capture the core topic or outcome of the interaction. Ensure the title is neutral or positive in tone, avoiding any negative connotations.

{formatted_conversation}

The response should just be the title, no explanations.
"""
        
        # Call Gemini with specific parameters for title generation
        from ai_assistant.utils.config import get_gemini_adhoc_model
        gemini_model = get_gemini_adhoc_model()
        title = call_gemini_direct(
            prompt=title_prompt,
            model_name=gemini_model,
            max_tokens=20,  # Keep it small for a 3-5 word title
            temperature=0.3  # Lower temperature for more consistent titles
        )
        
        logger.info(f"📝 Generated conversation title: {title}")
        return title
        
    except Exception as e:
        logger.error(f"❌ Error generating conversation title: {str(e)}")
        print(f"🔍 DEBUG: Full exception details:")
        print(f"  - Exception type: {type(e)}")
        print(f"  - Exception message: {str(e)}")
        import traceback
        print(f"  - Full traceback:")
        traceback.print_exc()
        # Return a fallback title if Gemini fails
        return "Conversation"

async def _get_title_from_action_log(session_id: str, user_id: int) -> str:
    """
    Generate title using action log summaries for the session.
    
    Args:
        session_id: Session ID to get summaries for
        user_id: User ID for the session
        
    Returns:
        str: Generated title or None if action logging not available
    """
    try:
        # Check if action logging is enabled
        config = get_config()
        action_logging_config = config.get('action_logging', {})
        
        if not action_logging_config.get('enabled', False):
            logger.info("📝 Action logging disabled - skipping action log title generation")
            return None

        # Import database manager
        from ai_assistant.utils.database_manager import db_manager
        
        # Get action logs for this specific session
        session = db_manager.get_session()
        try:
            from ai_assistant.models.action_log import AiActionLog
            
            # Query for actions from this specific session
            actions = session.query(AiActionLog)\
                           .filter(AiActionLog.user_id == user_id)\
                           .filter(AiActionLog.session_id == session_id)\
                           .order_by(AiActionLog.created_date.asc())\
                           .limit(10)\
                           .all()
            
            if not actions:
                logger.info(f"📭 No action logs found for session {session_id}")
                return None
            
            logger.info(f"📊 Found {len(actions)} action logs for session {session_id}")
            
            # Prepare conversation context from action log summaries
            summaries = []
            for action in actions:
                summaries.append(action.summary)
            
            conversation_context = " | ".join(summaries)
            
            # Generate title using Gemini with action log context
            title_prompt = f"""
Based on these AI interaction summaries from a conversation session, generate a concise and descriptive title (3-5 words). The title should capture the main topic or purpose of the conversation:

{conversation_context}

Response should be just the title, no explanations.
"""
            
            gemini_model = get_gemini_adhoc_model()
            title = call_gemini_direct(
                prompt=title_prompt,
                model_name=gemini_model,
                max_tokens=20,
                temperature=0.3
            )
            
            logger.info(f"📝 Generated title from action log: {title}")
            return title
            
        finally:
            session.close()
            
    except Exception as e:
        logger.error(f"❌ Error generating title from action log: {e}")
        return None