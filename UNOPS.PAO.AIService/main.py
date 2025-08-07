#!/usr/bin/env python3
"""
Main FastAPI application for UNOPS AI Agent with ADK

This is the clean entry point that sets up the FastAPI app and includes
routers for different endpoint groups. Most functionality has been moved
to organized modules for better maintainability.
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
from ai_assistant.tools import create_google_drive_tool
from ai_assistant.utils.framework_config import get_config, initialize_config

# Import routers
from routers.chat import router as chat_router
from routers.session import router as session_router
from routers.framework import add_framework_endpoints

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Global variables
google_drive_tool = None
app = None


@asynccontextmanager
async def lifespan(app_instance: FastAPI):
    """Application lifespan manager - load configurations once at startup"""
    logger.info("🚀 FastAPI server starting up...")

    # Load configurations once at startup
    logger.info("📋 Loading configurations...")
    
    # First try to load from team's config directory
    tools_config_path = 'config/tools.json'
    if not os.path.exists(tools_config_path):
        # Fallback to default tools config if team hasn't provided one
        logger.info("ℹ️ Team tools config not found, using default configuration")
        tools_config_path = 'config/tools/tools.json'
    
    config_manager.load_tools_config(tools_config_path)

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

    # Cleanup
    logger.info("🛑 FastAPI server shutting down...")
    if google_drive_tool:
        await google_drive_tool.cleanup()
        logger.info("✅ Google Drive tool cleaned up")


# Initialize configuration and create the FastAPI app globally
config = initialize_config()
from ai_assistant.app import create_app
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

        uvicorn.run(
            'main:app',
            host=server_config.get('host', '0.0.0.0'),
            port=server_config.get('port', 8000),
            reload=server_config.get('is_development', False),
            log_level="info"
        )

    except KeyboardInterrupt:
        logger.info("🛑 Server stopped by user")
    except Exception as e:
        logger.error(f"❌ Server failed to start: {str(e)}")
        import sys
        sys.exit(1)