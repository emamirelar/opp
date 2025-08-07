"""
Contextual Agent

This module defines the contextual agent that coordinates parallel gathering
of user details and screen context information.
"""

from google.adk.agents import ParallelAgent
from ..user_detail_agent import user_detail_agent
from ..screen_context_agent import screen_context_agent
from ..entity_detection_agent import entity_detection_agent
from ..geo_time_agent import geo_time_agent
from .utils import log_contextual_start, log_contextual_complete

contextual_agent = ParallelAgent(
    name="contextual_agent",
    description="Agent that gathers contextual information from the user, creates action plans, and retrieves geo-time data",
    sub_agents=[
        user_detail_agent,
        screen_context_agent,
        geo_time_agent,
    ],
)