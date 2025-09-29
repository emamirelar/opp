"""
Session Router

This module contains session-related endpoints like title generation
for better organization.
"""

import logging
import os
import json
import base64
from datetime import datetime, timezone
from fastapi import APIRouter, HTTPException, Request, Query
from google.adk.sessions import DatabaseSessionService

from ai_assistant.utils.api_config_manager import config_manager

# Vertex AI imports for title generation
import vertexai
from vertexai.generative_models import GenerativeModel, GenerationConfig, HarmBlockThreshold, HarmCategory

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

# Create router
router = APIRouter()


@router.get("/generate-title")
async def get_session_title(
    session_id: str = Query(..., description="Session ID to retrieve conversations from"),
    user_id: str = Query(..., description="User ID to retrieve conversations from"),
    request: Request = None
):
    """
    Get the first 2 conversations from a session and generate a title using Gemini
    """
    try:
        logger.info("="*50)
        logger.info("📋 INCOMING TITLE REQUEST")
        logger.info("="*50)
        logger.info("🔍 Request Details:")
        logger.info(f"   - session_id: {session_id}")
        logger.info(f"   - user_id: {user_id}")
        logger.info("="*50)

        # Validate session_id
        if not session_id or session_id.strip() == "":
            raise HTTPException(status_code=400, detail="Session ID is required")

        # Check if action logging is enabled and try to use action log summaries first
        try:
            user_id_int = int(user_id)
            title_from_action_log = await _get_title_from_action_log(session_id, user_id_int)
            if title_from_action_log:
                logger.info("✅ Generated title using action log summaries")
                return {
                    "session_id": session_id,
                    "formatted_conversation": "Generated from action log summaries",
                    "title": title_from_action_log,
                    "source": "action_log"
                }
        except Exception as action_log_error:
            logger.info(f"📋 Action log method failed, falling back to session history: {action_log_error}")

        # Fallback to current method using session service
        logger.info("📋 Using fallback method: session conversation history")
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")

        try:
            session = await session_service.get_session(
                app_name="ai_assistant",
                user_id=user_id,
                session_id=session_id
            )

            if not session:
                logger.warning(f"❌ Session not found: {session_id}")
                raise HTTPException(status_code=404, detail=f"Session '{session_id}' not found")

            logger.info(f"📋 Found session: {session_id}")

            conversation_history = session.events
            logger.info(f"📝 Conversation history length: {len(conversation_history)}")

            if not conversation_history:
                return {
                    "session_id": session_id,
                    "formatted_conversation": "No conversations found in this session",
                    "title": "Empty Session"
                }

            conversations = []
            conversation_count = 0
            for i, message in enumerate(conversation_history):
                if conversation_count >= 2:
                    break
                if hasattr(message, 'author') and message.author == "user":
                    user_message = ""
                    if hasattr(message, 'content') and message.content and hasattr(message.content, 'parts'):
                        user_parts = []
                        for part in message.content.parts:
                            if hasattr(part, 'text') and part.text:
                                user_parts.append(part.text)
                        user_message = " ".join(user_parts)
                    if user_message.strip():
                        assistant_message = ""
                        for j in range(i + 1, len(conversation_history)):
                            next_message = conversation_history[j]
                            if (hasattr(next_message, 'author') and
                                next_message.author in ["task_executor_agent", "response_formatter_agent", "api_caller_agent"]):
                                if hasattr(next_message, 'content') and next_message.content and hasattr(next_message.content, 'parts'):
                                    assistant_parts = []
                                    for part in next_message.content.parts:
                                        if hasattr(part, 'text') and part.text:
                                            assistant_parts.append(part.text)
                                    assistant_message = " ".join(assistant_parts)
                                if assistant_message:
                                    if assistant_message.startswith("```json"):
                                        assistant_message = assistant_message.replace("```json", "").replace("```", "").strip()
                                    try:
                                        import json
                                        json_data = json.loads(assistant_message)
                                        if "result" in json_data and json_data["result"]:
                                            first_result = json_data["result"][0]
                                            if "message" in first_result:
                                                assistant_message = first_result["message"]
                                    except json.JSONDecodeError:
                                        pass
                                    break
                        conversations.append({
                            "conversation_number": conversation_count + 1,
                            "user_message": user_message.strip(),
                            "assistant_message": assistant_message.strip() if assistant_message else "No response recorded"
                        })
                        conversation_count += 1
                        logger.info(f"✅ Added conversation {conversation_count}: User='{user_message[:30]}...', Assistant='{assistant_message[:30] if assistant_message else 'No response'}...'")
            
            logger.info(f"✅ Retrieved {len(conversations)} conversations")
            formatted_conversation = ""
            if conversations:
                formatted_display = []
                for conv in conversations:
                    formatted_display.append(f"👤 User: {conv['user_message']}")
                    formatted_display.append(f"🤖 Assistant: {conv['assistant_message']}")
                    formatted_display.append("---")
                formatted_conversation = "\n".join(formatted_display[:-1])
            
            try:
                title = generate_conversation_title(formatted_conversation)
                logger.info(f"📝 Generated title: {title}")
            except Exception as title_error:
                logger.warning(f"⚠️ Failed to generate title with Gemini: {title_error}")
                title = "Conversation"
            
            response = {
                "session_id": session_id,
                "formatted_conversation": formatted_conversation,
                "title": title,
                "source": "session_history"
            }
            return response
            
        except Exception as db_error:
            logger.error(f"❌ Database error: {db_error}")
            raise HTTPException(status_code=500, detail=f"Database error: {str(db_error)}")
            
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"❌ Unexpected error in title endpoint: {e}")
        import traceback
        logger.error(f"❌ Title endpoint traceback: {traceback.format_exc()}")
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}") 

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
        from ai_assistant.utils.api_config_manager import config_manager
        gemini_model = config_manager.get_gemini_adhoc_model()
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
        config = config_manager.framework_config
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
            
            gemini_model = config_manager.get_gemini_adhoc_model()
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

def call_gemini_direct(prompt: str, model_name: str = "gemini-1.5-flash", max_tokens: int = 100, temperature: float = 0.7) -> str:
    """
    Make a direct call to Gemini via Vertex AI.
    
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
        config = config_manager.framework_config
        google_cloud_config = config.get('google_cloud', {})
        
        # Get project ID and location from config
        project_id = google_cloud_config.get('project', os.getenv("GOOGLE_CLOUD_PROJECT_ID"))
        location = google_cloud_config.get('location', os.getenv("GOOGLE_CLOUD_LOCATION", "us-central1"))

        
        if not project_id:
            raise ValueError("Google Cloud Project ID not found in configuration or environment variables")
        
        logger.info(f"🔧 Initializing Vertex AI for project: {project_id}, location: {location}")
        
        # Initialize Vertex AI
        vertexai.init(project=project_id, location=location)

        safety_settings = {
            HarmCategory.HARM_CATEGORY_HARASSMENT: HarmBlockThreshold.BLOCK_NONE,
            HarmCategory.HARM_CATEGORY_HATE_SPEECH: HarmBlockThreshold.BLOCK_NONE,
            HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT: HarmBlockThreshold.BLOCK_NONE,
            HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT: HarmBlockThreshold.BLOCK_NONE,
        }
        
        # Create generation config
        generation_config = GenerationConfig(
            temperature=temperature,
            max_output_tokens=max_tokens,
            top_p=0.8,
            top_k=40
        )
        
        # Create model and generate content
        model = GenerativeModel(model_name)
        logger.info(f"🤖 Sending prompt to Gemini model: {model_name}")
        
        response = model.generate_content(
            prompt,
            generation_config=generation_config,
            safety_settings=safety_settings
        )

        print(response)
        
        # Extract the generated text
        if response.candidates and response.candidates[0].content.parts:
            generated_text = response.candidates[0].content.parts[0].text.strip()
            logger.info(f"✅ Gemini response generated successfully: {generated_text[:50]}...")
            return generated_text
        else:
            # Handle cases where no valid response was generated
            print(f"🔍 DEBUG: Response object details:")
            print(f"  - Response type: {type(response)}")
            print(f"  - Has candidates: {hasattr(response, 'candidates')}")
            print(f"  - Candidates: {getattr(response, 'candidates', 'N/A')}")
            print(f"  - Response attributes: {dir(response)}")
            
            if response.prompt_feedback and response.prompt_feedback.block_reason:
                error_msg = f"Gemini blocked the response due to: {response.prompt_feedback.block_reason}"
                logger.warning(f"⚠️ {error_msg}")
                print(f"🔍 DEBUG: Prompt feedback details:")
                print(f"  - Block reason: {response.prompt_feedback.block_reason}")
                print(f"  - Safety ratings: {getattr(response.prompt_feedback, 'safety_ratings', 'N/A')}")
                raise Exception(error_msg)
            else:
                error_msg = "No valid response could be generated from Gemini"
                logger.warning(f"⚠️ {error_msg}")
                print(f"🔍 DEBUG: No prompt feedback available")
                print(f"  - Response text: {getattr(response, 'text', 'N/A')}")
                print(f"  - Response content: {getattr(response, 'content', 'N/A')}")
                raise Exception(error_msg)
                
    except Exception as e:
        logger.error(f"❌ Error calling Gemini: {str(e)}")
        raise Exception(f"Gemini API call failed: {str(e)}")


@router.get("/user-sessions")
@router.post("/user-sessions")  # Support POST method
@router.get("/get-user-sessions")  # Add alias for C# compatibility
@router.post("/get-user-sessions")  # Support POST method for C# compatibility
async def get_user_sessions(
    request: Request,
    app_name: str = Query(None, description="Application name"),
    user_id: str = Query(None, description="User ID to retrieve sessions for")
):
    """
    Get all sessions for a specific user from the ADK session service
    """
    try:
        logger.info("="*50)
        
        # Handle cases where parameters are not provided in query string (POST requests from ASP.NET)
        if not app_name:
            app_name = config_manager.get_application_name()
            logger.info(f"📋 Using app_name from config: {app_name}")
        
        if not user_id:
            # Try to extract user_id from request headers (set by ASP.NET authentication)
            headers = dict(request.headers)
            user_id = headers.get('x-user-id') or headers.get('x-unops-user-id')
            if not user_id:
                # For now, use a default user_id for testing
                user_id = "1"
                logger.info(f"📋 No user_id provided, using default: {user_id}")
            else:
                logger.info(f"📋 Extracted user_id from headers: {user_id}")
        
        logger.info(f"📋 Getting user sessions for app: {app_name}, user: {user_id}")
        
        # Create session service
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")
        
        # Get all sessions for the user
        logger.info(f"🔍 Retrieving sessions for app: {app_name}, user: {user_id}")
        sessions_response = await session_service.list_sessions(app_name=app_name, user_id=user_id)
        
        # Extract sessions from the response object
        sessions = sessions_response.sessions if hasattr(sessions_response, 'sessions') else []
        
        if not sessions:
            logger.info(f"📭 No sessions found for user {user_id} in app {app_name}")
            return []
        
        logger.info(f"✅ Found {len(sessions)} sessions for user {user_id}")
        
        # Convert sessions to a serializable format
        # Note: C# code will join with database to get actual LastUpdated, Title, etc.
        session_list = []
        for session in sessions:
            # Handle different ADK Session object attributes
            session_id = getattr(session, 'id', getattr(session, 'session_id', getattr(session, 'sessionId', None)))
            
            session_data = {
                "id": session_id,
                "userId": int(user_id),
                "status": "Active",  # Default status
                "lastUpdated": datetime.now(timezone.utc).isoformat(),  # Placeholder - C# will use database value
                "title": "Chat Session",  # Placeholder - C# will use database value
                "starred": False,  # Placeholder - C# will use database value
                "archived": False,  # Placeholder - C# will use database value
                "aiGenerateTitle": True  # Placeholder - C# will use database value
            }
            session_list.append(session_data)
        
        logger.info(f"📋 Returning {len(session_list)} sessions")
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
    Get a specific session with its chat history from the ADK session service
    """
    try:
        logger.info("="*50)
        logger.info(f"📋 Getting session with chats for app: {app_name}, user: {user_id}, session: {session_id}")
        
        # Create session service
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")
        
        # Get the specific session
        logger.info(f"🔍 Retrieving session for app: {app_name}, user: {user_id}, session: {session_id}")
        session = await session_service.get_session(app_name=app_name, user_id=user_id, session_id=session_id)
        
        if not session:
            logger.info(f"📭 Session not found: {session_id}")
            raise HTTPException(status_code=404, detail=f"Session {session_id} not found")
        
        logger.info(f"✅ Found session: {session_id}")
        
        # Get conversation history from session events
        logger.info(f"💬 Retrieving conversation history for session: {session_id}")
        conversation_history = []
        if session and hasattr(session, 'events'):
            conversation_history = session.events
            logger.info(f"📝 Found {len(conversation_history)} events in session")
        
        logger.info(f"✅ Found {len(conversation_history) if conversation_history else 0} conversation items")
        
        # Handle different ADK Session object attributes
        session_id_attr = getattr(session, 'id', getattr(session, 'session_id', getattr(session, 'sessionId', session_id)))
        updated_at_attr = getattr(session, 'updated_at', getattr(session, 'updatedAt', getattr(session, 'last_updated', None)))
        
        # Provide default timestamp if none available (C# expects non-nullable DateTime)
        default_timestamp = datetime.now(timezone.utc)
        timestamp_iso = updated_at_attr.isoformat() if updated_at_attr else default_timestamp.isoformat()
        
        # Format the response to match expected C# SessionWithChats structure
        session_with_chats = {
            "session": {
                "id": session_id_attr,
                "userId": int(user_id),
                "status": "Active",
                "lastUpdated": timestamp_iso,
                "title": "Chat Session",
                "starred": False,
                "archived": False,
                "aiGenerateTitle": True
            },
            "chatMessages": []  # Changed from "chats" to match C# property name
        }
        
        # Convert conversation history to chat format
        if conversation_history:
            chat_items = []  # Collect all chat items first for sorting
            response_formatter_messages = {}  # Track response_formatter_agent messages by invocation_id
            
            for conv_item in conversation_history:
                # Handle different ADK message formats
                message_content = ""
                inline_data = []
                role = "user"
                timestamp = None
                author = getattr(conv_item, 'author', 'user')
                invocation_id = getattr(conv_item, 'invocation_id', None)
                
                logger.debug(f"📝 Processing conversation item: author={author}, invocation_id={invocation_id}")
                
                if hasattr(conv_item, 'content'):
                    # ADK Content object
                    if hasattr(conv_item.content, 'parts') and conv_item.content.parts:
                        # Extract text and inlineData from parts
                        text_parts = []
                        logger.debug(f"🔍 Processing {len(conv_item.content.parts)} parts for {author}")
                        for i, part in enumerate(conv_item.content.parts):
                            logger.debug(f"   Part {i}: has text={hasattr(part, 'text')}, has inlineData={hasattr(part, 'inlineData')}")
                            if hasattr(part, 'text') and part.text:
                                text_parts.append(part.text)
                                logger.debug(f"   Part {i} text: {part.text[:50]}...")
                            # Check for inlineData with different possible attribute names
                            inline_data_obj = None
                            if hasattr(part, 'inlineData') and part.inlineData:
                                inline_data_obj = part.inlineData
                            elif hasattr(part, 'inline_data') and part.inline_data:
                                inline_data_obj = part.inline_data
                            elif hasattr(part, 'data') and part.data:
                                inline_data_obj = part.data
                            
                            if inline_data_obj:
                                # Debug: Log the actual structure of the inlineData object
                                logger.debug(f"🔍 inlineData_obj type: {type(inline_data_obj)}")
                                logger.debug(f"🔍 inlineData_obj attributes: {dir(inline_data_obj)}")
                                
                                # Log all available attributes and their values
                                for attr in dir(inline_data_obj):
                                    if not attr.startswith('_'):
                                        try:
                                            value = getattr(inline_data_obj, attr)
                                            if value is not None:
                                                logger.debug(f"🔍 {attr}: {type(value)} = {str(value)[:100]}")
                                        except Exception as e:
                                            logger.debug(f"⚠️ Could not read {attr}: {e}")
                                
                                # Extract the actual data from the ADK object and safely convert to string
                                raw_data = getattr(inline_data_obj, 'data', '')
                                data_str = safe_convert_to_string(raw_data)
                                
                                # Get MIME type with fallback detection
                                mime_type = getattr(inline_data_obj, 'mimeType', None)
                                if not mime_type:
                                    # Try alternative attribute names
                                    mime_type = getattr(inline_data_obj, 'mime_type', None) or getattr(inline_data_obj, 'type', None)
                                
                                # If still no MIME type, detect from data or filename
                                if not mime_type:
                                    display_name = getattr(inline_data_obj, 'displayName', '') or getattr(inline_data_obj, 'display_name', '') or getattr(inline_data_obj, 'filename', '')
                                    mime_type = detect_mime_type_from_data(raw_data, display_name)
                                    logger.debug(f"🔧 Auto-detected MIME type: {mime_type} for {display_name}")
                                
                                # Ensure we have a valid MIME type
                                final_mime_type = safe_convert_to_string(mime_type) if mime_type else "application/octet-stream"
                                
                                inline_data_dict = {
                                    "mimeType": final_mime_type,
                                    "displayName": safe_convert_to_string(getattr(inline_data_obj, 'displayName', '') or getattr(inline_data_obj, 'display_name', '') or getattr(inline_data_obj, 'filename', '')),
                                    "data": data_str
                                }
                                inline_data.append(inline_data_dict)
                                logger.debug(f"📎 Found inlineData: mimeType={inline_data_dict['mimeType']}, displayName={inline_data_dict['displayName']}, dataLength={len(inline_data_dict['data']) if inline_data_dict['data'] else 0}")
                                logger.debug(f"🔧 Final MIME type assignment: {final_mime_type} (original: {mime_type})")
                                
                                # Debug: Check if data looks like valid base64
                                if inline_data_dict['data']:
                                    data_sample = inline_data_dict['data'][:20] if len(inline_data_dict['data']) > 20 else inline_data_dict['data']
                                    logger.debug(f"🔍 Data sample: {data_sample}")
                        message_content = safe_convert_to_string(" ".join(text_parts))
                    role = getattr(conv_item.content, 'role', author)
                elif hasattr(conv_item, 'parts'):
                    # Direct parts access
                    text_parts = []
                    for part in conv_item.parts:
                        if hasattr(part, 'text') and part.text:
                            text_parts.append(part.text)
                        # Check for inlineData with different possible attribute names
                        inline_data_obj = None
                        if hasattr(part, 'inlineData') and part.inlineData:
                            inline_data_obj = part.inlineData
                        elif hasattr(part, 'inline_data') and part.inline_data:
                            inline_data_obj = part.inline_data
                        elif hasattr(part, 'data') and part.data:
                            inline_data_obj = part.data
                        
                        if inline_data_obj:
                            # Debug: Log the actual structure of the inlineData object
                            logger.debug(f"🔍 inlineData_obj type: {type(inline_data_obj)}")
                            logger.debug(f"🔍 inlineData_obj attributes: {dir(inline_data_obj)}")
                            
                            # Extract the actual data from the ADK object and safely convert to string
                            raw_data = getattr(inline_data_obj, 'data', '')
                            data_str = safe_convert_to_string(raw_data)
                            
                            # Get MIME type with fallback detection
                            mime_type = getattr(inline_data_obj, 'mimeType', None)
                            if not mime_type:
                                # Try alternative attribute names
                                mime_type = getattr(inline_data_obj, 'mime_type', None) or getattr(inline_data_obj, 'type', None)
                            
                            # If still no MIME type, detect from data or filename
                            if not mime_type:
                                display_name = getattr(inline_data_obj, 'displayName', '') or getattr(inline_data_obj, 'display_name', '') or getattr(inline_data_obj, 'filename', '')
                                mime_type = detect_mime_type_from_data(raw_data, display_name)
                                logger.debug(f"🔧 Auto-detected MIME type: {mime_type} for {display_name}")
                            
                            # Ensure we have a valid MIME type
                            final_mime_type = safe_convert_to_string(mime_type) if mime_type else "application/octet-stream"
                            
                            inline_data_dict = {
                                "mimeType": final_mime_type,
                                "displayName": safe_convert_to_string(getattr(inline_data_obj, 'displayName', '') or getattr(inline_data_obj, 'display_name', '') or getattr(inline_data_obj, 'filename', '')),
                                "data": data_str
                            }
                            inline_data.append(inline_data_dict)
                            logger.debug(f"📎 Found inlineData: mimeType={inline_data_dict['mimeType']}, displayName={inline_data_dict['displayName']}, dataLength={len(inline_data_dict['data']) if inline_data_dict['data'] else 0}")
                            
                            # Debug: Check if data looks like valid base64
                            if inline_data_dict['data']:
                                data_sample = inline_data_dict['data'][:20] if len(inline_data_dict['data']) > 20 else inline_data_dict['data']
                                logger.debug(f"🔍 Data sample: {data_sample}")
                    message_content = safe_convert_to_string(" ".join(text_parts))
                    role = getattr(conv_item, 'role', author)
                else:
                    # Fallback to direct attributes
                    message_content = safe_convert_to_string(getattr(conv_item, 'text', getattr(conv_item, 'message', '')))
                    role = getattr(conv_item, 'role', author)
                
                # Debug logging for inlineData after extraction
                if inline_data:
                    logger.debug(f"📎 Total inlineData items for {author}: {len(inline_data)}")
                
                # Get timestamp and handle different formats
                timestamp = None
                if hasattr(conv_item, 'timestamp'):
                    timestamp = conv_item.timestamp
                elif hasattr(conv_item, 'created_at'):
                    timestamp = conv_item.created_at
                
                # Convert timestamp to ISO format
                timestamp_iso = None
                if timestamp:
                    try:
                        if isinstance(timestamp, (int, float)):
                            # Unix timestamp - convert to datetime
                            timestamp_iso = datetime.fromtimestamp(timestamp, tz=timezone.utc).isoformat()
                        elif hasattr(timestamp, 'isoformat'):
                            # Already a datetime object
                            timestamp_iso = timestamp.isoformat()
                        else:
                            # Try to convert string or other format
                            timestamp_iso = str(timestamp)
                    except Exception as ts_error:
                        logger.warning(f"⚠️ Failed to convert timestamp {timestamp}: {ts_error}")
                        timestamp_iso = None
                
                # Include messages that have either text content or inlineData
                if message_content or inline_data:
                    # Convert timestamp string to datetime if needed
                    timestamp_dt = None
                    if timestamp_iso:
                        try:
                            timestamp_dt = datetime.fromisoformat(timestamp_iso.replace('Z', '+00:00'))
                        except:
                            timestamp_dt = None
                    
                    # Handle assistant response messages (response_formatter_agent and TaskExecutorAgent)
                    if author in ["response_formatter_agent"] and invocation_id:
                        # Store only the latest assistant message per invocation_id
                        if invocation_id not in response_formatter_messages or (
                            timestamp_dt and (
                                not response_formatter_messages[invocation_id].get('_timestamp') or 
                                timestamp_dt > response_formatter_messages[invocation_id]['_timestamp']
                            )
                        ):
                            response_formatter_messages[invocation_id] = {
                                "role": "assistant",  # Map assistant agents to assistant role
                                "text": message_content,
                                "timestamp": timestamp_dt.isoformat() if timestamp_dt else None,
                                "inlineData": inline_data,
                                "_timestamp": timestamp_dt,  # Keep for comparison
                                "_sort_timestamp": timestamp_dt if timestamp_dt else datetime.min
                            }
                    elif author == "user":
                        # Always include user messages
                        chat_item = {
                            "role": "user",
                            "text": message_content,
                            "timestamp": timestamp_dt.isoformat() if timestamp_dt else None,
                            "inlineData": inline_data,
                            "_sort_timestamp": timestamp_dt if timestamp_dt else datetime.min
                        }
                        chat_items.append(chat_item)
                    # Skip other agent messages (api_caller_agent, etc.)
            
            # Add filtered assistant response messages to chat_items
            logger.info(f"🔍 Found {len(response_formatter_messages)} unique assistant responses across invocations")
            for invocation_id, response_msg in response_formatter_messages.items():
                logger.info(f"   - Invocation {invocation_id}: {response_msg['text'][:50]}...")
                # Remove the temporary timestamp field
                response_msg.pop('_timestamp', None)
                chat_items.append(response_msg)
            
            # Sort chat items by timestamp (oldest first for conversation flow)
            chat_items.sort(key=lambda x: x['_sort_timestamp'])
            
            # Remove the temporary sort field and add to session
            for chat_item in chat_items:
                del chat_item['_sort_timestamp']
                session_with_chats["chatMessages"].append(chat_item)
        
        logger.info(f"📋 Returning session with {len(session_with_chats['chatMessages'])} chat items")
        return session_with_chats
        
    except HTTPException:
        # Re-raise HTTP exceptions (like 404) as-is
        raise
    except Exception as e:
        logger.error(f"❌ Error retrieving session with chats: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Failed to retrieve session with chats: {str(e)}")


@router.get("/session-data")
async def get_session_data(
    app_name: str = Query(..., description="Application name"),
    user_id: str = Query(..., description="User ID"),
    session_id: str = Query(..., description="Session ID to retrieve")
):
    """
    Get basic session data without chat history from the ADK session service
    """
    try:
        logger.info("="*50)
        logger.info(f"📋 Getting session data for app: {app_name}, user: {user_id}, session: {session_id}")
        
        # Create session service
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")
        
        # Get the specific session
        logger.info(f"🔍 Retrieving session for app: {app_name}, user: {user_id}, session: {session_id}")
        session = await session_service.get_session(app_name=app_name, user_id=user_id, session_id=session_id)
        
        if not session:
            logger.info(f"📭 Session not found: {session_id}")
            return []
        
        logger.info(f"✅ Found session: {session_id}")
        
        # Handle different ADK Session object attributes
        session_id_attr = getattr(session, 'id', getattr(session, 'session_id', getattr(session, 'sessionId', session_id)))
        updated_at_attr = getattr(session, 'updated_at', getattr(session, 'updatedAt', getattr(session, 'last_updated', None)))
        
        # Provide default timestamp if none available (C# expects non-nullable DateTime)
        default_timestamp = datetime.now(timezone.utc)
        timestamp_iso = updated_at_attr.isoformat() if updated_at_attr else default_timestamp.isoformat()
        
        # Format the response to match expected structure (as array for compatibility)
        session_data = [{
            "id": session_id_attr,
            "userId": int(user_id),
            "status": "Active",
            "lastUpdated": timestamp_iso,
            "title": "Chat Session",
            "starred": False,
            "archived": False,
            "aiGenerateTitle": True
        }]
        
        logger.info(f"📋 Returning session data")
        return session_data
        
    except Exception as e:
        logger.error(f"❌ Error retrieving session data: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Failed to retrieve session data: {str(e)}")                