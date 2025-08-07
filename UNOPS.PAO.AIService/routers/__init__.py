"""
FastAPI Routers Package

Contains all API routers for the UNOPS AI Agent Framework.
"""

# Import routers for easy access
try:
    from .chat import router as chat_router
    from .framework import router as framework_router
    from .session import router as session_router
except ImportError:
    # Handle import errors gracefully during package installation
    chat_router = None
    framework_router = None
    session_router = None

__all__ = [
    "chat_router",
    "framework_router", 
    "session_router",
] 