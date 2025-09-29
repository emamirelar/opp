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
from typing import Optional
from google.adk.tools.tool_context import ToolContext

from .tools.search_corp_vector_store_tool import search_corp_vector_store

# Load entities metadata
def load_entities_metadata():
    """Load the entities metadata JSON file"""
    current_dir = os.path.dirname(os.path.abspath(__file__))
    metadata_path = os.path.join(current_dir, '..', 'config', 'entities-metadata.json')
    
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


def prepare_api_url(url: str) -> str:
    """
    Prepare the final URL for API calls by ensuring the base URL comes from config manager.
    For absolute URLs, extracts the path and combines it with the config base URL.
    For relative URLs, combines directly with the config base URL.
    
    Args:
        url: URL to prepare - can be relative (e.g., /api/partners) or absolute (e.g., https://localhost:44426/api/partners)
    
    Returns:
        str: The final URL to use for the API call with base URL from config
    
    Examples:
        # Relative URL - will be combined with base URL from config
        final_url = prepare_api_url("/api/users")
        
        # Absolute URL - path will be extracted and combined with config base URL
        final_url = prepare_api_url("https://someother.com/api/users")
        # Result: https://config-base-url/api/users
    """
    try:
        from .utils.api_config_manager import config_manager
        base_url = config_manager.get_api_base_url()
        
        # Extract path from URL (works for both absolute and relative URLs)
        if url.startswith(('http://', 'https://')):
            # For absolute URLs, extract the path part
            from urllib.parse import urlparse
            parsed_url = urlparse(url)
            path = parsed_url.path
            # Include query string and fragment if present
            if parsed_url.query:
                path += '?' + parsed_url.query
            if parsed_url.fragment:
                path += '#' + parsed_url.fragment
        else:
            # For relative URLs, use as-is
            path = url
        
        # Ensure proper URL joining with the config base URL
        if base_url.endswith('/') and path.startswith('/'):
            return base_url + path[1:]
        elif not base_url.endswith('/') and not path.startswith('/'):
            return base_url + '/' + path
        else:
            return base_url + path
            
    except ImportError:
        # Config manager not available, return url as-is
        return url

def invoke_app_api(url: str, method: str, params: Optional[dict] = None, headers: Optional[dict] = None, tool_context: Optional[ToolContext] = None) -> dict:
    """
    Invoke an API endpoint with minimal logging
    
    Args:
        url: URL to call - can be relative (e.g., /api/partners) or absolute (e.g., https://localhost:44426/api/partners)
        method: HTTP method (GET, POST, PUT, DELETE)
        params: Request parameters/body
        headers: Optional additional headers
    
    Returns:
        dict: API response or error information
    
    Examples:
        # GET request with relative URL (will use base URL from config)
        result = invoke_app_api("/api/users", "GET")
        
        # GET request with absolute URL (will use as-is)
        result = invoke_app_api("https://api.example.com/users", "GET")
        
        # POST request with data
        result = invoke_app_api(
            "/api/users", 
            "POST", 
            params={"name": "John", "email": "john@example.com"}
        )
        
        # GET request with query parameters
        result = invoke_app_api(
            "/api/users", 
            "GET", 
            params={"search": "john", "limit": 10}
        )
        
        # PUT request with custom headers
        result = invoke_app_api(
            "/api/users/123", 
            "PUT", 
            params={"name": "John Updated"},
            headers={"X-Custom-Header": "value"}
        )
    """

    print(tool_context)
    
    try:
        # Prepare the final URL using the dedicated function
        final_url = prepare_api_url(url)
        # Prepare request headers - start with default headers
        request_headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
        
        is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
        dev_email = os.getenv('DEV_EMAIL', '')
       
        if is_development and dev_email:
            current_timestamp = str(int(time.time()))
            
            iap_headers = {
                'x-goog-authenticated-user-email': f'accounts.google.com:{dev_email}',
                'x-goog-authenticated-user-id': f'accounts.google.com:dev-user-id-{current_timestamp}',
                'x-forwarded-user': dev_email,
                'x-forwarded-email': dev_email,
                'X-Dev-IAP-Simulation': 'true',
                'X-Dev-Auth-Timestamp': current_timestamp
            }
            request_headers.update(iap_headers)
        
        # Add any additional headers passed as parameter
        if headers:
            request_headers.update(headers)
        
        # Add IDP token to request headers if not already present
        if not request_headers.get('Authorization'):
            # Try to import config manager and auth helpers
            try:
                from .utils.api_config_manager import config_manager
                from .utils.auth_helpers import get_service_account_oidc_token
                
                # Get OAuth configuration from config manager
                oauth_config = config_manager.get_oauth_config()
                target_principal = oauth_config.get('target_principal')
                target_audience = oauth_config.get('client_id')
                
                if target_principal and target_audience:
                    # Get user email from tool_context if available, otherwise use dev_email for development
                    user_email = None
                    if tool_context and hasattr(tool_context, 'state') and tool_context.state:
                        user_email = tool_context.state.get('user_email')
                    elif is_development and dev_email:
                        user_email = dev_email
                    
                    # Check if this is a Google-related external API call
                    is_google_api = any(google_path in url for google_path in [
                        '/google-drive/', '/convert/url', '/convert/markdown-to-google-doc'
                    ])
                    
                    if is_google_api:
                        # For Google APIs, use service account token without impersonation
                        idp_token = get_service_account_oidc_token(
                            target_audience,
                            target_principal,
                            use_idp=False,
                            subject=None
                        )
                    else:
                        # For regular APIs, use impersonated token
                        idp_token = get_service_account_oidc_token(
                            target_audience,
                            target_principal,
                            use_idp=True,
                            subject=user_email
                        )
                    
                    if idp_token:
                        request_headers['Authorization'] = f"Bearer {idp_token}"
                    
                    # Add impersonation header for ALL APIs when user email is available
                    if user_email:
                        request_headers['x-unops-impersonated-user'] = user_email
                        
            except ImportError:
                # Config manager or auth helpers not available, continue without auth
                pass
        
        # In agent.py, add logging to see what user_email is being used
        print(f"🔍 User email from tool_context: {tool_context.state.get('user_email') if tool_context and tool_context.state else 'None'}")
        print(f"🔍 Dev email fallback: {dev_email}")
        print(f"🔍 Final user_email: {user_email}")

        # Get API timeout (default to 30 seconds if config not available)
        api_timeout = 30
        try:
            from .utils.api_config_manager import config_manager
            api_timeout = config_manager.get_api_timeout()
        except ImportError:
            pass
        
        # Prepare request body
        body = params or {}
        
        # Make the appropriate HTTP request based on method
        if method.upper() == 'GET':
            # Handle GET parameters properly
            if body:
                # For GET requests, convert body to query parameters
                query_params = '&'.join([f"{k}={v}" for k, v in body.items() if v is not None])
                if query_params:
                    separator = '&' if '?' in final_url else '?'
                    final_url += separator + query_params
            
            # Make GET request
            response = requests.get(final_url, headers=request_headers, timeout=api_timeout, verify=False)
            
        elif method.upper() == 'POST':
            # Make POST request with JSON body
            response = requests.post(final_url, json=body, headers=request_headers, timeout=api_timeout, verify=False)
            
        elif method.upper() == 'PUT':
            # Make PUT request with JSON body
            response = requests.put(final_url, json=body, headers=request_headers, timeout=api_timeout, verify=False)
            
        elif method.upper() == 'DELETE':
            # Make DELETE request
            response = requests.delete(final_url, headers=request_headers, timeout=api_timeout, verify=False)
            
        else:
            return {
                "status": "error",
                "error": f"Unsupported HTTP method: {method}",
                "supported_methods": ["GET", "POST", "PUT", "DELETE"]
            }
        
        # Process response
        if response.status_code >= 200 and response.status_code < 300:
            try:
                response_data = response.json()
                
                return {
                    "status": "success",
                    "status_code": response.status_code,
                    "response": response_data,
                    "api_call": f"{method.upper()} {final_url}",
                    "headers_sent": list(request_headers.keys())
                }
                
            except json.JSONDecodeError:
                return {
                    "status": "success",
                    "status_code": response.status_code,
                    "response": {"text": response.text},
                    "api_call": f"{method.upper()} {final_url}",
                    "headers_sent": list(request_headers.keys()),
                    "note": "Response was not JSON"
                }
                
        else:
            error_message = f"HTTP {response.status_code}"
            try:
                error_data = response.json()
                if isinstance(error_data, dict):
                    error_message = error_data.get('message', error_data.get('error', error_message))
            except:
                error_message = response.text if response.text else error_message
            
            return {
                "status": "error",
                "status_code": response.status_code,
                "error": error_message,
                "api_call": f"{method.upper()} {final_url}",
                "headers_sent": list(request_headers.keys())
            }
            
    except requests.exceptions.ConnectionError as conn_error:
        return {
            "status": "error",
            "error": f"Connection error: {str(conn_error)}",
            "api_call": f"{method.upper()} {final_url}",
            "connection_error": True
        }
        
    except requests.exceptions.Timeout as timeout_error:
        return {
            "status": "error",
            "error": f"Request timeout: {str(timeout_error)}",
            "api_call": f"{method.upper()} {final_url}",
            "timeout_error": True
        }
        
    except Exception as request_error:
        return {
            "status": "error",
            "error": f"Request failed: {str(request_error)}",
            "api_call": f"{method.upper()} {final_url}",
            "traceback": traceback.format_exc()
        }

google_search_agent = LlmAgent(
    model="gemini-2.0-flash",
    name="google_search_agent",
    description="Agent to call the google_search tool and returns the results as-is.",
    instruction="""Your sole job is to call the google_search tool and returns the results as-is.""",
    tools=[google_search]
)


# ## Examples
# **"Find private sector partners"** → `invoke_app_api("https://localhost:44426/api/partners", "GET", {{"PartnerGroupCode": "PRIVATE_SECTOR"}})`

# **"Find our key global partners"** → `invoke_app_api("https://localhost:44426/api/partners", "GET", {{"KeyGlobalPartner": true}})`

# **"What are the latest interactions with partners"** → `invoke_app_api("https://localhost:44426/api/interactions", "GET")`

# **"Create a new contact for Microsoft"** → `invoke_app_api("https://localhost:44426/api/contacts", "POST", {{"LastName": "Smith", "FirstName": "John", "Email": "john.smith@microsoft.com", "Title": "Director", "PartnerId": 123}})`

# **"Search for contacts in the technology sector"** → `invoke_app_api("https://localhost:44426/api/contacts", "GET", {{"SearchText": "technology"}})`

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
# print(instruction)


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