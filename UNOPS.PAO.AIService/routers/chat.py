"""
Chat Router

This module contains the chat endpoint and related functionality
extracted from main.py for better organization.
"""

import logging
import uuid
import json
from typing import List, Any, Dict, Union
from fastapi import APIRouter, HTTPException, Request, Form, File, UploadFile
from fastapi.responses import StreamingResponse, FileResponse
from google.adk.agents import RunConfig
from google.adk.agents.run_config import StreamingMode
from google.adk.runners import Runner
from google.adk.sessions import DatabaseSessionService
from google.genai import types
from pydantic import BaseModel

from ai_assistant.utils.config import get_database_url, get_config
from ai_assistant.utils.session_management import get_or_create_session, parse_request_state, update_session_state_in_database
from ai_assistant.utils.iap_validation import validate_iap_headers, extract_iap_headers_for_forwarding

logger = logging.getLogger(__name__)

# Create router
router = APIRouter()


async def translate_thought_to_non_technical(event_json_str: str) -> str:
    """
    Check if the SSE event is a thought and translate it to non-technical language.
    
    Args:
        event_json_str: JSON string of the SSE event
        
    Returns:
        JSON string of the event (modified if it was a thought, original otherwise)
    """
    try:
        # Parse the event
        event_data = json.loads(event_json_str)
        
        # Check if this is a thought event
        content = event_data.get('content', {})
        parts = content.get('parts', [])
        
        # Look for a thought part
        thought_found = False
        for part in parts:
            if isinstance(part, dict) and part.get('thought', False):
                thought_found = True
                original_text = part.get('text', '')
                
                if original_text:
                    # Translate the thought to non-technical language
                    translated_text = await _translate_with_gemini(original_text)
                    
                    # Update the text in the event
                    part['text'] = translated_text
                    logger.info(f"Translated thought: {original_text[:50]}... -> {translated_text[:50]}...")
                    
        # Return the modified event as JSON string
        return json.dumps(event_data)
        
    except Exception as e:
        logger.error(f"Error translating thought: {e}")
        # Return original event if translation fails
        return event_json_str


async def _translate_with_gemini(thought_text: str) -> str:
    """
    Use Gemini to translate technical thought text to non-technical language.
    
    Args:
        thought_text: The original technical thought text
        
    Returns:
        Translated non-technical text
    """
    try:
        # Get configuration
        config = get_config()
        google_cloud_config = config.get("google_cloud", {})
        project_id = google_cloud_config.get("project")
        location = google_cloud_config.get("location", "us-central1")
        
        # Initialize Google GenAI Client
        from google.genai import Client
        
        # Create prompt for translation
        prompt = f"""You are translating AI assistant internal thoughts into user-friendly language.

Original thought (contains technical details):
{thought_text}

Rewrite this thought to be conversational and non-technical. Remove any mentions of:
- Tool names (like google_search, invoke_app_api, etc.)
- Technical endpoints or API calls
- Parameter names or JSON structures
- Function calls or code references
- System prompts or instructions

Instead, focus on:
- What the assistant is trying to accomplish
- The strategy or approach being taken
- Why this approach makes sense
- What the user can expect next

Keep the tone friendly, conversational, and fun. Use natural language that a non-technical user would understand.
Maintain the same title as the original thought and maintain the same general style and fun attitude of the original thought.

Translated thought (user-friendly):"""

        # Use Google GenAI Client async API
        aclient = Client(vertexai=True, project=project_id, location=location).aio
        
        # try:
        response = await aclient.models.generate_content(
            model="gemini-2.0-flash-lite",
            contents=prompt,
            config=types.GenerateContentConfig(
                temperature=0.7,
                max_output_tokens=200,
                top_p=0.8,
                top_k=40
            )
        )
        
        # Extract the translated text
        if response.text:
            translated_text = response.text.strip()
            return translated_text
        else:
            logger.warning("Gemini returned no text, using original text")
            return thought_text
        # finally:
        #     # Close the async client to release resources
        #     if aclient.aclose:
        #         await aclient.aclose()
            
    except Exception as e:
        logger.error(f"Error calling Gemini for thought translation: {e}")
        # Return original text if translation fails
        return thought_text


@router.get("/test-stream")
async def test_stream():
    """Test endpoint to verify streaming is working without AI processing"""
    
    async def generate():
        import time
        import json
        import sys
        import asyncio
        
        # Send immediate ping
        ping_data = f"data: {json.dumps({'ping': 'test_started', 'timestamp': time.time()})}\n\n"
        yield ping_data
        
        for i in range(5):
            test_data = {
                "test_message": f"This is test chunk {i+1}",
                "timestamp": time.time(),
                "chunk_id": i+1
            }
            
            data = f"data: {json.dumps(test_data)}\n\n"
            yield data
            
            # Add a small delay to make streaming visible
            await asyncio.sleep(1)
        
        # Final message
        final_data = {"test_message": "Stream complete", "final": True}
        yield f"data: {json.dumps(final_data)}\n\n"

    return StreamingResponse(
        generate(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache", 
            "Expires": "0",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no",  # nginx
            "X-Proxy-Buffering": "no",  # other proxies
            "Transfer-Encoding": "chunked",
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
            "Access-Control-Allow-Headers": "Content-Type, Authorization",
        }
    )


@router.get("/test-streaming-ui")
async def test_streaming_ui():
    """Serve the HTML test page for streaming functionality"""
    import os
    
    # Get the absolute path to the HTML file
    current_dir = os.path.dirname(os.path.abspath(__file__))
    parent_dir = os.path.dirname(current_dir)  # Go up one level from routers/
    html_path = os.path.join(parent_dir, "test_streaming.html")
    
    if not os.path.exists(html_path):
        raise HTTPException(status_code=404, detail=f"Test HTML file not found at {html_path}")
    
    return FileResponse(html_path, media_type="text/html")


class ChatRequest(BaseModel):
    """Request model for chat endpoint"""
    app_name: str
    user_id: str
    user_email: str
    session_id: str
    message: str
    streaming: bool = False
    state: Any = ""


@router.api_route("/chat", methods=["POST", "HEAD"])
async def chat_endpoint(
    request: Request,
    # Form fields (for multipart requests)
    app_name: str = Form(None),
    user_id: str = Form(None), 
    user_email: str = Form(None),
    session_id: str = Form(None),
    message: str = Form(None),
    streaming: str = Form(None),
    state: str = Form(None),
    # GCS file paths (JSON string array)
    gcs_files: str = Form(None),
    # File uploads
    files: List[UploadFile] = File(None)
):
    """
    Custom chat endpoint that handles both JSON and multipart form data with files
    Supports HEAD method for interceptor header capture
    """
    try:
        # Handle HEAD requests for interceptor support
        if request.method == "HEAD":
            from fastapi import Response
            response = Response()
            response.headers["Cache-Control"] = "no-cache, no-store, must-revalidate"
            response.headers["Pragma"] = "no-cache"
            response.headers["Expires"] = "0"
            response.headers["Access-Control-Allow-Origin"] = "*"
            response.headers["Access-Control-Allow-Methods"] = "GET, POST, HEAD, OPTIONS"
            response.headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization"
            return response
        
        # Determine request type and parse data
        content_type = request.headers.get("content-type", "")
        is_multipart = "multipart/form-data" in content_type
        logger.info(f"Processing {'multipart' if is_multipart else 'JSON'} request")
        
        # DEBUG: Log the incoming session_id to verify frontend is sending it correctly
        logger.info(f"📥 INCOMING REQUEST - session_id parameter: '{session_id}' (type: {type(session_id).__name__}, empty: {not session_id})")
        
        if is_multipart:
            request_data = ChatRequest(
                app_name=app_name,
                user_id=user_id,
                user_email=user_email,
                session_id=session_id or "",
                message=message or "",
                streaming=streaming == "true" if streaming else False,
                state=state or ""
            )
            
            # Handle files
            if files and any(f for f in files if f.filename):
                file_count = len([f for f in files if f.filename])
                logger.info(f"Received {file_count} file(s)")
            else:
                files = []
                
        else:
            # Handle JSON request (backward compatibility)
            body = await request.json()
            request_data = ChatRequest(**body)
            files = []  # No files in JSON requests

        # Import agent function here to avoid circular imports
        from ai_assistant.agent import root_agent, create_agent_with_context

        # Create session service
        db_url = get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)

        # Parse state
        user_email = request_data.user_email
        parsed_state = parse_request_state(request_data.state)

        # Prepare initial state
        initial_state = parsed_state or {}
        initial_state['user_email'] = user_email
        
        # Extract page context for dynamic agent creation (not for user message)
        # page_context = initial_state.get('page_context_auto') if initial_state else None

        # Convert uploaded files to types.Part objects
        message_parts = []
        
        # Add the main text message first
        text_message = request_data.message
        if not text_message and files and any(f.filename for f in files):
            text_message = " "  # Default blank space for file-only uploads
        
        if text_message:
            message_parts.append(types.Part(text=text_message))

        # Add uploaded files as Blob parts
        uploaded_artifact_parts = []
        audio_files_for_artifacts = []
        
        # Process GCS files first (preferred over raw file uploads)
        # Handle case where gcs_files might be the string "null" or empty
        if gcs_files and gcs_files.strip() and gcs_files.strip().lower() != 'null':
            try:
                gcs_file_list = json.loads(gcs_files)
                if not isinstance(gcs_file_list, list):
                    gcs_file_list = []
                logger.info(f"Processing {len(gcs_file_list)} GCS file(s)")
                
                for gcs_file in gcs_file_list:
                    gcs_path = gcs_file.get('gcsPath')
                    file_name = gcs_file.get('name', 'unknown')
                    mime_type = gcs_file.get('mimeType', 'application/pdf')
                    
                    if gcs_path:
                        logger.info(f"Adding GCS file: {file_name} from {gcs_path}")
                        
                        # Create a types.Part with file_uri for GCS files
                        file_part = types.Part.from_uri(
                            file_uri=gcs_path,
                            mime_type=mime_type
                        )
                        message_parts.append(file_part)
                        
                        # Check if this is an audio file for special handling
                        is_audio = mime_type.startswith('audio/')
                        if is_audio:
                            audio_files_for_artifacts.append({
                                "filename": file_name,
                                "mime_type": mime_type,
                                "gcs_path": gcs_path
                            })
                        
                        uploaded_artifact_parts.append({
                            "filename": file_name,
                            "mime_type": mime_type,
                            "gcs_path": gcs_path,
                            "is_audio": is_audio
                        })
            except json.JSONDecodeError as e:
                logger.error(f"Failed to parse gcs_files JSON: {e}")
        
        # Process raw file uploads as fallback
        # NOTE: Gemini API has a 1MB (1024KB) limit for inline_data parts
        MAX_INLINE_FILE_SIZE = 1024 * 1024  # 1MB in bytes
        
        if files:
            for uploaded_file in files:
                if uploaded_file.filename:
                    file_content = await uploaded_file.read()
                    mime_type = uploaded_file.content_type or "application/octet-stream"
                    
                    # Check file size - Gemini has 1MB limit for inline data
                    if len(file_content) > MAX_INLINE_FILE_SIZE:
                        logger.warning(f"⚠️ File '{uploaded_file.filename}' ({len(file_content)/1024:.1f}KB) exceeds 1MB limit for inline upload. Please upload to GCS first.")
                        # Add a text message indicating the file was too large
                        message_parts.append(types.Part(text=f"[File '{uploaded_file.filename}' was too large ({len(file_content)/1024:.1f}KB) for direct upload. Maximum is 1024KB. Please use GCS upload instead.]"))
                        continue
                    
                    # Create a types.Part for each file (under 1MB)
                    file_part = types.Part(
                        inline_data=types.Blob(
                            mime_type=mime_type,
                            data=file_content
                        )
                    )
                    message_parts.append(file_part)
                    
                    # Check if this is an audio file for special handling
                    is_audio = mime_type.startswith('audio/')
                    if is_audio:
                        audio_files_for_artifacts.append({
                            "filename": uploaded_file.filename,
                            "mime_type": mime_type,
                            "size": len(file_content)
                        })
                    
                    uploaded_artifact_parts.append({
                        "filename": uploaded_file.filename,
                        "mime_type": mime_type,
                        "size": len(file_content),
                        "is_audio": is_audio
                    })
                    
        if uploaded_artifact_parts:
            initial_state['uploaded_files_metadata'] = uploaded_artifact_parts
        
        if audio_files_for_artifacts:
            initial_state['audio_files_metadata'] = audio_files_for_artifacts

        # Get or create session
        logger.info(f"🔍 Session management: app={request_data.app_name}, user={request_data.user_id}")
        logger.info(f"📋 Session ID from request: '{request_data.session_id}' (is_empty: {not request_data.session_id or request_data.session_id.strip() == ''})")
        session, actual_session_id, is_new_session = await get_or_create_session(
            session_service = session_service,
            app_name = request_data.app_name,
            user_id = request_data.user_id,
            session_id = request_data.session_id,
            initial_state = initial_state,
            user_prompt = text_message
        )
        
        # Log session context for debugging
        logger.info(f"📋 Session context: id={actual_session_id}, is_new={is_new_session}")
        
        # CRITICAL: Truncate events to prevent token limit exceeded errors
        # Keep only the most recent events to stay within context window limits
        # Note: Each event can be large (tool results, API responses), so we limit aggressively
        # Model limit: ~1M tokens (1,048,576). With ~2K instruction + ~20K per event average = ~30-40 events max
        MAX_EVENTS_TO_KEEP = 30  # Keep last 30 events (conservative to prevent token overflow)
        
        if hasattr(session, 'events') and session.events:
            original_event_count = len(session.events)
            logger.info(f"📝 Session has {original_event_count} existing events")
            
            # Truncate events if too many
            if original_event_count > MAX_EVENTS_TO_KEEP:
                # Keep only the most recent events
                session.events = session.events[-MAX_EVENTS_TO_KEEP:]
                logger.warning(
                    f"⚠️ Truncated session events: {original_event_count} → {len(session.events)} "
                    f"(kept last {MAX_EVENTS_TO_KEEP} events to prevent token limit exceeded)"
                )
                logger.warning(
                    f"⚠️ Older events ({original_event_count - len(session.events)} events) "
                    f"will not be included in context to prevent exceeding model token limit"
                )
            else:
                logger.info(f"📝 Session has {len(session.events)} events (within limit)")
            
            # Log the last few events for context
            recent_events = session.events[-3:] if len(session.events) > 3 else session.events
            for i, event in enumerate(recent_events):
                author = getattr(event, 'author', 'unknown')
                logger.info(f"   Event {i+1}: author={author}")
        else:
            if is_new_session:
                logger.info("📝 Session has no existing events (new conversation)")
            else:
                # This is concerning - existing session with no events might indicate recovery from corruption
                logger.warning(f"⚠️ Existing session {actual_session_id} has NO events - may have been recovered from corruption")
                logger.warning("⚠️ Conversation history may be lost - agent will not have previous context")

        # Ensure state is properly set before creating runner
        if not hasattr(session, 'state') or session.state is None:
            session.state = {}

        # Title is now set during session creation using the user prompt

        # IMPORTANT: Merge current request state with existing session state
        # This ensures uploaded_files_metadata from previous requests is preserved
        # The session.state already contains persisted data from get_or_create_session
        agent_context_state = dict(session.state)  # Start with existing session state
        
        # Merge current request's initial_state (current request takes precedence for non-file fields)
        # But for uploaded_files_metadata, we want to ACCUMULATE, not replace
        existing_files = agent_context_state.get('uploaded_files_metadata', [])
        current_files = initial_state.get('uploaded_files_metadata', [])
        
        # Update with current request state
        agent_context_state.update(initial_state)
        
        # Accumulate files from both existing session and current request
        if existing_files or current_files:
            # Combine files, avoiding duplicates based on gcs_path or filename
            combined_files = []
            seen_paths = set()
            
            # Add existing files first
            for f in existing_files:
                path_key = f.get('gcs_path') or f.get('filename', '')
                if path_key and path_key not in seen_paths:
                    combined_files.append(f)
                    seen_paths.add(path_key)
            
            # Add current files (new ones only)
            for f in current_files:
                path_key = f.get('gcs_path') or f.get('filename', '')
                if path_key and path_key not in seen_paths:
                    combined_files.append(f)
                    seen_paths.add(path_key)
            
            agent_context_state['uploaded_files_metadata'] = combined_files
            logger.info(f"📎 Session files context: {len(existing_files)} existing + {len(current_files)} new = {len(combined_files)} total files")
            
            # Persist accumulated files to session for future requests in this conversation
            if not is_new_session and combined_files:
                try:
                    await update_session_state_in_database(
                        session_service,
                        request_data.app_name,
                        request_data.user_id,
                        actual_session_id,
                        {'uploaded_files_metadata': combined_files}
                    )
                    logger.info(f"💾 Persisted {len(combined_files)} accumulated files to session state")
                except Exception as persist_error:
                    logger.warning(f"⚠️ Could not persist accumulated files: {persist_error}")
        
        # Create agent with dynamic state context (injected into instruction, not user message)
        # This keeps context out of conversation history while making it available to the agent
        if agent_context_state:
            agent = create_agent_with_context(agent_context_state)
            logger.info(f"Created agent with session state context (keys: {list(agent_context_state.keys())})")
        else:
            agent = root_agent
            logger.info("Using root agent without state")

        # Create runner with the appropriate agent
        runner = Runner(
            app_name = request_data.app_name,
            agent = agent,
            session_service = session_service
        )
        
        # Create user message with all parts (text + files)
        # NO context injection here - it's in the agent instruction instead
        user_message = types.Content(
            parts=message_parts,
            role="user"
        )
        
        logger.info(f"Starting agent processing with {len(message_parts)} message parts")

        # Handle streaming vs non-streaming
        streaming = request_data.streaming
        streaming = True
        if streaming:
            return await _handle_streaming_response(runner, request_data, actual_session_id, user_message, session_service)
        else:
            return await _handle_regular_response(runner, request_data, actual_session_id, user_message)

    except Exception as e:
        logger.error(f"ERROR in chat endpoint: {e}")
        
        error_message = "I encountered an issue while trying to do what you asked. Can you try again?"
        
        return {
            "events": [],
            "session_id": locals().get('actual_session_id', "error"),
            "error": True,
            "error_message": error_message
        }


async def _handle_streaming_response(runner, request_data, session_id, user_message, session_service):
    """Handle streaming response using the pattern that avoids buffering"""
    
    import asyncio
    import time
    import sys
    
    async def async_event_generator():
        """Async generator that processes events and yields immediately"""
        try:
            stream_mode = StreamingMode.SSE
            event_count = 0
            
            # Send an immediate response with the session_id
            session_response = f'data: {{"session_id": "{session_id}", "timestamp": {time.time()}}}\n\n'
            yield session_response
            
            async for event in runner.run_async(
                user_id=request_data.user_id,
                session_id=session_id,
                new_message=user_message,
                run_config=RunConfig(streaming_mode=stream_mode),
            ):
                event_count += 1
                sse_event = event.model_dump_json(exclude_none=True, by_alias=True)
                
                # Translate thought events to non-technical language
                # Comment this lline out to output raw thoughts (which contain tool names, etc.)
                sse_event = await translate_thought_to_non_technical(sse_event)
                
                data = f"data: {sse_event}\n\n"
                yield data
                
            # Log completion and verify session persistence
            logger.info(f"✅ Streaming completed with {event_count} events for session {session_id}")
            
            # Verify session has been updated with the conversation
            # Note: This verification can fail due to Gemini response serialization issues
            # (e.g., Transcription fields with string "null" instead of proper null)
            # This is a non-critical error - the streaming has already completed successfully
            try:
                session = await session_service.get_session(
                    app_name=request_data.app_name,
                    user_id=request_data.user_id,
                    session_id=session_id
                )
                if session and hasattr(session, 'events'):
                    logger.info(f"📋 Session {session_id} now has {len(session.events)} events stored")
                else:
                    logger.warning(f"⚠️ Session {session_id} not found or has no events after streaming")
            except Exception as verify_error:
                # Log at warning level since this is a known issue with certain Gemini responses
                # The session may still be persisted correctly, just can't be verified
                logger.warning(f"⚠️ Could not verify session persistence (may be Gemini response serialization issue): {verify_error}")
                
        except Exception as e:
            logger.error(f"Error in streaming: {e}")
            error_data = f'data: {{"error": "I encountered an issue. Please try again."}}\n\n'
            yield error_data

    # Return StreamingResponse with enhanced headers for maximum compatibility
    return StreamingResponse(
        async_event_generator(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache, no-store, must-revalidate",
            "Pragma": "no-cache",
            "Expires": "0",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no",  # nginx
            "X-Proxy-Buffering": "no",  # other proxies
            "Transfer-Encoding": "chunked",  # Force chunked encoding
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
            "Access-Control-Allow-Headers": "Content-Type, Authorization",
        }
    )


async def _handle_regular_response(runner, request_data, session_id, user_message):
    """Handle regular (non-streaming) response"""
    try:
        events = []

        async for event in runner.run_async(
            user_id=request_data.user_id,
            session_id=session_id,
            new_message=user_message,
        ):
            events.append(event)

        logger.info(f"Processing complete. Generated {len(events)} events")

        # Note: Action logging is now handled by the action_log_agent in the worker_agent flow
        # No need for additional logging here to avoid duplication

        return {
            "events": events,
            "session_id": session_id
        }

    except Exception as run_error:
        logger.error(f"Error during agent run: {run_error}")
        
        # Add specific debugging for the GenAI error
        if "text parameter" in str(run_error):
            logger.error("GenAI text parameter error - checking message structure")
            if hasattr(user_message, 'parts'):
                for i, part in enumerate(user_message.parts):
                    logger.error(f"Part {i}: text='{part.text if hasattr(part, 'text') else 'NO TEXT'}', type={type(part)}")
        
        # Return user-friendly error message instead of raising exception
        error_message = "I encountered an issue while trying to do what you asked. Can you try again?"
        
        return {
            "events": [],
            "session_id": session_id,
            "error": True,
            "error_message": error_message
        }
