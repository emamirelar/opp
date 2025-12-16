"""
Main AI Assistant Agent

This is the entry point for the AI assistant agent hierarchy.
"""

import json
import os
import time
import base64
import requests
import traceback
import logging

from pathlib import Path
from google.adk.agents import LlmAgent
from google.adk.tools.agent_tool import AgentTool
from google.adk.planners import BuiltInPlanner
from google.genai import types
from google.adk.tools import google_search
from typing import Optional
from google.adk.tools.tool_context import ToolContext

from .tools.search_corp_vector_store_tool import search_corp_vector_store
from .tools.invoke_app_api_tool import invoke_app_api

logger = logging.getLogger(__name__)


class MetadataError(Exception):
    """Raised when metadata loading fails"""
    pass


def _find_entities_metadata_file() -> str:
    """
    Find the entities-metadata.json file by checking multiple possible locations.
    
    Returns:
        str: Path to the entities-metadata.json file
        
    Raises:
        MetadataError: If entities-metadata.json file is not found
    """
    current_dir = Path(__file__).parent.absolute()
    
    # Possible locations to check for entities-metadata.json
    possible_paths = [
        # Jenkins deployment: AIService copied into UNOPS.PAO.AIService directory
        current_dir.parent.parent / "AIService" / "metadata" / "entities-metadata.json",  # /app/AIService/metadata/entities-metadata.json
        # Same level as current directory (for deployment scenarios)
        current_dir / "AIService" / "metadata" / "entities-metadata.json",
        # One level up from current directory (development scenario)
        current_dir.parent / "AIService" / "metadata" / "entities-metadata.json",
        # Two levels up (from utils -> ai_assistant -> UNOPS.PAO.AIService -> root)
        current_dir.parent.parent.parent / "AIService" / "metadata" / "entities-metadata.json",
        # Three levels up (alternative structure)
        current_dir.parent.parent.parent.parent / "AIService" / "metadata" / "entities-metadata.json"
    ]
    
    for path in possible_paths:
        if path.exists() and path.is_file():
            logger.debug(f"Found entities-metadata.json at: {path}")
            return str(path)
    
    # If not found, raise an error with helpful information
    searched_paths = [str(p) for p in possible_paths]
    raise MetadataError(
        f"entities-metadata.json file not found. Searched locations:\n" + 
        "\n".join(f"  - {p}" for p in searched_paths)
    )


def load_entities_metadata():
    """Load the entities metadata JSON file"""
    try:
        metadata_path = _find_entities_metadata_file()
        
        with open(metadata_path, 'r', encoding='utf-8') as f:
            metadata = json.load(f)
        
        logger.info(f"Entities metadata loaded successfully from: {metadata_path}")
        return metadata
        
    except MetadataError as e:
        logger.error(f"Metadata file not found: {e}")
        print(f"Warning: {e}")
        return {}
    except json.JSONDecodeError as e:
        logger.error(f"Error parsing entities-metadata.json: {e}")
        print(f"Warning: Error parsing entities-metadata.json: {e}")
        return {}
    except Exception as e:
        logger.error(f"Unexpected error loading entities metadata: {e}")
        print(f"Warning: Unexpected error loading entities metadata: {e}")
        return {}


def format_entities_metadata_as_markdown(metadata):
    """Convert entities metadata JSON to markdown format to avoid ADK template conflicts"""
    if not metadata:
        return "No metadata available"
    
    markdown_content = []
    
    # Handle metadata section
    if 'metadata' in metadata:
        meta_info = metadata['metadata']
        markdown_content.append("## Metadata")
        markdown_content.append(f"**Version:** {meta_info.get('version', 'N/A')}")
        markdown_content.append(f"**Generated Date:** {meta_info.get('generatedDate', 'N/A')}")
        markdown_content.append(f"**Description:** {meta_info.get('description', 'N/A')}")
        markdown_content.append("")
    
    # Handle request models section
    if 'requestModels' in metadata:
        markdown_content.append("## Request Models")
        markdown_content.append("These are reusable request model definitions used across multiple entities:")
        markdown_content.append("")
        
        for model_name, model_info in metadata['requestModels'].items():
            markdown_content.append(f"### {model_name}")
            
            if 'description' in model_info:
                markdown_content.append(f"**Description:** {model_info['description']}")
            
            if 'inheritsFrom' in model_info:
                markdown_content.append(f"**Inherits From:** {model_info['inheritsFrom']}")
            
            if 'fields' in model_info:
                markdown_content.append("**Fields:**")
                for field in model_info['fields']:
                    field_line = f"- {field.get('name', '')} ({field.get('dataType', 'string')})"
                    if field.get('required', False):
                        field_line += " *required*"
                    if 'description' in field:
                        field_line += f" - {field['description']}"
                    markdown_content.append(field_line)
            
            markdown_content.append("")
    
    # Handle entities section
    if 'entities' in metadata:
        markdown_content.append("## Entities")
        markdown_content.append("Available entities in the UNOPS CRM system:")
        markdown_content.append("")
        
        for entity_name, entity_info in metadata['entities'].items():
            markdown_content.append(f"### {entity_name}")
            
            if 'description' in entity_info:
                markdown_content.append(f"**Description:** {entity_info['description']}")
            
            # Handle data model
            if 'dataModel' in entity_info and 'fields' in entity_info['dataModel']:
                markdown_content.append("**Data Model:**")
                for field in entity_info['dataModel']['fields']:
                    field_line = f"- {field.get('name', '')} ({field.get('dataType', 'string')})"
                    if field.get('required', False):
                        field_line += " *required*"
                    if 'description' in field:
                        field_line += f" - {field['description']}"
                    markdown_content.append(field_line)
                markdown_content.append("")
            
            # Handle API endpoints
            if 'apiEndpoints' in entity_info and entity_info['apiEndpoints']:
                markdown_content.append("**API Endpoints:**")
                for endpoint in entity_info['apiEndpoints']:
                    endpoint_line = f"- **{endpoint.get('endpoint', '')}**"
                    if 'description' in endpoint:
                        endpoint_line += f" - {endpoint['description']}"
                    markdown_content.append(endpoint_line)
                    markdown_content.append(f"  **Method:** {endpoint.get('method', 'GET')}")
                    
                    if 'parameters' in endpoint and endpoint['parameters']:
                        markdown_content.append("  **Parameters:**")
                        for param in endpoint['parameters']:
                            param_line = f"    - {param.get('name', '')} ({param.get('dataType', 'string')})"
                            if param.get('required', False):
                                param_line += " *required*"
                            if 'description' in param:
                                param_line += f" - {param['description']}"
                            if 'structure' in param:
                                param_line += f" (Structure: {param['structure']})"
                            markdown_content.append(param_line)
                markdown_content.append("")
            elif 'apiEndpoints' in entity_info and not entity_info['apiEndpoints']:
                markdown_content.append("**API Endpoints:** None (read-only or derived entity)")
                markdown_content.append("")
            
            markdown_content.append("")  # Add blank line between entities
    
    result = "\n".join(markdown_content)
    # Replace curly braces with square brackets to avoid template conflicts
    result = result.replace("{", "[").replace("}", "]")
    return result


def format_user_context_for_instruction(state: dict) -> str:
    """Format user information from state into a concise instruction-friendly string"""
    if not state:
        return ""
    
    user_profile = state.get('user_profile', {})
    user_info = user_profile.get('userInfoWithOrgSettings', {})
    
    if not user_info:
        return ""
    
    context_parts = []
    
    # Add user name and basic info
    if user_info.get('Name'):
        context_parts.append(f"**User Name:** {user_info['Name']}")
    
    if user_info.get('Position'):
        context_parts.append(f"**Position:** {user_info['Position']}")
    
    if user_info.get('UserEmail'):
        context_parts.append(f"**Email:** {user_info['UserEmail']}")
    
    # Add organizational information
    if user_info.get('OrgUnitDescription'):
        context_parts.append(f"**Organization Unit:** {user_info['OrgUnitDescription']}")
    elif user_info.get('OrgUnit'):
        context_parts.append(f"**Organization Unit:** {user_info['OrgUnit']}")
    
    if user_info.get('DutyStation'):
        context_parts.append(f"**Duty Station:** {user_info['DutyStation']}")
    
    # Add supervisor information
    if user_info.get('SupervisorName'):
        supervisor_info = user_info['SupervisorName']
        if user_info.get('SupervisorEmail'):
            supervisor_info += f" ({user_info['SupervisorEmail']})"
        context_parts.append(f"**Supervisor:** {supervisor_info}")
    
    if context_parts:
        return "\n\n---\n**USER CONTEXT:**\n" + "\n".join(context_parts) + "\n---\n"
    return ""


def format_geo_context_for_instruction(state: dict) -> str:
    """Format geo information from state into a concise instruction-friendly string"""
    if not state:
        return ""
    
    geo_stats = state.get('user_geo_stats', {})
    if not geo_stats:
        return ""
    
    context_parts = []
    
    # Extract location information
    location = geo_stats.get('location', {})
    if location and location.get('status') == 'success':
        # Add city and country
        location_parts = []
        if location.get('city'):
            location_parts.append(location['city'])
        if location.get('region'):
            location_parts.append(location['region'])
        if location.get('country'):
            location_parts.append(location['country'])
        
        if location_parts:
            context_parts.append(f"**Location:** {', '.join(location_parts)}")
        
        # Add timezone
        if location.get('timezone'):
            context_parts.append(f"**Timezone:** {location['timezone']}")
        
        # Add coordinates if available
        if location.get('latitude') and location.get('longitude'):
            context_parts.append(f"**Coordinates:** {location['latitude']}, {location['longitude']}")
        
        # Add ISP information if available
        if location.get('isp'):
            context_parts.append(f"**ISP:** {location['isp']}")
    
    # Add current datetime
    if geo_stats.get('current_datetime'):
        context_parts.append(f"**Current DateTime (UTC):** {geo_stats['current_datetime']}")
    
    if context_parts:
        return "\n\n---\n**GEO CONTEXT:**\n" + "\n".join(context_parts) + "\n---\n"
    return ""


def format_page_context_for_instruction(page_context: dict) -> str:
    """Format page context data into a concise instruction-friendly string"""
    if not page_context:
        return ""
    
    component_data = page_context.get('component_data', {})
    context_parts = []
    
    # Add route information
    if 'route' in page_context:
        route = page_context['route']
        context_parts.append(f"**Current Page:** {route.get('path', 'Unknown')}")
    
    # Extract the main data object (recordData, partner, contact, interactions, etc.)
    if 'recordData' in component_data:
        record = component_data['recordData']
        # Extract key fields only to keep it concise
        if isinstance(record, dict):
            key_fields = {}
            for key in ['id', 'name', 'partnerCategoryName', 'status', 'partnerGroupName']:
                if key in record:
                    key_fields[key] = record[key]
            context_parts.append(f"\n**Currently Viewing Entity:** {key_fields}")
        else:
            context_parts.append(f"\n**Currently Viewing Entity:** {record}")
    
    # Add any other relevant data from component_data
    for key, value in component_data.items():
        if key in ['partner', 'contact', 'interaction'] and key != 'recordData':
            # Extract key fields only
            if isinstance(value, dict):
                key_fields = {k: v for k, v in value.items() if k in ['id', 'name', 'status']}
                context_parts.append(f"\n**{key.title()}:** {key_fields}")
            else:
                context_parts.append(f"\n**{key.title()}:** {value}")
    
    if context_parts:
        return "\n\n---\n**CURRENT PAGE CONTEXT:**\n" + "\n".join(context_parts) + "\n---\n"
    return ""


google_search_agent = LlmAgent(
    model="gemini-2.0-flash",
    name="google_search_agent",
    description="Agent to call the google_search tool and returns the results as-is.",
    instruction="""Your sole job is to call the google_search tool and returns the results as-is.""",
    tools=[google_search]
)


instruction_template = """
You are an experienced Partnerships Specialist for the United Nations Office for Project Services.
Your goal is to help the user with their request.
You will use the tools provided to you to help the user.
Respond in well-formed markdown.

## IMPORTANT: Page Context Awareness

The user's messages will include **CURRENT PAGE CONTEXT** information that tells you:
- What page the user is currently viewing
- What data is loaded on their screen (partner details, contact information, interaction records, etc.)
- The specific entity they are looking at (with full details)

**ALWAYS use this context to understand what the user is referring to.** For example:
- If they say "Tell me about this partner" and the context shows they're viewing "The World Bank" (ID: 443), you know they mean The World Bank
- If they ask "What contacts do we have?" and the context shows a partner record with associated contacts, use that data
- If they ask "Summarize this" and there's a record loaded, summarize that specific record

<**DO NOT ask the user to clarify which entity they mean if the context already provides it.**>

## UNOPS CRM System Entities and API Reference

You have access to a comprehensive UNOPS CRM system with the following entities and capabilities:

{entities_metadata}

Use this metadata to understand:
- Available entities and their data models
- Supported API endpoints and HTTP methods
- Required and optional parameters for each operation
- Request model structures for complex operations
- Relationships between entities

## Tools Available

**invoke_app_api** - Use this tool to interact with any of the entities in the CRM application (Partners, Contacts, Interactions, Opportunities, etc.).

**How to Use This Tool:**
1. **Review the metadata** for the entity you need to work with
2. **Check the endpoint descriptions** - Each endpoint includes:
   - `description`: What the endpoint does and when it's appropriate to use
   - `whenToUse`: Specific scenarios where this endpoint should be called (if present)
   - `whenNotToUse`: Scenarios where you should NOT use this endpoint (if present)
3. **Follow the guidance** - Use the `whenToUse` and `whenNotToUse` fields to determine the most appropriate endpoint for the user's request
4. **Examine the parameters** - Each endpoint lists required and optional parameters with descriptions
5. **Build your request** - Use the endpoint path, method, and parameters EXACTLY as stated in the metadata

**Important Guidelines:**
- ALWAYS check if there are prerequisite endpoints to call first (e.g., some endpoints instruct you to call `/search-fields` before constructing advanced search filters)
- For advanced search operations, carefully read the `description` and `whenToUse` fields to understand when to use simple search vs. advanced search vs. other specialized search methods
- Use the endpoint paths, methods, parameters, and request model structures EXACTLY AS STATED in the metadata
- DO NOT make up, augment, or modify endpoints, parameters, or request models in any way
- If an endpoint has a `filtersFormat` or special parameter structure, follow that structure precisely

PARAMETERS: url, method, params, headers

**search_corp_vector_store** - Searches corporate vector store/knowledge base.  Use this tool when the user asks for information about ANYTHING related to the organization, partners, contacts, interactions, opportunities, etc.
Use relevant entityTypeIds to get the most relevant information.  The entityTypeIds are: "BUSINESS_OPPORTUNITY", "FUNDING_SOURCE", "CONTINENT", "DUTY_STATION", "ORGANIZATION", "RFX", "PO", "GUIDANCE", "CONTRACT", "LTA", "POLICY", "SUPPLIER", "CLIENT", "COUNTRY", "BANK", "ORG_UNIT", "GEO_REGION", "PROCESS", "STANDARD", "INVOICE", "PAYMENT", "AGREEMENT", "HOST_COUNTRY_AGREEMENT", "PERSON", "PERSON_SKILL", "ENGAGEMENT", "PARTNER", "PROJECT", "SDG", "OUTPUT", "PERSON_ROLE", "LESSON_LEARNT", "PROPOSAL", "ORG_REPORT", "ORG_STRATEGY", "MOU", "EXTERNAL_PUBLICATION", "TEMPLATE", "RISK", "ISSUE".
If you are not sure about the entityTypeId, or if there are too many entityTypeIds, leave it blank which will return a wide spectrum of information.  You may wish to adjust the maxResults in that case.
PARAMETERS: query, applicationId, entityTypeId, entityId, maxResults

**google_search** - Searches the web for information.
PARAMETERS: query
Make sure to return the results in well-formed markdown along with links to the sources.

**Important Rules for your thought process:**
When you decide to use a tool, first explain your reasoning step-by-step. In your explanation, describe the *action* you are taking in plain language (e.g., 'I will look up the partner'). **Do not mention the specific internal tool name** (e.g., do not say 'I will use the `invoke_app_api` tool').
Don't talk about endpoints, parameters, or request models in your explanation.

Respond in WELL-FORMED MARKDOWN making proper use of different heading levels, bold text, and lists.

"""

# Create the final instruction by substituting the metadata
entities_metadata = load_entities_metadata()  # Load the metadata
instruction = instruction_template.format(
    entities_metadata=format_entities_metadata_as_markdown(entities_metadata)
)


root_agent = LlmAgent(
    name="root_agent",
    description="Root agent for the AI assistant",
    instruction=instruction,
    model="gemini-2.5-flash-lite",
    # model="gemini-2.5-flash-lite",
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
    tools=[invoke_app_api, search_corp_vector_store, AgentTool(google_search_agent)]
)


def create_agent_with_context(state: dict = None) -> LlmAgent:
    """
    Create an agent instance with optional user, page, and geo context injected into the instruction.
    This allows dynamic context without polluting the conversation history.
    
    Args:
        state: Optional state dictionary containing user_profile, page_context_auto, and user_geo_stats
        
    Returns:
        LlmAgent instance with context-aware instruction
    """
    # Build the instruction with optional user, page, and geo context
    user_context_instruction = ""
    page_context_instruction = ""
    geo_context_instruction = ""
    
    if state:
        user_context_instruction = format_user_context_for_instruction(state)
        geo_context_instruction = format_geo_context_for_instruction(state)
        page_context = state.get('page_context_auto')
        if page_context:
            page_context_instruction = format_page_context_for_instruction(page_context)
    
    # Combine base instruction with context (user context, geo context, then page context)
    full_instruction = instruction
    if user_context_instruction or geo_context_instruction or page_context_instruction:
        # Insert context right after the Page Context Awareness section
        combined_context = user_context_instruction + geo_context_instruction + page_context_instruction
        full_instruction = instruction.replace(
            "<**DO NOT ask the user to clarify which entity they mean if the context already provides it.**>",
            f"**DO NOT ask the user to clarify which entity they mean if the context already provides it.**\n\n{combined_context}"
        )
    
    return LlmAgent(
        name="root_agent",
        description="Root agent for the AI assistant",
        instruction=full_instruction,
        model="gemini-2.5-flash",
        generate_content_config=types.GenerateContentConfig(
            temperature=0.2,
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
        tools=[invoke_app_api, search_corp_vector_store, AgentTool(google_search_agent)]
    )