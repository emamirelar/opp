"""
Geo Time Agent Utilities

This module contains utility functions for gathering current time and location information.
"""

import json
import logging
import requests
from datetime import datetime, timezone
from typing import Dict, Any, Optional
from google.adk.agents.callback_context import CallbackContext

logger = logging.getLogger(__name__)

def get_current_time_info() -> Dict[str, Any]:
    """
    Get current time information in various formats.
    
    Returns:
        Dict containing current time information
    """
    try:
        now = datetime.now(timezone.utc)
        
        return {
            "utc_time": now.isoformat(),
            "formatted_utc": now.strftime("%Y-%m-%d %H:%M:%S UTC"),
            "day_of_week": now.strftime("%A"),
            "date": now.strftime("%Y-%m-%d"),
            "time": now.strftime("%H:%M:%S"),
            "timestamp": now.timestamp(),
            "timezone": "UTC"
        }
    except Exception as e:
        logger.error(f"Error getting time info: {e}")
        return {
            "error": f"Failed to get time information: {str(e)}",
            "utc_time": "unavailable",
            "formatted_utc": "unavailable"
        }


def get_user_location_info() -> Dict[str, Any]:
    """
    Get user location information based on IP address.
    
    Returns:
        Dict containing location information
    """
    try:
        # Use a free IP geolocation service
        response = requests.get("http://ip-api.com/json/", timeout=5)
        
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
            return {
                "error": f"Failed to get location (HTTP {response.status_code})",
                "country": "Unknown",
                "city": "Unknown",
                "timezone": "Unknown"
            }
            
    except requests.RequestException as e:
        logger.error(f"Error getting location info: {e}")
        return {
            "error": f"Failed to get location information: {str(e)}",
            "country": "Unknown", 
            "city": "Unknown",
            "timezone": "Unknown"
        }
    except Exception as e:
        logger.error(f"Unexpected error getting location info: {e}")
        return {
            "error": f"Unexpected error: {str(e)}",
            "country": "Unknown",
            "city": "Unknown", 
            "timezone": "Unknown"
        }


def get_geo_time_info_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Before model callback that gathers current time and location information.
    
    Args:
        callback_context: The callback context from Google ADK
        llm_request: The LLM request object (optional)
    """
    try:
        logger.info("Starting geo-time information gathering")
        
        # Get current time information
        time_info = get_current_time_info()
        
        # Get location information
        location_info = get_user_location_info()
        
        # Combine the information
        geo_time_data = {
            "timestamp": datetime.now().isoformat(),
            "time_info": time_info,
            "location_info": location_info,
            "status": "success"
        }
        
        # Store in session state
        if hasattr(callback_context, 'state'):
            callback_context.state['user_geo_stats'] = geo_time_data
            logger.info("Successfully stored geo-time information in session state")
        
        # Also store as JSON string for LLM processing
        geo_time_json = json.dumps(geo_time_data, indent=2)
        
        # Log the collected information
        logger.info(f"Collected geo-time info: {geo_time_json}")
        
    except Exception as e:
        logger.error(f"Error in geo-time agent before_model_callback: {e}")
        
        # Store error information
        error_data = {
            "timestamp": datetime.now().isoformat(),
            "status": "error",
            "error": str(e),
            "time_info": {"error": "Failed to get time info"},
            "location_info": {"error": "Failed to get location info"}
        }
        
        if hasattr(callback_context, 'state'):
            callback_context.state['user_geo_stats'] = error_data 