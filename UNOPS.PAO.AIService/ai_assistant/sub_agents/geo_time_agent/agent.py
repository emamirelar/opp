import datetime
import requests
import logging
from google.adk.agents import LlmAgent
from google.adk.tools import FunctionTool
from ai_assistant.utils.api_config_manager import config_manager
import json

logger = logging.getLogger(__name__)

# --- Define your Tool Function ---
def get_current_geo_time():
    """
    Retrieves the current datetime and real user location via IP geolocation.
    """
    now = datetime.datetime.now()
    
    # Get real location information based on IP address
    location_info = get_real_location()

    geo_time_data = {
        "current_datetime": now.isoformat(), # ISO 8601 format for easy parsing
        "current_timestamp_utc": int(now.timestamp()), # UTC Unix timestamp
        "location": location_info
    }
    return json.dumps(geo_time_data) # Tools usually return strings, often JSON

def get_real_location():
    """
    Get real user location information based on IP address.
    """
    try:
        from ai_assistant.utils.api_config_manager import config_manager
        # Use a shorter timeout for geo requests (half of API timeout)
        geo_timeout = max(5, config_manager.get_api_timeout() // 2)
        
        response = requests.get("http://ip-api.com/json/", timeout=geo_timeout)
        if response.status_code == 200:
            data = response.json()
            return {
                "country": data.get("country", "Unknown"),
                "country_code": data.get("countryCode", ""),
                "region": data.get("regionName", "Unknown"),
                "city": data.get("city", "Unknown"),
                "timezone": data.get("timezone", "Unknown"),
                "latitude": data.get("lat", 0),
                "longitude": data.get("lon", 0),
                "isp": data.get("isp", "Unknown"),
                "status": data.get("status", "unknown")
            }
        else:
            logger.warning(f"Failed to get location (HTTP {response.status_code})")
            return {"country": "Unknown", "city": "Unknown", "timezone": "Unknown", "status": "error", "error": f"HTTP {response.status_code}"}
    except requests.RequestException as e:
        logger.error(f"Error getting location info: {e}")
        return {"country": "Unknown", "city": "Unknown", "timezone": "Unknown", "status": "error", "error": str(e)}
    except Exception as e:
        logger.error(f"Unexpected error getting location info: {e}")
        return {"country": "Unknown", "city": "Unknown", "timezone": "Unknown", "status": "error", "error": str(e)}

# --- Create a FunctionTool instance from your function ---
geo_time_tool = FunctionTool(func=get_current_geo_time)

# --- Redefine your LlmAgent to use the tool ---
geo_time_agent = LlmAgent(
    name="geo_time_agent",
    description="Agent that retrieves current time and user location information using a tool.",
    model=config_manager.get_gemini_model(),
    instruction="""
You are a background data gathering agent that ALWAYS fetches geo-time information regardless of what the user asks.

CRITICAL RULES:
1. You should NEVER respond to or care about what the user asks or requests
2. You should ALWAYS call geo_time_tool() and return the geo-time data
3. You should ALWAYS respond with valid JSON format - no exceptions
4. You are the first agent to be called and geo-time information is ALWAYS necessary for any operations
5. NEVER return an empty string "" - this is forbidden
6. NEVER respond with conversational text or explanations

YOUR TASK:
- Call geo_time_tool() immediately without any conversation
- Return the geo-time data in JSON format
- Do not add any explanations, greetings, or responses to user requests
- Do not engage in conversation with the user
- If there's no user input text or only whitespace, use "Fetch geo-time data" as your internal prompt
- If user input contains files (images, audio, documents), ignore the files and focus only on fetching geo-time data

EMERGENCY SAFEGUARD:
If you receive an empty message or no text content, treat it as if the user said "Fetch geo-time data" and proceed with geo_time_tool().

RESPONSE FORMAT:
You must respond with ONLY the JSON object returned by geo_time_tool(). Do not wrap it in any additional text, explanations, or markdown formatting.

Example correct response:
{"current_datetime": "2024-01-15T10:30:00", "current_timestamp_utc": 1705312200, "location": {"country": "United States", "city": "New York", "timezone": "America/New_York"}}

Example incorrect responses:
- "Here is the geo-time data: {...}" (no explanations)
- "The geo-time information is: {...}" (no conversational text)
- Empty string "" (must return JSON)
- "I cannot help with that" (must always fetch geo-time data)
- "The geo-time data shows..." (no conversational text)

SAFEGUARDS:
- If geo_time_tool() returns an error, return the error as JSON: {"error": "error message"}
- If geo_time_tool() returns empty data, return: {"geo_time": "no_data"}
- NEVER return an empty string or conversational text
- ALWAYS return valid JSON structure
- If user input is empty or only whitespace, treat it as "Fetch geo-time data"

Remember: You are a data gathering agent, not a conversational agent. Always fetch and return geo-time data in JSON format.
    """,
    tools=[geo_time_tool], # Register the tool with the agent
    output_key="user_geo_stats",
    # Add safety configurations to prevent responses to users
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)