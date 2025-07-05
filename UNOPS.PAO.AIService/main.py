#!/usr/bin/env python3
"""
Main FastAPI application for Opportunity+ AI Agent with ADK
"""

import os
import uvicorn
from fastapi import FastAPI, HTTPException
from contextlib import asynccontextmanager
from google.adk.cli.fast_api import get_fast_api_app
from google.adk.sessions import DatabaseSessionService
from google.genai import types
from typing import Optional, Dict, Any
from pydantic import BaseModel
from fastapi.responses import StreamingResponse
import uuid

# Import our configuration manager
from ai_assistant.config_manager import config_manager

# Configuration from environment variables
PORT = int(os.getenv('PORT', 8000))
HOST = os.getenv('HOST', '0.0.0.0')
AGENT_DIR = os.getenv('AGENT_DIR', '.')
SESSION_DB_URL = os.getenv('DATABASE_URL', 'postgresql://postgres:0Y%2FX3YNxHLL0fL4T@localhost:5433/anusha')

# Enable web interface
SERVE_WEB_INTERFACE = os.getenv('SERVE_WEB_INTERFACE', 'true').lower() == 'true'


class ChatRequest(BaseModel):
    """Request model for chat endpoint"""
    app_name: str
    user_id: str
    session_id: str
    message: str
    streaming: bool = False
    state: Optional[Dict[str, Any]] = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager - load configurations once at startup"""
    print("FastAPI server starting up...")
    
    # Load configurations once at startup
    print("Loading configurations...")
    config_manager.load_tools_config('config/tools.json')
    
    print("All configurations loaded successfully!")
    
    yield
    
    print("FastAPI server shutting down...")


def create_fastapi_app():
    """Create and configure the FastAPI application with ADK"""
    
    # Create the FastAPI app with ADK integration
    app = get_fast_api_app(
        agents_dir=".",  # Current directory - will look for agent.py here
        session_service_uri=SESSION_DB_URL,
        allow_origins=["*"],
        web=SERVE_WEB_INTERFACE,
        trace_to_cloud=False,
        lifespan=lifespan  # Load configs once at startup
    )
    
    # Add our custom chat endpoint
    add_chat_endpoint(app)
    
    return app


def add_chat_endpoint(app: FastAPI):
    """Add the custom /chat endpoint to the FastAPI app"""
    
    @app.post("/chat")
    async def chat_endpoint(request: ChatRequest):
        """
        Custom chat endpoint that handles current_url context and state updates
        """
        try:
            print("\n" + "="*50)
            print("📥 INCOMING CHAT REQUEST")
            print("="*50)
            print(f"🔍 Request Details:")
            print(f"  - app_name: {request.app_name}")
            print(f"  - user_id: {request.user_id}")
            print(f"  - session_id: {request.session_id}")
            print(f"  - message: {request.message}")
            print(f"  - streaming: {request.streaming}")
            print(f"  - state: {request.state}")
            print("="*50)
            
            # Import here to avoid circular imports
            from google.adk.runners import Runner
            from google.adk.sessions import DatabaseSessionService
            from google.adk.agents import RunConfig
            from google.adk.agents.run_config import StreamingMode
            from ai_assistant.agent import root_agent
            
            # Handle null or empty session_id by generating a new one
            session_id = request.session_id
            if not session_id or session_id.strip() == "":
                session_id = str(uuid.uuid4())
                print(f"🆔 Generated new session_id: {session_id}")
            else:
                print(f"🆔 Using provided session_id: {session_id}")
            
            # Create session service
            session_service = DatabaseSessionService(db_url=SESSION_DB_URL)
            print(f"🔧 Created session service with DB: {SESSION_DB_URL}")
            
            # Get or create session
            print(f"🔍 Getting session for app: {request.app_name}, user: {request.user_id}, session: {session_id}")
            session = await session_service.get_session(
                app_name=request.app_name,
                user_id=request.user_id,
                session_id=session_id
            )
            
            if not session:
                print("🆕 Creating new session...")
                # Create new session if it doesn't exist
                initial_state = request.state or {}
                session = await session_service.create_session(
                    app_name=request.app_name,
                    user_id=request.user_id,
                    session_id=session_id,
                    state=initial_state
                )
                print(f"✅ New session created with state: {session.state}")
            else:
                print(f"📋 Found existing session with state: {session.state}")
                
                # Update existing session state if provided
                if request.state:
                    print(f"🔄 Updating session state with new data: {request.state}")
                    
                    # Merge new state with existing state
                    updated_state = {**session.state, **request.state}
                    print(f"🔀 Merged state: {updated_state}")
                    
                    try:
                        # Update session state using the session service
                        await session_service.update_session_state(
                            session=session,
                            state=updated_state
                        )
                        print(f"✅ Session state updated successfully via session service")
                        
                        # Refresh the session to get the updated state
                        session = await session_service.get_session(
                            app_name=request.app_name,
                            user_id=request.user_id,
                            session_id=session_id
                        )
                        print(f"🔄 Refreshed session state: {session.state}")
                        
                    except Exception as state_error:
                        print(f"⚠️ State update via session service failed: {state_error}")
                        print("🔄 Falling back to manual state update...")
                        
                        # Fallback: manually update session state
                        session.state.update(updated_state)
                        print(f"✅ Manual state update completed: {session.state}")
                        
                else:
                    print("ℹ️ No state update provided, using existing state")
            
            print(f"📊 Final session state before processing: {session.state}")
            
            # Ensure state is properly set before creating runner
            if not hasattr(session, 'state') or session.state is None:
                session.state = {}
                print("⚠️ Session state was None, initialized to empty dict")
            
            # Create runner
            print(f"🏃 Creating runner for agent: {root_agent.name}")
            runner = Runner(
                app_name=request.app_name,
                agent=root_agent,
                session_service=session_service
            )
            print(f"✅ Runner created successfully")
            
            # Create user message with proper role
            user_message = types.Content(
                parts=[types.Part(text=request.message)],
                role="user"
            )
            print(f"💬 Created user message: {request.message} (role: user)")
            
            print("\n🚀 STARTING AGENT PROCESSING...")
            print("="*50)
            
            # Handle streaming vs non-streaming
            if request.streaming:
                print("🌊 Using streaming mode")
                # Return streaming response
                async def event_generator():
                    try:
                        stream_mode = StreamingMode.SSE
                        print(f"🔄 Starting streaming with mode: {stream_mode}")
                        async for event in runner.run_async(
                            user_id=request.user_id,
                            session_id=session_id,
                            new_message=user_message,
                            run_config=RunConfig(streaming_mode=stream_mode),
                        ):
                            print(f" Streaming event: {type(event).__name__}")
                            # Format as SSE data
                            sse_event = event.model_dump_json(exclude_none=True, by_alias=True)
                            yield f"data: {sse_event}\n\n"
                    except Exception as e:
                        print(f"❌ Error in streaming: {e}")
                        import traceback
                        print(f"❌ Streaming traceback: {traceback.format_exc()}")
                        yield f'data: {{"error": "{str(e)}"}}\n\n'
                
                return StreamingResponse(
                    event_generator(),
                    media_type="text/event-stream",
                )
            else:
                print("📝 Using regular response mode")
                # Return regular response
                try:
                    events = []
                    print(f"🔄 Starting agent run...")
                    
                    # Add detailed debugging for the agent run
                    print(f"🔍 Debug info:")
                    print(f"  - User message: {user_message}")
                    print(f"  - User message parts: {user_message.parts if hasattr(user_message, 'parts') else 'No parts'}")
                    if hasattr(user_message, 'parts') and user_message.parts:
                        for i, part in enumerate(user_message.parts):
                            print(f"    Part {i}: {part.text if hasattr(part, 'text') else 'No text'}")
                    
                    async for event in runner.run_async(
                        user_id=request.user_id,
                        session_id=session_id,
                        new_message=user_message,
                    ):
                        print(f"📤 Received event: {type(event).__name__}")
                        # Try to get more info about the event without accessing .type
                        if hasattr(event, 'content'):
                            print(f"  Event content type: {type(event.content)}")
                            if hasattr(event.content, 'parts'):
                                print(f"  Event content parts: {len(event.content.parts) if event.content.parts else 0}")
                        if hasattr(event, 'author'):
                            print(f"  Event author: {event.author}")
                        if hasattr(event, 'actions'):
                            print(f"  Event actions: {event.actions}")
                        events.append(event)
                    
                    print(f"✅ Processing complete. Generated {len(events)} events")
                    for i, event in enumerate(events):
                        print(f"  Event {i+1}: {type(event).__name__}")
                    
                    return {
                        "events": events,
                        "session_id": session_id  # Return the session_id (especially important if auto-generated)
                    }
                    
                except Exception as run_error:
                    print(f"❌ Error during agent run: {run_error}")
                    print(f"❌ Error type: {type(run_error)}")
                    
                    # Add more specific debugging for the GenAI error
                    if "text parameter" in str(run_error):
                        print("🔍 DEBUGGING: This is the 'text parameter' error")
                        print(f"🔍 User message content: {user_message}")
                        print(f"🔍 User message type: {type(user_message)}")
                        if hasattr(user_message, 'parts'):
                            print(f"🔍 User message parts: {user_message.parts}")
                            for i, part in enumerate(user_message.parts):
                                print(f"🔍   Part {i}: text='{part.text if hasattr(part, 'text') else 'NO TEXT'}', type={type(part)}")
                        
                        print(f"🔍 Session state: {session.state if session else 'NO SESSION'}")
                    
                    import traceback
                    print(f"❌ Agent run traceback: {traceback.format_exc()}")
                    raise run_error
                
        except Exception as e:
            print(f"❌ ERROR in chat endpoint: {e}")
            print(f"❌ Error type: {type(e)}")
            import traceback
            print(f"❌ Full traceback: {traceback.format_exc()}")
            raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    print("Starting Opportunity+ AI Agent Server")
    print(f"Host: {HOST}")
    print(f"Port: {PORT}")
    print(f"Web Interface: {SERVE_WEB_INTERFACE}")
    print(f"Database: {SESSION_DB_URL}")
    
    app = create_fastapi_app()
    uvicorn.run(app, host=HOST, port=PORT)