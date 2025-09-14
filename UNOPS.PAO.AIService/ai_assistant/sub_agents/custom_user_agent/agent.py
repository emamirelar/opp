"""
Custom User Agent

A simple custom agent that responds to user messages using the BaseAgent pattern.
Based on the StoryFlowAgent example but simplified for basic user interaction.
"""

import logging
from typing import AsyncGenerator
from typing_extensions import override

from google.adk.agents import BaseAgent
from google.adk.agents.invocation_context import InvocationContext
from google.adk.events import Event
from google.genai import types
from pydantic import BaseModel, Field
import json
import re

from ai_assistant.utils.api_config_manager import config_manager

# Configure logging
logger = logging.getLogger(__name__)


class UserRequestAgent(BaseAgent):
    """
    Simple custom agent that responds to user messages.
    
    This agent demonstrates the basic structure of a custom ADK agent
    that can access user profile and screen context from state.
    """
    
    # Pydantic configuration to allow arbitrary types
    model_config = {"arbitrary_types_allowed": True}
    
    def __init__(self, name: str = "UserRequestAgent"):
        """
        Initialize the custom user agent.
        
        Args:
            name: The name of the agent
        """
        # Call super().__init__ with required parameters
        super().__init__(
            name=name,
            sub_agents=[]  # No sub-agents for this simple example
        )
        
        # Load application configuration
        try:
            self.application_name = config_manager.get_application_name()
            self.project_name = config_manager.get_project_name()
        except Exception:
            self.application_name = "Opportunity+"
            self.project_name = "UNOPS"
    
    def _extract_user_context(self, ctx: InvocationContext) -> dict:
        """
        Extract user profile and screen context from session state.
        
        Args:
            ctx: The invocation context containing session state
            
        Returns:
            dict: Extracted user context information
        """
        user_context = {
            "user_name": "User",
            "current_screen": "Dashboard",
            "user_profile": {},
            "screen_context": {}
        }
        
        if not ctx.session or not ctx.session.state:
            return user_context
        
        # Extract user profile
        user_profile = ctx.session.state.get('user_profile', {})
        if user_profile:
            user_context["user_profile"] = user_profile
            user_context["user_name"] = (
                user_profile.get('firstName') or 
                user_profile.get('name') or 
                user_profile.get('displayName') or 
                "User"
            )
        
        # Extract screen context
        screen_context = ctx.session.state.get('screen_context', {})
        if screen_context:
            user_context["screen_context"] = screen_context
            user_context["current_screen"] = (
                screen_context.get('current_screen') or 
                screen_context.get('page') or 
                screen_context.get('route') or 
                "Dashboard"
            )
        
        return user_context
    
    def _is_simple_request(self, user_message: str) -> bool:
        """
        Determine if user message is a simple request that doesn't need tools.
        
        Args:
            user_message: The user's input message
            
        Returns:
            bool: True if it's a simple request, False if tools are needed
        """
        message_lower = user_message.lower().strip()
        
        # Simple greetings
        simple_patterns = [
            "hi", "hello", "hey", "good morning", "good afternoon", "good evening",
            "thank", "thanks", "appreciate",
            "yes", "no", "okay", "sure", "alright",
            "what can you do", "how can you help", "help me",
            "what is this", "tell me about"
        ]
        
        # Check if it's a simple pattern
        return any(pattern in message_lower for pattern in simple_patterns)
    
    def _detect_entity_and_intent(self, user_message: str) -> dict:
        """
        Detect entity and intent from user message.
        
        Args:
            user_message: The user's input message
            
        Returns:
            dict: Detected entity and intent information
        """
        message_lower = user_message.lower().strip()
        
        # Entity patterns (from config_manager)
        entity_mappings = {
            'partner': ['partner', 'organization', 'collaborator', 'affiliate', 'institution', 'alliance', 'company'],
            'contact': ['contact', 'person', 'associate', 'connection', 'representative', 'individual', 'people'],
            'interaction': ['interaction', 'meeting', 'call', 'communication', 'engagement', 'discussion'],
            'engagement': ['engagement', 'project', 'assignment', 'task', 'venture', 'undertaking'],
            'document': ['document', 'file', 'attachment', 'record', 'paperwork', 'evidence']
        }
        
        # Intent patterns
        intent_mappings = {
            'search': ['search', 'find', 'look for', 'get', 'show me', 'list', 'browse', 'view'],
            'create': ['create', 'add', 'new', 'make', 'register', 'insert'],
            'update': ['update', 'modify', 'edit', 'change', 'revise'],
            'delete': ['delete', 'remove', 'destroy', 'eliminate']
        }
        
        detected_entity = None
        detected_intent = None
        
        # Detect entity
        for entity, synonyms in entity_mappings.items():
            if any(synonym in message_lower for synonym in synonyms):
                detected_entity = entity
                break
        
        # Detect intent
        for intent, patterns in intent_mappings.items():
            if any(pattern in message_lower for pattern in patterns):
                detected_intent = intent
                break
        
        # Default intent if entity found but no intent
        if detected_entity and not detected_intent:
            detected_intent = 'search'  # Default to search
        
        return {
            'entity': detected_entity,
            'intent': detected_intent,
            'has_tools_needed': detected_entity is not None
        }
    
    def _extract_parameters(self, user_message: str, entity: str) -> dict:
        """
        Extract parameters from user message for the detected entity.
        
        Args:
            user_message: The user's input message
            entity: The detected entity
            
        Returns:
            dict: Extracted parameters
        """
        params = {}
        
        # Look for quoted strings (search terms)
        quoted_matches = re.findall(r'"([^"]*)"', user_message)
        if quoted_matches:
            params['searchText'] = quoted_matches[0]
        
        # Look for names (capitalized words)
        elif entity in ['partner', 'contact']:
            # Extract potential names (2+ consecutive capitalized words)
            name_pattern = r'\b[A-Z][a-zA-Z]*(?:\s+[A-Z][a-zA-Z]*)+\b'
            name_matches = re.findall(name_pattern, user_message)
            if name_matches:
                params['searchText'] = name_matches[0]
        
        # If no specific search term found, use a general search
        if not params.get('searchText'):
            # Remove common words and extract meaningful terms
            words = user_message.lower().split()
            stop_words = {'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for', 'of', 'with', 'by', 'show', 'me', 'get', 'find', 'search'}
            meaningful_words = [word for word in words if word not in stop_words and len(word) > 2]
            if meaningful_words:
                params['searchText'] = ' '.join(meaningful_words[:3])  # First 3 meaningful words
        
        return params
    
    async def _execute_tool_directly(self, entity: str, intent: str, params: dict, ctx: InvocationContext) -> dict:
        """
        Execute tool directly without going through LoopAgent.
        
        Args:
            entity: The entity to operate on
            intent: The intent/action to perform
            params: Parameters for the tool
            ctx: The invocation context
            
        Returns:
            dict: Tool execution result
        """
        try:
            # Import the invoke_api_for_data function
            from ai_assistant.sub_agents.task_executor_agent.utils import invoke_api_for_data
            
            # Convert params to JSON string
            params_json = json.dumps(params)
            
            logger.info(f"[{self.name}] Executing tool: entity={entity}, intent={intent}, params={params_json}")
            
            # Execute the tool directly
            result = invoke_api_for_data(entity, intent, params_json, None)
            
            # Parse the result if it's a JSON string
            if isinstance(result, str):
                try:
                    result = json.loads(result)
                except json.JSONDecodeError:
                    result = {"error": "Failed to parse tool result", "raw_result": result}
            
            logger.info(f"[{self.name}] Tool execution completed successfully")
            return result
            
        except Exception as e:
            logger.error(f"[{self.name}] Tool execution failed: {str(e)}")
            return {
                "error": f"Tool execution failed: {str(e)}",
                "entity": entity,
                "intent": intent,
                "params": params
            }
    
    def _generate_response(self, user_message: str, user_context: dict) -> dict:
        """
        Generate a response based on user message and context.
        
        Args:
            user_message: The user's input message
            user_context: Extracted user context information
            
        Returns:
            dict: JSON response in the expected format
        """
        user_name = user_context["user_name"]
        current_screen = user_context["current_screen"]
        
        # Simple message classification and response generation
        message_lower = user_message.lower().strip()
        
        # Handle greetings
        if any(greeting in message_lower for greeting in ["hi", "hello", "hey", "good morning", "good afternoon", "good evening"]):
            response = {
                "result": [
                    {
                        "type": "markdown",
                        "message": f"Hi {user_name}! 👋 Welcome to {self.application_name}! I can see you're on the {current_screen} screen. I'm here to help you with your business data, partners, contacts, and much more. What would you like to do today?"
                    }
                ],
                "followUps": ["Search partners", "View interactions", "Create new contact"]
            }
        
        # Handle gratitude
        elif any(thanks in message_lower for thanks in ["thank", "thanks", "appreciate"]):
            response = {
                "result": [
                    {
                        "type": "markdown",
                        "message": f"You're very welcome, {user_name}! I'm always happy to help. If you need anything else while working on {current_screen}, just let me know!"
                    }
                ],
                "followUps": []
            }
        
        # Handle help requests
        elif any(help_word in message_lower for help_word in ["help", "what can you do", "how can you help"]):
            response = {
                "result": [
                    {
                        "type": "markdown",
                        "message": f"I'm your AI assistant for {self.application_name}! I can help you with partners, contacts, interactions, engagements, documents, and more. Since you're on {current_screen}, I can provide specific guidance for this area too."
                    }
                ],
                "followUps": ["Browse partners", "Help with entities", "Show available data"]
            }
        
        # Default response for other messages
        else:
            response = {
                "result": [
                    {
                        "type": "markdown",
                        "message": f"Hi {user_name}! I understand you said: \"{user_message}\". I'm still learning to handle different types of requests. For now, I can help with greetings and basic questions. You're currently on the {current_screen} screen."
                    }
                ],
                "followUps": ["Ask for help", "Say hello", "Thank the assistant"]
            }
        
        return response
    
    @override
    async def _run_async_impl(
        self, ctx: InvocationContext
    ) -> AsyncGenerator[Event, None]:
        """
        Implement the custom agent's main logic.
        
        Args:
            ctx: The invocation context containing the user's message and session state
            
        Yields:
            Event: Events representing the agent's response
        """
        logger.info(f"[{self.name}] Starting custom user agent processing.")
        
        # Extract user message from the context
        user_message = "Hello"  # Default message
        if ctx.new_message and ctx.new_message.parts:
            user_message = ctx.new_message.parts[0].text
        
        logger.info(f"[{self.name}] Processing user message: {user_message}")
        
        # Extract user context from session state
        user_context = self._extract_user_context(ctx)
        logger.info(f"[{self.name}] User context: {user_context['user_name']} on {user_context['current_screen']}")
        
        # Generate response
        response = self._generate_response(user_message, user_context)
        logger.info(f"[{self.name}] Generated response: {response}")
        
        # Create response content
        response_text = json.dumps(response, indent=2)
        response_content = types.Content(
            role='model',
            parts=[types.Part(text=response_text)]
        )
        
        # Store the response in session state if needed
        if ctx.session:
            ctx.session.state["last_response"] = response
        
        # Yield the response as a ModelResponseEvent
        yield ModelResponseEvent(
            content=response_content,
            author=self.name
        )
        
        logger.info(f"[{self.name}] Custom user agent processing completed.")


# Create the user request agent instance
user_request_agent = UserRequestAgent(name="UserRequestAgent")
