#!/usr/bin/env python3
"""
Main FastAPI application for Opportunity+ AI Agent with ADK
"""

import os
import uvicorn
import asyncio
import logging
import uuid
from fastapi import FastAPI, HTTPException, Query, Request
from contextlib import asynccontextmanager
from google.adk.cli.fast_api import get_fast_api_app
from google.adk.sessions import DatabaseSessionService
from google.adk.runners import Runner
from google.adk.agents import RunConfig
from google.adk.agents.run_config import StreamingMode
from google.genai import types
from typing import Optional, Dict, Any, List
from pydantic import BaseModel
from fastapi.responses import StreamingResponse

# Load environment variables from .env file
try:
    from dotenv import load_dotenv
    load_dotenv()
    print(f"✅ Loaded .env file - ENVIRONMENT: {os.getenv('CURRENT_ENV', 'not set')}")
except ImportError:
    print("⚠️ python-dotenv not installed, .env file won't be loaded")

# Framework imports
from framework_config import get_config, validate_config, get_environment_info
from ai_assistant.tools import create_google_drive_tool

# Import our configuration manager
from ai_assistant.config_manager import config_manager

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Global variables
google_drive_tool = None

# Define the FastAPI app globally
app = None # This will be assigned in create_fastapi_app()


def validate_iap_headers(headers: dict) -> dict:
    """
    Validate IAP (Identity-Aware Proxy) headers from incoming requests.

    This function validates the presence and format of Google Cloud IAP headers
    and returns validation results with extracted user information.

    Args:
        headers: Dictionary of request headers

    Returns:
        dict: Validation result with the following structure:
        {
            "valid": bool,
            "user_email": str or None,
            "user_id": str or None,
            "is_development": bool,
            "validation_errors": list,
            "extracted_headers": dict
        }
    """
    validation_result = {
        "valid": False,
        "user_email": None,
        "user_id": None,
        "is_development": False,
        "validation_errors": [],
        "extracted_headers": {}
    }

    try:
        # Convert headers to lowercase for case-insensitive comparison
        headers_lower = {k.lower(): v for k, v in headers.items()}

        # Define expected IAP headers
        expected_iap_headers = [
            'x-goog-authenticated-user-email',
            'x-goog-authenticated-user-id',
            'x-forwarded-user',
            'x-forwarded-email'
        ]

        # Check for development mode indicators
        is_dev_simulation = headers_lower.get('x-dev-iap-simulation', '').lower() == 'true'
        dev_timestamp = headers_lower.get('x-dev-auth-timestamp')

        # Check for DevIAPAuth cookie in development mode
        dev_iap_auth_cookie = headers_lower.get('cookie', '')
        if 'deviapauth=' in dev_iap_auth_cookie.lower():
            # Extract email from DevIAPAuth cookie
            import re
            cookie_match = re.search(r'deviapauth=([^;]+)', dev_iap_auth_cookie, re.IGNORECASE)
            if cookie_match:
                dev_email_encoded = cookie_match.group(1)
                try:
                    import urllib.parse
                    dev_email = urllib.parse.unquote(dev_email_encoded)
                    if '@' in dev_email and '.' in dev_email.split('@')[1]:
                        validation_result["user_email"] = dev_email
                        validation_result["is_development"] = True
                        logger.info(f"🧪 Development mode - Email extracted from DevIAPAuth cookie: {dev_email}")
                    else:
                        logger.warning(f"❌ Invalid email format in DevIAPAuth cookie: {dev_email}")
                except Exception as e:
                    logger.warning(f"❌ Error decoding DevIAPAuth cookie: {e}")

        if is_dev_simulation:
            validation_result["is_development"] = True
            logger.info("🧪 Development IAP simulation detected")

        # Extract and validate IAP headers
        extracted_headers = {}

        for header_name in expected_iap_headers:
            header_value = headers_lower.get(header_name)
            if header_value:
                extracted_headers[header_name] = header_value
                logger.info(f"✅ Found IAP header: {header_name}")
            else:
                validation_result["validation_errors"].append(f"Missing required IAP header: {header_name}")
                logger.warning(f"❌ Missing IAP header: {header_name}")

        # Validate user email format from IAP headers (if not already set from cookie)
        if not validation_result["user_email"]:
            user_email_header = headers_lower.get('x-goog-authenticated-user-email')
            if user_email_header:
                # IAP format: "accounts.google.com:user@domain.com"
                if ':' in user_email_header:
                    _, email = user_email_header.split(':', 1)
                    if '@' in email and '.' in email.split('@')[1]:
                        validation_result["user_email"] = email
                        logger.info(f"✅ Valid user email extracted from IAP header: {email}")
                    else:
                        validation_result["validation_errors"].append("Invalid email format in x-goog-authenticated-user-email")
                        logger.warning(f"❌ Invalid email format: {user_email_header}")
                else:
                    validation_result["validation_errors"].append("Invalid format for x-goog-authenticated-user-email (missing ':' separator)")
                    logger.warning(f"❌ Invalid header format: {user_email_header}")

        # Validate user ID format
        user_id_header = headers_lower.get('x-goog-authenticated-user-id')
        if user_id_header:
            # IAP format: "accounts.google.com:123456789"
            if ':' in user_id_header:
                _, user_id = user_id_header.split(':', 1)
                validation_result["user_id"] = user_id
                logger.info(f"✅ Valid user ID extracted: {user_id}")
            else:
                validation_result["validation_errors"].append("Invalid format for x-goog-authenticated-user-id (missing ':' separator)")
                logger.warning(f"❌ Invalid header format: {user_id_header}")

        # Check for forwarded headers as fallback
        if not validation_result["user_email"]:
            forwarded_email = headers_lower.get('x-forwarded-email')
            if forwarded_email and '@' in forwarded_email:
                validation_result["user_email"] = forwarded_email
                logger.info(f"✅ Using forwarded email as fallback: {forwarded_email}")

        if not validation_result["user_id"]:
            forwarded_user = headers_lower.get('x-forwarded-user')
            if forwarded_user:
                validation_result["user_id"] = forwarded_user
                logger.info(f"✅ Using forwarded user as fallback: {forwarded_user}")

        # Determine if validation is successful
        # In development mode, we're more lenient
        if validation_result["is_development"]:
            # For development, we only need basic email validation
            if validation_result["user_email"]:
                validation_result["valid"] = True
                logger.info("✅ Development mode validation successful")
            else:
                validation_result["validation_errors"].append("Development mode requires valid user email")
                logger.warning("❌ Development mode validation failed - missing user email")
        else:
            # For production, require all standard IAP headers
            required_for_production = ['x-goog-authenticated-user-email', 'x-goog-authenticated-user-id']
            missing_required = [h for h in required_for_production if h not in extracted_headers]

            if not missing_required and validation_result["user_email"]:
                validation_result["valid"] = True
                logger.info("✅ Production IAP validation successful")
            else:
                if missing_required:
                    validation_result["validation_errors"].extend([f"Missing required header for production: {h}" for h in missing_required])
                if not validation_result["user_email"]:
                    validation_result["validation_errors"].append("Valid user email required for production")
                logger.warning("❌ Production IAP validation failed")

        validation_result["extracted_headers"] = extracted_headers

        # Log validation summary
        if validation_result["valid"]:
            logger.info(f"✅ IAP validation successful - User: {validation_result['user_email']}")
        else:
            logger.warning(f"❌ IAP validation failed - Errors: {validation_result['validation_errors']}")

        return validation_result

    except Exception as e:
        error_msg = f"Error during IAP header validation: {str(e)}"
        validation_result["validation_errors"].append(error_msg)
        logger.error(f"❌ {error_msg}")
        return validation_result


def extract_iap_headers_for_forwarding(headers: dict) -> dict:
    """
    Extract ALL headers from incoming request for forwarding to other services.

    This function just returns all headers as-is, no filtering or processing.

    Args:
        headers: Dictionary of request headers

    Returns:
        dict: Dictionary of ALL headers to forward (exactly as received)
    """
    try:
        logger.info(f"📤 Forwarding ALL headers as-is: {len(headers)} headers")
        logger.info(f"📋 Header keys received: {list(headers.keys())}")
        
        # Log header values safely (mask sensitive data)
        for key, value in headers.items():
            if any(sensitive in key.lower() for sensitive in ['authorization', 'token', 'jwt', 'secret', 'password']):
                masked_value = f"{value[:10]}..." if len(value) > 10 else "***"
                logger.info(f"   {key}: {masked_value} (masked)")
            elif 'email' in key.lower():
                logger.info(f"   {key}: {value}")
            elif len(value) > 100:
                logger.info(f"   {key}: {value[:50]}... (truncated, length: {len(value)})")
            else:
                logger.info(f"   {key}: {value}")
        
        return headers

    except Exception as e:
        logger.error(f"❌ Error forwarding headers: {str(e)}")
        return {}


class ChatRequest(BaseModel):
    """Request model for chat endpoint"""
    app_name: str
    user_id: str
    session_id: str
    message: str
    streaming: bool = False
    state: str = ""


@asynccontextmanager
async def lifespan(app_instance: FastAPI): # Renamed 'app' to 'app_instance' to avoid confusion with global 'app'
    """Application lifespan manager - load configurations once at startup"""
    logger.info("🚀 FastAPI server starting up...")

    # Load configurations once at startup
    logger.info("📋 Loading configurations...")
    config_manager.load_tools_config('config/tools.json')

    # Initialize Google Drive tool if enabled
    global google_drive_tool
    config = get_config()
    google_drive_config = config.get('google_drive', {})
    agent_config = config.get('agent', {})

    if google_drive_config.get('enabled', False) and agent_config.get('enable_google_drive_agent', False):
        try:
            google_drive_tool = await create_google_drive_tool()
            logger.info("✅ Google Drive tool initialized successfully")
        except Exception as e:
            logger.warning(f"⚠️ Google Drive tool initialization failed: {str(e)}")
            logger.info("📝 Application will continue without Google Drive integration")
    else:
        logger.info("📝 Google Drive tool disabled in configuration")

    logger.info("✅ All configurations loaded successfully!")

    yield

    # Cleanup
    logger.info("🛑 FastAPI server shutting down...")
    if google_drive_tool:
        await google_drive_tool.cleanup()
        logger.info("✅ Google Drive tool cleaned up")


def create_fastapi_app_instance(): # Renamed the function to avoid conflict with global 'app'
    """Create and configure the FastAPI application with ADK"""
    config = get_config()
    database_config = config.get('database', {})
    server_config = config.get('server', {})

    # Create the FastAPI app with ADK integration
    fastapi_app_instance = get_fast_api_app( # Renamed variable
        agents_dir=".",  # Current directory - will look for agent.py here
        session_service_uri=database_config.get('url', 'sqlite:///./ai_agent.db'),
        allow_origins=server_config.get('allow_origins', ["*"]),
        web=server_config.get('serve_web_interface', True),
        trace_to_cloud=False,
        lifespan=lifespan  # Load configs once at startup
    )

    # Override the app metadata
    branding_config = config.get('branding', {})
    app_title = branding_config.get('application_name', 'AI Service')
    app_description = branding_config.get('description', 'AI Agent Framework')
    fastapi_app_instance.title = app_title
    fastapi_app_instance.description = f"AI Agent Framework for {app_title} with Google ADK integration"
    fastapi_app_instance.version = "1.0.0"

    # Add our custom endpoints
    add_framework_endpoints(fastapi_app_instance)
    add_chat_endpoint(fastapi_app_instance)
    add_title_endpoint(fastapi_app_instance)
    add_google_drive_endpoints(fastapi_app_instance)

    return fastapi_app_instance


def add_framework_endpoints(app: FastAPI):
    """Add framework-specific endpoints"""
    config = get_config()
    branding_config = config.get('branding', {})

    @app.get("/framework/info")
    async def get_framework_info():
        """Get framework information"""
        return {
            "framework": branding_config.get('application_name', 'AI Service'),
            "version": "1.0.0",
            "description": branding_config.get('description', 'AI Agent Framework'),
            "status": "running",
            "environment": get_environment_info()
        }

    @app.get("/framework/config")
    async def get_framework_config():
        """Get framework configuration"""
        return validate_config()

    @app.get("/framework/tools")
    async def get_available_tools():
        """Get available tools"""
        tools = []
        if google_drive_tool:
            tools.append(google_drive_tool.get_tool_info())
        return {"available_tools": tools, "total_tools": len(tools)}

    @app.get("/framework/iap-test")
    async def test_iap_validation(request: Request):
        """Test endpoint to demonstrate IAP header validation"""
        try:
            headers = dict(request.headers)

            # Validate IAP headers
            iap_validation = validate_iap_headers(headers)

            # Extract headers for forwarding
            iap_headers_to_forward = extract_iap_headers_for_forwarding(headers)

            return {
                "message": "IAP validation test completed",
                "validation_result": iap_validation,
                "headers_to_forward": iap_headers_to_forward,
                "all_headers": headers
            }
        except Exception as e:
            logger.error(f"❌ Error in IAP test endpoint: {e}")
            raise HTTPException(status_code=500, detail=str(e))


def add_chat_endpoint(app: FastAPI):
    """Add the custom /chat endpoint to the FastAPI app"""
    from ai_assistant.config_manager import config_manager

    @app.post("/chat")
    async def chat_endpoint(request_body: ChatRequest, request: Request):
        """
        Custom chat endpoint that handles current_url context and state updates
        """
        try:
            # Access headers
            headers = dict(request.headers)
            logger.info(f"📋 Request headers: {headers}")

            # Validate IAP headers
            iap_validation = validate_iap_headers(headers)
            logger.info(f"🔐 IAP validation result: {iap_validation}")

            # Handle IAP validation results
            if not iap_validation["valid"]:
                logger.warning(f"⚠️ IAP validation failed: {iap_validation['validation_errors']}")
                # You can choose to continue or return an error
                # For now, we'll log the warning but continue processing
                # In production, you might want to return an HTTP 401 or 403 here

            # Extract user information from IAP headers
            user_email = iap_validation.get("user_email")
            user_id = iap_validation.get("user_id")
            is_development = iap_validation.get("is_development", False)

            if user_email:
                logger.info(f"👤 Authenticated user: {user_email}")
            if user_id:
                logger.info(f"🆔 User ID: {user_id}")
            if is_development:
                logger.info("🧪 Running in development mode")

            # Extract IAP headers for forwarding to other services
            iap_headers_to_forward = extract_iap_headers_for_forwarding(headers)
            if iap_headers_to_forward:
                logger.info(f"📤 IAP headers available for forwarding: {list(iap_headers_to_forward.keys())}")

            # You can access specific headers like this:
            authorization = request.headers.get("authorization")
            user_agent = request.headers.get("user-agent")
            content_type = request.headers.get("content-type")

            logger.info(f"🔑 Authorization: {authorization}")
            logger.info(f"🌐 User-Agent: {user_agent}")
            logger.info(f"📄 Content-Type: {content_type}")

            # Import here to avoid circular imports
            from ai_assistant.agent import root_agent

            # Handle session creation vs retrieval
            original_session_id = request_body.session_id
            is_new_session_request = not original_session_id or original_session_id.strip() == ""
            
            if is_new_session_request:
                session_id = str(uuid.uuid4())
                logger.info(f"🆔 Empty session_id provided - generating new session: {session_id}")
            else:
                session_id = original_session_id
                logger.info(f"🆔 Using provided session_id: {session_id}")

            # Create session service
            db_url = config_manager.get_database_url()
            session_service = DatabaseSessionService(db_url=db_url)
            logger.info(f"🔧 Created session service with DB: {db_url[:50]}...")

            # Parse state if it's a JSON string
            parsed_state = None
            print(f"Request body state: {request_body.state}")
            if request_body.state:
                if isinstance(request_body.state, str):
                    try:
                        import json
                        print(f"Request body state inside try: {request_body.state}")
                        parsed_state = json.loads(request_body.state)
                        logger.info(f"📄 Parsed JSON state: {parsed_state}")
                    except json.JSONDecodeError as e:
                        logger.warning(f"⚠️ Failed to parse state JSON: {e}")
                        logger.info(f"🔍 Raw state value: {request_body.state}")
                        parsed_state = {}
                elif isinstance(request_body.state, dict):
                    parsed_state = request_body.state
                    logger.info(f"📊 Using dict state: {parsed_state}")
                else:
                    logger.warning(f"⚠️ Unexpected state type: {type(request_body.state)}")
                    parsed_state = {}

            print(f"Parsed state: {parsed_state}")

            # Handle session creation vs retrieval
            if is_new_session_request:
                # Always create a new session for empty session_id
                logger.info("🆕 Creating new session (empty session_id provided)...")
                initial_state = parsed_state or {}
                session = await session_service.create_session(
                    app_name=request_body.app_name,
                    user_id=request_body.user_id,
                    session_id=session_id,
                    state=initial_state
                )
                logger.info(f"✅ New session created with state: {session.state}")
            else:
                # Try to get existing session first
                logger.info(f"🔍 Getting existing session for app: {request_body.app_name}, user: {request_body.user_id}, session: {session_id}")
                session = await session_service.get_session(
                    app_name=request_body.app_name,
                    user_id=request_body.user_id,
                    session_id=session_id
                )

                if not session:
                    logger.info("🆕 Session not found - creating new session...")
                    # Create new session if it doesn't exist
                    initial_state = parsed_state or {}
                    session = await session_service.create_session(
                        app_name=request_body.app_name,
                        user_id=request_body.user_id,
                        session_id=session_id,
                        state=initial_state
                    )
                    logger.info(f"✅ New session created with state: {session.state}")
                else:
                    logger.info(f"📋 Found existing session with state: {session.state}")

                    # Update existing session state if provided
                    if parsed_state:
                        logger.info(f"🔄 Updating session state with new data: {parsed_state}")

                        # Merge new state with existing state
                        updated_state = {**session.state, **parsed_state}
                        logger.info(f"🔀 Merged state: {updated_state}")

                        try:
                            # Update session state directly on the session object
                            session.state.update(updated_state)
                            logger.info(f"✅ Session state updated in memory: {session.state}")
                            
                            # Save the updated session back to the database
                            await session_service.update_session(session)
                            logger.info(f"💾 Session state saved to database successfully")

                        except Exception as state_error:
                            logger.warning(f"⚠️ State update failed: {state_error}")
                            logger.info("🔄 Falling back to basic state initialization...")

                            # Fallback: ensure session has a state dict
                            if not hasattr(session, 'state') or session.state is None:
                                session.state = {}
                            session.state.update(updated_state)
                            logger.info(f"✅ Fallback state update completed: {session.state}")
                            
                            # Try to save the fallback state as well
                            try:
                                await session_service.update_session(session)
                                logger.info(f"💾 Fallback session state saved to database")
                            except Exception as save_error:
                                logger.error(f"❌ Failed to save fallback session state: {save_error}")

                    else:
                        logger.info("ℹ️ No state update provided, using existing state")

            # Add user_email and IAP headers to session state if available
            if user_email or iap_headers_to_forward:
                # Ensure session state exists
                if not hasattr(session, 'state') or session.state is None:
                    session.state = {}

                # Add user_email to session state (primary field)
                if user_email:
                    session.state['user_email'] = user_email
                    # Also keep header_email for backward compatibility
                    session.state['header_email'] = user_email
                    logger.info(f"📧 Added user_email to session state: {user_email}")

                # Store the complete IAP headers for API calls
                if iap_headers_to_forward:
                    session.state['iap_headers'] = iap_headers_to_forward
                    logger.info(f"🔐 Stored IAP headers in session state: {list(iap_headers_to_forward.keys())}")
                
                # Save the session with user_email and IAP headers updates
                try:
                    await session_service.update_session(session)
                    logger.info(f"💾 Session saved with user_email: {user_email} and IAP headers: {list(iap_headers_to_forward.keys()) if iap_headers_to_forward else 'None'}")
                except Exception as save_error:
                    logger.error(f"❌ Failed to save session with user_email and IAP headers: {save_error}")
            else:
                logger.info("⚠️ No user email or IAP headers found - session not updated")

            logger.info(f"📊 Final session state before processing: {session.state}")

            # Ensure state is properly set before creating runner
            if not hasattr(session, 'state') or session.state is None:
                session.state = {}
                logger.info("⚠️ Session state was None, initialized to empty dict")

            # Create runner
            logger.info(f"🏃 Creating runner for agent: {root_agent.name}")
            runner = Runner(
                app_name=request_body.app_name,
                agent=root_agent,
                session_service=session_service
            )
            logger.info(f"✅ Runner created successfully")

            # Create user message with proper role
            user_message = types.Content(
                parts=[types.Part(text=request_body.message)],
                role="user"
            )
            logger.info(f"💬 Created user message: {request_body.message} (role: user)")

            logger.info("\n🚀 STARTING AGENT PROCESSING...")
            logger.info("="*50)

            # Handle streaming vs non-streaming
            if request_body.streaming:
                logger.info("🌊 Using streaming mode")
                # Return streaming response
                async def event_generator():
                    try:
                        stream_mode = StreamingMode.SSE
                        logger.info(f"🔄 Starting streaming with mode: {stream_mode}")
                        async for event in runner.run_async(
                            user_id=request_body.user_id,
                            session_id=session_id,
                            new_message=user_message,
                            run_config=RunConfig(streaming_mode=stream_mode),
                        ):
                            logger.info(f"📤 Streaming event: {type(event).__name__}")
                            # Format as SSE data
                            sse_event = event.model_dump_json(exclude_none=True, by_alias=True)
                            yield f"data: {sse_event}\n\n"
                    except Exception as e:
                        logger.error(f"❌ Error in streaming: {e}")
                        import traceback
                        logger.error(f"❌ Streaming traceback: {traceback.format_exc()}")
                        yield f'data: {{"error": "{str(e)}"}}\n\n'

                return StreamingResponse(
                    event_generator(),
                    media_type="text/event-stream",
                )
            else:
                logger.info("📝 Using regular response mode")
                # Return regular response
                try:
                    events = []
                    logger.info(f"🔄 Starting agent run...")

                    # Add detailed debugging for the agent run
                    logger.info(f"🔍 Debug info:")
                    logger.info(f"   - User message: {user_message}")
                    logger.info(f"   - User message parts: {user_message.parts if hasattr(user_message, 'parts') else 'No parts'}")
                    if hasattr(user_message, 'parts') and user_message.parts:
                        for i, part in enumerate(user_message.parts):
                            logger.info(f"     Part {i}: {part.text if hasattr(part, 'text') else 'No text'}")

                    async for event in runner.run_async(
                        user_id=request_body.user_id,
                        session_id=session_id,
                        new_message=user_message,
                    ):
                        logger.info(f"📤 Received event: {type(event).__name__}")
                        # Try to get more info about the event without accessing .type
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

                    return {
                        "events": events,
                        "session_id": session_id  # Return the session_id (especially important if auto-generated)
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

                        logger.error(f"🔍 Session state: {session.state if session else 'NO SESSION'}")

                    import traceback
                    logger.error(f"❌ Agent run traceback: {traceback.format_exc()}")
                    raise run_error

        except Exception as e:
            logger.error(f"❌ ERROR in chat endpoint: {e}")
            logger.error(f"❌ Error type: {type(e)}")
            import traceback
            logger.error(f"❌ Full traceback: {traceback.format_exc()}")
            raise HTTPException(status_code=500, detail=str(e))


def add_title_endpoint(app: FastAPI):
    """Add the /title endpoint to get session's first 2 conversations and generate a title"""
    from ai_assistant.config_manager import config_manager

    @app.get("/generate-title")
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
            logger.info(f"🔍 Request Details:")
            logger.info(f"   - session_id: {session_id}")
            logger.info(f"   - user_id: {user_id}")
            logger.info("="*50)

            # Access and validate IAP headers if request is available
            if request:
                headers = dict(request.headers)
                logger.info(f"📋 Request headers: {headers}")

                # Validate IAP headers
                iap_validation = validate_iap_headers(headers)
                logger.info(f"🔐 IAP validation result: {iap_validation}")

                # Handle IAP validation results
                if not iap_validation["valid"]:
                    logger.warning(f"⚠️ IAP validation failed: {iap_validation['validation_errors']}")
                    # You can choose to continue or return an error
                    # For now, we'll log the warning but continue processing

                # Extract user information from IAP headers
                iap_user_email = iap_validation.get("user_email")
                iap_user_id = iap_validation.get("user_id")
                is_development = iap_validation.get("is_development", False)

                if iap_user_email:
                    logger.info(f"👤 Authenticated user from IAP: {iap_user_email}")
                if iap_user_id:
                    logger.info(f"🆔 User ID from IAP: {iap_user_id}")
                if is_development:
                    logger.info("🧪 Running in development mode")

                # Extract IAP headers for forwarding to other services
                iap_headers_to_forward = extract_iap_headers_for_forwarding(headers)
                if iap_headers_to_forward:
                    logger.info(f"📤 IAP headers available for forwarding: {list(iap_headers_to_forward.keys())}")
            else:
                logger.info("⚠️ No request object available for IAP validation")

            # Validate session_id
            if not session_id or session_id.strip() == "":
                raise HTTPException(status_code=400, detail="Session ID is required")

            # Create session service
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
                    from ai_assistant.workflow_agent.entity_detection.callback import generate_conversation_title
                    title = generate_conversation_title(formatted_conversation)
                    logger.info(f"📝 Generated title: {title}")
                except Exception as title_error:
                    logger.warning(f"⚠️ Failed to generate title with Gemini: {title_error}")
                    title = "Conversation"
                response = {
                    "session_id": session_id,
                    "formatted_conversation": formatted_conversation,
                    "title": title
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


def add_google_drive_endpoints(app: FastAPI):
    """Add Google Drive endpoints if available"""
    if google_drive_tool:
        @app.get("/google-drive/files")
        async def search_google_drive_files(
            query: str = None,
            name_contains: str = None,
            mime_type: str = None,
            max_results: int = 20
        ):
            """Search for files in Google Drive"""
            try:
                files = await google_drive_tool.find_files(
                    query=query,
                    name_contains=name_contains,
                    mime_type=mime_type,
                    max_results=max_results
                )
                return {"files": [file.to_dict() for file in files]}
            except Exception as e:
                raise HTTPException(status_code=500, detail=str(e))

        @app.get("/google-drive/files/{file_id}")
        async def get_google_drive_file(file_id: str):
            """Get information about a specific Google Drive file"""
            try:
                file_info = await google_drive_tool.get_file_info(file_id)
                if not file_info:
                    raise HTTPException(status_code=404, detail="File not found")
                return file_info.to_dict()
            except Exception as e:
                raise HTTPException(status_code=500, detail=str(e))

        @app.get("/google-drive/files/{file_id}/content")
        async def get_google_drive_file_content(file_id: str):
            """Get the content of a Google Drive file"""
            try:
                content = await google_drive_tool.read_file_content(file_id)
                if not content:
                    raise HTTPException(status_code=404, detail="File not found or content not readable")
                return content.to_dict()
            except Exception as e:
                raise HTTPException(status_code=500, detail=str(e))

# Initialize configuration and create the FastAPI app globally
from framework_config import initialize_config
config = initialize_config()  # This will load config based on ENVIRONMENT variable

app = create_fastapi_app_instance()  # Assign the app instance to the global 'app' variable

# The if __name__ == "__main__": block is no longer strictly necessary for Cloud Run,
# as Uvicorn will directly import 'app'. However, keeping it for local development is fine.
if __name__ == "__main__":
    try:
        # Log startup information (now that 'app' is globally available)
        server_config = config.get('server', {})
        database_config = config.get('database', {})
        branding_config = config.get('branding', {})

        logger.info("🚀 Starting Opportunity+ AI Agent Server")
        logger.info(f"📍 Host: {server_config.get('host', '0.0.0.0')}")
        logger.info(f"🔌 Port: {server_config.get('port', 8000)}")
        logger.info(f"🌐 Web Interface: {server_config.get('serve_web_interface', True)}")
        logger.info(f"💾 Database: {database_config.get('url', 'sqlite:///./ai_agent.db')}")
        logger.info(f"🔧 Development Mode: {server_config.get('is_development', True)}")
        logger.info(f"🏢 Application: {branding_config.get('application_name', 'AI Service')}")

        # Use the global 'app' instance
        uvicorn.run(
            app,
            host=server_config.get('host', '0.0.0.0'),
            port=server_config.get('port', 8000),
            log_level="info"
        )

    except KeyboardInterrupt:
        logger.info("🛑 Server stopped by user")
    except Exception as e:
        logger.error(f"❌ Server failed to start: {str(e)}")
        import sys
        sys.exit(1)