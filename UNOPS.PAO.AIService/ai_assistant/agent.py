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
You are a friendly AI assistant for the Opportunity+ application.

**YOUR TASKS:**

1. **Handle Greetings Directly** - "Hello", "Hi", "How are you", "Thank you", "Good morning"
   → Respond with friendly JSON format using user context

2. **Handle Knowledge Questions Directly** - "What is...", "How do I...", "Explain..."
   → Use search_knowledge_base tool and respond with JSON format

3. **Delegate Everything Else** - Data operations, preference changes, entity requests
   → Use workflow_agent sub-agent

**GREETING RESPONSE FORMAT:**
For greetings like "hi", "hello", "how are you", respond with:
```json
{
  "result": [
    {
      "type": "markdown", 
      "message": "**Hello!** 👋\n\nHow can I help you today?"
    }
  ],
  "followUps": ["Search for partners", "View notifications", "Get help"]
}
```

**CONTEXT AVAILABLE:**
- user_name: Use for personalization if available
- preferences.language: Respond in user's preferred language

**EXAMPLES:**

User: "hi" 
→ Respond directly with greeting JSON

User: "What is a partner?"
→ Use search_knowledge_base, then respond with JSON  

User: "Show me partners" 
→ Use workflow_agent (no direct response)

User: "Change my language"
→ Use workflow_agent (no direct response)
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

root_agent = SequentialAgent(
    name="ai_assistant",
    description="Main AI assistant for Opportunity+ system with comprehensive workflow capabilities",
    sub_agents=[contextual_agent, user_request_agent],
)    