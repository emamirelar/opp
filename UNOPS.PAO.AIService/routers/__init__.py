"""
FastAPI Routers Package

Contains all API routers for the UNOPS AI Agent.
"""

# Import routers for easy access
try:
    from .chat import router as chat_router
    from .session import router as session_router
except ImportError:
    # Handle import errors gracefully during package installation
    chat_router = None
    session_router = None

__all__ = [
    "chat_router",
    "session_router",
] 