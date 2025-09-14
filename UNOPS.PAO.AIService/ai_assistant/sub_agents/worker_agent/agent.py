"""
Main Workflow Agent

This module defines the main workflow agent that orchestrates the task executor
for processing user requests directly with dynamic tool discovery.
"""

from google.adk.agents import SequentialAgent
from ..task_planner_agent import task_planner_agent
from .utils import log_workflow_start, log_workflow_complete

worker_agent = SequentialAgent(
    name="worker_agent",
    description="Simplified workflow: Direct execution with dynamic tool discovery and injection",
    sub_agents=[
        task_planner_agent            # Self-discovering executor with dynamic tool injection
    ],
)