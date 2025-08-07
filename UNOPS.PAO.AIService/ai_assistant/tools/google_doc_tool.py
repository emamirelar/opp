"""
Google Doc Creation Tool

This module provides functionality to create Google Docs with content
using the Google Drive API and Google Docs API.
"""

import os
import logging
import time
import pickle
from typing import Optional, Dict, Any, List
from google.oauth2 import service_account
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError

import io
import json
import httplib2
import asyncio # Import asyncio for async/await

logger = logging.getLogger(__name__)

# It's better to define constants at the module level
DEFAULT_SCOPES = ['https://www.googleapis.com/auth/drive', 'https://www.googleapis.com/auth/documents']

class GoogleDocTool:
    """
    Tool for creating and managing Google Docs with content.
    """
    
    def __init__(self, config: Dict[str, Any]):
        """
        Initialize the Google Doc tool.
        
        Args:
            config: Configuration dictionary containing authentication details
        """
        self.config = config
        self.project_id = config.get('project_id')
        
        # Service Account configuration
        self.secret_name = config.get('secret_name')
        self.service_account_email = config.get('service_account_email')
        
        # OAuth configuration
        self.oauth_enabled = config.get('oauth_enabled', False)
        self.oauth_use_existing_client = config.get('oauth_use_existing_client', False)
        self.oauth_client_secrets_file = config.get('oauth_client_secrets_file')
        self.oauth_client_secret_name = config.get('oauth_client_secret_name')
        self.oauth_token_file = config.get('oauth_token_file', 'google_oauth_token.pickle')
        # Support both single port (legacy) and multiple ports
        if 'oauth_redirect_ports' in config:
            self.oauth_redirect_ports = config.get('oauth_redirect_ports', [8080, 8081, 8082, 9000, 9001, 9876])
        else:
            # Legacy single port support
            single_port = config.get('oauth_redirect_port', 8080)
            self.oauth_redirect_ports = [single_port]
        
        # Use default scopes if not provided
        self.scopes = config.get('scopes', DEFAULT_SCOPES)
        self.drive_service = None
        self.docs_service = None
        self.authenticated = False
        self._http_client = None # Managed at the instance level
        self._last_activity = time.time()
        
        auth_method = "OAuth" if self.oauth_enabled else "Service Account"
        logger.info(f"🔧 Google Doc Tool initialized for project: {self.project_id} (Auth: {auth_method})")
    
    def _create_and_configure_http_client(self):
        """Create and configure HTTP client with proper connection management.
        This should only be called once during initialization to maintain connection pooling.
        """
        try:
            # Create HTTP client with connection pooling and timeouts
            # Using conservative timeout values to prevent hanging
            http = httplib2.Http(
                timeout=30,  # 30 second timeout
                ca_certs=None,  # Use system CAs (secure default)
                disable_ssl_certificate_validation=False,  # Keep SSL validation
                # proxy_info=None,  # Can be configured if needed
            )
            
            # Configure connection pool settings
            # httplib2 manages connections internally, so we avoid resetting connections
            # to maintain pooling benefits. The instance should be long-lived.
            
            # Set additional headers for better API compliance
            http.force_exception_to_status_code = False  # Let real exceptions bubble up
            
            logger.debug("✅ HTTP client created with 30s timeout and connection pooling")
            return http
        except Exception as e:
            logger.error(f"❌ Error creating HTTP client: {e}")
            return None
    
    async def initialize(self) -> bool:
        """
        Initialize and authenticate the Google Doc tool.
        
        Returns:
            bool: True if authentication successful, False otherwise
        """
        if self.authenticated:
            logger.info("🔐 Google Doc Tool already authenticated.")
            return True # Already authenticated, no need to re-initialize
            
        try:
            logger.info("🔐 Authenticating Google Doc Tool...")
            
            # Get credentials from secret manager or service account file
            credentials = await self._get_credentials()
            
            if not credentials:
                logger.error("❌ Failed to get credentials for Google Doc Tool")
                logger.error("❌ Check Secret Manager configuration or GOOGLE_APPLICATION_CREDENTIALS environment variable")
                return False
            
            # Create HTTP client ONCE during initialization
            if not self._http_client: # Only create if not already exists
                self._http_client = self._create_and_configure_http_client()
                if not self._http_client:
                    logger.error("❌ Failed to create HTTP client")
                    return False
            
            # Build services with credentials (http and credentials are mutually exclusive)
            # Let credentials handle HTTP connections automatically for better compatibility
            self.drive_service = build(
                'drive', 'v3', 
                credentials=credentials,
                cache_discovery=False 
            )
            
            self.docs_service = build(
                'docs', 'v1', 
                credentials=credentials,
                cache_discovery=False 
            )
            
            self.authenticated = True
            self._last_activity = time.time()
            
            logger.info("✅ Google Doc Tool authenticated successfully")
            return True
            
        except Exception as e:
            logger.error(f"❌ Failed to authenticate Google Doc Tool: {str(e)}")
            return False
    
    async def _get_oauth_credentials(self):
        """
        Get OAuth credentials for user authentication.
        
        Returns:
            OAuth credentials or None if failed
        """
        try:
            creds = None
            
            # Check if we have saved OAuth tokens
            if os.path.exists(self.oauth_token_file):
                logger.info(f"🔐 Loading existing OAuth tokens from {self.oauth_token_file}")
                try:
                    with open(self.oauth_token_file, 'rb') as token:
                        creds = pickle.load(token)
                except Exception as e:
                    logger.warning(f"⚠️ Failed to load OAuth tokens: {e}")
                    creds = None
            
            # If there are no valid credentials available, initiate OAuth flow
            if not creds or not creds.valid:
                if creds and creds.expired and creds.refresh_token:
                    logger.info("🔄 Refreshing expired OAuth token...")
                    try:
                        await asyncio.to_thread(creds.refresh, Request())
                        logger.info("✅ OAuth token refreshed successfully")
                    except Exception as e:
                        logger.error(f"❌ Failed to refresh OAuth token: {e}")
                        creds = None
                else:
                    # Need to run OAuth flow
                    logger.info("🔐 Starting OAuth authorization flow...")
                    
                    try:
                        # Create OAuth flow - choose method based on configuration
                        if self.oauth_use_existing_client:
                            # Use existing client_id and get client_secret from Secret Manager
                            client_config = await self._build_oauth_client_config()
                            if not client_config:
                                logger.error("❌ Failed to build OAuth client configuration")
                                return None
                            
                            flow = InstalledAppFlow.from_client_config(
                                client_config, self.scopes
                            )
                        else:
                            # Use client_secrets.json file
                            if not self.oauth_client_secrets_file or not os.path.exists(self.oauth_client_secrets_file):
                                logger.error(f"❌ OAuth client secrets file not found: {self.oauth_client_secrets_file}")
                                return None
                            
                            flow = InstalledAppFlow.from_client_secrets_file(
                                self.oauth_client_secrets_file, self.scopes
                            )
                        
                        # Try each port until we find an available one
                        creds = None
                        last_error = None
                        
                        for port in self.oauth_redirect_ports:
                            try:
                                logger.info(f"🔌 Trying OAuth flow on port {port}...")
                                creds = await asyncio.to_thread(
                                    flow.run_local_server, 
                                    port=port,
                                    open_browser=True
                                )
                                logger.info(f"✅ OAuth flow successful on port {port}")
                                break
                            except OSError as e:
                                if "Only one usage" in str(e) or "Address already in use" in str(e):
                                    logger.debug(f"⚠️ Port {port} is in use, trying next...")
                                    last_error = e
                                    continue
                                else:
                                    # Different error, re-raise
                                    raise e
                        
                        if not creds:
                            logger.error(f"❌ All ports failed. Last error: {last_error}")
                            return None
                        
                        logger.info("✅ OAuth authorization completed successfully")
                        
                    except Exception as e:
                        logger.error(f"❌ OAuth flow failed: {e}")
                        return None
                
                # Save the credentials for the next run
                if creds:
                    try:
                        with open(self.oauth_token_file, 'wb') as token:
                            pickle.dump(creds, token)
                        logger.info(f"💾 OAuth tokens saved to {self.oauth_token_file}")
                    except Exception as e:
                        logger.warning(f"⚠️ Failed to save OAuth tokens: {e}")
            
            if creds and creds.valid:
                logger.info("✅ Using valid OAuth credentials")
                return creds
            else:
                logger.error("❌ OAuth credentials are invalid")
                return None
                
        except Exception as e:
            logger.error(f"❌ Error getting OAuth credentials: {e}")
            return None

    async def _build_oauth_client_config(self):
        """
        Build OAuth client configuration from existing client_id and Secret Manager client_secret.
        
        Returns:
            Dict containing OAuth client configuration or None if failed
        """
        try:
            # Get the full config to access google_cloud.oauth section
            from ai_assistant.utils.framework_config import get_config
            full_config = get_config()
            
            # Get client_id from google_cloud.oauth section
            google_cloud_config = full_config.get('google_cloud', {})
            oauth_config = google_cloud_config.get('oauth', {})
            client_id = oauth_config.get('client_id')
            
            if not client_id:
                logger.error("❌ client_id not found in google_cloud.oauth configuration")
                return None
            
            # Get client_secret from Secret Manager
            if not self.oauth_client_secret_name or not self.project_id:
                logger.error("❌ oauth_client_secret_name or project_id not configured")
                return None
            
            try:
                from google.cloud import secretmanager
                client = secretmanager.SecretManagerServiceClient()
                name = f"projects/{self.project_id}/secrets/{self.oauth_client_secret_name}/versions/latest"
                
                logger.info(f"🔐 Getting OAuth client secret from Secret Manager: {self.project_id}/{self.oauth_client_secret_name}")
                response = await asyncio.to_thread(client.access_secret_version, request={"name": name})
                client_secret = response.payload.data.decode("UTF-8").strip()
                
                logger.info("✅ OAuth client secret retrieved from Secret Manager")
                
            except Exception as e:
                logger.error(f"❌ Failed to get OAuth client secret from Secret Manager: {e}")
                return None
            
            # Build OAuth client configuration with all possible redirect ports
            redirect_uris = []
            for port in self.oauth_redirect_ports:
                redirect_uris.extend([f"http://localhost:{port}", f"http://localhost:{port}/"])
            
            client_config = {
                "installed": {
                    "client_id": client_id,
                    "client_secret": client_secret,
                    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
                    "token_uri": "https://oauth2.googleapis.com/token",
                    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
                    "redirect_uris": redirect_uris
                }
            }
            
            logger.info("✅ OAuth client configuration built successfully")
            return client_config
            
        except Exception as e:
            logger.error(f"❌ Error building OAuth client configuration: {e}")
            return None

    async def _get_credentials(self):
        """
        Get credentials from OAuth or service account.
        
        Returns:
            Credentials or None if failed
        """
        try:
            # Choose authentication method based on configuration
            if self.oauth_enabled:
                logger.info("🔐 Using OAuth authentication")
                return await self._get_oauth_credentials()
            else:
                logger.info("🔐 Using Service Account authentication")
                return await self._get_service_account_credentials()
                
        except Exception as e:
            logger.error(f"❌ Error getting credentials: {e}")
            return None

    async def _get_service_account_credentials(self):
        """
        Get service account credentials from secret manager or file.
        
        Returns:
            Service account credentials or None if failed
        """
        try:
            # Try to get credentials from secret manager first
            if self.secret_name and self.project_id: # Ensure both are present
                logger.info(f"🔐 Attempting to get credentials from Secret Manager: {self.project_id}/{self.secret_name}")
                try:
                    from google.cloud import secretmanager
                    client = secretmanager.SecretManagerServiceClient()
                    name = f"projects/{self.project_id}/secrets/{self.secret_name}/versions/latest"
                    # Wrap the potentially blocking network call
                    response = await asyncio.to_thread(client.access_secret_version, request={"name": name})
                    service_account_info = json.loads(response.payload.data.decode("UTF-8"))
                    
                    # Wrap credential creation as well for consistency
                    credentials = await asyncio.to_thread(
                        service_account.Credentials.from_service_account_info,
                        service_account_info, scopes=self.scopes
                    )
                    logger.info("✅ Using credentials from Secret Manager")
                    return credentials
                except Exception as e:
                    logger.error(f"❌ Failed to get credentials from Secret Manager: {e}")
                    # Continue to fallback method
            
            # Fallback to service account file
            service_account_file = os.getenv('GOOGLE_APPLICATION_CREDENTIALS')
            logger.info(f"🔐 Checking GOOGLE_APPLICATION_CREDENTIALS: {service_account_file}")
            
            # Use asyncio.to_thread for file system check for strict async compliance
            if service_account_file and await asyncio.to_thread(os.path.exists, service_account_file):
                try:
                    # Wrap the credential loading in asyncio.to_thread
                    credentials = await asyncio.to_thread(
                        service_account.Credentials.from_service_account_file,
                        service_account_file, scopes=self.scopes
                    )
                    logger.info("✅ Using credentials from service account file")
                    return credentials
                except Exception as e:
                    logger.error(f"❌ Failed to load service account file: {e}")
            elif service_account_file:
                logger.error(f"❌ Service account file not found: {service_account_file}")
            else:
                logger.error("❌ GOOGLE_APPLICATION_CREDENTIALS environment variable not set")
            
            logger.error("❌ No service account credentials found (neither Secret Manager config nor GOOGLE_APPLICATION_CREDENTIALS)")
            return None
            
        except Exception as e:
            logger.error(f"❌ Error getting service account credentials: {str(e)}")
            return None
    
    async def create_document(self, title: str, content: str, folder_id: Optional[str] = None, header_email: Optional[str] = None) -> Dict[str, Any]:
        """
        Create a Google Doc with content.
        
        Args:
            title: The title of the document
            content: The content to add to the document
            folder_id: Optional folder ID to create the document in
            header_email: Optional user email for permissions
            
        Returns:
            Dict containing document info or error
        """
        max_retries = 3
        retry_delay = 1 
        
        for attempt in range(max_retries):
            try:
                if not self.authenticated:
                    # Attempt initialization only if not authenticated
                    init_success = await self.initialize()
                    if not init_success:
                        logger.error(f"❌ Initialization failed, cannot create document (attempt {attempt + 1})")
                        if attempt < max_retries - 1:
                            await asyncio.sleep(retry_delay)
                            retry_delay *= 2
                            continue
                        return {"error": "Failed to authenticate Google Doc Tool after retries"}
                
                self._last_activity = time.time()
                
                logger.info(f"📄 Creating Google Doc: {title} (attempt {attempt + 1}/{max_retries})")
                
                # Step 1: Create empty Google Doc using Drive API
                file_metadata = {
                    'name': title,
                    'mimeType': 'application/vnd.google-apps.document'
                }
                
                if folder_id:
                    file_metadata['parents'] = [folder_id]
                
                # Create empty document
                file = await asyncio.to_thread(
                    self.drive_service.files().create(
                        body=file_metadata, 
                        fields='id, name, webViewLink'
                    ).execute
                )
                
                doc_id = file.get('id')
                doc_name = file.get('name')
                doc_link = file.get('webViewLink')
                
                logger.info(f"✅ Created empty Google Doc: {doc_name} (ID: {doc_id})")
                
                # Step 2: Add content to the document using Docs API (if content provided)
                if content and content.strip():
                    logger.info(f"📝 Adding content to Google Doc: {doc_name}")
                    await self._add_content_to_doc(doc_id, content)
                    logger.info(f"✅ Content added to Google Doc: {doc_name}")
                
                # Add permissions if header_email is provided
                if header_email:
                    await self._add_user_permissions(doc_id, header_email)
                
                return {
                    "id": doc_id,
                    "name": doc_name,
                    "webViewLink": doc_link,
                    "type": "google_doc",
                    "content_length": len(content) if content else 0,
                    "shared_with": header_email if header_email else None
                }
            
            except HttpError as error:
                error_msg = f"HTTP error creating Google Doc (attempt {attempt + 1}): {error.resp.status} - {error.content.decode()}"
                logger.error(f"❌ {error_msg}")
                if error.resp.status in [403, 500, 503] and attempt < max_retries - 1: # Retry for forbidden, internal server error, service unavailable
                    await asyncio.sleep(retry_delay)
                    retry_delay *= 2
                    continue
                return {"error": error_msg}
            except Exception as e:
                error_msg = f"Unexpected error creating Google Doc (attempt {attempt + 1}): {str(e)}"
                logger.error(f"❌ {error_msg}")
                if attempt < max_retries - 1:
                    await asyncio.sleep(retry_delay)
                    retry_delay *= 2
                    continue
                return {"error": error_msg}
        return {"error": "Failed to create Google Doc after multiple retries."}

    # If you *still* need to add content later, or want to do richer text, keep this:
    async def _add_content_to_doc(self, doc_id: str, content: str):
        """
        Add content to a Google Doc. For empty docs, insert at index 1.
        """
        try:
            # For newly created empty Google Docs, insert content at index 1
            # Google Docs use 1-based indexing and empty docs start at index 1
            requests = [
                {
                    'insertText': {
                        'location': {
                            'index': 1
                        },
                        'text': content
                    }
                }
            ]
            
            await asyncio.to_thread(
                self.docs_service.documents().batchUpdate(
                    documentId=doc_id,
                    body={'requests': requests}
                ).execute
            )
            
            logger.info(f"✅ Added content to document {doc_id}")
            
        except HttpError as error:
            logger.error(f"❌ HTTP Error adding content to document {doc_id}: {error.resp.status} - {error.content.decode()}")
            raise
        except Exception as e:
            logger.error(f"❌ Error adding content to document {doc_id}: {str(e)}")
            raise
            
    async def _add_user_permissions(self, file_id: str, user_email: str, role: str = 'writer', send_notification: bool = False):
        """
        Add user permissions to a Google Doc.
        
        Args:
            file_id: The Google Drive file ID
            user_email: Email address to grant permissions to
            role: Permission role ('reader', 'writer', 'owner')
            send_notification: Whether to send email notification (False recommended for service accounts)
        """
        try:
            permission = {
                'type': 'user',
                'role': role,
                'emailAddress': user_email
            }
            
            # Wrap synchronous execute call in asyncio.to_thread for non-blocking operation
            await asyncio.to_thread(
                self.drive_service.permissions().create(
                    fileId=file_id,
                    body=permission,
                    fields='id',
                    sendNotificationEmails=send_notification  # Explicit control over notifications
                ).execute
            )
            
            logger.info(f"✅ Added {role} permissions for {user_email} to document {file_id} (notification: {send_notification})")
            
        except HttpError as error:
            logger.warning(f"⚠️ Failed to add permissions for {user_email} to {file_id}: {error.resp.status} - {error.content.decode()}")
            # Don't re-raise - permissions failure shouldn't break document creation
        except Exception as e:
            logger.warning(f"⚠️ Unexpected error adding permissions for {user_email} to {file_id}: {str(e)}")
            
    async def create_document_from_data(self, title: str, data: Dict[str, Any], folder_id: Optional[str] = None, header_email: Optional[str] = None) -> Dict[str, Any]:
        """
        Create a Google Doc from structured data.
        """
        try:
            content = self._format_data_for_doc(data)
            return await self.create_document(title, content, folder_id, header_email)
            
        except Exception as e:
            error_msg = f"Error creating document from data: {str(e)}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
    
    def _format_data_for_doc(self, data: Dict[str, Any]) -> str:
        """
        Format structured data into readable document content.
        Enhanced to handle nested structures better and provide cleaner formatting.
        """
        content_parts = []
        
        def format_nested_item(item, indent_level=0):
            """Recursively format nested items with proper indentation"""
            indent = "  " * indent_level
            
            if isinstance(item, dict):
                for key, value in item.items():
                    if isinstance(value, (list, dict)):
                        content_parts.append(f"{indent}{key}:")
                        format_nested_item(value, indent_level + 1)
                    else:
                        content_parts.append(f"{indent}{key}: {value}")
            elif isinstance(item, list):
                for list_item in item:
                    if isinstance(list_item, (dict, list)):
                        content_parts.append(f"{indent}-")
                        format_nested_item(list_item, indent_level + 1)
                    else:
                        content_parts.append(f"{indent}- {list_item}")
            else:
                content_parts.append(f"{indent}{item}")
        
        if isinstance(data, dict):
            # Add a document title if there's a 'title' or 'name' field
            title_field = data.get('title') or data.get('name') or data.get('subject')
            if title_field:
                content_parts.append(f"Document: {title_field}")
                content_parts.append("=" * (len(f"Document: {title_field}")))
                content_parts.append("")
            
            format_nested_item(data, 0)
        else:
            content_parts.append(str(data))
        
        return "\n".join(content_parts)
    
    async def cleanup(self):
        """Clean up resources."""
        try:
            # httplib2.Http has a close method, but it's not always critical to call.
            # googleapiclient services don't generally have a `close()` method that needs calling.
            # The underlying httplib2 client is what manages connections.
            if self._http_client:
                # httplib2.Http does have a .close() method to close connections
                self._http_client.close()
                self._http_client = None # Clear reference
            self.drive_service = None
            self.docs_service = None
            self.authenticated = False
            logger.info("✅ Google Doc Tool cleaned up")
        except Exception as e:
            logger.error(f"❌ Error cleaning up Google Doc Tool: {str(e)}")


async def create_google_doc_tool() -> Optional[GoogleDocTool]:
    """
    Create and initialize a Google Doc tool.
    
    Returns:
        GoogleDocTool instance or None if failed
    """
    try:
        from ai_assistant.utils.framework_config import get_config, initialize_config
        
        # Try to get config, initialize if not available
        try:
            config = get_config()
        except Exception:
            # Configuration not initialized, initialize it now
            logger.info("🔧 Initializing framework configuration...")
            config = initialize_config()
        
        google_drive_config = config.get('google_drive', {})
        
        if not google_drive_config.get('enabled', False):
            logger.info("📝 Google Drive is disabled in configuration")
            return None
        
        logger.info(f"🔧 Creating Google Doc tool with config: project_id={google_drive_config.get('project_id')}, secret_name={google_drive_config.get('secret_name')}")
        logger.info(f"🔧 Scopes configured: {google_drive_config.get('scopes', DEFAULT_SCOPES)}")
        
        tool = GoogleDocTool(google_drive_config)
        
        success = await tool.initialize()
        
        if success:
            logger.info("✅ Google Doc Tool created successfully")
            return tool
        else:
            logger.error("❌ Failed to initialize Google Doc Tool")
            return None
            
    except Exception as e:
        logger.error(f"❌ Error creating Google Doc Tool: {str(e)}")
        return None 