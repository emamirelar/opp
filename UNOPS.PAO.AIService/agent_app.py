#!/usr/bin/env python3
"""
ADK Agent Application Entry Point
================================

This file provides the proper entry point for the Google ADK to load your agents.
Use this instead of YAML configuration which is not ready for production.
"""

from ai_assistant.task_executor_agent import task_executor_agent

# Export the main agent for ADK to discover
app = task_executor_agent

# For backward compatibility and explicit naming
def get_agent():
    """Return the main agent for ADK"""
    return task_executor_agent

# Make sure the agent is available at module level
__all__ = ['app', 'task_executor_agent', 'get_agent']
