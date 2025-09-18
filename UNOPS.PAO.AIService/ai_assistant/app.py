#!/usr/bin/env python3
"""
UNOPS AI Agent Framework - FastAPI App Factory

This module provides the app creation functionality that teams can import
to create their customized AI service instances.
"""

import logging
import os
from contextlib import asynccontextmanager

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

# Framework imports
from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.utils.framework_config import get_config

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
# External API tools are handled automatically by the agent system


@asynccontextmanager
async def lifespan(app_instance: FastAPI):
    """
    Application lifecycle manager for startup/shutdown events
    """
    # Startup
    logger.info("🚀 Starting UNOPS AI Agent application...")
    
    try:
        # External API tools are initialized automatically by the agent system
        logger.info("✅ External API tools will be initialized by agent system")
        
        # Load entity configurations
        config_manager.load_tools_config()
        logger.info("✅ Entity configurations loaded")
        
        # Load framework configuration
        config = get_config()
        logger.info("✅ Framework configuration loaded")
        
        yield
        
    except Exception as e:
        logger.error(f"❌ Failed to initialize application: {str(e)}")
        raise
    finally:
        # Shutdown
        logger.info("🔄 Shutting down UNOPS AI Agent application...")
        logger.info("✅ External API tools cleaned up automatically")


def create_app():
    """
    Create and configure the FastAPI application with ADK
    
    This function is the main entry point for teams to create their AI service.
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
    framework_root = os.path.dirname(os.path.dirname(__file__))  # Points to the package root
    
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


def add_routers_and_endpoints(app: FastAPI):
    """Add all routers and endpoints to the FastAPI app"""
    # Add chat router with proper API prefix
    app.include_router(chat_router, prefix="/api/ai-assistant")
    logger.info("✅ Chat router included at /api/ai-assistant")

    # Add session router with proper API prefix  
    app.include_router(session_router, prefix="/api/ai-assistant")
    logger.info("✅ Session router included at /api/ai-assistant")

    # Add action log router with proper API prefix
    app.include_router(action_log_router, prefix="/api/ai-assistant") 
    logger.info("✅ Action log router included at /api/ai-assistant")

    # Add framework endpoints (external API tools handled automatically)
    add_framework_endpoints(None)
    from routers.framework import router as framework_router
    
    # Framework router typically goes at root level for system endpoints
    app.include_router(framework_router, prefix="/framework")
    logger.info("✅ Framework router included at /framework")


 