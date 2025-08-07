"""
UNOPS AI Agent Framework

A comprehensive AI agent framework built with Google ADK for UNOPS applications.
Supports multiple teams with configurable entities, APIs, and business logic.
"""

__version__ = "1.0.0"
__author__ = "UNOPS Technology Team"

# Import main components for easy access
try:
    from .agent import root_agent
except ImportError:
    root_agent = None

# Import models package to ensure it's available
try:
    from . import models
except ImportError:
    models = None

# Export main components
__all__ = [
    "root_agent",
    "models",
] 