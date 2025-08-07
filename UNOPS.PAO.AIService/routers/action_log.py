"""
Action Log Router

This module contains endpoints related to AI action logging,
including the generate-suggestions endpoint.
"""

import logging
from typing import List
from fastapi import APIRouter, HTTPException, Query
from pydantic import BaseModel

from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.utils.database_manager import db_manager

logger = logging.getLogger(__name__)

# Create router
router = APIRouter()


class SuggestionsResponse(BaseModel):
    """Response model for suggestions endpoint"""
    suggestions: List[str]
    user_id: int
    total_actions_found: int


@router.get("/generate-suggestions", response_model=SuggestionsResponse)
async def generate_suggestions(
    user_id: str = Query(..., description="User ID to generate suggestions for")
):
    """
    Generate AI suggestions based on the user's recent conversation history.
    
    This endpoint looks at the last 3 sessions for the user and the last 3 chats
    from those sessions, then uses Gemini to generate 3 personalized suggestions
    based on their recent conversation patterns.
    
    Args:
        user_id: The ID of the user to generate suggestions for
        
    Returns:
        SuggestionsResponse: Contains an array of 3 suggestion strings
        
    Raises:
        HTTPException: If there are errors retrieving conversation history
    """
    try:
        logger.info("="*50)
        logger.info("💡 GENERATE SUGGESTIONS REQUEST")
        logger.info("="*50)
        logger.info(f"🔍 Request Details:")
        logger.info(f"   - user_id: {user_id}")
        logger.info("="*50)

        # Get recent conversation history from sessions
        logger.info(f"📋 Retrieving recent conversations for user {user_id}...")
        recent_conversations = await _get_recent_conversations(user_id)
        
        if not recent_conversations:
            logger.info(f"📭 No recent conversations found for user {user_id}")
            return SuggestionsResponse(
                suggestions=[
                    "Start by asking me about your projects or data",
                    "I can help you search for information or create documents", 
                    "Try asking me to visualize your data or generate reports"
                ],
                user_id=int(user_id),
                total_actions_found=0
            )

        logger.info(f"📊 Found {len(recent_conversations)} recent conversations for user {user_id}")

        # Generate suggestions using Gemini
        suggestions = await _generate_suggestions_with_gemini_from_conversations(recent_conversations, user_id)
        
        logger.info(f"✅ Generated {len(suggestions)} suggestions for user {user_id}")
        
        # Log this suggestion generation action (if action logging is enabled)
        try:
            config = config_manager.framework_config
            action_logging_config = config.get('action_logging', {})
            
            if action_logging_config.get('enabled', False):
                summary = f"Generated {len(suggestions)} personalized suggestions based on recent conversations"
                success = db_manager.insert_action_log(
                    summary=summary,
                    user_id=int(user_id),
                    created_by=int(user_id),
                    entity_type="suggestions",
                    changes_log={"suggestions": suggestions, "conversations_analyzed": len(recent_conversations)}
                )
                if success:
                    logger.info(f"✅ Logged suggestion generation action for user {user_id}")
                else:
                    logger.warning(f"⚠️ Failed to log suggestion generation action for user {user_id}")
        except Exception as log_error:
            logger.error(f"❌ Error logging suggestion generation: {log_error}")
        
        return SuggestionsResponse(
            suggestions=suggestions,
            user_id=int(user_id),
            total_actions_found=len(recent_conversations)
        )

    except HTTPException:
        # Re-raise HTTP exceptions as-is
        raise
    except Exception as e:
        logger.error(f"❌ Error generating suggestions: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Internal server error while generating suggestions: {str(e)}"
        )


async def _generate_suggestions_with_gemini(context_data: List[dict], user_id: int) -> List[str]:
    """
    Use Gemini to generate personalized suggestions based on user's action history.
    
    Args:
        context_data: List of recent action data for the user
        user_id: The user ID for context
        
    Returns:
        List[str]: List of 3 suggestion strings
    """
    try:
        logger.info("🤖 Generating suggestions with Gemini...")
        
        # Import Gemini dependencies
        try:
            import google.generativeai as genai
            from google.generativeai.types import HarmCategory, HarmBlockThreshold
        except ImportError:
            logger.error("❌ google-generativeai not available, using fallback suggestions")
            return _get_fallback_suggestions()
        
        # Get Gemini model configuration
        gemini_model = config_manager.get_gemini_model()
        logger.info(f"🔧 Using Gemini model: {gemini_model}")
        
        # Prepare the prompt
        context_text = ""
        for i, action in enumerate(context_data, 1):
            context_text += f"{i}. {action['summary']}\n"
            if action['entity_type']:
                context_text += f"   Entity: {action['entity_type']}\n"
            if action['created_date']:
                context_text += f"   Date: {action['created_date']}\n"
            context_text += "\n"
        
        prompt = f"""
Based on this user's recent AI interaction history, generate exactly 3 personalized and actionable suggestions for what they might want to do next. 

Recent interactions:
{context_text}

Guidelines:
- Make suggestions specific and actionable
- Consider the user's interaction patterns and topics
- Each suggestion should be a complete sentence
- Focus on practical next steps or related tasks
- Keep suggestions concise (under 100 characters each)
- Return ONLY the 3 suggestions, one per line, without numbers or bullets
- Do not include any extra text, explanations, or formatting

Example format:
Check the status of your recent project updates
Review pending approvals in your workflow
Search for related documents from last month
"""

        # Configure the model
        model = genai.GenerativeModel(
            model_name=gemini_model,
            generation_config={
                "temperature": 0.7,
                "top_p": 0.8,
                "top_k": 40,
                "max_output_tokens": 500,
            },
            safety_settings={
                HarmCategory.HARM_CATEGORY_HARASSMENT: HarmBlockThreshold.BLOCK_MEDIUM_AND_ABOVE,
                HarmCategory.HARM_CATEGORY_HATE_SPEECH: HarmBlockThreshold.BLOCK_MEDIUM_AND_ABOVE,
                HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT: HarmBlockThreshold.BLOCK_MEDIUM_AND_ABOVE,
                HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT: HarmBlockThreshold.BLOCK_MEDIUM_AND_ABOVE,
            }
        )

        # Generate suggestions
        logger.info("🧠 Calling Gemini API...")
        response = model.generate_content(prompt)
        
        if not response or not response.text:
            logger.warning("⚠️ Empty response from Gemini, using fallback suggestions")
            return _get_fallback_suggestions()
        
        # Parse the response into individual suggestions
        suggestions_text = response.text.strip()
        suggestions = []
        
        for line in suggestions_text.split('\n'):
            line = line.strip()
            if line and not line.startswith(('-', '*', '•', '1.', '2.', '3.')):
                # Remove any numbering or bullets that might have been added
                line = line.lstrip('123.-*•').strip()
                if line:
                    suggestions.append(line)
        
        # Ensure we have exactly 3 suggestions
        if len(suggestions) < 3:
            logger.warning(f"⚠️ Only got {len(suggestions)} suggestions, adding fallbacks")
            fallbacks = _get_fallback_suggestions()
            suggestions.extend(fallbacks[len(suggestions):])
        elif len(suggestions) > 3:
            suggestions = suggestions[:3]
        
        logger.info(f"✅ Generated {len(suggestions)} suggestions successfully")
        return suggestions

    except Exception as e:
        logger.error(f"❌ Error calling Gemini for suggestions: {e}")
        logger.info("🔄 Falling back to default suggestions")
        return _get_fallback_suggestions()


async def _get_recent_conversations(user_id: str) -> List[dict]:
    """
    Get recent conversations from the user's recent sessions.
    
    Args:
        user_id: The user ID to get conversations for
        
    Returns:
        List[dict]: List of recent conversation data
    """
    try:
        # Import required modules
        from google.adk.sessions import DatabaseSessionService
        
        # Get database URL and create session service
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        
        logger.info(f"🔧 Created session service for recent conversations retrieval")
        
        # Access the database session factory to query sessions directly
        conversations = []
        
        with session_service.database_session_factory() as db_session:
            # Import the StorageSession model from the session service module
            from google.adk.sessions.database_session_service import StorageSession
            
            # Get the last 3 sessions for this user
            recent_sessions = db_session.query(StorageSession)\
                                     .filter(StorageSession.user_id == user_id)\
                                     .filter(StorageSession.app_name == "ai_assistant")\
                                     .order_by(StorageSession.update_time.desc())\
                                     .limit(3)\
                                     .all()
            
            logger.info(f"📊 Found {len(recent_sessions)} recent sessions for user {user_id}")
            
            # For each session, get the session object with events
            for storage_session in recent_sessions:
                try:
                    # Get the full session with events using the session service
                    session = await session_service.get_session(
                        app_name="ai_assistant",
                        user_id=user_id,
                        session_id=storage_session.id
                    )
                    
                    if session and hasattr(session, 'events') and session.events:
                        logger.info(f"📋 Found {len(session.events)} events in session {storage_session.id}")
                        
                        # Get the last 3 user and assistant message pairs
                        conversation_count = 0
                        for i, message in enumerate(session.events):
                            if conversation_count >= 3:  # Limit to last 3 conversations
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
                                    # Look for the assistant response
                                    assistant_message = ""
                                    for j in range(i + 1, len(session.events)):
                                        next_message = session.events[j]
                                        if (hasattr(next_message, 'author') and
                                            next_message.author in ["user_request_agent", "response_formatter_agent", "entity_detection_agent", "api_caller_agent"]):
                                            if hasattr(next_message, 'content') and next_message.content and hasattr(next_message.content, 'parts'):
                                                assistant_parts = []
                                                for part in next_message.content.parts:
                                                    if hasattr(part, 'text') and part.text:
                                                        assistant_parts.append(part.text)
                                                assistant_message = " ".join(assistant_parts)
                                            if assistant_message:
                                                break
                                    
                                    # Add the conversation pair
                                    conversation_pair = f"User: {user_message}"
                                    if assistant_message:
                                        # Clean up JSON formatting if present
                                        if assistant_message.startswith("```json"):
                                            assistant_message = assistant_message.replace("```json", "").replace("```", "").strip()
                                        try:
                                            import json
                                            parsed = json.loads(assistant_message)
                                            if isinstance(parsed, dict) and 'result' in parsed:
                                                # Extract the main message from the result array
                                                result = parsed['result']
                                                if isinstance(result, list) and len(result) > 0:
                                                    for item in result:
                                                        if isinstance(item, dict) and item.get('type') == 'markdown':
                                                            assistant_message = item.get('message', assistant_message)
                                                            break
                                        except:
                                            pass  # Keep original message if JSON parsing fails
                                        
                                        conversation_pair += f" | Assistant: {assistant_message[:300]}..."  # Limit response length
                                    
                                    conversations.append({
                                        'session_id': storage_session.id,
                                        'timestamp': getattr(message, 'timestamp', None),
                                        'content': conversation_pair,
                                        'user_message': user_message,
                                        'assistant_message': assistant_message[:200] if assistant_message else ""
                                    })
                                    conversation_count += 1
                                    
                except Exception as session_error:
                    logger.warning(f"⚠️ Error processing session {storage_session.id}: {session_error}")
                    continue
        
        logger.info(f"✅ Retrieved {len(conversations)} total conversations for user {user_id}")
        return conversations
        
    except Exception as e:
        logger.error(f"❌ Error retrieving recent conversations: {e}")
        import traceback
        logger.error(f"Full traceback: {traceback.format_exc()}")
        return []


async def _generate_suggestions_with_gemini_from_conversations(conversations: List[dict], user_id: str) -> List[str]:
    """
    Use Gemini to generate personalized suggestions based on user's recent conversations.
    
    Args:
        conversations: List of recent conversation data for the user
        user_id: The user ID for context
        
    Returns:
        List[str]: List of 3 suggestion strings
    """
    try:
        logger.info("🤖 Generating suggestions with Gemini from conversations...")
        
        # Import required function from session.py
        from routers.session import call_gemini_direct
        
        # Prepare the conversation context
        context_text = ""
        for i, conv in enumerate(conversations, 1):
            # Format conversation content for context
            content_preview = str(conv['content'])[:200] + "..." if len(str(conv['content'])) > 200 else str(conv['content'])
            context_text += f"{i}. Session: {conv['session_id'][:8]}... | {content_preview}\n"
            if conv['timestamp']:
                context_text += f"   Time: {conv['timestamp']}\n"
            context_text += "\n"
        
        prompt = f"""
Analyze the following recent conversation history and generate exactly 3 personalized, intelligent suggestions for what the user might want to do next. Make the suggestions specific to their actual conversation topics and patterns.

Recent Conversations:
{context_text}

SMART SUGGESTION RULES:
1. **Analyze conversation topics** - Look for specific entities, names, projects, or subjects mentioned
2. **Identify patterns** - What type of information were they seeking? What tasks were they working on?
3. **Suggest logical next steps** - What would naturally follow from their recent activities?
4. **Be specific and actionable** - Include specific names, entities, or projects when mentioned
5. **Make each suggestion unique** - Don't repeat similar concepts

SUGGESTION TYPES TO CONSIDER:
- Continue/follow up on specific entities mentioned (e.g., "Get more details about [Specific Partner Name]")
- Create outputs from discussed data (e.g., "Export [specific data] to Google Sheets") 
- Explore related information (e.g., "Search for contacts in [specific sector mentioned]")
- Generate visualizations (e.g., "Create a diagram of [specific topic discussed]")
- Perform next logical actions (e.g., "Update [specific entity] with recent changes")

Examples of GOOD context-aware suggestions:
- "Continue editing Partner ABC Corp details" (when partner ABC Corp was discussed)
- "Get more information about Bill & Melinda Gates Foundation" (when this foundation was mentioned)
- "Create a report on healthcare partnerships" (when healthcare partnerships were discussed)
- "Search for contacts in renewable energy sector" (when renewable energy was a topic)

IMPORTANT: Make suggestions that feel like natural continuations of their actual conversations, not generic suggestions.

Return exactly 3 suggestions, one per line, no numbering or bullets or extra text.
"""
        
        logger.info("🚀 Sending conversation context to Gemini for suggestion generation...")
        
        # Use the same call_gemini_direct function as generate-title
        gemini_model = config_manager.get_gemini_adhoc_model()
        generated_text = call_gemini_direct(
            prompt=prompt,
            model_name=gemini_model,
            max_tokens=200,  # Slightly larger for 3 suggestions
            temperature=0.7  # Good balance for creative but relevant suggestions
        )
        
        # Parse the response into individual suggestions
        suggestions = [s.strip() for s in generated_text.split('\n') if s.strip()]
        
        # Ensure we have exactly 3 suggestions
        if len(suggestions) >= 3:
            final_suggestions = suggestions[:3]
        else:
            # Pad with fallback suggestions if needed
            final_suggestions = suggestions + _get_fallback_suggestions()[len(suggestions):3]
        
        logger.info(f"✅ Generated suggestions from conversations: {final_suggestions}")
        return final_suggestions
        
    except Exception as e:
        logger.error(f"❌ Error generating suggestions from conversations: {e}")
        return _get_fallback_suggestions()


def _get_fallback_suggestions() -> List[str]:
    """
    Get fallback suggestions when Gemini is unavailable or fails.
    
    Returns:
        List[str]: List of 3 default suggestion strings
    """
    return [
        "Start by asking me about your projects or data",
        "I can help you search for information or create documents",
        "Try asking me to visualize your data or generate reports"
    ]