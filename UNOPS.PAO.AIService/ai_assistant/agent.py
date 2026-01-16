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
from .tools.lookup_entity_metadata_tool import get_json_for_entity
from .utils.metadata_utils import load_entities_metadata

logger = logging.getLogger(__name__)



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
    
    # Handle workflows section (multi-step API patterns)
    if 'workflows' in metadata:
        workflows = metadata['workflows']
        markdown_content.append("## Multi-Step Workflows")
        if 'description' in workflows:
            markdown_content.append(f"{workflows['description']}")
        if 'dataPassthrough' in workflows:
            markdown_content.append(f"**DATA PASSTHROUGH:** {workflows['dataPassthrough']}")
        markdown_content.append("")
        
        if 'patterns' in workflows:
            for pattern in workflows['patterns']:
                markdown_content.append(f"### {pattern.get('name', 'Workflow')}")
                
                # Handle step1 and step2 structure
                if 'step1' in pattern:
                    step1 = pattern['step1']
                    markdown_content.append(f"- **Step 1**: `{step1.get('endpoint', '')}` - {step1.get('purpose', '')}")
                    if 'returns' in step1:
                        markdown_content.append(f"  - **Returns:** {step1['returns']}")
                
                if 'step2' in pattern:
                    step2 = pattern['step2']
                    markdown_content.append(f"- **Step 2**: `{step2.get('endpoint', '')}` - {step2.get('purpose', '')}")
                    if 'receives' in step2:
                        markdown_content.append(f"  - **Receives:** {step2['receives']}")
                
                if 'userConfirmation' in pattern:
                    markdown_content.append(f"- **User Confirmation:** {pattern['userConfirmation']}")
                
                # Also handle legacy 'steps' array if present
                if 'steps' in pattern:
                    for step in pattern['steps']:
                        step_num = step.get('step', '')
                        endpoint = step.get('endpoint', '')
                        purpose = step.get('purpose', '')
                        markdown_content.append(f"- **Step {step_num}**: `{endpoint}` - {purpose}")
                
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
                    
                    # Add workflow indicators if present
                    if 'workflowStep' in endpoint:
                        markdown_content.append(f"  **Workflow Step:** {endpoint['workflowStep']}")
                    if 'prerequisite' in endpoint:
                        markdown_content.append(f"  **Prerequisite:** {endpoint['prerequisite']}")
                    
                    # Add whenToUse, responseNote, importantNote, requestSource if present
                    if 'whenToUse' in endpoint:
                        markdown_content.append(f"  **When To Use:** {endpoint['whenToUse']}")
                    if 'requestSource' in endpoint:
                        markdown_content.append(f"  **Request Source:** {endpoint['requestSource']}")
                    if 'responseNote' in endpoint:
                        markdown_content.append(f"  **Response Note:** {endpoint['responseNote']}")
                    if 'importantNote' in endpoint:
                        markdown_content.append(f"  **IMPORTANT:** {endpoint['importantNote']}")
                    
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
                    
                    # Handle requestBody section (for endpoints without 'parameters')
                    if 'requestBody' in endpoint:
                        rb = endpoint['requestBody']
                        markdown_content.append("  **Request Body:**")
                        if 'description' in rb:
                            markdown_content.append(f"    {rb['description']}")
                        if 'fields' in rb:
                            for field in rb['fields']:
                                field_line = f"    - {field.get('name', '')} ({field.get('dataType', 'string')})"
                                if field.get('required', False):
                                    field_line += " *required*"
                                if 'description' in field:
                                    field_line += f" - {field['description']}"
                                markdown_content.append(field_line)
                        if 'importantNotes' in rb:
                            markdown_content.append("  **IMPORTANT NOTES:**")
                            for note in rb['importantNotes']:
                                markdown_content.append(f"    - {note}")
                        if 'exampleRequest' in rb:
                            markdown_content.append("  **EXAMPLE REQUEST (use this format!):**")
                            import json
                            example_json = json.dumps(rb['exampleRequest'], indent=4)
                            for line in example_json.split('\n'):
                                markdown_content.append(f"    {line}")
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


def format_uploaded_files_for_instruction(state: dict) -> str:
    """Format uploaded files metadata into instruction context with GCS paths.
    
    This allows the agent to know about uploaded files and their GCS storage paths,
    which is essential for passing file references to API endpoints like /generate-proposal.
    These files persist throughout the conversation session.
    
    Args:
        state: State dictionary containing uploaded_files_metadata
        
    Returns:
        Formatted string with file information including GCS paths
    """
    if not state:
        return ""
    
    uploaded_files = state.get('uploaded_files_metadata', [])
    if not uploaded_files:
        return ""
    
    context_parts = []
    context_parts.append(f"**{len(uploaded_files)} file(s) available in this conversation session:**")
    context_parts.append("(These files persist across all messages in this conversation - you can reference them anytime)")
    context_parts.append("")
    
    gcs_paths = []
    mime_types = []
    
    for i, file_info in enumerate(uploaded_files, 1):
        filename = file_info.get('filename', 'Unknown')
        mime_type = file_info.get('mime_type', 'application/octet-stream')
        gcs_path = file_info.get('gcs_path', '')
        
        file_line = f"{i}. **{filename}** (Type: {mime_type})"
        if gcs_path:
            file_line += f"\n   - GCS Path: `{gcs_path}`"
            gcs_paths.append(gcs_path)
            mime_types.append(mime_type)
        
        context_parts.append(file_line)
    
    # Add summary of GCS paths for easy API usage
    if gcs_paths:
        context_parts.append("")
        context_parts.append("**Ready-to-use for API calls:**")
        context_parts.append(f"- newDocumentStoragePaths: {gcs_paths}")
        context_parts.append(f"- newDocumentMimeTypes: {mime_types}")
    
    if context_parts:
        return "\n\n---\n**UPLOADED FILES CONTEXT (Session-Persistent):**\n" + "\n".join(context_parts) + "\n---\n"
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

## UNOPS CRM System Overview

You have access to a comprehensive UNOPS CRM system with entities including:
- **Partners** - Partner organizations
- **Contacts** - Individual contacts within partner organizations
- **Interactions** - Interactions/meetings with partners and contacts
- **Opportunities** - Business opportunities and proposals
- **Documents** - Document management
- **And more** - Additional entities for managing partnerships and opportunities

**To get detailed information about any entity or endpoint, use the `get_json_for_entity` tool.**
This tool provides:
- Entity data models and field definitions
- Available API endpoints and their parameters
- Request/response structures
- Workflow patterns and multi-step operations

**When to use `get_json_for_entity`:**
- Before calling an API endpoint, lookup the entity to understand required parameters
- When user asks about available operations for an entity
- When you need to understand the data structure of an entity
- When you need to find a specific endpoint's parameters

**Examples:**
- `get_json_for_entity(entity_name="Opportunity")` - Get all Opportunity entity details
- `get_json_for_entity(entity_name="Partner")` - Get all Partner entity details
- `get_json_for_entity(endpoint_path="/api/opportunity/create")` - Find specific endpoint details

## Multi-Step Workflows

Some operations follow a multi-step pattern:
- **Opportunity Creation from Documents**: 
  1. Step 1: `/api/opportunity/generate-proposal` - Analyzes documents and returns structured proposal data
  2. Step 2: `/api/opportunity/create-from-proposal` - Creates the opportunity using Step 1's response data
  - **CRITICAL**: When user confirms creation, pass the COMPLETE Step 1 response to Step 2. Do NOT construct a new empty request.

For workflow details, use `get_json_for_entity()` to get the full workflow patterns.

## Tools Available

**get_json_for_entity** - Lookup entity metadata and API endpoint details on-demand.
- Use this tool when user asks anything about a particular entity or its data.
- Use this tool BEFORE calling APIs to understand required parameters and request structures
- PARAMETERS: entity_name (e.g., "Opportunity", "Partner"), endpoint_path (e.g., "/api/opportunity/create"), or leave empty for summary

**invoke_app_api** - Use this tool to interact with any of the entities in the CRM application (Partners, Contacts, Interactions, Opportunities, etc.).

**How to Use invoke_app_api:**
1. **First, lookup the entity** using `get_json_for_entity(entity_name="EntityName")` to understand available endpoints
2. **Review the endpoint details** - Check description, whenToUse, parameters, and request body structure
3. **Check for prerequisites** - Some endpoints require calling other endpoints first (e.g., `/search-fields` before advanced search)
4. **Build your request** - Use the endpoint path, method, and parameters EXACTLY as stated in the metadata
5. **Call the API** - Use invoke_app_api with the correct url, method, and params

**Important Guidelines:**
- ALWAYS use `get_json_for_entity` first to understand endpoint requirements
- Use the endpoint paths, methods, parameters, and request model structures EXACTLY AS STATED in the metadata
- DO NOT make up, augment, or modify endpoints, parameters, or request models in any way
- For workflows, pass the COMPLETE Step 1 response to Step 2 without modification

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

**Working with Uploaded Files (Session-Persistent):**
Files uploaded in this conversation are **remembered throughout the entire session**. When the user uploads files (documents, PDFs, etc.):

1. The **UPLOADED FILES CONTEXT** section shows ALL files uploaded during this conversation session (not just the current message)
2. You can reference files from earlier messages in the same conversation - they persist across turns
3. If the user says "analyze the document" or "use those files" without uploading new ones, check the UPLOADED FILES CONTEXT for previously uploaded files
4. When calling APIs that need document references (like creating opportunities), use the GCS paths from the context
5. Always use the exact GCS paths provided (starting with `gs://`) - do not ask the user for paths if they're already in the context
6. If files were uploaded earlier in the conversation but the user's current message doesn't include them, they're still available in UPLOADED FILES CONTEXT

**Multi-Step Workflows:**
Some operations follow a multi-step pattern defined in the "workflows" section of the metadata. When you see:
- **workflowStep**: Indicates this endpoint is part of a workflow sequence
- **prerequisite**: Another endpoint that must be called first
- **requestSource**: Where to get the data for this request

For these workflows:
1. Call Step 1 endpoint first (generates/extracts data)
2. Present results to user and ask for confirmation
3. When user confirms, call Step 2 with the COMPLETE response data from Step 1

The system automatically handles data passthrough, but always aim to pass the full response from Step 1 to Step 2.

Respond in WELL-FORMED MARKDOWN making proper use of different heading levels, bold text, and lists.

"""

# Create the final instruction (no longer includes full metadata - agent uses get_json_for_entity tool instead)
instruction = instruction_template


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
        tools=[get_json_for_entity, invoke_app_api, search_corp_vector_store, AgentTool(google_search_agent)]
    )


def create_agent_with_context(state: dict = None) -> LlmAgent:
    """
    Create an agent instance with optional user, page, geo, and file context injected into the instruction.
    This allows dynamic context without polluting the conversation history.
    
    Args:
        state: Optional state dictionary containing user_profile, page_context_auto, user_geo_stats, 
               and uploaded_files_metadata
        
    Returns:
        LlmAgent instance with context-aware instruction
    """
    # Build the instruction with optional user, page, geo, and file context
    user_context_instruction = ""
    page_context_instruction = ""
    geo_context_instruction = ""
    files_context_instruction = ""
    
    if state:
        user_context_instruction = format_user_context_for_instruction(state)
        geo_context_instruction = format_geo_context_for_instruction(state)
        files_context_instruction = format_uploaded_files_for_instruction(state)
        page_context = state.get('page_context_auto')
        if page_context:
            page_context_instruction = format_page_context_for_instruction(page_context)
    
    # Combine base instruction with context (user context, geo context, files context, then page context)
    full_instruction = instruction
    if user_context_instruction or geo_context_instruction or page_context_instruction or files_context_instruction:
        # Insert context right after the Page Context Awareness section
        combined_context = user_context_instruction + geo_context_instruction + files_context_instruction + page_context_instruction
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
        tools=[get_json_for_entity, invoke_app_api, search_corp_vector_store, AgentTool(google_search_agent)]
    )