"""
Smart User Agent - Built from Scratch

A high-performance custom agent that intelligently routes user requests:
1. Simple requests → Direct response (greetings, thanks, basic help)
2. Complex requests → Tool detection → Direct tool execution (no LoopAgent overhead)

This eliminates the complexity of the existing AIService architecture.
"""

import logging
import json
import re
from typing import AsyncGenerator, Dict, Any, Optional
from typing_extensions import override

from google.adk.agents import BaseAgent
from google.adk.agents.invocation_context import InvocationContext
from google.adk.events import Event
from google.genai import types

# Configure logging
logger = logging.getLogger(__name__)


class SmartUserAgent(BaseAgent):
    """
    Smart User Agent that intelligently routes requests for optimal performance.
    
    Architecture:
    User Request → Smart Routing → Either:
    - Direct Response (simple requests)
    - Tool Detection + Direct Execution (complex requests)
    """
    
    model_config = {"arbitrary_types_allowed": True}
    
    def __init__(self, name: str = "SmartUserAgent"):
        """Initialize the Smart User Agent."""
        super().__init__(name=name, sub_agents=[])
        
        # Load basic configuration
        self.application_name = "Opportunity+"
        self.project_name = "UNOPS"
        
        logger.info(f"[{self.name}] Initialized Smart User Agent")
    
    def _extract_user_context(self, ctx: InvocationContext) -> Dict[str, Any]:
        """Extract user profile and screen context from session state."""
        context = {
            "user_name": "User",
            "current_screen": "Dashboard",
            "user_profile": {},
            "screen_context": {}
        }
        
        if ctx and ctx.session and ctx.session.state:
            # Extract user profile
            user_profile = ctx.session.state.get('user_profile', {})
            if user_profile:
                context["user_profile"] = user_profile
                context["user_name"] = (
                    user_profile.get('firstName') or 
                    user_profile.get('name') or 
                    user_profile.get('displayName') or 
                    "User"
                )
            
            # Extract screen context
            screen_context = ctx.session.state.get('screen_context', {})
            if screen_context:
                context["screen_context"] = screen_context
                context["current_screen"] = (
                    screen_context.get('current_screen') or 
                    screen_context.get('page') or 
                    screen_context.get('route') or 
                    "Dashboard"
                )
        
        return context
    
    def _is_simple_request(self, message: str) -> bool:
        """
        Determine if this is a simple request that can be handled directly.
        
        Simple requests: greetings, thanks, basic help, confirmations
        Complex requests: anything requiring data operations or tools
        """
        message_lower = message.lower().strip()
        
        # Simple request patterns
        simple_patterns = [
            # Greetings
            "hi", "hello", "hey", "good morning", "good afternoon", "good evening",
            # Gratitude
            "thank", "thanks", "appreciate", "grateful",
            # Confirmations
            "yes", "no", "okay", "sure", "alright", "fine",
            # Basic help
            "what can you do", "how can you help", "help me", "what is this",
            # Pleasantries
            "how are you", "what's up", "how's it going"
        ]
        
        return any(pattern in message_lower for pattern in simple_patterns)
    
    def _detect_entity_and_intent(self, message: str) -> Dict[str, Any]:
        """
        Detect entity and intent from user message for tool execution.
        
        Returns:
            dict: {
                'entity': str | None,
                'intent': str | None, 
                'confidence': float,
                'needs_tools': bool
            }
        """
        message_lower = message.lower().strip()
        
        # Entity mapping (based on your business entities)
        entity_patterns = {
            'Partner': ['partner', 'organization', 'company', 'collaborator', 'affiliate', 'institution'],
            'Contact': ['contact', 'person', 'people', 'individual', 'representative', 'associate'],
            'Interaction': ['interaction', 'meeting', 'call', 'communication', 'discussion', 'engagement'],
            'Engagement': ['engagement', 'project', 'assignment', 'task', 'venture'],
            'Document': ['document', 'file', 'attachment', 'record', 'paperwork']
        }
        
        # Intent mapping
        intent_patterns = {
            'search': ['search', 'find', 'look for', 'get', 'show', 'list', 'browse', 'view', 'details'],
            'create': ['create', 'add', 'new', 'make', 'register', 'insert'],
            'update': ['update', 'modify', 'edit', 'change', 'revise'],
            'delete': ['delete', 'remove', 'destroy', 'eliminate']
        }
        
        detected_entity = None
        detected_intent = None
        confidence = 0.0
        
        # Detect entity
        for entity, patterns in entity_patterns.items():
            matches = sum(1 for pattern in patterns if pattern in message_lower)
            if matches > 0:
                detected_entity = entity
                confidence += matches * 0.3
                break
        
        # Detect intent
        for intent, patterns in intent_patterns.items():
            matches = sum(1 for pattern in patterns if pattern in message_lower)
            if matches > 0:
                detected_intent = intent
                confidence += matches * 0.2
                break
        
        # Default to search if entity found but no intent
        if detected_entity and not detected_intent:
            detected_intent = 'search'
            confidence += 0.1
        
        return {
            'entity': detected_entity,
            'intent': detected_intent,
            'confidence': confidence,
            'needs_tools': detected_entity is not None
        }
    
    def _extract_parameters(self, message: str, entity: str) -> Dict[str, Any]:
        """Extract search parameters from user message."""
        params = {}
        
        # Look for quoted strings first
        quoted_matches = re.findall(r'"([^"]*)"', message)
        if quoted_matches:
            params['searchText'] = quoted_matches[0]
            return params
        
        # Look for capitalized names (likely proper nouns)
        name_pattern = r'\b[A-Z][a-zA-Z]*(?:\s+[A-Z][a-zA-Z]*)*\b'
        name_matches = re.findall(name_pattern, message)
        
        # Filter out common words that might be capitalized
        common_caps = {'Partner', 'Contact', 'Interaction', 'Document', 'Show', 'Get', 'Find', 'Search'}
        meaningful_names = [name for name in name_matches if name not in common_caps and len(name) > 2]
        
        if meaningful_names:
            params['searchText'] = meaningful_names[0]
            return params
        
        # Extract meaningful words as fallback
        words = message.lower().split()
        stop_words = {'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for', 'of', 'with', 'by', 
                     'show', 'me', 'get', 'find', 'search', 'list', 'view', 'all', 'some'}
        
        meaningful_words = [word for word in words if word not in stop_words and len(word) > 2]
        
        if meaningful_words:
            params['searchText'] = ' '.join(meaningful_words[:2])  # First 2 meaningful words
        
        return params
    
    async def _execute_tool_direct(self, entity: str, intent: str, params: Dict[str, Any]) -> Dict[str, Any]:
        """
        Execute tool directly without LoopAgent overhead.
        
        This is the key performance optimization - direct tool execution.
        """
        try:
            # Import the optimized tool function
            from ai_assistant.sub_agents.task_executor_agent.utils import invoke_api_for_data
            
            params_json = json.dumps(params)
            
            logger.info(f"[{self.name}] Direct tool execution: {entity}.{intent} with {params_json}")
            
            # Execute tool directly
            result = invoke_api_for_data(entity, intent, params_json, None)
            
            # Parse result if needed
            if isinstance(result, str):
                try:
                    result = json.loads(result)
                except json.JSONDecodeError:
                    result = {"message": result, "type": "text"}
            
            logger.info(f"[{self.name}] Tool execution successful")
            return result
            
        except Exception as e:
            logger.error(f"[{self.name}] Tool execution failed: {str(e)}")
            return {
                "error": f"Failed to execute {entity} {intent}: {str(e)}",
                "type": "error"
            }
    
    def _create_simple_response(self, message: str, user_context: Dict[str, Any]) -> Dict[str, Any]:
        """Generate direct response for simple requests."""
        message_lower = message.lower().strip()
        user_name = user_context["user_name"]
        current_screen = user_context["current_screen"]
        
        # Handle greetings
        if any(greeting in message_lower for greeting in ["hi", "hello", "hey", "good morning", "good afternoon"]):
            return {
                "result": [{
                    "type": "markdown",
                    "message": f"Hi {user_name}! 👋 Welcome to {self.application_name}! I can see you're on the {current_screen} screen. I'm here to help you with your business data. What would you like to do today?"
                }],
                "followUps": ["Search partners", "View interactions", "Create contact"]
            }
        
        # Handle gratitude
        elif any(thanks in message_lower for thanks in ["thank", "thanks", "appreciate"]):
            return {
                "result": [{
                    "type": "markdown",
                    "message": f"You're very welcome, {user_name}! Happy to help anytime. If you need anything else while on {current_screen}, just ask!"
                }],
                "followUps": []
            }
        
        # Handle help requests
        elif any(help_word in message_lower for help_word in ["help", "what can you do", "how can you help"]):
            return {
                "result": [{
                    "type": "markdown",
                    "message": f"I can help you with {self.application_name} data! I can search partners, contacts, interactions, engagements, and documents. Just tell me what you're looking for!"
                }],
                "followUps": ["Search partners", "Find contacts", "Show interactions"]
            }
        
        # Default response
        else:
            return {
                "result": [{
                    "type": "markdown",
                    "message": f"Hi {user_name}! I can help you with data operations in {self.application_name}. Try asking me to search for partners, contacts, or other business data."
                }],
                "followUps": ["Search partners", "Find contacts", "Show help"]
            }
    
    @override
    async def _run_async_impl(self, ctx: InvocationContext) -> AsyncGenerator[Event, None]:
        """
        Main agent logic with smart routing and direct tool execution.
        
        Flow:
        1. Extract user message and context
        2. Smart routing decision: Simple vs Complex
        3. Simple → Direct response
        4. Complex → Tool detection → Direct tool execution
        5. Return formatted response
        """
        logger.info(f"[{self.name}] Processing user request")
        
        # Extract user message
        user_message = "Hello"
        if ctx.new_message and ctx.new_message.parts:
            user_message = ctx.new_message.parts[0].text
        
        # Extract user context
        user_context = self._extract_user_context(ctx)
        
        logger.info(f"[{self.name}] User: {user_context['user_name']} | Screen: {user_context['current_screen']} | Message: '{user_message}'")
        
        # Smart routing decision
        if self._is_simple_request(user_message):
            # Simple request → Direct response
            logger.info(f"[{self.name}] Route: SIMPLE → Direct response")
            response = self._create_simple_response(user_message, user_context)
        
        else:
            # Complex request → Tool detection + execution
            logger.info(f"[{self.name}] Route: COMPLEX → Tool detection + execution")
            
            # Detect entity and intent
            detection = self._detect_entity_and_intent(user_message)
            
            if detection['needs_tools']:
                # Extract parameters and execute tool
                params = self._extract_parameters(user_message, detection['entity'])
                tool_result = await self._execute_tool_direct(
                    detection['entity'], 
                    detection['intent'], 
                    params
                )
                
                # Format tool result as response
                if "error" in tool_result:
                    response = {
                        "result": [{
                            "type": "markdown",
                            "message": f"Sorry {user_context['user_name']}, I encountered an issue: {tool_result['error']}"
                        }],
                        "followUps": ["Try again", "Search partners", "Get help"]
                    }
                else:
                    # Tool executed successfully
                    response = tool_result  # Tool already returns proper format
            
            else:
                # No tools needed, fallback to simple response
                response = self._create_simple_response(user_message, user_context)
        
        # Store response in session state
        if ctx.session:
            ctx.session.state["last_response"] = response
        
        # Create and yield the response event
        response_content = types.Content(
            role='model',
            parts=[types.Part(text=json.dumps(response, indent=2))]
        )
        
        yield Event(content=response_content, author=self.name)
        
        logger.info(f"[{self.name}] Request processed successfully")


# Create the agent instance
smart_user_agent = SmartUserAgent(name="SmartUserAgent")
