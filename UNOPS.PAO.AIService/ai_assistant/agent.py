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

from google.adk.agents import LlmAgent
from google.adk.tools.agent_tool import AgentTool
from google.adk.planners import BuiltInPlanner
from google.genai import types
from google.adk.tools import google_search
from typing import Optional
from google.adk.tools.tool_context import ToolContext

from .tools.search_corp_vector_store_tool import search_corp_vector_store
from .tools.invoke_app_api import invoke_app_api

# Load entities metadata
def load_entities_metadata():
    """Load the entities metadata JSON file"""
    current_dir = os.path.dirname(os.path.abspath(__file__))
    metadata_path = os.path.join(current_dir, '..', '..', 'AIService', 'metadata', 'entities-metadata.json')
    
    try:
        with open(metadata_path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except FileNotFoundError:
        print(f"Warning: entities-metadata.json not found at {metadata_path}")
        return {}
    except json.JSONDecodeError as e:
        print(f"Warning: Error parsing entities-metadata.json: {e}")
        return {}

# Load the metadata
entities_metadata = load_entities_metadata()

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
        markdown_content.append("Available entities in the UNOPS PAO system:")
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
                    endpoint_line = f"- **{endpoint.get('method', 'GET')}** {endpoint.get('endpoint', '')}"
                    if 'description' in endpoint:
                        endpoint_line += f" - {endpoint['description']}"
                    markdown_content.append(endpoint_line)
                    
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

## UNOPS PAO System Entities and API Reference

You have access to a comprehensive UNOPS PAO system with the following entities and capabilities:

{entities_metadata}

Use this metadata to understand:
- Available entities and their data models
- Supported API endpoints and HTTP methods
- Required and optional parameters for each operation
- Request model structures for complex operations
- Relationships between entities

When processing requests, refer to this metadata to ensure accurate API calls and data handling.

## Tools Available

**invoke_app_api** - Use this tool to search for any of the corporate entities in the CRM application you have been provided metadata about (Partners, Contacts, Interactions, etc.).
Based on the information you have in the entity metadata, identify which would be the appropriate endpoint to call and use this tool to make direct HTTP requests to API endpoints
You have all the information needed to use this tool to retrieve information from the application (you have information about about the entities available, their data model, the endpoints they support, and the parameters and request models for the APIs).
PARAMETERS: url, method, params, headers, tool_context

**search_corp_vector_store** - Searches corporate vector store/knowledge base.  Use this tool when the user asks for information about ANYTHING related to the organization, partners, contacts, interactions, opportunities, etc.
Use relevant entityTypeIds to get the most relevant information.  The entityTypeIds are: "ENGAGEMENT", "PROJECT", "POLICY", "LEGAL_AGREEMENT".
If you are not sure about the entityTypeIds, leave it blank.
PARAMETERS: query, applicationId, entityTypeId, entityId, maxResults, isMultiToolRequest

**google_search** - Searches the web for information.
PARAMETERS: query
Make sure to return the results in well-formed markdown along with links to the sources.

When you provide your thoughts, make sure to not use the names of the specific tools. Just describe what you would do and substitute the tool name with a short description of what the tool does.  Use non-technical language.
Respond in WELL-FORMED MARKDOWN making proper use of different heading levels, bold text, and lists.

"""

# Create the final instruction by substituting the metadata
instruction = instruction_template.format(
    entities_metadata=format_entities_metadata_as_markdown(entities_metadata)
)


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
    tools=[invoke_app_api, search_corp_vector_store, AgentTool(google_search_agent)]
)