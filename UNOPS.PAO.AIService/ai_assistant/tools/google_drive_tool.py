#!/usr/bin/env python3
"""
Google Drive Tool for AI Agent Framework

This tool provides comprehensive Google Drive integration capabilities including:
- Authentication via service account or user credentials
- File search and discovery
- File content reading (text, documents, spreadsheets)
- Batch file operations
- Secure credential management via Google Secret Manager

The tool is designed to be extensible and reusable across different applications.
"""

import json
import logging
import os
import io
import mimetypes
from typing import Dict, List, Optional, Any, Union, Tuple
from dataclasses import dataclass, asdict
from datetime import datetime, timedelta
import asyncio
from concurrent.futures import ThreadPoolExecutor
import hashlib
import re

# Google API imports
from google.oauth2.credentials import Credentials
from google.oauth2 import service_account
from google.auth.transport.requests import Request
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from google.cloud import secretmanager

# Framework imports
from framework_config import get_config

# Configure logging
logger = logging.getLogger(__name__)

# Google Drive API scopes
SCOPES = [
    'https://www.googleapis.com/auth/drive.readonly',
    'https://www.googleapis.com/auth/drive.file',
    'https://www.googleapis.com/auth/drive.metadata.readonly'
]

# Supported file types for content extraction
SUPPORTED_MIME_TYPES = {
    'text/plain': 'text',
    'text/csv': 'text',
    'text/html': 'text',
    'text/xml': 'text',
    'application/json': 'text',
    'application/javascript': 'text',
    'application/x-javascript': 'text',
    'text/javascript': 'text',
    'application/pdf': 'pdf',
    'application/vnd.google-apps.document': 'google_doc',
    'application/vnd.google-apps.spreadsheet': 'google_sheet',
    'application/vnd.google-apps.presentation': 'google_slides',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'docx',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'xlsx',
    'application/vnd.openxmlformats-officedocument.presentationml.presentation': 'pptx',
    'application/msword': 'doc',
    'application/vnd.ms-excel': 'xls',
    'application/vnd.ms-powerpoint': 'ppt'
}

@dataclass
class DriveFile:
    """Represents a Google Drive file with metadata"""
    id: str
    name: str
    mime_type: str
    size: Optional[int] = None
    created_time: Optional[str] = None
    modified_time: Optional[str] = None
    owners: Optional[List[str]] = None
    parents: Optional[List[str]] = None
    shared: bool = False
    web_view_link: Optional[str] = None
    download_url: Optional[str] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for JSON serialization"""
        return asdict(self)
    
    @classmethod
    def from_api_response(cls, file_data: Dict[str, Any]) -> 'DriveFile':
        """Create DriveFile from Google Drive API response"""
        return cls(
            id=file_data.get('id', ''),
            name=file_data.get('name', ''),
            mime_type=file_data.get('mimeType', ''),
            size=int(file_data.get('size', 0)) if file_data.get('size') else None,
            created_time=file_data.get('createdTime'),
            modified_time=file_data.get('modifiedTime'),
            owners=[owner.get('displayName', owner.get('emailAddress', '')) 
                   for owner in file_data.get('owners', [])],
            parents=file_data.get('parents', []),
            shared=file_data.get('shared', False),
            web_view_link=file_data.get('webViewLink'),
            download_url=file_data.get('downloadUrl')
        )

@dataclass
class FileContent:
    """Represents file content with metadata"""
    file_info: DriveFile
    content: str
    content_type: str
    encoding: str = 'utf-8'
    extraction_method: str = 'direct'
    error: Optional[str] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for JSON serialization"""
        return {
            'file_info': self.file_info.to_dict(),
            'content': self.content,
            'content_type': self.content_type,
            'encoding': self.encoding,
            'extraction_method': self.extraction_method,
            'error': self.error
        }

class GoogleDriveAuthenticator:
    """Handles Google Drive authentication via multiple methods"""
    
    def __init__(self, config: Optional[Dict[str, Any]] = None):
        self.config = config or get_config().get('google_drive', {})
        # Convert dataclass to dict if needed
        if hasattr(self.config, '__dict__'):
            self.config = asdict(self.config)
        self.credentials = None
        self.service = None
        
    async def authenticate(self) -> bool:
        """Authenticate using the best available method"""
        try:
            # Try service account authentication first
            if await self._authenticate_service_account():
                logger.info("Successfully authenticated using service account")
                return True
            
            # Try user credentials from secret manager
            if await self._authenticate_from_secret_manager():
                logger.info("Successfully authenticated using secret manager credentials")
                return True
            
            # Try local credentials file
            if await self._authenticate_from_local_file():
                logger.info("Successfully authenticated using local credentials")
                return True
            
            logger.error("All authentication methods failed")
            return False
            
        except Exception as e:
            logger.error(f"Authentication failed: {str(e)}")
            return False
    
    async def _authenticate_service_account(self) -> bool:
        """Authenticate using service account credentials"""
        try:
            # Check for service account key file
            service_account_file = os.getenv('GOOGLE_APPLICATION_CREDENTIALS')
            if service_account_file and os.path.exists(service_account_file):
                self.credentials = service_account.Credentials.from_service_account_file(
                    service_account_file, scopes=SCOPES
                )
                self.service = build('drive', 'v3', credentials=self.credentials)
                return True
            
            # Check for service account key in environment variable
            service_account_key = os.getenv('GOOGLE_SERVICE_ACCOUNT_KEY')
            if service_account_key:
                service_account_info = json.loads(service_account_key)
                self.credentials = service_account.Credentials.from_service_account_info(
                    service_account_info, scopes=SCOPES
                )
                self.service = build('drive', 'v3', credentials=self.credentials)
                return True
            
            return False
            
        except Exception as e:
            logger.debug(f"Service account authentication failed: {str(e)}")
            return False
    
    async def _authenticate_from_secret_manager(self) -> bool:
        """Authenticate using credentials from Google Secret Manager"""
        try:
            if not self.config.get('project_id') or not self.config.get('secret_name'):
                return False
            
            # Get credentials from Secret Manager
            client = secretmanager.SecretManagerServiceClient()
            secret_name = f"projects/{self.config['project_id']}/secrets/{self.config['secret_name']}/versions/latest"
            
            response = client.access_secret_version(request={"name": secret_name})
            secret_data = response.payload.data.decode('UTF-8')
            
            # Try to parse as service account JSON
            try:
                service_account_info = json.loads(secret_data)
                self.credentials = service_account.Credentials.from_service_account_info(
                    service_account_info, scopes=SCOPES
                )
                self.service = build('drive', 'v3', credentials=self.credentials)
                return True
            except json.JSONDecodeError:
                # Try to parse as user credentials
                creds_data = json.loads(secret_data)
                self.credentials = Credentials.from_authorized_user_info(creds_data, SCOPES)
                
                # Refresh if necessary
                if self.credentials.expired and self.credentials.refresh_token:
                    self.credentials.refresh(Request())
                
                self.service = build('drive', 'v3', credentials=self.credentials)
                return True
            
        except Exception as e:
            logger.debug(f"Secret manager authentication failed: {str(e)}")
            return False
    
    async def _authenticate_from_local_file(self) -> bool:
        """Authenticate using local credentials file"""
        try:
            token_file = self.config.get('token_file', 'token.json')
            credentials_file = self.config.get('client_secrets_file', 'credentials.json')
            
            # Load existing token
            if os.path.exists(token_file):
                self.credentials = Credentials.from_authorized_user_file(token_file, SCOPES)
            
            # If no valid credentials, perform OAuth flow
            if not self.credentials or not self.credentials.valid:
                if self.credentials and self.credentials.expired and self.credentials.refresh_token:
                    self.credentials.refresh(Request())
                else:
                    if not os.path.exists(credentials_file):
                        return False
                    
                    flow = InstalledAppFlow.from_client_secrets_file(credentials_file, SCOPES)
                    self.credentials = flow.run_local_server(port=0)
                
                # Save credentials for future use
                with open(token_file, 'w') as token:
                    token.write(self.credentials.to_json())
            
            self.service = build('drive', 'v3', credentials=self.credentials)
            return True
            
        except Exception as e:
            logger.debug(f"Local file authentication failed: {str(e)}")
            return False
    
    def get_service(self):
        """Get the authenticated Google Drive service"""
        return self.service

class GoogleDriveTool:
    """Comprehensive Google Drive tool for AI agents"""
    
    def __init__(self, config: Optional[Dict[str, Any]] = None):
        self.config = config or get_config().get('google_drive', {})
        # Convert dataclass to dict if needed
        if hasattr(self.config, '__dict__'):
            self.config = asdict(self.config)
        self.authenticator = GoogleDriveAuthenticator(self.config)
        self.service = None
        self.authenticated = False
        self.executor = ThreadPoolExecutor(max_workers=5)
        
        # Cache for file metadata
        self._file_cache = {}
        self._cache_expiry = {}
        self._cache_ttl = timedelta(minutes=30)
    
    async def initialize(self) -> bool:
        """Initialize the Google Drive tool"""
        try:
            if await self.authenticator.authenticate():
                self.service = self.authenticator.get_service()
                self.authenticated = True
                logger.info("Google Drive tool initialized successfully")
                return True
            else:
                logger.error("Failed to authenticate with Google Drive")
                return False
        except Exception as e:
            logger.error(f"Failed to initialize Google Drive tool: {str(e)}")
            return False
    
    def _is_cache_valid(self, key: str) -> bool:
        """Check if cache entry is still valid"""
        return (key in self._cache_expiry and 
                datetime.now() < self._cache_expiry[key])
    
    def _cache_file_info(self, file_id: str, file_info: DriveFile) -> None:
        """Cache file information"""
        self._file_cache[file_id] = file_info
        self._cache_expiry[file_id] = datetime.now() + self._cache_ttl
    
    async def find_files(self, 
                        query: str = None,
                        name_contains: str = None,
                        mime_type: str = None,
                        folder_id: str = None,
                        max_results: int = None,
                        include_trashed: bool = False) -> List[DriveFile]:
        """
        Find files in Google Drive based on various criteria
        
        Args:
            query: Custom Google Drive query string
            name_contains: Search for files containing this text in name
            mime_type: Filter by MIME type
            folder_id: Search within specific folder
            max_results: Maximum number of results to return
            include_trashed: Include trashed files
            
        Returns:
            List of DriveFile objects
        """
        if not self.authenticated:
            raise RuntimeError("Google Drive tool not authenticated")
        
        try:
            # Build query
            query_parts = []
            
            if query:
                # For custom queries, pass them through as-is (they should be pre-formatted)
                query_parts.append(query)
            
            if name_contains:
                # Escape special characters in name_contains and format for name search
                escaped_name = self._escape_query(name_contains)
                query_parts.append(f"name contains '{escaped_name}'")
            
            if mime_type:
                query_parts.append(f"mimeType = '{mime_type}'")
            
            if folder_id:
                query_parts.append(f"'{folder_id}' in parents")
            
            if not include_trashed:
                query_parts.append("trashed = false")
            
            final_query = " and ".join(query_parts) if query_parts else None
            
            # Execute search
            page_size = min(max_results or self.config.get('search_page_size', 20), 100)
            results = []
            page_token = None
            
            while True:
                response = await asyncio.get_event_loop().run_in_executor(
                    self.executor,
                    lambda: self.service.files().list(
                        q=final_query,
                        pageSize=page_size,
                        pageToken=page_token,
                        fields="nextPageToken, files(id, name, mimeType, size, createdTime, modifiedTime, owners, parents, shared, webViewLink)"
                    ).execute()
                )
                
                files = response.get('files', [])
                for file_data in files:
                    drive_file = DriveFile.from_api_response(file_data)
                    results.append(drive_file)
                    self._cache_file_info(drive_file.id, drive_file)
                
                # Check if we have enough results or no more pages
                if (max_results and len(results) >= max_results) or not response.get('nextPageToken'):
                    break
                
                page_token = response.get('nextPageToken')
            
            # Limit results if specified
            if max_results:
                results = results[:max_results]
            
            logger.info(f"Found {len(results)} files matching criteria")
            return results
            
        except Exception as e:
            logger.error(f"Error finding files: {str(e)}")
            raise

    def _escape_query(self, query: str) -> str:
        """Escape special characters in Google Drive search queries"""
        if not query:
            return query
        
        # Replace single quotes with escaped single quotes
        escaped = query.replace("'", "\\'")
        
        # Return the escaped query without additional formatting
        # The caller will add the appropriate search syntax
        return escaped
    
    async def get_file_info(self, file_id: str) -> Optional[DriveFile]:
        """Get detailed information about a specific file"""
        if not self.authenticated:
            raise RuntimeError("Google Drive tool not authenticated")
        
        try:
            # Check cache first
            if self._is_cache_valid(file_id):
                return self._file_cache[file_id]
            
            # Fetch from API
            file_data = await asyncio.get_event_loop().run_in_executor(
                self.executor,
                lambda: self.service.files().get(
                    fileId=file_id,
                    fields="id, name, mimeType, size, createdTime, modifiedTime, owners, parents, shared, webViewLink"
                ).execute()
            )
            
            drive_file = DriveFile.from_api_response(file_data)
            self._cache_file_info(file_id, drive_file)
            
            return drive_file
            
        except HttpError as e:
            if e.resp.status == 404:
                logger.warning(f"File not found: {file_id}")
                return None
            logger.error(f"Error getting file info: {str(e)}")
            raise
        except Exception as e:
            logger.error(f"Error getting file info: {str(e)}")
            raise
    
    async def read_file_content(self, file_id: str) -> Optional[FileContent]:
        """Read the content of a single file"""
        if not self.authenticated:
            raise RuntimeError("Google Drive tool not authenticated")
        
        try:
            # Get file info
            file_info = await self.get_file_info(file_id)
            if not file_info:
                return None
            
            # Check if file type is supported
            if file_info.mime_type not in SUPPORTED_MIME_TYPES:
                return FileContent(
                    file_info=file_info,
                    content="",
                    content_type="unsupported",
                    error=f"Unsupported file type: {file_info.mime_type}"
                )
            
            # Check file size
            max_size = self.config.get('max_file_size', 10485760)  # 10MB default
            if file_info.size and file_info.size > max_size:
                return FileContent(
                    file_info=file_info,
                    content="",
                    content_type="error",
                    error=f"File too large: {file_info.size} bytes (max: {max_size})"
                )
            
            # Extract content based on file type
            content_type = SUPPORTED_MIME_TYPES[file_info.mime_type]
            
            if content_type == 'text':
                content = await self._read_text_file(file_id)
                return FileContent(
                    file_info=file_info,
                    content=content,
                    content_type=content_type,
                    extraction_method="direct"
                )
            
            elif content_type == 'google_doc':
                content = await self._read_google_doc(file_id)
                return FileContent(
                    file_info=file_info,
                    content=content,
                    content_type=content_type,
                    extraction_method="export"
                )
            
            elif content_type == 'google_sheet':
                content = await self._read_google_sheet(file_id)
                return FileContent(
                    file_info=file_info,
                    content=content,
                    content_type=content_type,
                    extraction_method="export"
                )
            
            elif content_type == 'pdf':
                content = await self._read_pdf_file(file_id)
                return FileContent(
                    file_info=file_info,
                    content=content,
                    content_type=content_type,
                    extraction_method="text_extraction"
                )
            
            else:
                return FileContent(
                    file_info=file_info,
                    content="",
                    content_type="unsupported",
                    error=f"Content extraction not implemented for: {content_type}"
                )
                
        except Exception as e:
            logger.error(f"Error reading file content: {str(e)}")
            return FileContent(
                file_info=file_info if 'file_info' in locals() else None,
                content="",
                content_type="error",
                error=str(e)
            )
    
    async def read_multiple_files(self, file_ids: List[str]) -> List[FileContent]:
        """Read content from multiple files concurrently"""
        if not self.authenticated:
            raise RuntimeError("Google Drive tool not authenticated")
        
        try:
            # Use asyncio.gather for concurrent execution
            tasks = [self.read_file_content(file_id) for file_id in file_ids]
            results = await asyncio.gather(*tasks, return_exceptions=True)
            
            # Handle exceptions and filter None results
            file_contents = []
            for i, result in enumerate(results):
                if isinstance(result, Exception):
                    logger.error(f"Error reading file {file_ids[i]}: {str(result)}")
                    file_contents.append(FileContent(
                        file_info=None,
                        content="",
                        content_type="error",
                        error=str(result)
                    ))
                elif result is not None:
                    file_contents.append(result)
            
            return file_contents
            
        except Exception as e:
            logger.error(f"Error reading multiple files: {str(e)}")
            raise
    
    async def _read_text_file(self, file_id: str) -> str:
        """Read content from a text file"""
        try:
            file_content = await asyncio.get_event_loop().run_in_executor(
                self.executor,
                lambda: self.service.files().get_media(fileId=file_id).execute()
            )
            
            # Try to decode as UTF-8, fall back to latin-1 if needed
            try:
                return file_content.decode('utf-8')
            except UnicodeDecodeError:
                return file_content.decode('latin-1', errors='replace')
                
        except Exception as e:
            logger.error(f"Error reading text file: {str(e)}")
            raise
    
    async def _read_google_doc(self, file_id: str) -> str:
        """Read content from a Google Doc by exporting as plain text"""
        try:
            content = await asyncio.get_event_loop().run_in_executor(
                self.executor,
                lambda: self.service.files().export(
                    fileId=file_id,
                    mimeType='text/plain'
                ).execute()
            )
            
            return content.decode('utf-8')
            
        except Exception as e:
            logger.error(f"Error reading Google Doc: {str(e)}")
            raise
    
    async def _read_google_sheet(self, file_id: str) -> str:
        """Read content from a Google Sheet by exporting as CSV"""
        try:
            content = await asyncio.get_event_loop().run_in_executor(
                self.executor,
                lambda: self.service.files().export(
                    fileId=file_id,
                    mimeType='text/csv'
                ).execute()
            )
            
            return content.decode('utf-8')
            
        except Exception as e:
            logger.error(f"Error reading Google Sheet: {str(e)}")
            raise
    
    async def _read_pdf_file(self, file_id: str) -> str:
        """Read content from a PDF file (requires additional libraries)"""
        try:
            # Note: This is a placeholder implementation
            # In a real implementation, you would use libraries like PyPDF2 or pdfplumber
            # to extract text from PDF files
            
            file_content = await asyncio.get_event_loop().run_in_executor(
                self.executor,
                lambda: self.service.files().get_media(fileId=file_id).execute()
            )
            
            # For now, return a message indicating PDF text extraction is not implemented
            return f"[PDF Content - {len(file_content)} bytes] - Text extraction not implemented"
            
        except Exception as e:
            logger.error(f"Error reading PDF file: {str(e)}")
            raise
    
    async def search_files_by_content(self, 
                                    search_term: str,
                                    file_types: List[str] = None,
                                    max_results: int = 10) -> List[Tuple[DriveFile, str]]:
        """
        Search for files containing specific content
        
        Args:
            search_term: Text to search for
            file_types: List of file types to search in
            max_results: Maximum number of results
            
        Returns:
            List of tuples (DriveFile, matching_content_snippet)
        """
        if not self.authenticated:
            raise RuntimeError("Google Drive tool not authenticated")
        
        try:
            # Build query for full-text search - escape the search term
            escaped_search_term = self._escape_query(search_term)
            query = f"fullText contains '{escaped_search_term}'"
            
            # Add file type filters if specified
            if file_types:
                mime_queries = []
                for file_type in file_types:
                    # Map common file types to MIME types
                    if file_type == 'document':
                        mime_queries.append("mimeType = 'application/vnd.google-apps.document'")
                    elif file_type == 'spreadsheet':
                        mime_queries.append("mimeType = 'application/vnd.google-apps.spreadsheet'")
                    elif file_type == 'text':
                        mime_queries.append("mimeType contains 'text/'")
                
                if mime_queries:
                    query += " and (" + " or ".join(mime_queries) + ")"
            
            # Find files
            files = await self.find_files(query=query, max_results=max_results)
            
            # Read content and extract snippets
            results = []
            for file in files:
                try:
                    content = await self.read_file_content(file.id)
                    if content and content.content:
                        # Extract snippet around search term
                        snippet = self._extract_snippet(content.content, search_term)
                        results.append((file, snippet))
                except Exception as e:
                    logger.warning(f"Could not read content from {file.name}: {str(e)}")
            
            return results
            
        except Exception as e:
            logger.error(f"Error searching files by content: {str(e)}")
            raise
    
    def _extract_snippet(self, content: str, search_term: str, context_length: int = 100) -> str:
        """Extract a snippet of text around the search term"""
        try:
            # Find the position of the search term (case-insensitive)
            lower_content = content.lower()
            lower_term = search_term.lower()
            
            pos = lower_content.find(lower_term)
            if pos == -1:
                return content[:context_length] + "..." if len(content) > context_length else content
            
            # Extract context around the term
            start = max(0, pos - context_length // 2)
            end = min(len(content), pos + len(search_term) + context_length // 2)
            
            snippet = content[start:end]
            
            # Add ellipsis if we're not at the beginning/end
            if start > 0:
                snippet = "..." + snippet
            if end < len(content):
                snippet = snippet + "..."
            
            return snippet
            
        except Exception as e:
            logger.error(f"Error extracting snippet: {str(e)}")
            return content[:context_length] + "..." if len(content) > context_length else content
    
    def get_tool_info(self) -> Dict[str, Any]:
        """Get information about the Google Drive tool"""
        return {
            "name": "Google Drive Tool",
            "version": "1.0.0",
            "description": "Comprehensive Google Drive integration for AI agents",
            "authenticated": self.authenticated,
            "supported_operations": [
                "find_files",
                "get_file_info", 
                "read_file_content",
                "read_multiple_files",
                "search_files_by_content"
            ],
            "supported_file_types": list(SUPPORTED_MIME_TYPES.keys()),
            "configuration": {
                "max_file_size": self.config.get('max_file_size', 10485760),
                "search_page_size": self.config.get('search_page_size', 20),
                "secret_name": self.config.get('secret_name', 'GoogleDriveAI'),
                "project_id": self.config.get('project_id', '')
            }
        }
    
    async def cleanup(self):
        """Clean up resources"""
        if self.executor:
            self.executor.shutdown(wait=True)
        self._file_cache.clear()
        self._cache_expiry.clear()
        logger.info("Google Drive tool cleaned up")

# =============================================================================
# TOOL FACTORY AND REGISTRATION
# =============================================================================

async def create_google_drive_tool(config: Optional[Dict[str, Any]] = None) -> GoogleDriveTool:
    """Factory function to create and initialize a Google Drive tool"""
    tool = GoogleDriveTool(config)
    if await tool.initialize():
        return tool
    else:
        raise RuntimeError("Failed to initialize Google Drive tool")

# For backward compatibility and direct usage
__all__ = [
    'GoogleDriveTool',
    'DriveFile',
    'FileContent',
    'GoogleDriveAuthenticator',
    'create_google_drive_tool',
    'SUPPORTED_MIME_TYPES',
    'SCOPES'
] 