"""
Main AI Assistant Agent

This is the entry point for the AI assistant agent hierarchy.
"""

# Import the main task executor agent
from .task_executor_agent import task_executor_agent

# --- Root Agent Definition ---
# This is the entry point for the entire agent hierarchy
root_agent = task_executor_agent