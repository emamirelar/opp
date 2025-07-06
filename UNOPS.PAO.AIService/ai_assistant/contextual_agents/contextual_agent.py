from google.adk.agents import ParallelAgent
from .user_detail_agent import user_detail_agent
from .screen_context_agent import screen_context_agent
from ..agent_callbacks import contextual_agent_gate_callback

contextual_agent = ParallelAgent(
    name="contextual_agent",
    description="Agent that gathers contextual information from the user",
    sub_agents=[
        user_detail_agent,
        screen_context_agent,
    ],
    # Use before_agent_callback for ParallelAgent (not gate_callback)
    # This callback can skip the entire parallel agent execution if both caches are valid
    before_agent_callback=contextual_agent_gate_callback
) 