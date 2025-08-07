"""
Session Router

This module contains session-related endpoints like title generation
for better organization.
"""

import logging
import os
import json
from fastapi import APIRouter, HTTPException, Request, Query
from google.adk.sessions import DatabaseSessionService

from ai_assistant.utils.api_config_manager import config_manager

# Vertex AI imports for title generation
import vertexai
from vertexai.generative_models import GenerativeModel, GenerationConfig, HarmBlockThreshold, HarmCategory

logger = logging.getLogger(__name__)

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
                                next_message.author in ["user_request_agent", "response_formatter_agent", "entity_detection_agent", "api_caller_agent"]):
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
async def get_user_sessions(
    app_name: str = Query(..., description="Application name"),
    user_id: str = Query(..., description="User ID to retrieve sessions for")
):
    """
    Get all sessions for a specific user from the ADK session service
    """
    try:
        logger.info("="*50)
        logger.info(f"📋 Getting user sessions for app: {app_name}, user: {user_id}")
        
        # Create session service
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")
        
        # Get all sessions for the user
        logger.info(f"🔍 Retrieving sessions for app: {app_name}, user: {user_id}")
        sessions = await session_service.list_sessions(app_name=app_name, user_id=user_id)
        
        if not sessions:
            logger.info(f"📭 No sessions found for user {user_id} in app {app_name}")
            return []
        
        logger.info(f"✅ Found {len(sessions)} sessions for user {user_id}")
        
        # Convert sessions to a serializable format
        session_list = []
        for session in sessions:
            session_data = {
                "id": session.session_id,
                "userId": int(user_id),
                "status": "Active",  # Default status
                "lastUpdated": session.updated_at.isoformat() if session.updated_at else None,
                "title": "Chat Session",  # Default title - could be enhanced later
                "starred": False,  # Default values
                "archived": False,
                "aiGenerateTitle": True
            }
            session_list.append(session_data)
        
        logger.info(f"📋 Returning {len(session_list)} sessions")
        return session_list
        
    except Exception as e:
        logger.error(f"❌ Error retrieving user sessions: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Failed to retrieve user sessions: {str(e)}")                