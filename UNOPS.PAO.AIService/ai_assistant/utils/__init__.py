"""
AI Assistant Utils Package

This package contains utility modules for the AI assistant including:
- API configuration management
- Authentication helpers  
- Cache utilities
- Common callbacks and utilities
- Database management
- Framework configuration
- IAP validation
- Session management
- UI configuration management
"""

# Import commonly used utilities for convenience
from .api_config_manager import config_manager
from .auth_helpers import get_service_account_oidc_token

__all__ = [
    'config_manager',
    'get_service_account_oidc_token',
    'invoke_api_tool',
    'construct_api_url', 
    'exit_loop_on_success',
    'detect_entity_from_url'
]
