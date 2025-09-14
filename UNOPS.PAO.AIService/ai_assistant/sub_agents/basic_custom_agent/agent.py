"""
Basic Custom Agent - Hello World Test

A minimal custom agent to test the BaseAgent setup.
Just handles basic hello/response to verify the foundation works.
"""

import logging
from typing import AsyncGenerator
from typing_extensions import override

from google.adk.agents import BaseAgent
from google.adk.agents.invocation_context import InvocationContext
from google.adk.events import Event
from google.genai import types
import json

# Configure logging
logger = logging.getLogger(__name__)


class BasicCustomAgent(BaseAgent):
    """
    Basic custom agent for testing the foundation.
    Just handles hello → response to verify everything works.
    """
    
    model_config = {"arbitrary_types_allowed": True}
    
    def __init__(self, name: str = "BasicCustomAgent"):
        """Initialize the basic custom agent."""
        super().__init__(name=name, sub_agents=[])
        logger.info(f"[{self.name}] Basic custom agent initialized")
    
    @override
    async def _run_async_impl(self, ctx: InvocationContext) -> AsyncGenerator[Event, None]:
        """
        Basic implementation - just handle hello and respond.
        """
        logger.info(f"[{self.name}] Processing request")
        
        # Extract user message
        user_message = "Hello"
        if ctx.new_message and ctx.new_message.parts:
            user_message = ctx.new_message.parts[0].text
        
        logger.info(f"[{self.name}] User said: '{user_message}'")
        
        # Create a simple response
        response = {
            "result": [
                {
                    "type": "markdown",
                    "message": f"Hello! You said: '{user_message}'. This is a basic custom agent working correctly! 👋"
                }
            ],
            "followUps": ["Say hello again", "Test another message"]
        }
        
        # Create response content
        response_text = json.dumps(response, indent=2)
        response_content = types.Content(
            role='model',
            parts=[types.Part(text=response_text)]
        )
        
        # Yield the response event
        yield Event(content=response_content, author=self.name)
        
        logger.info(f"[{self.name}] Response sent successfully")


# Create the agent instance
basic_custom_agent = BasicCustomAgent(name="BasicCustomAgent")
