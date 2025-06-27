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
            # Import here to avoid circular imports
            from google.adk.runners import Runner
            from google.adk.sessions import DatabaseSessionService
            from google.adk.agents import RunConfig
            from google.adk.agents.run_config import StreamingMode
            from ai_assistant.agent import root_agent
            
            # Create session service
            session_service = DatabaseSessionService(db_url=SESSION_DB_URL)
            
            # Get or create session
            session = await session_service.get_session(
                app_name=request.app_name,
                user_id=request.user_id,
                session_id=request.session_id
            )
            
            if not session:
                # Create new session if it doesn't exist
                session = await session_service.create_session(
                    app_name=request.app_name,
                    user_id=request.user_id,
                    session_id=request.session_id,
                    state=request.state or {}
                )
            else:
                # Update existing session state if provided
                if request.state:
                    # Merge new state with existing state
                    updated_state = {**session.state, **request.state}
                    
                    # Create a new event to update session state
                    from google.adk.events.event import Event
                    import time
                    import uuid
                    
                    state_update_event = Event(
                        id=str(uuid.uuid4()),
                        invocation_id="state_update",
                        author="system",
                        timestamp=time.time(),
                        actions={
                            "state_delta": updated_state
                        }
                    )
                    
                    # Append the state update event
                    await session_service.append_event(session, state_update_event)
                    
                    # Update session state locally
                    session.state.update(updated_state)
            
            # Create runner
            runner = Runner(
                app_name=request.app_name,
                agent=root_agent,
                session_service=session_service
            )
            
            # Create user message
            user_message = types.Content(parts=[types.Part(text=request.message)])
            
            # Handle streaming vs non-streaming
            if request.streaming:
                # Return streaming response
                async def event_generator():
                    try:
                        stream_mode = StreamingMode.SSE
                        async for event in runner.run_async(
                            user_id=request.user_id,
                            session_id=request.session_id,
                            new_message=user_message,
                            run_config=RunConfig(streaming_mode=stream_mode),
                        ):
                            # Format as SSE data
                            sse_event = event.model_dump_json(exclude_none=True, by_alias=True)
                            yield f"data: {sse_event}\n\n"
                    except Exception as e:
                        yield f'data: {{"error": "{str(e)}"}}\n\n'
                
                return StreamingResponse(
                    event_generator(),
                    media_type="text/event-stream",
                )
            else:
                # Return regular response
                events = [
                    event
                    async for event in runner.run_async(
                        user_id=request.user_id,
                        session_id=request.session_id,
                        new_message=user_message,
                    )
                ]
                return {"events": events}
                
        except Exception as e:
            print(f"Error in chat endpoint: {e}")
            raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    print("Starting Opportunity+ AI Agent Server")
    print(f"Host: {HOST}")
    print(f"Port: {PORT}")
    print(f"Web Interface: {SERVE_WEB_INTERFACE}")
    print(f"Database: {SESSION_DB_URL}")
    
    app = create_fastapi_app()
    uvicorn.run(app, host=HOST, port=PORT)