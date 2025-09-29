#!/usr/bin/env python3
"""
Main FastAPI application for UNOPS AI Agent with ADK

This is the single entry point that sets up the FastAPI app, handles all
configuration, and starts the server.
"""

import logging
import os
from contextlib import asynccontextmanager

import uvicorn
from fastapi import FastAPI
from google.adk.cli.fast_api import get_fast_api_app

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

# Load environment variables from .env file
try:
    from dotenv import load_dotenv
    load_dotenv()
    print(f"✅ Loaded .env file - ENVIRONMENT: {os.getenv('CURRENT_ENV', 'not set')}")
except ImportError:
    print("⚠️ python-dotenv not installed, .env file won't be loaded")

# Framework imports
from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.utils.framework_config import get_config, initialize_config

# Import routers
from routers.chat import router as chat_router
from routers.session import router as session_router
from routers.action_log import router as action_log_router
from routers.framework import add_framework_endpoints

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Global variables
app = None


@asynccontextmanager
async def lifespan(app_instance: FastAPI):
    """Application lifespan manager - load configurations once at startup"""
    logger.info("🚀 UNOPS AI Agent application starting up...")

    try:
        # Load configurations once at startup
        logger.info("📋 Loading configurations...")
        
        # First try to load from team's config directory
        tools_config_path = 'config/tools.json'
        if not os.path.exists(tools_config_path):
            # Fallback to default tools config if team hasn't provided one
            logger.info("ℹ️ Team tools config not found, using default configuration")
            tools_config_path = 'config/tools/tools.json'
        
        config_manager.load_tools_config(tools_config_path)
        logger.info("✅ Entity configurations loaded")
        
        # Initialize action logging table if enabled
        action_logging_config = config.get('action_logging', {})
        if action_logging_config.get('enabled', False):
            try:
                from ai_assistant.utils.database_manager import db_manager
                table_created = db_manager.create_action_log_table_if_not_exists()
                if table_created:
                    logger.info("✅ Action logging table initialized successfully")
                else:
                    logger.warning("⚠️ Action logging table creation failed")
                    logger.info("📝 Application will continue without action logging")
            except Exception as e:
                logger.warning(f"⚠️ Action logging initialization failed: {str(e)}")
                logger.info("📝 Application will continue without action logging")
        else:
            logger.info("📝 Action logging disabled in configuration")

        logger.info("✅ All configurations loaded successfully!")

        yield

    except Exception as e:
        logger.error(f"❌ Failed to initialize application: {str(e)}")
        raise
    finally:
        # Cleanup
        logger.info("🔄 Shutting down UNOPS AI Agent application...")
        logger.info("✅ External API tools cleaned up automatically")


def add_routers_and_endpoints(app: FastAPI):
    """Add all routers and endpoints to the FastAPI app"""
    # Add chat router with proper API prefix
    app.include_router(chat_router)
    # Add session router with proper API prefix  
    app.include_router(session_router)
    # Add action log router with proper API prefix
    app.include_router(action_log_router)
     
    # Add framework endpoints (external API tools handled automatically)
    add_framework_endpoints(None)
    from routers.framework import router as framework_router
    # Framework router typically goes at root level for system endpoints
    app.include_router(framework_router)


def create_app():
    """
    Create and configure the FastAPI application with ADK
    
    This function creates the AI service application with all necessary configuration.
    """
    config = get_config()
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

    # Get the framework's installed location for agents_dir
    framework_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))  # Points to the package root
    
    # Create the FastAPI app with ADK integration
    fastapi_app_instance = get_fast_api_app(
        agents_dir=framework_root,  # Use framework's installed location
        session_service_uri=database_config.get('url', 'sqlite:///./ai_agent.db'),
        artifact_service_uri=artifact_service_uri,
        allow_origins=server_config.get('allow_origins', ["*"]),
        web=server_config.get('serve_web_interface', True),
        trace_to_cloud=False,
        lifespan=lifespan
    )

    # Override the app metadata
    branding_config = config.get('branding', {})
    app_title = branding_config.get('application_name', 'AI Service')
    app_description = branding_config.get('description', 'AI Agent Framework')
    fastapi_app_instance.title = app_title
    fastapi_app_instance.description = f"AI Agent Framework for {app_title} with Google ADK integration"
    fastapi_app_instance.version = "1.0.0"

    # Add routers and endpoints
    add_routers_and_endpoints(fastapi_app_instance)

    return fastapi_app_instance


# Initialize configuration and create the FastAPI app globally
config = initialize_config()
app = create_app()

if __name__ == "__main__":
    try:
        # Log startup information
        server_config = config.get('server', {})
        database_config = config.get('database', {})
        branding_config = config.get('branding', {})

        logger.info(f"🚀 Starting {branding_config.get('application_name', 'UNOPS AI Agent')} Server")
        logger.info(f"📍 Host: {server_config.get('host', '0.0.0.0')}")
        logger.info(f"🔌 Port: {server_config.get('port', 8000)}")
        logger.info(f"🌐 Web Interface: {server_config.get('serve_web_interface', True)}")
        logger.info(f"💾 Database: {database_config.get('url', 'sqlite:///./ai_agent.db')}")
        logger.info(f"🔧 Development Mode: {server_config.get('is_development', False)}")
        logger.info(f"🏢 Application: {branding_config.get('application_name', 'AI Service')}")

        # Enhanced uvicorn configuration for streaming support
        uvicorn.run(
            'main:app',
            host=server_config.get('host', '0.0.0.0'),
            port=server_config.get('port', 8000),
            reload=server_config.get('is_development', False),
            log_level="info",
            # Critical flags for streaming to work properly
            loop="asyncio",           # Use asyncio event loop for streaming
            access_log=False,         # Disable access logging to prevent buffering
            server_header=False,      # Reduce header overhead
            date_header=False,        # Reduce header overhead
            # Ensure single worker for streaming compatibility
            workers=1 if not server_config.get('is_development', False) else None
        )

    except KeyboardInterrupt:
        logger.info("🛑 Server stopped by user")
    except Exception as e:
        logger.error(f"❌ Server failed to start: {str(e)}")
        import sys
        sys.exit(1)