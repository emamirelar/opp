#!/usr/bin/env python3
"""
Simple Configuration Loader

Loads configuration from <environment>.json files.
Environment is determined by the CURRENT_ENV variable.
"""

import json
import os
import logging
from typing import Dict, Any, Optional
from pathlib import Path
from google.cloud import secretmanager

CONFIG_DIR = "../AIService/config" # config directory - this should ALWAYS be the same for ALL projects 


logger = logging.getLogger(__name__)

class ConfigurationError(Exception):
    """Raised when configuration loading fails"""
    pass

# Global config instance
_config_loader: Optional['ConfigLoader'] = None

def set_config_directory(config_dir: str = CONFIG_DIR):
    """
    Set the configuration directory path.
    
    Call this in their main.py to specify where their config folder is located.
    
    Args:
        config_dir: Path to the config directory (default: CONFIG_DIR)
                    Application expects config files directly in this directory
    """
    global _config_loader
    _config_loader = ConfigLoader(config_dir)
    logger.debug(f"Application config directory: {config_dir}")
    

class ConfigLoader:
    """Simple configuration loader for JSON files"""
    
    def __init__(self, config_dir: str = CONFIG_DIR):
        print(f"============== config.py: CONFIGLOADER INITIALIZED ==============")
        self.config_dir = Path(config_dir)
        self._config: Optional[Dict[str, Any]] = None
        self._environment: str = os.getenv('CURRENT_ENV')
        # Raise an error if the environment is not set
        if self._environment is None:
            raise ValueError("CURRENT_ENV is not set")
    

    def load_config(self) -> Dict[str, Any]:
        """Load configuration for the specified environment"""
        environment = self._environment
        print(f"Loading config for environment: {environment}")
        if environment is None:
            raise ConfigurationError("Environment is not set. Please set the CURRENT_ENV environment variable.")
        
        config_file = self.config_dir / f"{environment}.json"
        
        if not config_file.exists():
            raise ConfigurationError(f"Configuration file not found: {config_file}")
        
        try:
            with open(config_file, 'r', encoding='utf-8') as f:
                self._config = json.load(f)
            
            logger.info(f"Configuration loaded successfully for environment: {environment}")
            print(f"Developer email: {self._config.get('developer', {}).get('email', '')}")
            return self._config
            
        except json.JSONDecodeError as e:
            raise ConfigurationError(f"Invalid JSON in configuration file: {e}")
        except Exception as e:
            raise ConfigurationError(f"Failed to load configuration: {e}")
    

    def get_config(self) -> Dict[str, Any]:
        """Get the loaded configuration"""
        if self._config is None:
            raise ConfigurationError("Configuration not loaded. Call load_config() first.")
        return self._config
    

    def get_environment(self) -> str:
        """Get the current environment"""
        return self._environment


    def get_database_url(self) -> str:
        """Get the database URL"""
        return self._config.get('database', {}).get('url')
    

    def get_oauth_config(self) -> Dict[str, str]:
        """Get OAuth configuration from google_cloud section"""
        try:
            google_cloud_config = self._config.get("google_cloud", {})
            oauth_config = google_cloud_config.get("oauth", {})
            
            return {
                "client_id": oauth_config.get("client_id", ""),
                "target_principal": oauth_config.get("target_principal", "")
            }
        except Exception as e:
            print(f"⚠️ Warning: Could not load OAuth config, using defaults: {e}")
            return {}
    

    def get_identity_toolkit_api_key(self) -> str:
        """Get Identity Toolkit API key from Google Secret Manager with caching"""
        try:
            google_cloud_config = self._config.get("google_cloud", {})
            oauth_config = google_cloud_config.get("oauth", {})
            
            # Get the secret name from configuration
            secret_name = oauth_config.get("identity_toolkit_api_key_secret")
            if not secret_name:
                raise ConfigurationError("No identity_toolkit_api_key_secret configured in OAuth settings")
            
            # Get project ID from configuration
            project_id = google_cloud_config.get("project")
            if not project_id:
                raise ConfigurationError("No project configured for Google Cloud")
            
            # Retrieve the secret from Secret Manager
            api_key = self.get_secret_from_secret_manager(secret_name, project_id)
            
            if api_key:
                return api_key
            else:
                raise ConfigurationError(f"Failed to retrieve Identity Toolkit API key from secret: {secret_name}")
                
        except Exception as e:
            raise ConfigurationError(f"Could not load Identity Toolkit API key from secret: {e}")


    def get_secret_from_secret_manager(self, secret_name: str, project_id: Optional[str] = None) -> Optional[str]:
        """Get secret value from Google Secret Manager"""
        try:
            client = secretmanager.SecretManagerServiceClient()
            name = f"projects/{project_id}/secrets/{secret_name}/versions/latest"
            
            response = client.access_secret_version(request={"name": name})
            secret_value = response.payload.data.decode("UTF-8")
            return secret_value
            
        except Exception as e:
            raise ConfigurationError(f"Failed to retrieve secret {secret_name}: {e}")
    


def get_config() -> Dict[str, Any]:
    """Get the current configuration"""
    global _config_loader
    if _config_loader is None:
        # Auto-initialize with default path
        set_config_directory(CONFIG_DIR)
    return _config_loader.load_config()


def get_environment() -> str:
    """Get the current environment"""
    global _config_loader
    if _config_loader is None:
        raise ConfigurationError("Configuration not loaded. Call get_config() first.")
    return _config_loader.get_environment()


def get_application_name() -> str:
    """Get the application name"""
    config = get_config()
    return config.get('branding', {}).get('application_name', 'AI Agent')

def get_database_url() -> str:
    """Get the database URL"""
    global _config_loader
    if _config_loader is None:
        raise ConfigurationError("Configuration not loaded. Call get_config() first.")
    return _config_loader.get_database_url()

def get_oauth_config() -> Dict[str, str]:
    """Get OAuth configuration from the config loader"""
    global _config_loader
    if _config_loader is None:
        # Auto-initialize with default path
        set_config_directory(CONFIG_DIR)
    return _config_loader.get_oauth_config()

def get_identity_toolkit_api_key() -> str:
    """Get Identity Toolkit API key from the config loader"""
    global _config_loader
    if _config_loader is None:
        # Auto-initialize with default path
        set_config_directory(CONFIG_DIR)
    return _config_loader.get_identity_toolkit_api_key()


def get_api_base_url() -> str:
    """Get the API base URL"""
    config = get_config()
    return config.get('server', {}).get('api_base_url', '')


def get_api_timeout() -> int:
    """Get the API timeout"""
    config = get_config()
    return config.get('runtime', {}).get('api_timeout', 30)


def get_gemini_adhoc_model() -> str:
    """Get the Gemini adhoc model"""
    config = get_config()
    return config.get('runtime', {}).get('gemini_adhoc_model', 'gemini-2.0-flash-001')



def validate_config() -> Dict[str, Any]:
    """Validate the current configuration"""
    try:
        config = get_config()
        issues = []
        
        # Basic validation
        required_sections = ['branding', 'server', 'database']
        for section in required_sections:
            if section not in config:
                issues.append(f"Missing required section: {section}")
        
        return {
            "valid": len(issues) == 0,
            "issues": issues,
            "environment": get_environment()
        }
    except ConfigurationError as e:
        return {
            "valid": False,
            "issues": [str(e)],
            "environment": "unknown"
        }
