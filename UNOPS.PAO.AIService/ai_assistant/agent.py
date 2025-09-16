"""
Main AI Assistant Agent

This is the entry point for the AI assistant agent hierarchy.
"""

from google.adk.agents import SequentialAgent

# Import sub-agents
from .sub_agents.user_request_agent import user_request_agent

# --- Root Agent Definition ---
# This is the entry point for the entire agent hierarchy
root_agent = user_request_agent
