"""
API Worker Agent Module

This module contains the API worker agent that uses a LoopAgent to iterate through
detected entities and call the appropriate API endpoints based on tools.json configuration.

Components:
- Agent definition with LoopAgent
- API utility functions  
- API calling logic
- Exit mechanisms for loop termination
"""

from .agent import api_worker_agent

__all__ = ['api_worker_agent'] 