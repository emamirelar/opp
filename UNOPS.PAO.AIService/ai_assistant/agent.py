import json
import logging
from typing import Dict, List, Any, Optional
import asyncio

# Framework imports
try:
    from framework_config import get_config
    from ai_assistant.tools import GoogleDriveTool, create_google_drive_tool, GoogleSheetTool, create_google_sheet_tool, GoogleDocTool, create_google_doc_tool
    GOOGLE_DRIVE_AVAILABLE = True
    GOOGLE_SHEETS_AVAILABLE = True
except ImportError as e:
    GOOGLE_DRIVE_AVAILABLE = False
    GOOGLE_SHEETS_AVAILABLE = False
    logging.warning(f"Google Drive/Sheets tools not available - check framework_config and tools setup: {e}")
    # Still try to import get_config separately
    try:
        from framework_config import get_config
    except ImportError:
        logging.error("Failed to import get_config - framework_config may not be available")
        # Define a dummy get_config function as fallback
        def get_config():
            from types import SimpleNamespace
            return SimpleNamespace(
                google_drive=SimpleNamespace(enabled=False),
                database=SimpleNamespace(url=""),
                server=SimpleNamespace(host="0.0.0.0", port=8000)
            )

from datetime import datetime
from google.adk.agents import Agent, SequentialAgent
from google.adk.tools import FunctionTool
from google.adk.tools import agent_tool as AgentTool
from google.adk.tools.base_tool import BaseTool
from google.adk.tools.tool_context import ToolContext
from google.genai import types

from .workflow_agent.agent import workflow_agent
from .contextual_agents import contextual_agent
from .search_agent import search_agent
from .agent_callbacks import (
    get_cache_performance_stats,
    force_cache_refresh,
    api_success_callback,
    user_request_after_model_callback
)
from .cache import entity_cache, auto_cleanup
from ai_assistant.config_manager import config_manager

# Initialize Google Drive tool
google_drive_tool = None

# Initialize Google Sheet and Doc tools
google_sheet_tool = None
google_doc_tool = None

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
            if GOOGLE_SHEETS_AVAILABLE:
                # Don't initialize here - do it lazily when first used
                self.sheet_tool = None
                logging.info("✅ Google Sheet tool wrapper initialized (will initialize on first use)")
            else:
                logging.warning("⚠️ Google Sheets not available")
        except Exception as e:
            logging.error(f"❌ Failed to initialize Google Sheet tool: {e}")
    
    async def _get_sheet_tool(self):
        """Get or create the Google Sheet tool instance"""
        if self.sheet_tool is None:
            try:
                # Ensure configuration is initialized
                from framework_config import initialize_config
                config = initialize_config('dev')  # Initialize config if not already done
                
                google_drive_config = config.get('google_drive', {})
                if not google_drive_config.get('enabled', False):
                    logging.warning("⚠️ Google Drive not enabled in config")
                    return None
                
                logging.info("🔧 Initializing Google Sheet tool...")
                self.sheet_tool = await create_google_sheet_tool()
                
                if self.sheet_tool is None:
                    logging.error("❌ create_google_sheet_tool returned None")
                    return None
                
                logging.info("🔐 Authenticating Google Sheet tool...")
                auth_success = await self.sheet_tool.initialize()
                
                if not auth_success:
                    logging.error("❌ Google Sheet tool authentication failed")
                    self.sheet_tool = None
                    return None
                
                logging.info("✅ Google Sheet tool initialized and authenticated successfully")
                
            except Exception as e:
                logging.error(f"❌ Failed to initialize Google Sheet tool: {str(e)}")
                self.sheet_tool = None
                return None
        
        return self.sheet_tool
    
    def create_spreadsheet_from_list(self, tool_context: ToolContext, title: str, data: List[Dict[str, Any]], folder_id: str = "") -> str:
        """Create a Google Sheet from list data with session state access"""
        try:
            # Get user_email from session state
            user_email = None
            if tool_context and hasattr(tool_context, 'state') and tool_context.state:
                user_email = tool_context.state.get('user_email')
                if user_email:
                    logging.info(f"📧 Using user_email for permissions: {user_email}")
                else:
                    logging.warning("⚠️ No user_email found in session state - permissions may not be granted")
            
            # Initialize tool if needed
            if not self.sheet_tool:
                # Run async initialization in sync context
                loop = asyncio.get_event_loop()
                if loop.is_running():
                    import concurrent.futures
                    with concurrent.futures.ThreadPoolExecutor() as executor:
                        future = executor.submit(asyncio.run, self._get_sheet_tool())
                        self.sheet_tool = future.result()
                else:
                    self.sheet_tool = asyncio.run(self._get_sheet_tool())
            
            if not self.sheet_tool:
                return json.dumps({"error": "Google Sheet tool not available"})
            
            # Create the spreadsheet - handle async call properly
            async def create_sheet():
                return await self.sheet_tool.create_spreadsheet_from_list_data(
                    title=title,
                    data=data,
                    folder_id=folder_id if folder_id else None,
                    header_email=user_email
                )
            
            # Run the async function in sync context
            loop = asyncio.get_event_loop()
            if loop.is_running():
                import concurrent.futures
                with concurrent.futures.ThreadPoolExecutor() as executor:
                    future = executor.submit(asyncio.run, create_sheet())
                    result = future.result()
            else:
                result = asyncio.run(create_sheet())
            
            logging.info(f"✅ Created Google Sheet: {title}")
            return json.dumps(result)
            
        except Exception as e:
            logging.error(f"❌ Error creating Google Sheet: {e}")
            return json.dumps({"error": f"Failed to create Google Sheet: {str(e)}"})
    
    def create_spreadsheet_with_headers(self, tool_context: ToolContext, title: str, headers: List[str], data: List[List[Any]], folder_id: str = "") -> str:
        """Create a Google Sheet with headers and data with session state access"""
        try:
            # Get user_email from session state
            user_email = None
            if tool_context and hasattr(tool_context, 'state') and tool_context.state:
                user_email = tool_context.state.get('user_email')
                if user_email:
                    logging.info(f"📧 Using user_email for permissions: {user_email}")
                else:
                    logging.warning("⚠️ No user_email found in session state - permissions may not be granted")
            
            # Initialize tool if needed
            if not self.sheet_tool:
                # Run async initialization in sync context
                loop = asyncio.get_event_loop()
                if loop.is_running():
                    import concurrent.futures
                    with concurrent.futures.ThreadPoolExecutor() as executor:
                        future = executor.submit(asyncio.run, self._get_sheet_tool())
                        self.sheet_tool = future.result()
                else:
                    self.sheet_tool = asyncio.run(self._get_sheet_tool())
            
            if not self.sheet_tool:
                return json.dumps({"error": "Google Sheet tool not available"})
            
            # Create the spreadsheet - handle async call properly
            async def create_sheet():
                return await self.sheet_tool.create_spreadsheet_with_headers(
                    title=title,
                    headers=headers,
                    data_rows=data,
                    folder_id=folder_id if folder_id else None,
                    header_email=user_email
                )
            
            # Run the async function in sync context
            loop = asyncio.get_event_loop()
            if loop.is_running():
                import concurrent.futures
                with concurrent.futures.ThreadPoolExecutor() as executor:
                    future = executor.submit(asyncio.run, create_sheet())
                    result = future.result()
            else:
                result = asyncio.run(create_sheet())
            
            logging.info(f"✅ Created Google Sheet with headers: {title}")
            return json.dumps(result)
            
        except Exception as e:
            logging.error(f"❌ Error creating Google Sheet with headers: {e}")
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
            if GOOGLE_SHEETS_AVAILABLE:  # This should be GOOGLE_DRIVE_AVAILABLE for docs
                # Don't initialize here - do it lazily when first used
                self.doc_tool = None
                logging.info("✅ Google Doc tool wrapper initialized (will initialize on first use)")
            else:
                logging.warning("⚠️ Google Docs not available")
        except Exception as e:
            logging.error(f"❌ Failed to initialize Google Doc tool: {e}")
    
    async def _get_doc_tool(self):
        """Get or create the Google Doc tool instance"""
        if self.doc_tool is None:
            try:
                # Ensure configuration is initialized
                from framework_config import initialize_config
                config = initialize_config('dev')  # Initialize config if not already done
                
                google_drive_config = config.get('google_drive', {})
                if not google_drive_config.get('enabled', False):
                    logging.warning("⚠️ Google Drive not enabled in config")
                    return None
                
                logging.info("🔧 Initializing Google Doc tool...")
                self.doc_tool = await create_google_doc_tool()
                
                if self.doc_tool is None:
                    logging.error("❌ create_google_doc_tool returned None")
                    return None
                
                logging.info("🔐 Authenticating Google Doc tool...")
                auth_success = await self.doc_tool.initialize()
                
                if not auth_success:
                    logging.error("❌ Google Doc tool authentication failed")
                    self.doc_tool = None
                    return None
                
                logging.info("✅ Google Doc tool initialized and authenticated successfully")
                
            except Exception as e:
                logging.error(f"❌ Failed to initialize Google Doc tool: {str(e)}")
                self.doc_tool = None
                return None
        
        return self.doc_tool
    
    def create_document_from_text(self, tool_context: ToolContext, title: str, content: str, folder_id: str = "") -> str:
        """Create a Google Doc from text with session state access"""
        try:
            # Get user_email from session state
            user_email = None
            if tool_context and hasattr(tool_context, 'state') and tool_context.state:
                user_email = tool_context.state.get('user_email')
                if user_email:
                    logging.info(f"📧 Using user_email for permissions: {user_email}")
                else:
                    logging.warning("⚠️ No user_email found in session state - permissions may not be granted")
            
            # Initialize tool if needed
            if not self.doc_tool:
                # Run async initialization in sync context
                loop = asyncio.get_event_loop()
                if loop.is_running():
                    import concurrent.futures
                    with concurrent.futures.ThreadPoolExecutor() as executor:
                        future = executor.submit(asyncio.run, self._get_doc_tool())
                        self.doc_tool = future.result()
                else:
                    self.doc_tool = asyncio.run(self._get_doc_tool())
            
            if not self.doc_tool:
                return json.dumps({"error": "Google Doc tool not available"})
            
            # Create the document - handle async call properly
            async def create_doc():
                return await self.doc_tool.create_document_from_data(
                    title=title,
                    data={"content": content},
                    folder_id=folder_id if folder_id else None,
                    header_email=user_email
                )
            
            # Run the async function in sync context
            loop = asyncio.get_event_loop()
            if loop.is_running():
                import concurrent.futures
                with concurrent.futures.ThreadPoolExecutor() as executor:
                    future = executor.submit(asyncio.run, create_doc())
                    result = future.result()
            else:
                result = asyncio.run(create_doc())
            
            logging.info(f"✅ Created Google Doc: {title}")
            return json.dumps(result)
            
        except Exception as e:
            logging.error(f"❌ Error creating Google Doc: {e}")
            return json.dumps({"error": f"Failed to create Google Doc: {str(e)}"})


# Initialize tool wrappers
google_sheet_wrapper = GoogleSheetToolWrapper()
google_doc_wrapper = GoogleDocToolWrapper()

# Create function tools for Google Sheet operations
def create_google_sheet_from_list_data(tool_context: ToolContext, title: str, data: str, folder_id: str = "") -> str:
    """Create a Google Sheet from list data with session state access
    
    Args:
        tool_context: Tool context with session state
        title: The title of the spreadsheet
        data: JSON string containing list of dictionaries
        folder_id: Optional folder ID to create the sheet in (empty string for root)
        
    Returns:
        JSON string with result or error
    """
    try:
        # Parse the JSON data
        data_list = json.loads(data)
        if not isinstance(data_list, list):
            return json.dumps({"error": "Data must be a JSON array of objects"})
        
        return google_sheet_wrapper.create_spreadsheet_from_list(tool_context, title, data_list, folder_id)
    except json.JSONDecodeError as e:
        return json.dumps({"error": f"Invalid JSON data: {str(e)}"})
    except Exception as e:
        logging.error(f"Error in create_google_sheet_from_list_data: {e}")
        return json.dumps({"error": f"Failed to create Google Sheet: {str(e)}"})

def create_google_sheet_with_headers_data(tool_context: ToolContext, title: str, headers: str, data: str, folder_id: str = "") -> str:
    """Create a Google Sheet with headers and data with session state access
    
    Args:
        tool_context: Tool context with session state
        title: The title of the spreadsheet
        headers: JSON string containing list of header strings
        data: JSON string containing list of data rows (each row is a list of values)
        folder_id: Optional folder ID to create the sheet in (empty string for root)
        
    Returns:
        JSON string with result or error
    """
    try:
        # Parse the JSON data
        headers_list = json.loads(headers)
        data_rows_list = json.loads(data)
        
        if not isinstance(headers_list, list):
            return json.dumps({"error": "Headers must be a JSON array of strings"})
        if not isinstance(data_rows_list, list):
            return json.dumps({"error": "Data rows must be a JSON array of arrays"})
        
        return google_sheet_wrapper.create_spreadsheet_with_headers(tool_context, title, headers_list, data_rows_list, folder_id)
    except json.JSONDecodeError as e:
        return json.dumps({"error": f"Invalid JSON data: {str(e)}"})
    except Exception as e:
        logging.error(f"Error in create_google_sheet_with_headers_data: {e}")
        return json.dumps({"error": f"Failed to create Google Sheet: {str(e)}"})

# Create function tools for Google Doc operations
def create_google_doc_from_text_data(tool_context: ToolContext, title: str, content: str, folder_id: str = "") -> str:
    """Create a Google Doc from text with session state access
    
    Args:
        tool_context: Tool context with session state
        title: The title of the document
        content: The text content for the document
        folder_id: Optional folder ID to create the doc in (empty string for root)
        
    Returns:
        JSON string with result or error
    """
    return google_doc_wrapper.create_document_from_text(tool_context, title, content, folder_id)

ROOT_PROMPT = f"""
You are a friendly AI assistant for the {config_manager.get_project_name()}.

**YOUR CAPABILITIES:**
- Access to Google Drive, Sheets, and Docs
- Search Google Drive documents
- Create Google Docs and Sheets
- Read content from Google Drive documents and Sheets
- Make API calls
- Generate mermaid diagrams and visual charts
- Create organizational charts and relationship diagrams

**YOUR TASKS:**

1.  **Handle Greetings Directly:** "Hello", "Hi", "How are you", "Thank you", "Good morning"
    → Respond with friendly JSON format using user context.

2.  **Handle Knowledge Questions Directly:** "What is...", "How do I...", "Explain..."
    → Use `search_google_drive_knowledge` tool to find relevant documents.

3.  **Handle Cache Commands:** "cache stats", "clear cache", "refresh cache"
    → Use cache management tools.

4.  **Handle Web Search Questions:** Current events, external information, latest news.
    → Use `search_agent` when information is not in Google Drive or for current/external topics.

5.  **Handle Google Drive Operations:** File search, content reading, and management.
    → Use Google Drive tools for file operations.

6.  **Handle Google Sheets Operations:** Create spreadsheets, export data, or generate reports in spreadsheet format.
    → Use Google Sheets tools for tabular data (e.g., create a sheet from a list, export tabular data, generate reports).
    **Available tools:**
    - `create_google_sheet_from_list_data(title, data, folder_id)` - Create sheet from JSON string of list of dictionaries
    - `create_google_sheet_with_headers_data(title, headers, data, folder_id)` - Create sheet with JSON string of headers and data rows

7.  **Handle Google Docs Operations:** Create documents, export text, or generate reports in document format.
    → Use Google Docs tools for document operations (e.g., create a doc from text, export notes, generate reports).
    **CRITICAL:** If the user asks to create a document, ALWAYS call the tool:
    - `create_google_doc_from_text_data(title, content, folder_id)`
    **How to use:**
    - `title`: The title for the new Google Doc (e.g., "Partner News: Bill Gates Foundation")
    - `content`: The text to put in the document (e.g., the news, summary, or notes)
    - `folder_id`: (optional) The Google Drive folder to save in, or leave blank for default

8.  **Handle Mermaid Diagram Requests:** Create visual diagrams, organizational charts, or relationship mappings.
    → **CRITICAL:** You CAN and SHOULD generate mermaid diagrams directly in your JSON response.
    **How to handle mermaid requests:**
    - Generate the mermaid syntax (e.g., "graph TD\\n A --> B")
    - Use `"type": "mermaid"` in your JSON response
    - Include `"entity": "Partner"` (or relevant entity) field
    - Provide a friendly markdown message explaining the diagram
    - **NEVER say you cannot create diagrams** - you can generate the mermaid code
    - **Example:** User asks "Create a mermaid diagram of partner hierarchy" → Generate mermaid syntax and return with `"type": "mermaid"`

9.  **Delegate Everything Else:** Data operations, preference changes, entity requests.
    → Use `workflow_agent` sub-agent.

**CRITICAL RULE - ALWAYS DELEGATE DATA REQUESTS:**
If the user asks for specific data you don't have complete information about, you MUST use the `workflow_agent`. Screen context is background information, not the complete answer.

**EXAMPLES OF WHEN TO DELEGATE:**
- User asks about related entities (contacts for a partner, opportunities for a contact, etc.)
- User asks for lists or details not in your current context
- User asks for specific information that requires API calls
- User asks for data operations (create, update, delete)
- User asks about entities not fully represented in your context

**NEVER respond with "I cannot create a Google Doc." ALWAYS use the `create_google_doc_from_text_data` tool if the user asks for a document.**

**RESPONSE FORMAT WITH SOURCES:**
When using `search_agent` or `search_google_drive_knowledge`, include sources in your JSON response. The tool/agent returns `{{content, sources}}`; use `content` for the message and the `sources` array for the `sources` field.

```json
{{
  "result": [
    {{
      "type": "markdown", 
      "message": "Your response content here..."
    }}
  ],
  "sources": [
    {{
      "title": "Source Title",
      "url": "[https://example.com](https://example.com)",
      "description": "Brief description of the source"
    }}
  ],
  "followUps": ["Action 1", "Action 2", "Action 3"]
}}
```

**CRITICAL: MEANINGFUL FOLLOW-UPS REQUIRED**
When generating JSON responses, you MUST include 2-4 meaningful followUps that:
- Build on the current response (ask deeper questions).
- Explore related aspects (suggest related topics).
- Provide actionable next steps (help users take concrete actions).
- Are contextually relevant (match user's intent/screen context).

**🚨 CRITICAL JSON FORMATTING RULES:**
- **NEVER wrap JSON responses in markdown code blocks** (no ```json or ```)
- **Return JSON as plain text** - the frontend expects raw JSON
- **Ensure all JSON is valid** - no trailing commas, proper escaping
- **For markdown content**, include it directly in the `message` field
- **No extra formatting or explanatory text** outside the JSON structure
- **For any links in the message field**, use `target="_blank"` to open in separate window: `[link text](url){{:target="_blank"}}`
- **Example of CORRECT format:**
```json
{{
  "result": [
    {{
      "type": "markdown",
      "message": "**Hello!** 👋\\n\\nHow can I help you today?"
    }}
  ],
  "followUps": ["Action 1", "Action 2", "Action 3"]
}}
```
- **Example of INCORRECT format:**
```
Here's your response:

```json
{{
  "result": [...]
}}
```
```

**FOLLOW-UP EXAMPLES:**
- If discussing permissions: "How do I assign roles to users?", "What are the differences between admin and user roles?"
- If showing partner information: "How do I add a new contact to this partner?", "What opportunities exist for this partner?"
- If explaining a process: "What are the prerequisites for this process?", "How do I troubleshoot common issues?"
- If providing search results: "Can you show me more details about [specific item]?", "How do I filter these results?"

**FOLLOW-UP CATEGORIES:**
- "details" - For getting more specific information
- "related" - For exploring related topics or entities
- "action" - For taking next steps or performing actions
- "troubleshooting" - For solving problems or issues
- "configuration" - For setup or customization questions

**GREETING RESPONSE FORMAT:**
For greetings like "hi", "hello", "how are you", respond with:
```json
{{
  "result": [
    {{
      "type": "markdown", 
      "message": "**Hello!** 👋\\n\\nHow can I help you today?"
    }}
  ],
  "followUps": []
}}
```

**CONTEXT AVAILABLE:**
- user_name: Use for personalization if available
- preferences.language: Respond in user's preferred language
- screen_context: Background information about current screen (NOT the complete answer)

**EXAMPLES:**

User: "hi" 
→ Respond directly with greeting JSON

User: "What is a partner?"
→ Use search_google_drive_knowledge, then respond with JSON including sources

User: "What's the latest news about UNOPS?"
→ Use search_agent, then respond with JSON including sources

User: "Find my project documents in Google Drive"
→ Use search_google_drive, then respond with JSON

User: "cache stats"
→ Use get_cache_stats, then respond with JSON

User: "Show me partners" 
→ Use workflow_agent (no direct response)

User: "Show me contacts for this partner" (while on partner screen)
→ Use workflow_agent (even though you have partner context)

User: "What are the opportunities for this contact?" (while on contact screen)
→ Use workflow_agent (even though you have contact context)

User: "Create a mermaid diagram of partner hierarchy"
→ Generate mermaid syntax and respond with JSON using `"type": "mermaid"`

User: "Show me a flowchart of the approval process"
→ Generate mermaid syntax and respond with JSON using `"type": "mermaid"`

**REMEMBER:** Screen context is background information, not the complete answer. When in doubt, delegate to workflow_agent!
"""

async def _get_google_drive_tool():
    """Get or create the Google Drive tool instance"""
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
    """
    Search Google Drive for knowledge-related documents and content.
    This is a sync wrapper around the async implementation.

    Args:
        query: The search query from the user
        max_results: Maximum number of results to return
        
    Returns:
        Dict with formatted content and sources for JSON response
    """
    
    # Run the async function in the event loop
    try:
        loop = asyncio.get_event_loop()
        if loop.is_running():
            # If we're already in an event loop, create a task
            import concurrent.futures
            with concurrent.futures.ThreadPoolExecutor() as executor:
                future = executor.submit(asyncio.run, _search_google_drive_knowledge_async(query))
                return future.result()
        else:
            # If no event loop is running, use asyncio.run
            return asyncio.run(_search_google_drive_knowledge_async(query))
    except Exception as e:
        logging.error(f"Error in search_google_drive_knowledge wrapper: {e}")
        return {
            "content": f"Error searching Google Drive: {str(e)}. Please try again or contact support.",
            "sources": []
        }

async def _search_google_drive_knowledge_async(query: str) -> dict:
    """
    Async implementation of Google Drive knowledge search.

    Args:
        query: The search query from the user
        max_results: Maximum number of results to return
        
    Returns:
        Dict with formatted content and sources for JSON response
    """
    
    print(f"🔍 Searching Google Drive for knowledge: '{query}'")
    
    try:
        drive_tool = await _get_google_drive_tool()
        if not drive_tool:
            config = get_config()
            if not config.get('google_drive', {}).get('enabled', False):
                return {
                    "content": "Google Drive search is disabled in the configuration. To enable it, contact your administrator.",
                    "sources": []
                }
            else:
                return {
                    "content": "Google Drive search is not available due to authentication issues. Please check the logs for more details or contact your administrator.",
                    "sources": []
                }
        
        # First, search for files by name/title
        files_by_name = await drive_tool.find_files(
            name_contains=query,
            max_results=5
        )
        
        # Then search for files by content
        files_by_content = await drive_tool.search_files_by_content(
            search_term=query,
            max_results=5
        )
        
        # Combine and deduplicate results
        all_files = {}
        for file in files_by_name:
            all_files[file.id] = file
        
        # files_by_content returns tuples of (file, snippet)
        for file, snippet in files_by_content:
            all_files[file.id] = file
        
        results = list(all_files.values())[:5]
        
        if not results:
            return {
                "content": "No relevant documents found in Google Drive. You might want to try a different search term or check if the documents exist.",
                "sources": []
            }
        
        # Format results for the agent
        formatted_content = "📁 **Google Drive Knowledge Results:**\n\n"
        sources = []
        
        for i, file in enumerate(results, 1):
            formatted_content += f"**Document {i}: {file.name}**\n"
            
            # Try to get the full content for analysis
            try:
                content = await drive_tool.read_file_content(file.id)
                if content and content.content:
                    # Include the full content for AI analysis
                    formatted_content += f"*Content:*\n{content.content}\n\n"
                else:
                    formatted_content += f"*Type:* {file.mime_type}\n"
                    formatted_content += f"*Modified:* {file.modified_time}\n\n"
            except Exception as e:
                formatted_content += f"*Type:* {file.mime_type}\n"
                formatted_content += f"*Modified:* {file.modified_time}\n"
                formatted_content += f"*Error reading content:* {str(e)}\n\n"
            
            # Add to sources array
            sources.append({
                "title": file.name,
                "url": file.web_view_link or f"https://drive.google.com/file/d/{file.id}/view",
                "description": f"Google Drive document: {file.name}"
            })
        
        print(f"✅ Found {len(results)} relevant documents in Google Drive")
        return {
            "content": formatted_content,
            "sources": sources
        }
        
    except Exception as e:
        print(f"❌ Error searching Google Drive for knowledge: {str(e)}")
        logging.error(f"Error in Google Drive knowledge search: {str(e)}", exc_info=True)
        return {
            "content": f"Error searching Google Drive: {str(e)}. Please check the logs for more details or contact your administrator.",
            "sources": []
        }

def search_google_drive(query: str, file_type: Optional[str] = None, max_results: int = 10) -> str:
    """Search for files in Google Drive (sync wrapper)"""
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
    """Search for files in Google Drive (async implementation)"""
    drive_tool = await _get_google_drive_tool()
    if not drive_tool:
        return json.dumps({
            "error": "Google Drive tool is not available",
            "suggestion": "Check if Google Drive is enabled in configuration"
        })
    
    try:
        # Instead of passing raw query, use name_contains parameter for better compatibility
        # This avoids issues with special characters like "+" in raw queries
        files = await drive_tool.find_files(
            name_contains=query,  # Use name_contains instead of query
            mime_type=file_type,
            max_results=max_results
        )
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
    """Read content from a Google Drive file (sync wrapper)"""
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
    """Read content from a Google Drive file (async implementation)"""
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
    """Search for files based on content (sync wrapper)"""
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
    """Search for files based on content (async implementation)"""
    drive_tool = await _get_google_drive_tool()
    if not drive_tool:
        return json.dumps({
            "error": "Google Drive tool is not available",
            "suggestion": "Check if Google Drive is enabled in configuration"
        })
    
    try:
        files_with_snippets = await drive_tool.search_files_by_content(
            search_term=search_text,
            file_types=file_types,
            max_results=max_results
        )
        
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

def get_cache_stats() -> str:
    """
    Get comprehensive cache performance statistics.
    
    Returns:
        Formatted string with cache statistics
    """
    
    print("📊 Getting cache performance statistics...")
    
    try:
        # Run automatic cleanup first
        auto_cleanup()
        
        # Get comprehensive stats
        stats = get_cache_performance_stats()
        
        if stats.get('error'):
            return f"❌ Error getting cache stats: {stats['error']}"
        
        # Format stats for display
        user_cache = stats['user_profile_cache']
        screen_cache = stats['screen_context_cache']
        performance = stats['performance_insights']
        ttl_config = stats['ttl_config']
        
        formatted_stats = f"""📊 **Cache Performance Statistics**

**User Profile Cache:**
- Valid entries: {user_cache['valid_entries']}
- Expired entries: {user_cache['expired_entries']}
- Memory usage: {user_cache['memory_usage_estimate']}

**Screen Context Cache:**
- Valid entries: {screen_cache['valid_entries']}
- Expired entries: {screen_cache['expired_entries']}
- Memory usage: {screen_cache['memory_usage_estimate']}

**Performance Impact:**
- Expected response time: {performance['performance_impact']['expected_response_time']}
- Cache effectiveness: {performance['cache_effectiveness']}

**TTL Configuration:**
- User profile: {ttl_config['user_profile_ttl_hours']} hours
- Screen context: {ttl_config['screen_context_ttl_hours']} hours

**Current Status:**
- Both caches ready: {'✅' if performance['cache_effectiveness']['both_caches_ready'] else '❌'}
- Last URL: {stats.get('last_url', 'None')}
"""
        
        return formatted_stats
        
    except Exception as e:
        print(f"❌ Error getting cache stats: {e}")
        return f"❌ Error getting cache stats: {e}"

def clear_cache(cache_type: str = "all") -> str:
    """
    Clear cache(s) by type.
    
    Args:
        cache_type: Type of cache to clear ("all", "user", "screen")
        
    Returns:
        Formatted string with clear result
    """
    
    print(f"🧹 Clearing cache: {cache_type}")
    
    try:
        if cache_type.lower() == "all":
            entity_cache.clear_all_caches()
            return "✅ All caches cleared successfully"
        elif cache_type.lower() == "user":
            entity_cache.user_cache.clear()
            return "✅ User profile cache cleared successfully"
        elif cache_type.lower() == "screen":
            entity_cache.screen_cache.clear()
            return "✅ Screen context cache cleared successfully"
        else:
            return f"❌ Invalid cache type '{cache_type}'. Use 'all', 'user', or 'screen'."
            
    except Exception as e:
        print(f"❌ Error clearing cache: {e}")
        return f"❌ Error clearing cache: {e}"

def refresh_cache(target: str = "all") -> str:
    """
    Force refresh of cache(s).
    
    Args:
        target: What to refresh ("all", "user", "screen")
        
    Returns:
        Formatted string with refresh result
    """
    
    print(f"🔄 Force refreshing cache: {target}")
    
    try:
        if target.lower() == "all":
            result = force_cache_refresh()
        elif target.lower() == "user":
            result = force_cache_refresh(email=None, screen_url=None)
        elif target.lower() == "screen":
            result = force_cache_refresh(email=None, screen_url="")
        else:
            return f"❌ Invalid target '{target}'. Use 'all', 'user', or 'screen'."
        
        if result.get('error'):
            return f"❌ Error refreshing cache: {result['error']}"
        
        refreshed = result.get('refreshed', [])
        return f"✅ Cache refresh completed: {', '.join(refreshed)}"
        
    except Exception as e:
        print(f"❌ Error refreshing cache: {e}")
        return f"❌ Error refreshing cache: {e}"

# Cache management hook for API operations
def notify_api_success(entity_type: str, operation: str, result: dict) -> None:
    """
    Notify the cache system when API operations succeed.
    
    This should be called by the workflow agent after successful operations.
    
    Args:
        entity_type: Type of entity that was modified
        operation: Type of operation (create, update, delete)
        result: The API operation result
    """
    
    print(f"🔄 Notifying cache system: {entity_type} {operation}")
    
    try:
        api_success_callback(entity_type, operation, result)
        print(f"✅ Cache system notified successfully")
    except Exception as e:
        print(f"❌ Error notifying cache system: {e}")
    
user_request_agent = Agent(
    name="user_request_agent",
    model=config_manager.get_gemini_model(),
    description="Main AI assistant for Opportunity+ system with comprehensive workflow capabilities",
    instruction=ROOT_PROMPT,
    tools=[
        FunctionTool(func=search_google_drive_knowledge),
        FunctionTool(func=get_cache_stats),
        FunctionTool(func=clear_cache),
        FunctionTool(func=refresh_cache),
        AgentTool.AgentTool(agent=search_agent),
        FunctionTool(func=search_google_drive),
        FunctionTool(func=read_google_drive_file),
        FunctionTool(func=search_google_drive_content),
        FunctionTool(func=create_google_sheet_from_list_data),
        FunctionTool(func=create_google_sheet_with_headers_data),
        FunctionTool(func=create_google_doc_from_text_data)
    ],
    sub_agents=[workflow_agent],
    after_model_callback=user_request_after_model_callback
)    
    
root_agent = SequentialAgent(
    name="ai_assistant",
    description="Main AI assistant for Opportunity+ system with comprehensive workflow capabilities",
    sub_agents=[contextual_agent, user_request_agent],
)    
