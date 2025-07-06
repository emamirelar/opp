"""
Google Doc Creation Tool

This module provides functionality to create Google Docs with content
using the Google Drive API and Google Docs API.
"""

import os
import logging
from typing import Optional, Dict, Any, List
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from googleapiclient.http import MediaIoBaseUpload
import io
import json

logger = logging.getLogger(__name__)

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
        self.secret_name = config.get('secret_name')
        self.service_account_email = config.get('service_account_email')
        self.scopes = config.get('scopes', ['https://www.googleapis.com/auth/drive', 'https://www.googleapis.com/auth/documents'])
        self.drive_service = None
        self.docs_service = None
        self.authenticated = False
        
        logger.info(f"🔧 Google Doc Tool initialized for project: {self.project_id}")
    
    async def initialize(self) -> bool:
        """
        Initialize and authenticate the Google Doc tool.
        
        Returns:
            bool: True if authentication successful, False otherwise
        """
        try:
            logger.info("🔐 Authenticating Google Doc Tool...")
            
            # Get credentials from secret manager or service account file
            credentials = await self._get_credentials()
            
            if not credentials:
                logger.error("❌ Failed to get credentials for Google Doc Tool")
                return False
            
            # Build services
            self.drive_service = build('drive', 'v3', credentials=credentials)
            self.docs_service = build('docs', 'v1', credentials=credentials)
            self.authenticated = True
            
            logger.info("✅ Google Doc Tool authenticated successfully")
            return True
            
        except Exception as e:
            logger.error(f"❌ Failed to authenticate Google Doc Tool: {str(e)}")
            return False
    
    async def _get_credentials(self):
        """
        Get credentials from secret manager or service account file.
        
        Returns:
            Service account credentials or None if failed
        """
        try:
            # Try to get credentials from secret manager first
            if self.secret_name:
                from google.cloud import secretmanager
                client = secretmanager.SecretManagerServiceClient()
                name = f"projects/{self.project_id}/secrets/{self.secret_name}/versions/latest"
                response = client.access_secret_version(request={"name": name})
                service_account_info = json.loads(response.payload.data.decode("UTF-8"))
                
                credentials = service_account.Credentials.from_service_account_info(
                    service_account_info, scopes=self.scopes
                )
                logger.info("✅ Using credentials from Secret Manager")
                return credentials
            
            # Fallback to service account file
            service_account_file = os.getenv('GOOGLE_APPLICATION_CREDENTIALS')
            if service_account_file and os.path.exists(service_account_file):
                credentials = service_account.Credentials.from_service_account_file(
                    service_account_file, scopes=self.scopes
                )
                logger.info("✅ Using credentials from service account file")
                return credentials
            
            logger.error("❌ No credentials found")
            return None
            
        except Exception as e:
            logger.error(f"❌ Error getting credentials: {str(e)}")
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
        try:
            if not self.authenticated:
                await self.initialize()
            
            if not self.authenticated:
                return {"error": "Failed to authenticate Google Doc Tool"}
            
            logger.info(f"📄 Creating Google Doc: {title}")
            
            # Create the document
            file_metadata = {
                'name': title,
                'mimeType': 'application/vnd.google-apps.document'
            }
            
            if folder_id:
                file_metadata['parents'] = [folder_id]
            
            # Create the file
            file = self.drive_service.files().create(
                body=file_metadata, 
                fields='id, name, webViewLink'
            ).execute()
            
            doc_id = file.get('id')
            doc_name = file.get('name')
            doc_link = file.get('webViewLink')
            
            logger.info(f"✅ Created Google Doc: {doc_name} (ID: {doc_id})")
            
            # Add permissions if header_email is provided
            if header_email:
                await self._add_user_permissions(doc_id, header_email)
            
            # Add content to the document
            if content:
                await self._add_content_to_doc(doc_id, content)
            
            return {
                "id": doc_id,
                "name": doc_name,
                "webViewLink": doc_link,
                "type": "google_doc",
                "content_length": len(content) if content else 0,
                "shared_with": header_email if header_email else None
            }
            
        except HttpError as error:
            error_msg = f"HTTP error creating Google Doc: {error}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
        except Exception as e:
            error_msg = f"Unexpected error creating Google Doc: {str(e)}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
    
    async def _add_content_to_doc(self, doc_id: str, content: str):
        """
        Add content to a Google Doc.
        
        Args:
            doc_id: The document ID
            content: The content to add
        """
        try:
            # Prepare the content for Google Docs API
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
            
            # Execute the request
            self.docs_service.documents().batchUpdate(
                documentId=doc_id,
                body={'requests': requests}
            ).execute()
            
            logger.info(f"✅ Added content to document {doc_id}")
            
        except Exception as e:
            logger.error(f"❌ Error adding content to document: {str(e)}")
            raise
    
    async def _add_user_permissions(self, file_id: str, user_email: str):
        """
        Add user permissions to a Google Doc.
        
        Args:
            file_id: The file ID to add permissions to
            user_email: The email address to grant permissions to
        """
        try:
            permission = {
                'type': 'user',
                'role': 'writer',
                'emailAddress': user_email
            }
            
            self.drive_service.permissions().create(
                fileId=file_id,
                body=permission,
                fields='id'
            ).execute()
            
            logger.info(f"✅ Added write permissions for {user_email} to document {file_id}")
            
        except Exception as e:
            logger.error(f"❌ Error adding permissions for {user_email}: {str(e)}")
            # Don't raise the exception - permissions failure shouldn't break the document creation
    
    async def create_document_from_data(self, title: str, data: Dict[str, Any], folder_id: Optional[str] = None, header_email: Optional[str] = None) -> Dict[str, Any]:
        """
        Create a Google Doc from structured data.
        
        Args:
            title: The title of the document
            data: Structured data to format into the document
            folder_id: Optional folder ID
            header_email: Optional user email for permissions
            
        Returns:
            Dict containing document info or error
        """
        try:
            # Format the data into readable content
            content = self._format_data_for_doc(data)
            
            # Create the document
            return await self.create_document(title, content, folder_id, header_email)
            
        except Exception as e:
            error_msg = f"Error creating document from data: {str(e)}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
    
    def _format_data_for_doc(self, data: Dict[str, Any]) -> str:
        """
        Format structured data into readable document content.
        
        Args:
            data: The data to format
            
        Returns:
            Formatted content string
        """
        content_parts = []
        
        if isinstance(data, dict):
            for key, value in data.items():
                if isinstance(value, list):
                    content_parts.append(f"{key}:")
                    for item in value:
                        if isinstance(item, dict):
                            content_parts.append(f"  - {json.dumps(item, indent=2)}")
                        else:
                            content_parts.append(f"  - {item}")
                    content_parts.append("")
                elif isinstance(value, dict):
                    content_parts.append(f"{key}:")
                    content_parts.append(json.dumps(value, indent=2))
                    content_parts.append("")
                else:
                    content_parts.append(f"{key}: {value}")
                    content_parts.append("")
        else:
            content_parts.append(str(data))
        
        return "\n".join(content_parts)
    
    async def cleanup(self):
        """Clean up resources."""
        try:
            if self.drive_service:
                self.drive_service.close()
            if self.docs_service:
                self.docs_service.close()
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
        from framework_config import get_config
        
        config = get_config()
        google_drive_config = config.get('google_drive', {})
        
        if not google_drive_config.get('enabled', False):
            logger.info("📝 Google Drive is disabled in configuration")
            return None
        
        # Create the tool
        tool = GoogleDocTool(google_drive_config)
        
        # Initialize it
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