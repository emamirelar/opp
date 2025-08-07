"""
IAP (Identity-Aware Proxy) Validation Utilities

This module contains IAP header validation and extraction logic
moved from main.py for better organization.
"""

import logging
import re
import urllib.parse
from typing import Dict, Any

logger = logging.getLogger(__name__)


def validate_iap_headers(headers: dict) -> dict:
    """
    Validate IAP (Identity-Aware Proxy) headers from incoming requests.

    This function validates the presence and format of Google Cloud IAP headers
    and returns validation results with extracted user information.

    Args:
        headers: Dictionary of request headers

    Returns:
        dict: Validation result with the following structure:
        {
            "valid": bool,
            "user_email": str or None,
            "user_id": str or None,
            "is_development": bool,
            "validation_errors": list,
            "extracted_headers": dict
        }
    """
    validation_result = {
        "valid": False,
        "user_email": None,
        "user_id": None,
        "is_development": False,
        "validation_errors": [],
        "extracted_headers": {}
    }

    try:
        # Convert headers to lowercase for case-insensitive comparison
        headers_lower = {k.lower(): v for k, v in headers.items()}

        # Define expected IAP headers
        expected_iap_headers = [
            'x-goog-authenticated-user-email',
            'x-goog-authenticated-user-id',
            'x-forwarded-user',
            'x-forwarded-email'
        ]

        # Check for development mode indicators
        is_dev_simulation = headers_lower.get('x-dev-iap-simulation', '').lower() == 'true'
        dev_timestamp = headers_lower.get('x-dev-auth-timestamp')

        # Check for DevIAPAuth cookie in development mode
        dev_iap_auth_cookie = headers_lower.get('cookie', '')
        if 'deviapauth=' in dev_iap_auth_cookie.lower():
            # Extract email from DevIAPAuth cookie
            cookie_match = re.search(r'deviapauth=([^;]+)', dev_iap_auth_cookie, re.IGNORECASE)
            if cookie_match:
                dev_email_encoded = cookie_match.group(1)
                try:
                    dev_email = urllib.parse.unquote(dev_email_encoded)
                    if '@' in dev_email and '.' in dev_email.split('@')[1]:
                        validation_result["user_email"] = dev_email
                        validation_result["is_development"] = True
                        logger.info(f"🧪 Development mode - Email extracted from DevIAPAuth cookie: {dev_email}")
                    else:
                        logger.warning(f"❌ Invalid email format in DevIAPAuth cookie: {dev_email}")
                except Exception as e:
                    logger.warning(f"❌ Error decoding DevIAPAuth cookie: {e}")

        if is_dev_simulation:
            validation_result["is_development"] = True
            logger.info("🧪 Development IAP simulation detected")

        # Extract and validate IAP headers
        extracted_headers = {}

        for header_name in expected_iap_headers:
            header_value = headers_lower.get(header_name)
            if header_value:
                extracted_headers[header_name] = header_value
                logger.info(f"✅ Found IAP header: {header_name}")
            else:
                validation_result["validation_errors"].append(f"Missing required IAP header: {header_name}")
                logger.warning(f"❌ Missing IAP header: {header_name}")

        # Validate user email format from IAP headers (if not already set from cookie)
        if not validation_result["user_email"]:
            user_email_header = headers_lower.get('x-goog-authenticated-user-email')
            if user_email_header:
                # IAP format: "accounts.google.com:user@domain.com"
                if ':' in user_email_header:
                    _, email = user_email_header.split(':', 1)
                    if '@' in email and '.' in email.split('@')[1]:
                        validation_result["user_email"] = email
                        logger.info(f"✅ Valid user email extracted from IAP header: {email}")
                    else:
                        validation_result["validation_errors"].append("Invalid email format in x-goog-authenticated-user-email")
                        logger.warning(f"❌ Invalid email format: {user_email_header}")
                else:
                    validation_result["validation_errors"].append("Invalid format for x-goog-authenticated-user-email (missing ':' separator)")
                    logger.warning(f"❌ Invalid header format: {user_email_header}")

        # Validate user ID format
        user_id_header = headers_lower.get('x-goog-authenticated-user-id')
        if user_id_header:
            # IAP format: "accounts.google.com:123456789"
            if ':' in user_id_header:
                _, user_id = user_id_header.split(':', 1)
                validation_result["user_id"] = user_id
                logger.info(f"✅ Valid user ID extracted: {user_id}")
            else:
                validation_result["validation_errors"].append("Invalid format for x-goog-authenticated-user-id (missing ':' separator)")
                logger.warning(f"❌ Invalid header format: {user_id_header}")

        # Check for forwarded headers as fallback
        if not validation_result["user_email"]:
            forwarded_email = headers_lower.get('x-forwarded-email')
            if forwarded_email and '@' in forwarded_email:
                validation_result["user_email"] = forwarded_email
                logger.info(f"✅ Using forwarded email as fallback: {forwarded_email}")

        if not validation_result["user_id"]:
            forwarded_user = headers_lower.get('x-forwarded-user')
            if forwarded_user:
                validation_result["user_id"] = forwarded_user
                logger.info(f"✅ Using forwarded user as fallback: {forwarded_user}")

        # Determine if validation is successful
        # In development mode, we're more lenient
        if validation_result["is_development"]:
            # For development, we only need basic email validation
            if validation_result["user_email"]:
                validation_result["valid"] = True
                logger.info("✅ Development mode validation successful")
            else:
                validation_result["validation_errors"].append("Development mode requires valid user email")
                logger.warning("❌ Development mode validation failed - missing user email")
        else:
            # For production, require all standard IAP headers
            required_for_production = ['x-goog-authenticated-user-email', 'x-goog-authenticated-user-id']
            missing_required = [h for h in required_for_production if h not in extracted_headers]

            if not missing_required and validation_result["user_email"]:
                validation_result["valid"] = True
                logger.info("✅ Production IAP validation successful")
            else:
                if missing_required:
                    validation_result["validation_errors"].extend([f"Missing required header for production: {h}" for h in missing_required])
                if not validation_result["user_email"]:
                    validation_result["validation_errors"].append("Valid user email required for production")
                logger.warning("❌ Production IAP validation failed")

        validation_result["extracted_headers"] = extracted_headers

        # Log validation summary
        if validation_result["valid"]:
            logger.info(f"✅ IAP validation successful - User: {validation_result['user_email']}")
        else:
            logger.warning(f"❌ IAP validation failed - Errors: {validation_result['validation_errors']}")

        return validation_result

    except Exception as e:
        error_msg = f"Error during IAP header validation: {str(e)}"
        validation_result["validation_errors"].append(error_msg)
        logger.error(f"❌ {error_msg}")
        return validation_result


def extract_iap_headers_for_forwarding(headers: dict) -> dict:
    """
    Extract ALL headers from incoming request for forwarding to other services.

    This function just returns all headers as-is, no filtering or processing.

    Args:
        headers: Dictionary of request headers

    Returns:
        dict: Dictionary of ALL headers to forward (exactly as received)
    """
    try:
        logger.info(f"📤 Forwarding ALL headers as-is: {len(headers)} headers")
        logger.info(f"📋 Header keys received: {list(headers.keys())}")
        
        # Log header values safely (mask sensitive data)
        for key, value in headers.items():
            if any(sensitive in key.lower() for sensitive in ['authorization', 'token', 'jwt', 'secret', 'password']):
                masked_value = f"{value[:10]}..." if len(value) > 10 else "***"
                logger.info(f"   {key}: {masked_value} (masked)")
            elif 'email' in key.lower():
                logger.info(f"   {key}: {value}")
            elif len(value) > 100:
                logger.info(f"   {key}: {value[:50]}... (truncated, length: {len(value)})")
            else:
                logger.info(f"   {key}: {value}")
        
        return headers

    except Exception as e:
        logger.error(f"❌ Error forwarding headers: {str(e)}")
        return {} 