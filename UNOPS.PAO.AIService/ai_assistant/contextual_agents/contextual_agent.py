from google.adk.agents import ParallelAgent
from .user_detail_agent import user_detail_agent
from .screen_context_agent import screen_context_agent

contextual_agent = ParallelAgent(
    name="contextual_agent",
    description="Agent that gathers contextual information from the user",
    sub_agents=[
        user_detail_agent,
        screen_context_agent,
    ]
) 