"""
Framework Router

This module contains framework-specific endpoints extracted from main.py
for better organization.
"""

import logging
from fastapi import APIRouter, HTTPException, Request
from ai_assistant.utils.framework_config import get_config, get_environment_info, validate_config
from ai_assistant.utils.iap_validation import validate_iap_headers, extract_iap_headers_for_forwarding

logger = logging.getLogger(__name__)

# Create router
router = APIRouter()


def add_framework_endpoints(google_drive_tool=None):
    """Add framework-specific endpoints"""
    config = get_config()
    branding_config = config.get('branding', {})

    @router.get("/framework/info")
    async def get_framework_info():
        """Get framework information"""
        return {
            "framework": branding_config.get('application_name', 'AI Service'),
            "version": "1.0.0",
            "description": branding_config.get('description', 'AI Agent Framework'),
            "status": "running",
            "environment": get_environment_info()
        }

    @router.get("/framework/config")
    async def get_framework_config():
        """Get framework configuration"""
        return validate_config()

    @router.get("/framework/tools")
    async def get_available_tools():
        """Get available tools"""
        tools = []
        # Future tools can be added here
        return {"available_tools": tools, "total_tools": len(tools)}

    @router.get("/framework/iap-test")
    async def test_iap_validation(request: Request):
        """Test endpoint to demonstrate IAP header validation"""
        try:
            headers = dict(request.headers)

            # Validate IAP headers
            iap_validation = validate_iap_headers(headers)

            # Extract headers for forwarding
            iap_headers_to_forward = extract_iap_headers_for_forwarding(headers)

            return {
                "message": "IAP validation test completed",
                "validation_result": iap_validation,
                "headers_to_forward": iap_headers_to_forward,
                "all_headers": headers
            }
        except Exception as e:
            logger.error(f"❌ Error in IAP test endpoint: {e}")
            raise HTTPException(status_code=500, detail=str(e))

    logger.info("✅ Framework endpoints registered successfully") 