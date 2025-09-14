#!/usr/bin/env python3
"""
ADK Agent Application Entry Point
================================

This file provides the proper entry point for the Google ADK to load your agents.
Use this instead of YAML configuration which is not ready for production.
"""

from ai_assistant.sub_agents.user_request_agent.agent import user_request_agent

# Export the main agent for ADK to discover
app = user_request_agent

# For backward compatibility and explicit naming
def get_agent():
    """Return the main agent for ADK"""
    return user_request_agent

# Make sure the agent is available at module level
__all__ = ['app', 'user_request_agent', 'get_agent']
