"""
Google Drive Utility Functions

This module contains all Google Drive related utilities using external APIs
for search, file reading, and content operations.
All functions use invoke_api_tool from common_callbacks for consistency.
"""

import json
import logging
from typing import List, Optional, Dict, Any

# All Google Drive operations now use the external APIs below

def search_external_drive_service(query: str, external_endpoint_url: str, auth_headers: Optional[Dict[str, str]] = None) -> str:
    """
    Search using external service for file IDs, then read content from URLs
    
    Args:
        query: Search query
        external_endpoint_url: Your company's search endpoint
        auth_headers: Optional authentication headers for external API
        
    Returns:
        JSON string with formatted results
    """
    try:
        from ai_assistant.utils.common_callbacks import invoke_api_tool
        
        # Step 1: Call external search service to get file IDs
        print(f"🔍 [EXTERNAL-SEARCH] Calling external endpoint: {external_endpoint_url}")
        print(f"🔍 [EXTERNAL-SEARCH] Query: {query}")
        
        # Prepare headers for external API
        headers = {"Content-Type": "application/json"}
        if auth_headers:
            headers.update(auth_headers)
        
        response = invoke_api_tool(
            url=external_endpoint_url,
            method="POST",
            body={"query": query, "maxResults": 20},
            headers=headers
        )
        
        if response.get("status") == "error":
            return json.dumps({
                    "content": f"Error calling external search service: {response.get('error')}",
                    "sources": []
            })

        # Extract documents from API response structure
        response_data = response.get("response", response)
        documents = response_data.get("documents", [])
        print(f"🔍 [EXTERNAL-SEARCH] Found {len(documents)} documents from external service")
        
        if not documents:
                return json.dumps({
                        "content": "No files found for your search query in the external service.",
                    "sources": []
                })
        
        # Extract webViewLinks for URL content reading
        web_view_links = [doc.get("webViewLink") for doc in documents if doc.get("webViewLink")]
        print(f"🔍 [EXTERNAL-SEARCH] Extracted {len(web_view_links)} webViewLinks: {web_view_links[:3]}...")
        
        # Step 2: Use URL content reader to read content from webViewLinks
        return _read_external_files_from_urls(web_view_links, documents, query)
            
    except Exception as e:
        logging.error(f"Error in external search integration: {e}")
        return json.dumps({
            "content": f"Error processing external search results: {str(e)}",
            "sources": []
        })
        
def search_unops_google_drive(query: str, auth_headers: Optional[Dict[str, str]] = None) -> str:
    """
    Convenience function for UNOPS external Google Drive search service
    
    Args:
        query: Search query
        auth_headers: Optional authentication headers
        
    Returns:
        JSON string with formatted results
    """
    unops_endpoint = "https://api.ai.dev.unops.org/v1/tools/google-drive/search"
    return search_external_drive_service(query, unops_endpoint, auth_headers)

def _read_external_files_from_urls(web_view_links: List[str], external_docs: List[Dict], original_query: str) -> str:
    """Read content from URLs using the URL content reader"""
    try:
        if not web_view_links:
            return json.dumps({
                "content": "No webViewLinks found in the external search results",
                "sources": []
            })
        
        print(f"🌐 [URL-CONTENT-READ] Reading content from {len(web_view_links)} URLs...")
        
        # Create a mapping of webViewLink to external metadata for enrichment
        external_metadata = {doc.get("webViewLink"): doc for doc in external_docs if doc.get("webViewLink")}
        
        # Format results similar to knowledge search
        formatted_content = f"📁 **External Search Results for '{original_query}':**\n\n"
        sources = []
        successful_reads = 0
        
        for i, url in enumerate(web_view_links, 1):
            external_doc = external_metadata.get(url, {})
            doc_name = external_doc.get('name', f'Document {i}')
            
            print(f"🌐 [URL-CONTENT-READ] Processing document {i}/{len(web_view_links)}: {doc_name}")
            
            # Read content using the URL content reader
            content_result = read_content_from_url(
                url=url, 
                title=doc_name,
                description=f"Document from external search: {original_query}"
            )
            
            try:
                content_data = json.loads(content_result)
            except json.JSONDecodeError:
                content_data = {"error": "Failed to parse content result", "content": content_result}
            
            formatted_content += f"**Document {i}: {doc_name}**\n"
            
            if content_data.get("error"):
                formatted_content += f"*Error reading content:* {content_data['error']}\n"
                formatted_content += f"*Type:* {external_doc.get('mimeType', 'Unknown')}\n"
                formatted_content += f"*Last Updated:* {external_doc.get('updatedAt', 'Unknown')}\n\n"
                continue
            
            # Successfully read content
            successful_reads += 1
            
            # Add metadata from external service
            if external_doc.get('updatedAt'):
                formatted_content += f"*Last Updated:* {external_doc['updatedAt']}\n"
            if external_doc.get('mimeType'):
                formatted_content += f"*Type:* {external_doc['mimeType']}\n"
            
            # Add content (truncate if very long)
            content_text = content_data.get("content", "")
            if isinstance(content_text, dict):
                # If content is still a dict, convert to string
                content_text = json.dumps(content_text, indent=2)
            
            content_text = str(content_text)
            if len(content_text) > 2000:  # Truncate very long content
                content_text = content_text[:2000] + "...\n\n[Content truncated - full document available at link]"
            
            formatted_content += f"*Content:*\n{content_text}\n\n"
            
            sources.append({
                "title": doc_name,
                "url": url,
                "description": f"Document: {doc_name}",
                "last_updated": external_doc.get('updatedAt', 'Unknown'),
                "file_type": external_doc.get('mimeType', 'Unknown')
            })
        
        print(f"🌐 [URL-CONTENT-READ] Successfully read {successful_reads}/{len(web_view_links)} documents")
        
        # Add summary at the end
        if successful_reads > 0:
            formatted_content += f"\n---\n**Summary:** Found {len(external_docs)} matching documents, successfully read content from {successful_reads} files using URL content reader."
        
        return json.dumps({
            "content": formatted_content,
            "sources": sources,
            "stats": {
                "total_found": len(external_docs),
                "content_read": successful_reads,
                "query": original_query
            }
        })
        
    except Exception as e:
        logging.error(f"Error reading external search files from URLs: {str(e)}", exc_info=True)
        return json.dumps({
            "content": f"Error reading file contents from URLs: {str(e)}",
            "sources": []
        })

def read_content_from_url(url: str, include_json: bool = True, output_format: str = "markdown", title: str = "", description: str = "") -> str:
    """
    Read content from any URL using the external convert/url API
    
    Args:
        url: The URL to read content from (can be Google Drive webViewLink or any URL)
        include_json: Whether to include JSON in the response (default: True)
        output_format: Format for the output (default: "markdown")
        title: Optional title for the content
        description: Optional description for the content
        
    Returns:
        JSON string with the content and metadata
    """
    try:
        from ai_assistant.utils.common_callbacks import invoke_api_tool
        
        convert_endpoint = "https://api.ai.dev.unops.org/v1/convert/url"
        
        print(f"🌐 [URL-CONVERT] Reading content from URL: {url}")
        
        headers = {"Content-Type": "application/json"}
        
        body = {
            "includeJson": include_json,
            "outputFormat": output_format,
            "gcsOutput": "",
            "chunkSize": 1,
            "embeddingsModel": "",
            "title": title,
            "description": description,
            "url": url
        }
        
        response = invoke_api_tool(
            url=convert_endpoint,
            method="POST",
            body=body,
            headers=headers
        )
        
        if response.get("status") == "error":
            return json.dumps({
                "error": f"Error calling URL convert service: {response.get('error')}",
                "url": url
            })
        
        # Handle both JSON and string responses
        response_data = response.get("response", response)
        content_data = None
        
        if isinstance(response_data, dict):
            # If it's already a dict, try to get the 'data' field
            content_data = response_data.get("data", response_data)
        elif isinstance(response_data, str):
            try:
                # Try to parse as JSON
                parsed_response = json.loads(response_data)
                content_data = parsed_response.get("data", parsed_response)
            except json.JSONDecodeError:
                # If it fails, use the string as is
                content_data = response_data
        else:
            content_data = response_data
        
        print(f"🌐 [URL-CONVERT] Successfully read content from URL")
        
        return json.dumps({
            "content": content_data,
            "url": url,
            "format": output_format,
            "success": True
        })
        
    except Exception as e:
        logging.error(f"Error reading content from URL {url}: {str(e)}", exc_info=True)
        return json.dumps({
            "error": f"Failed to read content from URL: {str(e)}",
            "url": url,
            "suggestion": "Check if the URL is accessible and the convert service is available"
        })

def convert_markdown_to_google_doc(markdown_content: str, filename: str, metadata: Optional[Dict[str, Any]] = None) -> str:
    """
    Convert markdown content to Google Doc using external API
    
    Args:
        markdown_content: Markdown content as string
        filename: Name of the file
        metadata: Optional metadata for the document
        
    Returns:
        JSON string with the conversion result
    """
    try:
        from ai_assistant.utils.common_callbacks import invoke_api_tool
        import requests
        
        convert_endpoint = "https://api.ai.dev.unops.org/v1/convert/markdown-to-google-doc"
        
        print(f"📄 [MARKDOWN-TO-GDOC] Converting markdown file: {filename}")
        
        # For multipart/form-data, we need to use requests directly since invoke_api_tool expects JSON
        # But we'll still get the headers from invoke_api_tool's pattern
        headers = {}  # Don't set Content-Type for multipart, requests will handle it
        
        # Convert string content to bytes for file upload
        file_data = markdown_content.encode('utf-8')
        
        # Prepare the files and data for multipart upload
        files = {
            'file': (filename, file_data, 'text/markdown')
        }
        
        data = {
            'data': json.dumps(metadata or {})
        }
        
        # Use invoke_api_tool but with a special handling for multipart
        # We'll make the request directly but follow the same auth pattern
        try:
            import inspect
            frame = inspect.currentframe()
            tool_context = None
            while frame:
                if 'tool_context' in frame.f_locals:
                    tool_context = frame.f_locals['tool_context']
                    break
                frame = frame.f_back
        except Exception:
            tool_context = None
        
        # Build headers following the same pattern as invoke_api_tool
        request_headers = {}
        
        # Add development headers if needed
        import os
        is_development = os.getenv('IS_DEVELOPMENT', '').upper() == 'TRUE'
        dev_email = os.getenv('DEV_EMAIL', '')
        
        if is_development and dev_email:
            print("🧪 [MARKDOWN-TO-GDOC] Adding development IAP headers...")
            import time
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
            print(f"✅ [MARKDOWN-TO-GDOC] Added development IAP headers for email: {dev_email}")
        
        # Add IDP token if available (same logic as invoke_api_tool)
        if not request_headers.get('Authorization'):
            print("🔐 [MARKDOWN-TO-GDOC] Attempting to get IDP token...")
            from ai_assistant.utils.api_config_manager import config_manager
            from ai_assistant.utils.auth_helpers import get_service_account_oidc_token
            
            oauth_config = config_manager.get_oauth_config()
            target_principal = oauth_config.get('target_principal')
            target_audience = oauth_config.get('client_id')
            if target_principal and target_audience:
                # Get user email from tool_context if available, otherwise use dev_email for development
                user_email = None
                if tool_context and hasattr(tool_context, 'state') and tool_context.state:
                    user_email = tool_context.state.get('user_email')
                
                # Always fall back to dev_email in development if user_email is not available
                if not user_email and is_development and dev_email:
                    user_email = dev_email
                    print(f"🧪 [MARKDOWN-TO-GDOC] Using dev_email for IDP token: {dev_email}")
                
                # For Google Doc creation, use service account token without impersonation
                print(f"🔍 [MARKDOWN-TO-GDOC] Using service account as target_principal: {target_principal}")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] target_audience: {target_audience}")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] target_principal: {target_principal}")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] use_idp: False")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] subject: None")
                print(f"🔍 [MARKDOWN-TO-GDOC-AUTH-PARAMS] user_email for impersonation header: {user_email}")
                idp_token = get_service_account_oidc_token(
                    target_audience,
                    target_principal,  # Use original service account email from config
                    use_idp=False,
                    subject=None
                )
                if idp_token:
                    request_headers['Authorization'] = f"Bearer {idp_token}"
                    print(f"🔐 [MARKDOWN-TO-GDOC] Added IDP token to Authorization header")
                    
                    # Log token details for debugging
                    try:
                        import base64
                        parts = idp_token.split('.')
                        if len(parts) >= 2:
                            payload = parts[1]
                            # Add padding if needed
                            payload += '=' * (4 - len(payload) % 4)
                            decoded = base64.b64decode(payload)
                            token_data = json.loads(decoded)
                            print(f"🔍 [MARKDOWN-TO-GDOC-TOKEN] Token details:")
                            print(f"   sub: {token_data.get('sub', 'Not Present')}")
                            print(f"   email: {token_data.get('email', 'Not Present')}")
                            print(f"   aud: {token_data.get('aud', 'Not Present')}")
                            print(f"   iss: {token_data.get('iss', 'Not Present')}")
                    except Exception as e:
                        print(f"❌ [MARKDOWN-TO-GDOC-TOKEN] Could not decode token: {e}")
                else:
                    print(f"❌ [MARKDOWN-TO-GDOC] Failed to get IDP token - will proceed without Authorization header")
            else:
                print(f"⚠️ [MARKDOWN-TO-GDOC] Missing OAuth config - target_principal: {target_principal}, client_id: {target_audience}")
        
        # Add impersonated user header for Google Doc creation
        # Use user_email from tool_context or dev_email for development
        impersonated_user_email = None
        print(f"🔍 [MARKDOWN-TO-GDOC] Checking for impersonation user email:")
        print(f"   tool_context exists: {tool_context is not None}")
        
        if tool_context and hasattr(tool_context, 'state') and tool_context.state:
            impersonated_user_email = tool_context.state.get('user_email')
            print(f"   Found user_email in tool_context.state: {impersonated_user_email}")
        elif is_development and dev_email:
            impersonated_user_email = dev_email
            print(f"   Using dev_email for impersonation: {dev_email}")
        else:
            print(f"   No user email found in tool_context or dev_email")
            
        if impersonated_user_email:
            request_headers['x-unops-impersonated-user'] = impersonated_user_email
            print(f"🔐 [MARKDOWN-TO-GDOC] Added impersonated user header: {impersonated_user_email}")
        else:
            print(f"⚠️ [MARKDOWN-TO-GDOC] No user email available for impersonation header")
        
        print("🔐 [MARKDOWN-TO-GDOC] Final request headers being sent:")
        print(f"📋 Total headers: {len(request_headers)}")
        print(f"📋 Header keys: {list(request_headers.keys())}")
        
        # Log final headers safely (same as invoke_api_tool)
        for key, value in request_headers.items():
            if any(sensitive in key.lower() for sensitive in ['authorization', 'token', 'jwt', 'secret', 'password']):
                masked_value = f"{value[:10]}..." if len(value) > 10 else "***"
                print(f"   {key}: {masked_value} (masked)")
            elif 'email' in key.lower():
                print(f"   {key}: {value}")
            elif len(str(value)) > 100:
                print(f"   {key}: {str(value)[:50]}... (truncated)")
            elif key.lower() in ['content-type', 'accept', 'user-agent']:
                print(f"   {key}: {value}")
        else:
                print(f"   {key}: {str(value)[:50]}...")
                
        print(f"📄 [MARKDOWN-TO-GDOC] Making multipart request to: {convert_endpoint}")
        
        # Make the multipart request
        response = requests.post(
            convert_endpoint,
            files=files,
            data=data,
            headers=request_headers,
            timeout=60,
            verify=False
        )
        
        print(f"📄 [MARKDOWN-TO-GDOC] Response status: {response.status_code}")
        print(f"📄 [MARKDOWN-TO-GDOC] Request headers sent: {list(request_headers.keys())}")
        
        # Enhanced error logging for IAP issues
        if response.status_code == 401:
            print("❌ [MARKDOWN-TO-GDOC] 401 Unauthorized - IAP credentials invalid")
            print(f"   Request headers: {list(request_headers.keys())}")
            if 'Authorization' in request_headers:
                print(f"   Authorization header present: {request_headers['Authorization'][:20]}...")
            print(f"   Response text: {response.text[:200]}")
        elif response.status_code == 403:
            print("❌ [MARKDOWN-TO-GDOC] 403 Forbidden - IAP access denied")
            print(f"   Response text: {response.text[:200]}")
        
        if response.status_code >= 200 and response.status_code < 300:
            try:
                response_data = response.json()
                print(f"✅ [MARKDOWN-TO-GDOC] Successfully converted markdown to Google Doc")
                
                return json.dumps({
                    "status": "success",
                    "response": response_data,
                    "filename": filename
                })
            except json.JSONDecodeError:
                return json.dumps({
                    "status": "success",
                    "response": {"text": response.text},
                    "filename": filename,
                    "note": "Response was not JSON"
                })
        else:
            error_message = f"HTTP {response.status_code}"
            try:
                error_data = response.json()
                if isinstance(error_data, dict):
                    error_message = error_data.get('message', error_data.get('error', error_message))
            except:
                error_message = response.text if response.text else error_message
            
            print(f"❌ [MARKDOWN-TO-GDOC] Error {response.status_code}: {error_message}")
        
        return json.dumps({
                "status": "error",
                "error": error_message,
                "filename": filename
            })
        
    except Exception as e:
        logging.error(f"Error converting markdown to Google Doc: {str(e)}", exc_info=True)
        return json.dumps({
            "error": f"Failed to convert markdown to Google Doc: {str(e)}",
            "filename": filename,
            "suggestion": "Check if the convert service is available and the file is valid markdown"
        }) 