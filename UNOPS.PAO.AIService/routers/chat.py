"""
Chat Router

This module contains the chat endpoint and related functionality
extracted from main.py for better organization.
"""

import logging
import uuid
from typing import List, Any
from fastapi import APIRouter, HTTPException, Request, Form, File, UploadFile
from fastapi.responses import StreamingResponse, FileResponse
from google.adk.agents import RunConfig
from google.adk.agents.run_config import StreamingMode
from google.adk.runners import Runner
from google.adk.sessions import DatabaseSessionService
from google.genai import types
from pydantic import BaseModel

from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.utils.session_management import get_or_create_session, parse_request_state
from ai_assistant.utils.iap_validation import validate_iap_headers, extract_iap_headers_for_forwarding

logger = logging.getLogger(__name__)

# Create router
router = APIRouter()


@router.get("/test-stream")
async def test_stream():
    """Test endpoint to verify streaming is working without AI processing"""
    
    async def generate():
        import time
        import json
        import sys
        import asyncio
        
        logger.info("🧪 TEST STREAM: Starting test stream")
        
        # Send immediate ping
        ping_data = f"data: {json.dumps({'ping': 'test_started', 'timestamp': time.time()})}\n\n"
        logger.info("🧪 TEST STREAM: Sending ping")
        sys.stdout.flush()
        yield ping_data
        
        for i in range(5):
            test_data = {
                "test_message": f"This is test chunk {i+1}",
                "timestamp": time.time(),
                "chunk_id": i+1
            }
            
            data = f"data: {json.dumps(test_data)}\n\n"
            logger.info(f"🧪 TEST STREAM: Yielding chunk {i+1}")
            sys.stdout.flush()
            yield data
            
            # Add a small delay to make streaming visible
            await asyncio.sleep(1)
        
        # Final message
        final_data = {"test_message": "Stream complete", "final": True}
        yield f"data: {json.dumps(final_data)}\n\n"
        logger.info("🧪 TEST STREAM: Complete")

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


@router.post("/chat")
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
    # File uploads
    files: List[UploadFile] = File(None)
):
    """
    Custom chat endpoint that handles both JSON and multipart form data with files
    """
    try:
        # Access headers
        headers = dict(request.headers)
        logger.info(f"📋 Request headers: {headers}")
        
        # Determine request type and parse data
        content_type = request.headers.get("content-type", "")
        is_multipart = "multipart/form-data" in content_type
        logger.info(f"🔍 [DEBUG] Content-Type: {content_type}")
        logger.info(f"🔍 [DEBUG] Is multipart: {is_multipart}")
        
        if is_multipart:
            # Process multipart form data request with files
            logger.info("📁 Processing multipart form data request with files")
            
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
            logger.info(f"🔍 [DEBUG] Raw files parameter: {files}")
            if files and any(f for f in files if f.filename):
                logger.info(f"📎 Received {len([f for f in files if f.filename])} files:")
                for i, file in enumerate(files):
                    if file.filename:
                        logger.info(f"   File {i+1}: {file.filename} ({file.content_type})")
            else:
                logger.info("📎 No files received")
                files = []
                
        else:
            # Handle JSON request (backward compatibility)
            logger.info("📄 Processing JSON request")
            body = await request.json()
            request_data = ChatRequest(**body)
            files = []  # No files in JSON requests
            logger.info(f"📋 Request body: {body}")

        # Import agent here to avoid circular imports
        from ai_assistant.agent import root_agent

        # Create session service
        db_url = config_manager.get_database_url()
        session_service = DatabaseSessionService(db_url=db_url)
        logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")

        # Parse state
        user_email = request_data.user_email
        parsed_state = parse_request_state(request_data.state)
        logger.info(f"📊 Final parsed state: {parsed_state}")

        # Prepare initial state
        initial_state = parsed_state or {}
        initial_state['user_email'] = user_email

        # Convert uploaded files to types.Part objects
        message_parts = []
        
        # Add the main text message first
        text_message = request_data.message
        if not text_message and files and any(f.filename for f in files):
            text_message = " "  # Default blank space for file-only uploads
            logger.info("📄 No text message provided with file upload - using default blank space")
        
        if text_message:
            message_parts.append(types.Part(text=text_message))

        # Add uploaded files as Blob parts
        uploaded_artifact_parts = []
        audio_files_for_artifacts = []
        
        if files:
            for uploaded_file in files:
                if uploaded_file.filename:
                    file_content = await uploaded_file.read()
                    mime_type = uploaded_file.content_type or "application/octet-stream"
                    
                    # Create a types.Part for each file
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
                        logger.info(f"🎵 Audio file detected: {uploaded_file.filename}")
                    
                    uploaded_artifact_parts.append({
                        "filename": uploaded_file.filename,
                        "mime_type": mime_type,
                        "size": len(file_content),
                        "is_audio": is_audio
                    })
                    logger.info(f"📎 Converted file '{uploaded_file.filename}' to types.Part.")
                    
            if uploaded_artifact_parts:
                initial_state['uploaded_files_metadata'] = uploaded_artifact_parts
                logger.info(f"📁 Added metadata for {len(uploaded_artifact_parts)} uploaded files to initial state.")
            
            if audio_files_for_artifacts:
                initial_state['audio_files_metadata'] = audio_files_for_artifacts
                logger.info(f"🎵 Prepared {len(audio_files_for_artifacts)} audio files metadata for agent processing")

        # Get or create session
        session, actual_session_id, is_new_session = await get_or_create_session(
            session_service=session_service,
            app_name=request_data.app_name,
            user_id=request_data.user_id,
            session_id=request_data.session_id,
            initial_state=initial_state
        )

        logger.info(f"📊 Final session state before processing: {session.state}")

        # Ensure state is properly set before creating runner
        if not hasattr(session, 'state') or session.state is None:
            session.state = {}
            logger.info("⚠️ Session state was None, initialized to empty dict")

        # Create runner
        logger.info(f"🏃 Creating runner for agent: {root_agent.name}")
        runner = Runner(
            app_name=request_data.app_name,
            agent=root_agent,
            session_service=session_service
        )
        logger.info("✅ Runner created successfully")

        # Create user message with all parts (text + files)
        user_message = types.Content(
            parts=message_parts,
            role="user"
        )
        logger.info(f"💬 Created user message with {len(message_parts)} parts. Text message: '{request_data.message}'")

        logger.info("\n🚀 STARTING AGENT PROCESSING...")
        logger.info("="*50)

        # Handle streaming vs non-streaming
        streaming = request_data.streaming
        streaming = True
        if streaming:
            logger.info("🌊 Using streaming mode")
            return await _handle_streaming_response(runner, request_data, actual_session_id, user_message)
        else:
            logger.info("📝 Using regular response mode")
            return await _handle_regular_response(runner, request_data, actual_session_id, user_message)

    except Exception as e:
        logger.error(f"❌ ERROR in chat endpoint: {e}")
        logger.error(f"❌ Error type: {type(e)}")
        import traceback
        logger.error(f"❌ Full traceback: {traceback.format_exc()}")
        
        # Return user-friendly error message with 200 status instead of 500
        error_message = "I encountered an issue while trying to do what you asked. Can you try again?"
        
        return {
            "events": [],
            "session_id": locals().get('actual_session_id', "error"),
            "error": True,
            "error_message": error_message
        }


async def _handle_streaming_response(runner, request_data, session_id, user_message):
    """Handle streaming response using the pattern that avoids buffering"""
    
    import asyncio
    import time
    import sys
    
    async def async_event_generator():
        """Async generator that processes events and yields immediately"""
        try:
            stream_mode = StreamingMode.SSE
            logger.info(f"🔄 ===STREAMING=== Starting streaming with mode: {stream_mode}")
            
            # Send an immediate ping to establish the stream
            ping_data = f"data: {{'ping': 'stream_started', 'timestamp': {time.time()}}}\n\n"
            logger.info("📤 PING: Sending initial ping")
            yield ping_data
            
            async for event in runner.run_async(
                user_id=request_data.user_id,
                session_id=session_id,
                new_message=user_message,
                run_config=RunConfig(streaming_mode=stream_mode),
            ):
                start_time = time.time()
                logger.info(f"📤 Got streaming event: {type(event).__name__}")
                
                sse_event = event.model_dump_json(exclude_none=True, by_alias=True)
                data = f"data: {sse_event}\n\n"
                
                process_time = time.time() - start_time
                logger.info(f"📤 IMMEDIATELY yielding: {data[:100]}... (processing took {process_time:.3f}s)")
                
                # Force flush stdout to ensure logs appear immediately
                sys.stdout.flush()
                
                yield data
                
        except Exception as e:
            logger.error(f"❌ Error in streaming: {e}")
            import traceback
            logger.error(f"❌ Streaming traceback: {traceback.format_exc()}")
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
        logger.info("🔄 ===REGULAR=== Starting agent run...")

        # Add detailed debugging for the agent run
        logger.info("🔍 Debug info:")
        logger.info(f"   - User message: {user_message}")
        logger.info(f"   - User message parts: {len(user_message.parts) if hasattr(user_message, 'parts') else 'No parts'}")
        if hasattr(user_message, 'parts') and user_message.parts:
            for i, part in enumerate(user_message.parts):
                logger.info(f"     Part {i}: Text='{part.text if hasattr(part, 'text') else 'NO TEXT'}', Blob Present={True if hasattr(part, 'inline_data') and part.inline_data else False}, Type={type(part)}")

        async for event in runner.run_async(
            user_id=request_data.user_id,
            session_id=session_id,
            new_message=user_message,
        ):
            logger.info(f"📤 Received event: {type(event).__name__}")
            if hasattr(event, 'content'):
                logger.info(f"   Event content type: {type(event.content)}")
                if hasattr(event.content, 'parts'):
                    logger.info(f"   Event content parts: {len(event.content.parts) if event.content.parts else 0}")
            if hasattr(event, 'author'):
                logger.info(f"   Event author: {event.author}")
            if hasattr(event, 'actions'):
                logger.info(f"   Event actions: {event.actions}")
            events.append(event)

        logger.info(f"✅ Processing complete. Generated {len(events)} events")
        for i, event in enumerate(events):
            logger.info(f"   Event {i+1}: {type(event).__name__}")

        # Note: Action logging is now handled by the action_log_agent in the worker_agent flow
        # No need for additional logging here to avoid duplication

        return {
            "events": events,
            "session_id": session_id
        }

    except Exception as run_error:
        logger.error(f"❌ Error during agent run: {run_error}")
        logger.error(f"❌ Error type: {type(run_error)}")

        # Add more specific debugging for the GenAI error
        if "text parameter" in str(run_error):
            logger.error("🔍 DEBUGGING: This is the 'text parameter' error")
            logger.error(f"🔍 User message content: {user_message}")
            logger.error(f"🔍 User message type: {type(user_message)}")
            if hasattr(user_message, 'parts'):
                logger.error(f"🔍 User message parts: {user_message.parts}")
                for i, part in enumerate(user_message.parts):
                    logger.error(f"🔍   Part {i}: text='{part.text if hasattr(part, 'text') else 'NO TEXT'}', type={type(part)}")

        import traceback
        logger.error(f"❌ Agent run traceback: {traceback.format_exc()}")
        
        # Return user-friendly error message instead of raising exception
        error_message = "I encountered an issue while trying to do what you asked. Can you try again?"
        
        return {
            "events": [],
            "session_id": session_id,
            "error": True,
            "error_message": error_message
        }


# Removed: _log_successful_interaction function
# Action logging is now properly handled by the action_log_agent in the worker_agent flow
# This eliminates duplicate logging and ensures intelligent LLM-generated summaries
     