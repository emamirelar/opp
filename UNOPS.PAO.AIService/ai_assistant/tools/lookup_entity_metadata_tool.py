"""
Entity Metadata Lookup Tool for AI Assistant

This tool allows the agent to lookup entity metadata on-demand, reducing
the instruction size by moving detailed API documentation to a queryable tool.
"""

import json
import logging
from typing import Optional
from google.adk.tools.tool_context import ToolContext

from ..utils.metadata_utils import load_entities_metadata

logger = logging.getLogger(__name__)


def _format_single_entity_metadata(entity_name: str, entity_info: dict) -> str:
    """
    Format a single entity's metadata as markdown.
    
    Args:
        entity_name: Name of the entity
        entity_info: Entity information dictionary
        
    Returns:
        Formatted markdown string
    """
    markdown_content = []
    
    markdown_content.append(f"## {entity_name}")
    
    if 'description' in entity_info:
        markdown_content.append(f"**Description:** {entity_info['description']}")
        markdown_content.append("")
    
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
            
            # Add whenToUse, whenNotToUse, responseNote, importantNote, requestSource if present
            if 'whenToUse' in endpoint:
                markdown_content.append(f"  **When To Use:** {endpoint['whenToUse']}")
            if 'whenNotToUse' in endpoint:
                markdown_content.append(f"  **When NOT To Use:** {endpoint['whenNotToUse']}")
            if 'requestSource' in endpoint:
                markdown_content.append(f"  **Request Source:** {endpoint['requestSource']}")
            if 'responseNote' in endpoint:
                markdown_content.append(f"  **Response Note:** {endpoint['responseNote']}")
            if 'importantNote' in endpoint:
                markdown_content.append(f"  **IMPORTANT:** {endpoint['importantNote']}")
            
            # Handle filtersFormat (for advanced-search endpoints)
            if 'filtersFormat' in endpoint:
                ff = endpoint['filtersFormat']
                markdown_content.append("  **Filters Format:**")
                if 'description' in ff:
                    markdown_content.append(f"    {ff['description']}")
                if 'structure' in ff:
                    markdown_content.append(f"    **Structure:** {ff['structure']}")
                if 'availableOperators' in ff:
                    operators = ', '.join(ff['availableOperators'])
                    markdown_content.append(f"    **Available Operators:** {operators}")
                if 'commonFields' in ff:
                    fields = ', '.join(ff['commonFields'])
                    markdown_content.append(f"    **Common Fields:** {fields}")
                if 'workflowStages' in ff:
                    stages = ', '.join(ff['workflowStages'])
                    markdown_content.append(f"    **Workflow Stages:** {stages}")
                if 'examples' in ff and ff['examples']:
                    markdown_content.append("    **Examples:**")
                    for example in ff['examples']:
                        if 'scenario' in example:
                            markdown_content.append(f"      - **{example['scenario']}:**")
                        if 'filters' in example:
                            markdown_content.append(f"        `{example['filters']}`")
                markdown_content.append("")
            
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
            
            # Handle requestBody section
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
                    example_json = json.dumps(rb['exampleRequest'], indent=4)
                    for line in example_json.split('\n'):
                        markdown_content.append(f"    {line}")
            
            markdown_content.append("")
    elif 'apiEndpoints' in entity_info and not entity_info['apiEndpoints']:
        markdown_content.append("**API Endpoints:** None (read-only or derived entity)")
        markdown_content.append("")
    
    result = "\n".join(markdown_content)
    # Replace curly braces with square brackets to avoid template conflicts
    result = result.replace("{", "[").replace("}", "]")
    return result


def _search_endpoint_metadata(metadata: dict, endpoint_path: str) -> str:
    """
    Search for a specific endpoint across all entities.
    
    Args:
        metadata: Full metadata dictionary
        endpoint_path: Endpoint path to search for (e.g., "/api/opportunity/create")
        
    Returns:
        Formatted markdown with matching endpoint information
    """
    markdown_content = []
    markdown_content.append(f"## Endpoint Search: {endpoint_path}")
    markdown_content.append("")
    
    found_endpoints = []
    
    # Search through all entities
    entities = metadata.get('entities', {})
    for entity_name, entity_info in entities.items():
        endpoints = entity_info.get('apiEndpoints', [])
        for endpoint in endpoints:
            if endpoint.get('endpoint', '').lower() == endpoint_path.lower():
                found_endpoints.append((entity_name, endpoint))
    
    if found_endpoints:
        for entity_name, endpoint in found_endpoints:
            markdown_content.append(f"### Found in Entity: {entity_name}")
            markdown_content.append(f"**Endpoint:** {endpoint.get('endpoint', '')}")
            markdown_content.append(f"**Method:** {endpoint.get('method', 'GET')}")
            if 'description' in endpoint:
                markdown_content.append(f"**Description:** {endpoint['description']}")
            
            # Add workflow indicators if present
            if 'workflowStep' in endpoint:
                markdown_content.append(f"**Workflow Step:** {endpoint['workflowStep']}")
            if 'prerequisite' in endpoint:
                markdown_content.append(f"**Prerequisite:** {endpoint['prerequisite']}")
            
            # Add whenToUse, whenNotToUse, responseNote, importantNote, requestSource if present
            if 'whenToUse' in endpoint:
                markdown_content.append(f"**When To Use:** {endpoint['whenToUse']}")
            if 'whenNotToUse' in endpoint:
                markdown_content.append(f"**When NOT To Use:** {endpoint['whenNotToUse']}")
            if 'requestSource' in endpoint:
                markdown_content.append(f"**Request Source:** {endpoint['requestSource']}")
            if 'responseNote' in endpoint:
                markdown_content.append(f"**Response Note:** {endpoint['responseNote']}")
            if 'importantNote' in endpoint:
                markdown_content.append(f"**IMPORTANT:** {endpoint['importantNote']}")
            
            # Handle filtersFormat (for advanced-search endpoints)
            if 'filtersFormat' in endpoint:
                ff = endpoint['filtersFormat']
                markdown_content.append("**Filters Format:**")
                if 'description' in ff:
                    markdown_content.append(f"  {ff['description']}")
                if 'structure' in ff:
                    markdown_content.append(f"  **Structure:** {ff['structure']}")
                if 'availableOperators' in ff:
                    operators = ', '.join(ff['availableOperators'])
                    markdown_content.append(f"  **Available Operators:** {operators}")
                if 'commonFields' in ff:
                    fields = ', '.join(ff['commonFields'])
                    markdown_content.append(f"  **Common Fields:** {fields}")
                if 'workflowStages' in ff:
                    stages = ', '.join(ff['workflowStages'])
                    markdown_content.append(f"  **Workflow Stages:** {stages}")
                if 'examples' in ff and ff['examples']:
                    markdown_content.append("  **Examples:**")
                    for example in ff['examples']:
                        if 'scenario' in example:
                            markdown_content.append(f"    - **{example['scenario']}:**")
                        if 'filters' in example:
                            markdown_content.append(f"      `{example['filters']}`")
                markdown_content.append("")
            
            markdown_content.append("")
            
            # Add all endpoint details
            if 'parameters' in endpoint and endpoint['parameters']:
                markdown_content.append("**Parameters:**")
                for param in endpoint['parameters']:
                    param_line = f"- {param.get('name', '')} ({param.get('dataType', 'string')})"
                    if param.get('required', False):
                        param_line += " *required*"
                    if 'description' in param:
                        param_line += f" - {param['description']}"
                    markdown_content.append(param_line)
                markdown_content.append("")
            
            if 'requestBody' in endpoint:
                rb = endpoint['requestBody']
                markdown_content.append("**Request Body:**")
                if 'description' in rb:
                    markdown_content.append(f"  {rb['description']}")
                if 'fields' in rb:
                    for field in rb['fields']:
                        field_line = f"  - {field.get('name', '')} ({field.get('dataType', 'string')})"
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
                    example_json = json.dumps(rb['exampleRequest'], indent=4)
                    for line in example_json.split('\n'):
                        markdown_content.append(f"    {line}")
                markdown_content.append("")
    else:
        markdown_content.append(f"**No endpoint found matching:** {endpoint_path}")
        markdown_content.append("")
        markdown_content.append("Available entities:")
        for entity_name in entities.keys():
            markdown_content.append(f"- {entity_name}")
    
    result = "\n".join(markdown_content)
    result = result.replace("{", "[").replace("}", "]")
    return result


def _get_metadata_summary(metadata: dict) -> str:
    """
    Get a summary of all available entities and workflows.
    
    Args:
        metadata: Full metadata dictionary
        
    Returns:
        Formatted markdown summary
    """
    markdown_content = []
    markdown_content.append("## UNOPS CRM System Metadata Summary")
    markdown_content.append("")
    
    # Add workflows summary
    if 'workflows' in metadata:
        workflows = metadata['workflows']
        markdown_content.append("### Multi-Step Workflows")
        if 'description' in workflows:
            markdown_content.append(workflows['description'])
        if 'patterns' in workflows:
            for pattern in workflows['patterns']:
                markdown_content.append(f"- **{pattern.get('name', 'Workflow')}**")
                if 'step1' in pattern:
                    step1 = pattern['step1']
                    markdown_content.append(f"  - Step 1: `{step1.get('endpoint', '')}`")
                if 'step2' in pattern:
                    step2 = pattern['step2']
                    markdown_content.append(f"  - Step 2: `{step2.get('endpoint', '')}`")
        markdown_content.append("")
    
    # Add entities list
    entities = metadata.get('entities', {})
    if entities:
        markdown_content.append("### Available Entities")
        markdown_content.append("Use `get_json_for_entity` with entity_name to get detailed information about any entity.")
        markdown_content.append("")
        for entity_name, entity_info in entities.items():
            description = entity_info.get('description', 'No description')
            endpoint_count = len(entity_info.get('apiEndpoints', []))
            markdown_content.append(f"- **{entity_name}**: {description}")
            markdown_content.append(f"  - {endpoint_count} API endpoint(s) available")
        markdown_content.append("")
    
    result = "\n".join(markdown_content)
    result = result.replace("{", "[").replace("}", "]")
    return result


def get_json_for_entity(
    entity_name: Optional[str] = None,
    endpoint_path: Optional[str] = None,
    search_term: Optional[str] = None,
    tool_context: Optional[ToolContext] = None
) -> str:
    """
    Lookup metadata for entities, endpoints, or search for specific information.
    
    This tool provides detailed information about UNOPS CRM system entities and their API endpoints.
    Use this tool when you need to understand:
    - Entity data models and field definitions
    - Available API endpoints and their parameters
    - Request/response structures
    - Workflow patterns and multi-step operations
    
    **When to use this tool:**
    - Before calling an API endpoint, lookup the entity to understand required parameters
    - When user asks about available operations for an entity
    - When you need to understand the data structure of an entity
    - When you need to find a specific endpoint's parameters
    
    **Examples:**
    - `get_json_for_entity(entity_name="Opportunity")` - Get all Opportunity entity details
    - `get_json_for_entity(entity_name="Partner")` - Get all Partner entity details
    - `get_json_for_entity(endpoint_path="/api/opportunity/create")` - Find specific endpoint
    - `get_json_for_entity()` - Get summary of all available entities
    
    Args:
        entity_name: Name of entity to lookup (e.g., "Opportunity", "Partner", "Contact", "Interaction")
        endpoint_path: Specific endpoint path to lookup (e.g., "/api/opportunity/create")
        search_term: Search for any term in metadata (currently not implemented, use entity_name or endpoint_path)
    
    Returns:
        Formatted markdown with relevant metadata information
    """
    try:
        metadata = load_entities_metadata()
        
        if not metadata:
            return "⚠️ Error: Could not load entities metadata. Metadata file may be missing or corrupted."
        
        # Priority 1: Lookup specific entity
        if entity_name:
            entities = metadata.get('entities', {})
            entity_info = entities.get(entity_name)
            
            if entity_info:
                logger.info(f"📋 Looked up entity metadata for: {entity_name}")
                return _format_single_entity_metadata(entity_name, entity_info)
            else:
                # Entity not found, return list of available entities
                available_entities = list(entities.keys())
                result = f"⚠️ Entity '{entity_name}' not found.\n\n"
                result += "**Available entities:**\n"
                for name in available_entities:
                    result += f"- {name}\n"
                result += f"\nUse `get_json_for_entity(entity_name=\"{available_entities[0] if available_entities else 'Contact'}\")` to get details."
                return result
        
        # Priority 2: Search for specific endpoint
        if endpoint_path:
            logger.info(f"📋 Searching for endpoint: {endpoint_path}")
            return _search_endpoint_metadata(metadata, endpoint_path)
        
        # Priority 3: Return summary if no specific query
        logger.info("📋 Returning metadata summary")
        return _get_metadata_summary(metadata)
        
    except Exception as e:
        logger.error(f"❌ Error in get_json_for_entity: {e}")
        import traceback
        return f"⚠️ Error looking up metadata: {str(e)}\n\nTraceback:\n{traceback.format_exc()}"
