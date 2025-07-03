from datetime import datetime
from google.adk.agents import Agent, SequentialAgent
from google.adk.tools import FunctionTool

from ai_assistant.workflow_agent.response_formatter.agent import create_response_agent
from .knowledge_manager import KnowledgeManager
from .workflow_agent.agent import workflow_agent
from .contextual_agents import contextual_agent

knowledge_mngr = KnowledgeManager()

# Remove the old callback function as it's now in screen_context_agent.py
# Remove the old format_response_if_needed function as it's not needed anymore

ROOT_PROMPT = """
You are an AI assistant that responds to the user's request:

1. **Greetings** - "Hello", "Hi", "How are you", "Thank you", "Good morning"
2. **Knowledge Questions** - "What is...", "How do I...", "Explain..."

You are presented with a tool and sub agent
    - search_knowledge_base
    - workflow_agent

Use your intelligence to determine which tool to use and which sub agent to delegate to.

Without navigating to the tool or sub agent, your ability to answer will be only greetings or based on previous data extraction.
Hence, remember to call a tool or sub agent to answer the user's request.

**CRITICAL: If the user request is ANYTHING else, WITHOUT ANY DOUBT, redirect to the subagent workflow_agent.**

**Only respond with JSON for greetings and knowledge questions:**
```json
{
  "result": [
    {
      "type": "markdown", 
      "message": "Your personalized response with PROPER MARKDOWN formatting"
    }
  ],
  "followUps": ["Action 1", "Action 2", "Action 3"]
}
```

**MARKDOWN FORMATTING RULES:**
- Use **bold** for important information
- Use line breaks for readability
- Use lists when presenting multiple items
- Use headers (##) for sections
- Avoid wall of text - structure your content

**Context available:**
- user_name: For personalization
- preferences.language: User's preferred language

**Good greeting examples:**
- "**Hello [user_name]!** 👋\n\nHow can I help you today?"
- "**Good morning [user_name]!** 👋\n\nWhat would you like to accomplish?"

**Good followUps in user's language:**
- English: ["Search for partners", "View notifications", "Get help"]
- Spanish: ["Buscar socios", "Ver notificaciones", "Obtener ayuda"]
- French: ["Rechercher partenaires", "Voir notifications", "Obtenir aide"]
"""

def search_knowledge_base(query: str, max_results: int = 3) -> str:
    """
    Search the knowledge base for relevant information.

    Args:
        query: The search query from the user
        max_results: Maximum number of results to return
        
    Returns:
        Formatted string with relevant knowledge
    """
    
    print(f"🔍 Searching knowledge base for: '{query}'")
    
    try:
        # Search for relevant chunks
        results = knowledge_mngr.search_knowledge(query, max_results)
        
        if not results:
            return "No relevant information found in the knowledge base. I can help you with general questions or direct you to appropriate resources."
        
        # Format results for the agent
        formatted_results = "📚 **Knowledge Base Results:**\n\n"
        
        for i, result in enumerate(results, 1):
            formatted_results += f"**Result {i}:**\n"
            formatted_results += f"{result['text']}\n"
            formatted_results += f"*Source: {result['source']}*\n\n"
        
        print(f"✅ Found {len(results)} relevant knowledge chunks")
        return formatted_results
        
    except Exception as e:
        print(f"❌ Error searching knowledge base: {e}")
        return "I encountered an error while searching the knowledge base. Please try rephrasing your question."
    
# get_user_profile function moved to contextual_agents/user_detail_agent.py

user_request_agent = Agent(
    name="user_request_agent",
    model="gemini-2.0-flash-001",
    description="Main AI assistant for Opportunity+ system with comprehensive workflow capabilities",
    instruction=ROOT_PROMPT,
    tools=[
        FunctionTool(
            func=search_knowledge_base
        )
    ],
    sub_agents=[workflow_agent]
)  

# gather_screen_context function moved to contextual_agents/screen_context_agent.py
# screen_context_agent moved to contextual_agents/screen_context_agent.py
# user_detail_agent moved to contextual_agents/user_detail_agent.py
# contextual_agent moved to contextual_agents/contextual_agent.py

root_agent = SequentialAgent(
    name="ai_assistant",
    description="Main AI assistant for Opportunity+ system with comprehensive workflow capabilities",
    sub_agents=[contextual_agent, user_request_agent],
)    