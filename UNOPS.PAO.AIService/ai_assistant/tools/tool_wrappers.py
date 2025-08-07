"""
Tool Wrapper Classes

This module contains wrapper classes for Google Sheets and Docs tools
moved from agent.py for better organization.
"""

import json
import logging
from typing import List, Dict, Any
from google.adk.tools.base_tool import BaseTool
from google.adk.tools.tool_context import ToolContext
from ai_assistant.utils.framework_config import get_config

# Import Google tools
from .google_sheet_tool import create_google_sheet_tool
from .google_doc_tool import create_google_doc_tool

class GoogleSheetToolWrapper(BaseTool):
    """Wrapper for Google Sheet operations with session state access"""
    
    def __init__(self):
        super().__init__(
            name="google_sheet_tool",
            description="Create Google Sheets with data and manage permissions"
        )
        self.sheet_tool = None
        self._initialize_tool()
    
    def _initialize_tool(self):
        """Initialize the Google Sheet tool"""
        try:
            self.sheet_tool = None # Lazily initialized
            logging.info("✅ Google Sheet tool wrapper initialized (will initialize on first use)")
        except Exception as e:
            logging.error(f"❌ Failed to initialize Google Sheet tool wrapper: {e}")
    
    async def _get_sheet_tool(self):
        """Get or create the Google Sheet tool instance"""
        if self.sheet_tool is None:
            try:
                config = get_config() # Ensure config is available
                google_drive_config = config.get('google_drive', {})
                if not google_drive_config.get('enabled', False):
                    logging.warning("⚠️ Google Drive not enabled in config, Sheets tool will not initialize.")
                    return None
                
                logging.info("🔧 Lazily initializing Google Sheet tool...")
                self.sheet_tool = await create_google_sheet_tool()
                
                if self.sheet_tool is None:
                    logging.error("❌ create_google_sheet_tool returned None during lazy init")
                    return None
                
                logging.info("🔐 Authenticating Google Sheet tool...")
                auth_success = await self.sheet_tool.initialize()
                
                if not auth_success:
                    logging.error("❌ Google Sheet tool authentication failed during lazy init")
                    self.sheet_tool = None
                    return None
                
                logging.info("✅ Google Sheet tool initialized and authenticated successfully")
                
            except Exception as e:
                logging.error(f"❌ Failed to initialize Google Sheet tool during lazy init: {str(e)}")
                self.sheet_tool = None
                return None
        
        return self.sheet_tool
    
    async def create_spreadsheet_from_list(self, tool_context: ToolContext, title: str, data: List[Dict[str, Any]], folder_id: str = "") -> str:
        """Create a Google Sheet from list data with session state access"""
        try:
            user_email = tool_context.state.get('user_email') # Access user_email directly from tool_context.state
            if not user_email:
                logging.warning("⚠️ No user_email found in tool_context state - permissions may not be granted for Sheets.")
            
            sheet_tool_instance = await self._get_sheet_tool()
            if not sheet_tool_instance:
                return json.dumps({"error": "Google Sheet tool not available or failed to initialize."})
            
            result = await sheet_tool_instance.create_spreadsheet_from_list_data(
                title=title,
                data=data,
                folder_id=folder_id if folder_id else None,
                header_email=user_email
            )
            logging.info(f"✅ Created Google Sheet: {title}")
            return json.dumps(result)
            
        except Exception as e:
            logging.error(f"❌ Error creating Google Sheet from list data: {e}", exc_info=True)
            return json.dumps({"error": f"Failed to create Google Sheet: {str(e)}"})
    
    async def create_spreadsheet_with_headers(self, tool_context: ToolContext, title: str, headers: List[str], data: List[List[Any]], folder_id: str = "") -> str:
        """Create a Google Sheet with headers and data with session state access"""
        try:
            user_email = tool_context.state.get('user_email') # Access user_email directly from tool_context.state
            if not user_email:
                logging.warning("⚠️ No user_email found in tool_context state - permissions may not be granted for Sheets with headers.")
            
            sheet_tool_instance = await self._get_sheet_tool()
            if not sheet_tool_instance:
                return json.dumps({"error": "Google Sheet tool not available or failed to initialize."})
            
            result = await sheet_tool_instance.create_spreadsheet_with_headers(
                title=title,
                headers=headers,
                data_rows=data,
                folder_id=folder_id if folder_id else None,
                header_email=user_email
            )
            logging.info(f"✅ Created Google Sheet with headers: {title}")
            return json.dumps(result)
            
        except Exception as e:
            logging.error(f"❌ Error creating Google Sheet with headers: {e}", exc_info=True)
            return json.dumps({"error": f"Failed to create Google Sheet: {str(e)}"})


class GoogleDocToolWrapper(BaseTool):
    """Wrapper for Google Doc operations with session state access"""
    
    def __init__(self):
        super().__init__(
            name="google_doc_tool",
            description="Create Google Docs with content and manage permissions"
        )
        self.doc_tool = None
        self._initialize_tool()
    
    def _initialize_tool(self):
        """Initialize the Google Doc tool"""
        try:
            self.doc_tool = None # Lazily initialized
            logging.info("✅ Google Doc tool wrapper initialized (will initialize on first use)")
        except Exception as e:
            logging.error(f"❌ Failed to initialize Google Doc tool wrapper: {e}")
    
    async def _get_doc_tool(self):
        """Get or create the Google Doc tool instance"""
        if self.doc_tool is None:
            try:
                config = get_config() # Ensure config is available
                google_drive_config = config.get('google_drive', {})
                if not google_drive_config.get('enabled', False):
                    logging.warning("⚠️ Google Drive not enabled in config, Docs tool will not initialize.")
                    return None
                
                logging.info("🔧 Lazily initializing Google Doc tool...")
                self.doc_tool = await create_google_doc_tool()
                
                if self.doc_tool is None:
                    logging.error("❌ create_google_doc_tool returned None during lazy init")
                    return None
                
                logging.info("🔐 Authenticating Google Doc tool...")
                auth_success = await self.doc_tool.initialize()
                
                if not auth_success:
                    logging.error("❌ Google Doc tool authentication failed during lazy init")
                    self.doc_tool = None
                    return None
                
                logging.info("✅ Google Doc tool initialized and authenticated successfully")
                
            except Exception as e:
                logging.error(f"❌ Failed to initialize Google Doc tool during lazy init: {str(e)}")
                self.doc_tool = None
                return None
        
        return self.doc_tool
    
    async def create_document_from_text(self, tool_context: ToolContext, title: str, content: str, folder_id: str = "") -> str:
        """Create a Google Doc from text with session state access"""
        try:
            user_email = tool_context.state.get('user_email') # Access user_email directly from tool_context.state
            if not user_email:
                logging.warning("⚠️ No user_email found in tool_context state - permissions may not be granted for Docs.")
            
            doc_tool_instance = await self._get_doc_tool()
            if not doc_tool_instance:
                return json.dumps({"error": "Google Doc tool not available or failed to initialize."})
            
            result = await doc_tool_instance.create_document_from_data(
                title=title,
                data={"content": content},
                folder_id=folder_id if folder_id else None,
                header_email=user_email
            )
            logging.info(f"✅ Created Google Doc: {title}")
            return json.dumps(result)
            
        except Exception as e:
            logging.error(f"❌ Error creating Google Doc: {e}", exc_info=True)
            return json.dumps({"error": f"Failed to create Google Doc: {str(e)}"}) 