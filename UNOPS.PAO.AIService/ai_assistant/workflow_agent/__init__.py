"""
Workflow Agent Module

This module exports the main workflow agent that orchestrates entity detection,
API operations, and response formatting through a sequential agent architecture.
"""

from .agent import workflow_agent

__all__ = ['workflow_agent'] 