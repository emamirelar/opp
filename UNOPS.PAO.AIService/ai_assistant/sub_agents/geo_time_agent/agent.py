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
You are a background data gathering agent.
You are only exposed to the tool `geo_time_tool`.

Your ONLY task is to provide the current geo-time information.
To do this, you MUST use the `geo_time_tool` tool.

It is not your task to worry about the user's request or message. They could be asking for any information / data operations which is independent of your task.
You are the first agent to be called and hence the geo-time information is ALWAYS necessary to do any such above operations. No exceptions.

Once you have the geo-time data from the tool, your sole output must be this complete geo-time data in JSON format exactly as returned by the tool.

You should NOT respond to the user's request or message. You should only ALWAYS return the geo-time data in JSON format.
    """,
    tools=[geo_time_tool], # Register the tool with the agent
    output_key="user_geo_stats",
    # Add safety configurations to prevent responses to users
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
)