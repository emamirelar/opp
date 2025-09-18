"""
Main AI Assistant Agent

This is the entry point for the AI assistant agent hierarchy.
"""

# from google.adk.agents import SequentialAgent

# # Import sub-agents
# from .sub_agents.user_request_agent import user_request_agent

# # --- Root Agent Definition ---
# # This is the entry point for the entire agent hierarchy
# root_agent = user_request_agent


from google.adk.agents import LlmAgent
from google.adk.tools.agent_tool import AgentTool
from google.adk.planners import BuiltInPlanner
from google.genai import types
from google.adk.tools import google_search

from .tools.invoke_app_api_tool import invoke_api_tool
from .tools.search_corp_vector_store_tool import search_corp_vector_store

google_search_agent = LlmAgent(
    model="gemini-2.0-flash",
    name="google_search_agent",
    description="Agent to call the google_search tool and returns the results as-is.",
    instruction="""Your sole job is to call the google_search tool and returns the results as-is.""",
    tools=[google_search]
)

instruction = """
You are an experienced Partnerships Specialist for the United Nations Office for Project Services.
Your goal is to help the user with their request.
You will use the tools provided to you to help the user.
Respond in well-formed markdown.

Tools:
**invoke_api_tool** - Automatically finds and calls API endpoints
PARAMETERS: entity_name, intent, params, isMultiToolRequest

**search_corp_vector_store** - Searches corporate vector store/knowledge base.  Use this tool when the user asks for information about ANYTHING related to the organization, partners, contacts, interactions, opportunities, etc.
Use relevant entityTypeIds to get the most relevant information.  The entityTypeIds are: "ENGAGEMENT", "PROJECT", "POLICY", "LEGAL_AGREEMENT".
If you are not sure about the entityTypeIds, leave it blank.
PARAMETERS: query, applicationId, entityTypeId, entityId, maxResults, isMultiToolRequest

**google_search** - Searches the web for information.
PARAMETERS: query
Make sure to return the results in well-formed markdown along with links to the sources.

## Examples
**"Find private sector partners"** → `invoke_api_tool("Partner", "Search", {"PartnerGroupCode": "PRIVATE_SECTOR"}, true)`

**"Find our key global partners"** → `invoke_api_tool("Partner", "Search", {"KeyGlobalPartner": true}, true)`

**"What are the latest interactions with partners"** → `invoke_api_tool("Interaction", "Search", {}, false)`

Respond in WELL-FORMED MARKDOWN making proper use of diffferent heading levels, bold text, and lists.

"""

root_agent = LlmAgent(
    name="root_agent",
    description="Root agent for the AI assistant",
    instruction=instruction,
    model="gemini-2.5-flash",
    generate_content_config=types.GenerateContentConfig(
        temperature=0.2, # More deterministic output
        # max_output_tokens=250,
        safety_settings=[
            types.SafetySetting(
                category=types.HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
                threshold=types.HarmBlockThreshold.BLOCK_LOW_AND_ABOVE
            )
        ]
    ),
    planner=BuiltInPlanner(
        thinking_config=types.ThinkingConfig(
            include_thoughts=True,
            thinking_budget=1024,
        )
    ),
    tools=[invoke_api_tool, search_corp_vector_store, AgentTool(google_search_agent)]
)