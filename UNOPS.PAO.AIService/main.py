#!/usr/bin/env python3
"""
Main FastAPI application for UNOPS AI Agent with ADK

This is the single entry point that sets up the FastAPI app, handles all
configuration, and starts the server.

main.py loads the single environment variable CURRENT_ENV which is used to load the correct configuration file.
All other configuration is loaded from the configuration file (through utils.config.py which references the CURRENT_ENV environment variable to load the correct configuration file).
"""

print("🐍 Python service starting...")
import sys
print(f"🐍 Python version: {sys.version}")
print("🐍 Loading basic imports...")
import logging
import os
from contextlib import asynccontextmanager

print(f"🐍 Working directory: {os.getcwd()}")
print(f"🐍 Environment variables: CURRENT_ENV={os.getenv('CURRENT_ENV', 'NOT SET')}")

print("🐍 Loading FastAPI and uvicorn...")
import uvicorn
from fastapi import FastAPI

print("🐍 Loading Google ADK...")
from google.adk.cli.fast_api import get_fast_api_app

print("🐍 Loading configuration modules...")
# Configuration
from ai_assistant.utils.config import get_config
from ai_assistant.utils.config import get_database_url

# Patch ADK to use naive UTC for session timestamps (PostgreSQL + asyncpg compatibility)
from ai_assistant.utils.adk_session_patch import apply_adk_session_timestamp_patch
apply_adk_session_timestamp_patch()

print("🐍 Loading routers...")
# Routers
from routers.chat import router as chat_router
from routers.session import router as session_router

print("🐍 All imports completed successfully!")

# Fix OpenTelemetry context issues
import warnings
warnings.filterwarnings("ignore", category=UserWarning, message=".*opentelemetry.*")

# Disable OpenTelemetry completely via environment variable
os.environ["OTEL_SDK_DISABLED"] = "true"

# Additional OpenTelemetry suppression
try:
    from opentelemetry.context import _RUNTIME_CONTEXT
    # Monkey patch to suppress context detach errors
    original_detach = _RUNTIME_CONTEXT.detach
    def safe_detach(token):
        try:
            return original_detach(token)
        except ValueError as e:
            if "was created in a different Context" in str(e):
                # Silently ignore context errors that don't affect functionality
                pass
            else:
                raise
    _RUNTIME_CONTEXT.detach = safe_detach
except ImportError:
    # OpenTelemetry not installed or different version
    pass


# Load environment variables (.env file on local and actual environment variables on other environments)
ENVIRONMENT_DIR = os.path.join(os.path.dirname(__file__), '..', 'AIService')
print(f"ENVIRONMENT_DIR: {ENVIRONMENT_DIR}")
try:
    from dotenv import load_dotenv
    # Look for .env file in the config directory
    config_dir = ENVIRONMENT_DIR
    env_file_path = os.path.join(config_dir, '.env')
    load_dotenv(env_file_path)
    print(f"✅ Loaded .env file from {env_file_path} - ENVIRONMENT: {os.getenv('CURRENT_ENV', 'not set')}")
    # Raise an error if the environment is not set
    if os.getenv('CURRENT_ENV') is None:
        raise ValueError("CURRENT_ENV is not set")
except ImportError:
    print("⚠️ python-dotenv not installed, .env file won't be loaded")



@asynccontextmanager
async def lifespan(app_instance: FastAPI):
    """Application lifespan manager - load configurations once at startup"""
    logger.info("🚀 UNOPS AI Agent application starting up...")

    try:
        # TODO: Any application specific startup logic here
        yield

    except Exception as e:
        logger.error(f"❌ Application startup FAILED: {str(e)}")
        raise
    finally:
        # Cleanup
        logger.info("🔄 Shutting down UNOPS AI Agent application...")



def add_routers_and_endpoints(app: FastAPI):
    """Add all routers and endpoints to the FastAPI app"""
    print("🔧 Adding routers to FastAPI app...")
    
    # ROUTE on just /
    print("🔧 Adding chat_router and session_router to root...")
    app.include_router(chat_router)
    app.include_router(session_router)

    # ROUTE on /api/ai-assistant
    print("🔧 Adding chat_router and session_router to /api/ai-assistant...")
    app.include_router(chat_router, prefix='/api/ai-assistant')
    app.include_router(session_router, prefix='/api/ai-assistant')
    
    print("🔧 Routers added successfully!")
    


def create_app():
    """
    Create and configure the FastAPI application with ADK
    
    This function creates the AI service application with all necessary configuration.
    """
    database_config = config.get('database', {})
    server_config = config.get('server', {})

    # Configure artifacts service
    artifact_service_uri = None
    artifact_config = config.get('artifacts', {})
    artifact_service_type = artifact_config.get('service_type', 'InMemoryArtifactService')
    if artifact_service_type == 'GcsArtifactService':
        gcs_bucket_name = artifact_config.get('gcs_bucket_name')
        if not gcs_bucket_name:
            raise ValueError("GCS bucket name must be specified in config for GcsArtifactService.")
        artifact_service_uri = f"gs://{gcs_bucket_name}"
        logger.info(f"Using GcsArtifactService with URI: {artifact_service_uri}")
    else:
        logger.info("Using InMemoryArtifactService")

    # Create the FastAPI app with ADK integration
    agents_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'ai_assistant') # Points to the ai_assistant directory (root of the agents)
    
    print("🔍 Attempting to get database URL...")
    try:
        db_url = get_database_url()
        print(f"✅ Database URL obtained: {db_url[:20]}..." if db_url else "❌ Database URL is None")
    except Exception as e:
        print(f"❌ Database URL loading failed: {e}")
        raise
    
    fastapi_app_instance = get_fast_api_app(
        agents_dir = agents_dir,
        session_service_uri = db_url,
        artifact_service_uri = artifact_service_uri,
        allow_origins = server_config.get('allow_origins'),
        web = server_config.get('serve_web_interface'),
        trace_to_cloud = False,
        lifespan = lifespan
    )

    # Override the app metadata
    branding_config = config.get('branding', {})
    app_title = branding_config.get('application_name', 'AI Agent')
    app_description = branding_config.get('description', 'In-app AI Agent')
    fastapi_app_instance.title = app_title
    fastapi_app_instance.description = app_description
    fastapi_app_instance.version = "1.0.0"

    # Add routers and endpoints
    add_routers_and_endpoints(fastapi_app_instance)

    return fastapi_app_instance



# Initialize configuration and create the FastAPI app globally
print("🔍 Attempting to load configuration...")
try:
    config = get_config()
    print("✅ Configuration loaded successfully")
except Exception as e:
    print(f"❌ Configuration loading failed: {e}")
    raise

# Set Vertex AI env vars for google-genai/ADK (required after google-genai upgrade)
# So the agent's model calls get project/location when not passed explicitly
google_cloud = config.get("google_cloud", {})
if google_cloud.get("use_vertex_ai", True):
    if not os.environ.get("GOOGLE_GENAI_USE_VERTEXAI"):
        os.environ["GOOGLE_GENAI_USE_VERTEXAI"] = "true"
    if not os.environ.get("GOOGLE_CLOUD_PROJECT") and google_cloud.get("project"):
        os.environ["GOOGLE_CLOUD_PROJECT"] = google_cloud["project"]
    if not os.environ.get("GOOGLE_CLOUD_LOCATION") and google_cloud.get("location"):
        os.environ["GOOGLE_CLOUD_LOCATION"] = google_cloud["location"]

server_config = config.get('server')
# Raise an error if the server config is not set
if server_config is None:
    raise ValueError("Server config is not set")

# Configure logging
log_level_str = server_config.get('log_level', 'info').upper()
logging_level = getattr(logging, log_level_str, logging.INFO) # Default to logging.INFO
logging.basicConfig(
    level = logging_level,
    format = '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

app = create_app()

if __name__ == "__main__":
    try:
        # Log startup information
        database_config = config.get('database', {})
        branding_config = config.get('branding', {})
        is_development = server_config.get('is_development', False)

        logger.info(f"🚀 Starting {branding_config.get('application_name')} Server")
        logger.info(f"📍 Host: {server_config.get('host')}")
        logger.info(f"🔌 Port: {server_config.get('port')}")
        logger.info(f"🌐 Web Interface: {server_config.get('serve_web_interface')}")
        
        # Smart database logging - show URL for local, secret name for hosted
        if 'url' in database_config:
            # Local development - show the URL
            logger.info(f"💾 Database: {database_config['url']}")
        elif 'secret_name' in database_config:
            # Hosted environment - show the secret name (not the actual credentials)
            logger.info(f"💾 Database: Secret Manager ({database_config['secret_name']})")
        else:
            logger.info(f"💾 Database: Configuration missing")

        logger.info(f"🔧 Development Mode: {is_development}")
        logger.info(f"🏢 Application: {branding_config.get('application_name')}")

        # Enhanced uvicorn configuration for streaming support
        uvicorn.run(
            'main:app',
            host = server_config.get('host'),
            port = server_config.get('port'),
            reload = is_development,
            log_level = server_config.get('log_level', 'info'),
            # Critical flags for streaming to work properly
            loop = "asyncio",           # Use asyncio event loop for streaming
            access_log = False,         # Disable access logging to prevent buffering
            server_header = False,      # Reduce header overhead
            date_header = False,        # Reduce header overhead
            # Ensure single worker for streaming compatibility
            workers = 1 if not is_development else None
        )

    except KeyboardInterrupt:
        logger.info("🛑 Server stopped by user")
    except Exception as e:
        logger.error(f"❌ Server failed to start: {str(e)}")
        import sys
        sys.exit(1)
