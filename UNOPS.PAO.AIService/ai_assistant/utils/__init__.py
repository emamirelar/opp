"""
AI Assistant Utilities Package

Contains utility modules for the AI assistant including configuration management.
"""

# Make key utilities available at package level
from .api_config_manager import config_manager

__all__ = ['config_manager']
