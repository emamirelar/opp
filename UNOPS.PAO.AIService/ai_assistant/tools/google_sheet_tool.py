"""
Google Sheets Creation Tool

This module provides functionality to create Google Sheets with data
using the Google Drive API and Google Sheets API.
"""

import os
import logging
from typing import Optional, Dict, Any, List, Union
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
import json

logger = logging.getLogger(__name__)

class GoogleSheetTool:
    """
    Tool for creating and managing Google Sheets with data.
    """
    
    def __init__(self, config: Dict[str, Any]):
        """
        Initialize the Google Sheet tool.
        
        Args:
            config: Configuration dictionary containing authentication details
        """
        self.config = config
        self.project_id = config.get('project_id')
        self.secret_name = config.get('secret_name')
        self.service_account_email = config.get('service_account_email')
        self.scopes = config.get('scopes', ['https://www.googleapis.com/auth/drive', 'https://www.googleapis.com/auth/spreadsheets'])
        self.drive_service = None
        self.sheets_service = None
        self.authenticated = False
        
        logger.info(f"🔧 Google Sheet Tool initialized for project: {self.project_id}")
    
    async def initialize(self) -> bool:
        """
        Initialize and authenticate the Google Sheet tool.
        
        Returns:
            bool: True if authentication successful, False otherwise
        """
        try:
            logger.info("🔐 Authenticating Google Sheet Tool...")
            
            # Get credentials from secret manager or service account file
            credentials = await self._get_credentials()
            
            if not credentials:
                logger.error("❌ Failed to get credentials for Google Sheet Tool")
                return False
            
            # Build services
            self.drive_service = build('drive', 'v3', credentials=credentials)
            self.sheets_service = build('sheets', 'v4', credentials=credentials)
            self.authenticated = True
            
            logger.info("✅ Google Sheet Tool authenticated successfully")
            return True
            
        except Exception as e:
            logger.error(f"❌ Failed to authenticate Google Sheet Tool: {str(e)}")
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
    
    async def create_spreadsheet(self, title: str, data: Optional[Union[List[List], Dict[str, Any]]] = None, folder_id: Optional[str] = None, header_email: Optional[str] = None) -> Dict[str, Any]:
        """
        Create a Google Sheet with data.
        
        Args:
            title: The title of the spreadsheet
            data: Data to add to the spreadsheet (can be 2D list or dict)
            folder_id: Optional folder ID to create the sheet in
            header_email: Optional user email for permissions
            
        Returns:
            Dict containing spreadsheet info or error
        """
        try:
            if not self.authenticated:
                await self.initialize()
            
            if not self.authenticated:
                return {"error": "Failed to authenticate Google Sheet Tool"}
            
            logger.info(f"📊 Creating Google Sheet: {title}")
            
            # Create the spreadsheet
            file_metadata = {
                'name': title,
                'mimeType': 'application/vnd.google-apps.spreadsheet'
            }
            
            if folder_id:
                file_metadata['parents'] = [folder_id]
            
            # Create the file
            file = self.drive_service.files().create(
                body=file_metadata, 
                fields='id, name, webViewLink'
            ).execute()
            
            sheet_id = file.get('id')
            sheet_name = file.get('name')
            sheet_link = file.get('webViewLink')
            
            logger.info(f"✅ Created Google Sheet: {sheet_name} (ID: {sheet_id})")
            
            # Add permissions if header_email is provided
            if header_email:
                await self._add_user_permissions(sheet_id, header_email)
            
            # Add data to the spreadsheet
            if data:
                await self._add_data_to_sheet(sheet_id, data)
            
            return {
                "id": sheet_id,
                "name": sheet_name,
                "webViewLink": sheet_link,
                "type": "google_sheet",
                "data_rows": len(data) if data else 0,
                "shared_with": header_email if header_email else None
            }
            
        except HttpError as error:
            error_msg = f"HTTP error creating Google Sheet: {error}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
        except Exception as e:
            error_msg = f"Unexpected error creating Google Sheet: {str(e)}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
    
    async def _add_data_to_sheet(self, sheet_id: str, data: Union[List[List], Dict[str, Any]]):
        """
        Add data to a Google Sheet.
        
        Args:
            sheet_id: The spreadsheet ID
            data: The data to add (2D list or dict)
        """
        try:
            # Convert data to 2D list format
            if isinstance(data, dict):
                sheet_data = self._convert_dict_to_sheet_data(data)
            elif isinstance(data, list):
                sheet_data = data
            else:
                sheet_data = [[str(data)]]
            
            # Prepare the request
            body = {
                'values': sheet_data
            }
            
            # Execute the request
            self.sheets_service.spreadsheets().values().update(
                spreadsheetId=sheet_id,
                range='A1',
                valueInputOption='RAW',
                body=body
            ).execute()
            
            logger.info(f"✅ Added {len(sheet_data)} rows to sheet {sheet_id}")
            
        except Exception as e:
            logger.error(f"❌ Error adding data to sheet: {str(e)}")
            raise
    
    async def _add_user_permissions(self, file_id: str, user_email: str):
        """
        Add user permissions to a Google Sheet.
        
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
            
            logger.info(f"✅ Added write permissions for {user_email} to sheet {file_id}")
            
        except Exception as e:
            logger.error(f"❌ Error adding permissions for {user_email}: {str(e)}")
            # Don't raise the exception - permissions failure shouldn't break the sheet creation
    
    def _convert_dict_to_sheet_data(self, data: Dict[str, Any]) -> List[List]:
        """
        Convert dictionary data to 2D list format for Google Sheets.
        
        Args:
            data: Dictionary data to convert
            
        Returns:
            2D list suitable for Google Sheets
        """
        sheet_data = []
        
        # Add header row
        headers = list(data.keys())
        sheet_data.append(headers)
        
        # Add data row
        values = []
        for header in headers:
            value = data[header]
            if isinstance(value, (dict, list)):
                values.append(json.dumps(value))
            else:
                values.append(str(value))
        sheet_data.append(values)
        
        return sheet_data
    
    async def create_spreadsheet_from_list_data(self, title: str, data: List[Dict[str, Any]], folder_id: Optional[str] = None, header_email: Optional[str] = None) -> Dict[str, Any]:
        """
        Create a Google Sheet from a list of dictionaries (like API results).
        
        Args:
            title: The title of the spreadsheet
            data: List of dictionaries to convert to sheet
            folder_id: Optional folder ID
            header_email: Optional user email for permissions
            
        Returns:
            Dict containing spreadsheet info or error
        """
        try:
            if not data:
                return await self.create_spreadsheet(title, [["No data available"]], folder_id, header_email)
            
            # Convert list of dicts to 2D list
            sheet_data = self._convert_list_of_dicts_to_sheet_data(data)
            
            # Create the spreadsheet
            return await self.create_spreadsheet(title, sheet_data, folder_id, header_email)
            
        except Exception as e:
            error_msg = f"Error creating spreadsheet from list data: {str(e)}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
    
    def _convert_list_of_dicts_to_sheet_data(self, data: List[Dict[str, Any]]) -> List[List]:
        """
        Convert a list of dictionaries to 2D list format for Google Sheets.
        
        Args:
            data: List of dictionaries to convert
            
        Returns:
            2D list suitable for Google Sheets
        """
        if not data:
            return [["No data available"]]
        
        # Get all unique keys from all dictionaries
        all_keys = set()
        for item in data:
            all_keys.update(item.keys())
        
        # Sort keys for consistent ordering
        headers = sorted(list(all_keys))
        sheet_data = [headers]
        
        # Add data rows
        for item in data:
            row = []
            for header in headers:
                value = item.get(header, "")
                if isinstance(value, (dict, list)):
                    row.append(json.dumps(value))
                else:
                    row.append(str(value))
            sheet_data.append(row)
        
        return sheet_data
    
    async def create_spreadsheet_with_headers(self, title: str, headers: List[str], data_rows: List[List], folder_id: Optional[str] = None, header_email: Optional[str] = None) -> Dict[str, Any]:
        """
        Create a Google Sheet with custom headers and data rows.
        
        Args:
            title: The title of the spreadsheet
            headers: List of column headers
            data_rows: List of data rows (each row is a list of values)
            folder_id: Optional folder ID
            header_email: Optional user email for permissions
            
        Returns:
            Dict containing spreadsheet info or error
        """
        try:
            # Combine headers and data
            sheet_data = [headers] + data_rows
            
            # Create the spreadsheet
            return await self.create_spreadsheet(title, sheet_data, folder_id, header_email)
            
        except Exception as e:
            error_msg = f"Error creating spreadsheet with headers: {str(e)}"
            logger.error(f"❌ {error_msg}")
            return {"error": error_msg}
    
    async def cleanup(self):
        """Clean up resources."""
        try:
            if self.drive_service:
                self.drive_service.close()
            if self.sheets_service:
                self.sheets_service.close()
            logger.info("✅ Google Sheet Tool cleaned up")
        except Exception as e:
            logger.error(f"❌ Error cleaning up Google Sheet Tool: {str(e)}")


async def create_google_sheet_tool() -> Optional[GoogleSheetTool]:
    """
    Create and initialize a Google Sheet tool.
    
    Returns:
        GoogleSheetTool instance or None if failed
    """
    try:
        from ai_assistant.utils.framework_config import get_config
        
        config = get_config()
        google_drive_config = config.get('google_drive', {})
        
        if not google_drive_config.get('enabled', False):
            logger.info("📝 Google Drive is disabled in configuration")
            return None
        
        # Create the tool
        tool = GoogleSheetTool(google_drive_config)
        
        # Initialize it
        success = await tool.initialize()
        
        if success:
            logger.info("✅ Google Sheet Tool created successfully")
            return tool
        else:
            logger.error("❌ Failed to initialize Google Sheet Tool")
            return None
            
    except Exception as e:
        logger.error(f"❌ Error creating Google Sheet Tool: {str(e)}")
        return None 