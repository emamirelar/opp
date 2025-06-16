import asyncio
from google.adk.runners import Runner
from google.adk.sessions import BaseSessionService
from google.genai import types


def add_user_query_to_history(
    session_service: BaseSessionService, 
    app_name: str, 
    user_id: str, 
    session_id: str, 
    user_query: str
):
    """Add user query to the session interaction history."""
    try:
        # The Google ADK Runner automatically handles conversation history
        # So we just log that we received the query
        print(f"📝 User query logged: {user_query[:50]}{'...' if len(user_query) > 50 else ''}")
        
    except Exception as e:
        print(f"Error adding user query to history: {e}")


async def call_agent_async(runner: Runner, user_id: str, session_id: str, user_input: str):
    """Call the agent asynchronously and handle the response."""
    try:
        # Create content using the correct format
        content = types.Content(role="user", parts=[types.Part(text=user_input)])
        
        print(f"\n🤖 Processing: {user_input}")
        
        # Run the agent with the user input
        async for event in runner.run_async(
            user_id=user_id,
            session_id=session_id,
            new_message=content
        ):
            # Check if this is the final response
            if event.is_final_response():
                if (
                    event.content 
                    and event.content.parts 
                    and hasattr(event.content.parts[0], "text") 
                    and event.content.parts[0].text
                ):
                    final_response = event.content.parts[0].text.strip()
                    print(f"Agent: {final_response}")
                else:
                    print("Agent: [No text content in response]")
            else:
                # Handle intermediate events if needed
                if event.content and event.content.parts:
                    for part in event.content.parts:
                        if hasattr(part, "text") and part.text and not part.text.isspace():
                            # This might be streaming text, but for simplicity we'll skip intermediate parts
                            pass
            
    except Exception as e:
        print(f"Agent: I encountered an error: {e}") 