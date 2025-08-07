"""
Google Drive Utility Functions

This module contains all Google Drive related utilities including search, 
file reading, and content operations moved from agent.py for better organization.
"""

import json
import logging
import asyncio
from typing import List, Optional, Dict, Any
from ai_assistant.utils.framework_config import get_config

# Import Google Drive tools
from .google_drive_tool import GoogleDriveTool, create_google_drive_tool

# Global variable for lazy initialization
google_drive_tool = None

async def _get_google_drive_tool():
    """Helper function for Google Drive tool (lazily initialized)"""
    global google_drive_tool
    if google_drive_tool is None:
        try:
            config = get_config()
            if not config.get('google_drive', {}).get('enabled', False):
                logging.info("Google Drive is disabled in configuration")
                return None
            
            logging.info("Initializing Google Drive tool...")
            google_drive_tool = await create_google_drive_tool()
            
            if google_drive_tool is None:
                logging.error("create_google_drive_tool returned None")
                return None
            
            logging.info("Authenticating Google Drive tool...")
            auth_success = await google_drive_tool.initialize()
            
            if not auth_success:
                logging.error("Google Drive tool authentication failed")
                google_drive_tool = None
                return None
            
            logging.info("Google Drive tool initialized and authenticated successfully")
            
        except Exception as e:
            logging.error(f"Failed to initialize Google Drive tool: {str(e)}", exc_info=True)
            google_drive_tool = None
            return None
    
    return google_drive_tool

def search_google_drive_knowledge(query: str) -> str:
    """Search Google Drive for knowledge documents with async wrapper"""
    try:
        loop = asyncio.get_event_loop()
        if loop.is_running():
            import concurrent.futures
            with concurrent.futures.ThreadPoolExecutor() as executor:
                future = executor.submit(asyncio.run, _search_google_drive_knowledge_async(query))
                return future.result()
        else:
            return asyncio.run(_search_google_drive_knowledge_async(query))
    except Exception as e:
        logging.error(f"Error in search_google_drive_knowledge wrapper: {e}")
        return json.dumps({
            "content": f"Error searching Google Drive: {str(e)}. Please try again or contact support.",
            "sources": []
        })

async def _search_google_drive_knowledge_async(query: str) -> str:
    """Async implementation of Google Drive knowledge search"""
    try:
        drive_tool = await _get_google_drive_tool()
        if not drive_tool:
            config = get_config()
            if not config.get('google_drive', {}).get('enabled', False):
                return json.dumps({
                    "content": "Google Drive search is disabled in the configuration. To enable it, contact your administrator.",
                    "sources": []
                })
            else:
                return json.dumps({
                    "content": "Google Drive search is not available due to authentication issues. Please check the logs for more details or contact your administrator.",
                    "sources": []
                })
        
        files_by_name = await drive_tool.find_files(name_contains=query, max_results=5)
        files_by_content = await drive_tool.search_files_by_content(search_term=query, max_results=5)
        
        all_files = {}
        for file in files_by_name:
            all_files[file.id] = file
        for file, snippet in files_by_content:
            all_files[file.id] = file
        
        results = list(all_files.values())[:5]
        
        if not results:
            return json.dumps({
                "content": "No relevant documents found in Google Drive. You might want to try a different search term or check if the documents exist.",
                "sources": []
            })
        
        formatted_content = "📁 **Google Drive Knowledge Results:**\n\n"
        sources = []
        
        for i, file in enumerate(results, 1):
            formatted_content += f"**Document {i}: {file.name}**\n"
            try:
                content = await drive_tool.read_file_content(file.id)
                if content and content.content:
                    formatted_content += f"*Content:*\n{content.content}\n\n"
                else:
                    formatted_content += f"*Type:* {file.mime_type}\n*Modified:* {file.modified_time}\n\n"
            except Exception as e:
                formatted_content += f"*Type:* {file.mime_type}\n*Modified:* {file.modified_time}\n*Error reading content:* {str(e)}\n\n"
            
            sources.append({
                "title": file.name,
                "url": file.web_view_link or f"https://drive.google.com/file/d/{file.id}/view",
                "description": f"Google Drive document: {file.name}"
            })
        
        return json.dumps({
            "content": formatted_content,
            "sources": sources
        })
        
    except Exception as e:
        logging.error(f"Error in Google Drive knowledge search: {str(e)}", exc_info=True)
        return json.dumps({
            "content": f"Error searching Google Drive: {str(e)}. Please check the logs for more details or contact your administrator.",
            "sources": []
        })

def search_google_drive(query: str, file_type: Optional[str] = None, max_results: int = 10) -> str:
    """Search Google Drive files with async wrapper"""
    try:
        loop = asyncio.get_event_loop()
        if loop.is_running():
            import concurrent.futures
            with concurrent.futures.ThreadPoolExecutor() as executor:
                future = executor.submit(asyncio.run, _search_google_drive_async(query, file_type, max_results))
                return future.result()
        else:
            return asyncio.run(_search_google_drive_async(query, file_type, max_results))
    except Exception as e:
        logging.error(f"Error in search_google_drive wrapper: {e}")
        return json.dumps({
            "error": f"Failed to search Google Drive: {str(e)}",
            "suggestion": "Check your Google Drive permissions and try again"
        })

async def _search_google_drive_async(query: str, file_type: Optional[str] = None, max_results: int = 10) -> str:
    """Async implementation of Google Drive file search"""
    drive_tool = await _get_google_drive_tool()
    if not drive_tool:
        return json.dumps({
            "error": "Google Drive tool is not available",
            "suggestion": "Check if Google Drive is enabled in configuration"
        })
    
    try:
        files = await drive_tool.find_files(name_contains=query, mime_type=file_type, max_results=max_results)
        return json.dumps({
            "files": [file.to_dict() for file in files],
            "count": len(files),
            "query": query
        })
    except Exception as e:
        logging.error(f"Error searching Google Drive: {e}")
        return json.dumps({
            "error": f"Failed to search Google Drive: {str(e)}",
            "suggestion": "Check your Google Drive permissions and try again"
        })

def read_google_drive_file(file_id: str, format: Optional[str] = None) -> str:
    """Read Google Drive file content with async wrapper"""
    try:
        loop = asyncio.get_event_loop()
        if loop.is_running():
            import concurrent.futures
            with concurrent.futures.ThreadPoolExecutor() as executor:
                future = executor.submit(asyncio.run, _read_google_drive_file_async(file_id, format))
                return future.result()
        else:
            return asyncio.run(_read_google_drive_file_async(file_id, format))
    except Exception as e:
        logging.error(f"Error in read_google_drive_file wrapper: {e}")
        return json.dumps({
            "error": f"Failed to read file: {str(e)}",
            "suggestion": "Check if the file exists and you have permission to access it"
        })

async def _read_google_drive_file_async(file_id: str, format: Optional[str] = None) -> str:
    """Async implementation of Google Drive file reading"""
    drive_tool = await _get_google_drive_tool()
    if not drive_tool:
        return json.dumps({
            "error": "Google Drive tool is not available",
            "suggestion": "Check if Google Drive is enabled in configuration"
        })
    
    try:
        content = await drive_tool.read_file_content(file_id)
        if content:
            return json.dumps({
                "file_id": file_id,
                "content": content.content,
                "file_info": content.file_info.to_dict(),
                "format": format or "default"
            })
        else:
            return json.dumps({
                "error": "File content could not be read",
                "suggestion": "Check if the file exists and is accessible"
            })
    except Exception as e:
        logging.error(f"Error reading Google Drive file {file_id}: {e}")
        return json.dumps({
            "error": f"Failed to read file: {str(e)}",
            "suggestion": "Check if the file exists and you have permission to access it"
        })

def search_google_drive_content(search_text: str, file_types: Optional[List[str]] = None, max_results: int = 10) -> str:
    """Search Google Drive content with async wrapper"""
    try:
        loop = asyncio.get_event_loop()
        if loop.is_running():
            import concurrent.futures
            with concurrent.futures.ThreadPoolExecutor() as executor:
                future = executor.submit(asyncio.run, _search_google_drive_content_async(search_text, file_types, max_results))
                return future.result()
        else:
            return asyncio.run(_search_google_drive_content_async(search_text, file_types, max_results))
    except Exception as e:
        logging.error(f"Error in search_google_drive_content wrapper: {e}")
        return json.dumps({
            "error": f"Failed to search content: {str(e)}",
            "suggestion": "Try a different search term or check your permissions"
        })

async def _search_google_drive_content_async(search_text: str, file_types: Optional[List[str]] = None, max_results: int = 10) -> str:
    """Async implementation of Google Drive content search"""
    drive_tool = await _get_google_drive_tool()
    if not drive_tool:
        return json.dumps({
            "error": "Google Drive tool is not available",
            "suggestion": "Check if Google Drive is enabled in configuration"
        })
    
    try:
        files_with_snippets = await drive_tool.search_files_by_content(search_term=search_text, file_types=file_types, max_results=max_results)
        
        results = []
        for file, snippet in files_with_snippets:
            results.append({
                "file": file.to_dict(),
                "snippet": snippet
            })
        
        return json.dumps({
            "results": results,
            "count": len(results),
            "search_text": search_text
        })
    except Exception as e:
        logging.error(f"Error searching Google Drive content: {e}")
        return json.dumps({
            "error": f"Failed to search content: {str(e)}",
            "suggestion": "Try a different search term or check your permissions"
        }) 