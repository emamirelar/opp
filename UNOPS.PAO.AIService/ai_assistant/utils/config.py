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

logger = logging.getLogger(__name__)

class ConfigurationError(Exception):
    """Raised when configuration loading fails"""
    pass

def _find_aiservice_config_dir() -> str:
    """
    Find the AIService config directory by checking multiple possible locations.
    
    Returns:
        str: Path to the AIService/config directory
        
    Raises:
        ConfigurationError: If AIService/config directory is not found
    """
    current_dir = Path(__file__).parent.absolute()
    
    # Possible locations to check for AIService folder
    possible_paths = [
         # Jenkins deployment: AIService copied into UNOPS.PAO.AIService directory
        current_dir.parent.parent / "AIService" / "config",  # /app/AIService/config
        # Same level as current directory (for deployment scenarios)
        current_dir / "AIService" / "config",
        # One level up from current directory (development scenario)
        current_dir.parent / "AIService" / "config", 
        # Two levels up (from utils -> ai_assistant -> UNOPS.PAO.AIService -> root)
        current_dir.parent.parent.parent / "AIService" / "config",
        # Three levels up (alternative structure)
        current_dir.parent.parent.parent.parent / "AIService" / "config"
    ]
    
    for path in possible_paths:
        if path.exists() and path.is_dir():
            logger.debug(f"Found AIService config directory at: {path}")
            return str(path)
    
    # If not found, raise an error with helpful information
    searched_paths = [str(p) for p in possible_paths]
    raise ConfigurationError(
        f"AIService/config directory not found. Searched locations:\n" + 
        "\n".join(f"  - {p}" for p in searched_paths)
    )

CONFIG_DIR = _find_aiservice_config_dir() # config directory - dynamically located

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
    # print(f"Application name: {config.get('branding', {}).get('application_name', 'AI Agent')}")
    return config.get('branding', {}).get('application_name', 'AI Agent')


def _parse_connection_string_to_url(connection_string: str) -> str:
    """
    Parse .NET-style connection string to PostgreSQL URL format.
    
    Input format: Username=user;Password=pass;Host=host;Port=port;Database=db;
    Output format: postgresql://user:pass@host:port/db
    """
    try:
        # Parse connection string parameters
        params = {}
        for pair in connection_string.split(';'):
            if '=' in pair:
                key, value = pair.split('=', 1)
                params[key.strip()] = value.strip()
        
        # Extract required parameters
        username = params.get('Username') or params.get('User ID') or params.get('UserId')
        password = params.get('Password')
        host = params.get('Host') or params.get('Server')
        port = params.get('Port', '5432')
        database = params.get('Database') or params.get('Initial Catalog')
        
        # Validate required parameters
        missing_params = []
        if not username:
            missing_params.append('Username/User ID')
        if not password:
            missing_params.append('Password')
        if not host:
            missing_params.append('Host/Server')
        if not database:
            missing_params.append('Database/Initial Catalog')
        
        if missing_params:
            raise ConfigurationError(f"Missing required connection string parameters: {', '.join(missing_params)}")
        
        # URL-encode password if it contains special characters
        import urllib.parse
        encoded_password = urllib.parse.quote(password, safe='')
        
        # Construct PostgreSQL URL with asyncpg driver (required for ADK 1.3.0+)
        postgresql_url = f"postgresql+asyncpg://{username}:{encoded_password}@{host}:{port}/{database}"
        
        logger.info(f"Successfully converted connection string to PostgreSQL URL format (asyncpg)")
        return postgresql_url
        
    except Exception as e:
        raise ConfigurationError(f"Failed to parse connection string: {e}. Connection string format should be: Username=user;Password=pass;Host=host;Port=port;Database=db;")


def get_database_url() -> str:
    """Get the database URL"""
    # PRIORITY 1: Check environment variable first (set by batch scripts with IAM token)
    env_db_url = os.getenv('DATABASE_URL')
    if env_db_url:
        logger.info("Using DATABASE_URL from environment variable (IAM authentication)")
        return env_db_url
    
    config = get_config()
    database_config = config.get('database', {})
    
    # PRIORITY 2: For local development without IAM, use direct URL from config
    if 'url' in database_config:
        logger.info("Using database URL from config file")
        return database_config['url']
    
    # For dev/test/qa environments, get from secrets manager
    if 'secret_name' in database_config:
        secret_name = database_config['secret_name']
        
        # Get project ID from configuration
        google_cloud_config = config.get("google_cloud", {})
        project_id = google_cloud_config.get("project")
        if not project_id:
            raise ConfigurationError("No project configured for Google Cloud")
        
        # Retrieve the connection string from Secret Manager
        connection_string = _get_secret_from_secret_manager(secret_name, project_id)
        
        if not connection_string:
            raise ConfigurationError(f"Failed to retrieve database connection string from secret: {secret_name}")
        
        # Parse .NET connection string to PostgreSQL URL format
        return _parse_connection_string_to_url(connection_string)
    
    raise ConfigurationError("Database configuration must contain either 'url' (for local) or 'secret_name' (for cloud environments)")


def get_oauth_config() -> Dict[str, str]:
    """Get OAuth configuration from the config loader"""
    config = get_config()
    try:
        google_cloud_config = config.get("google_cloud", {})
        oauth_config = google_cloud_config.get("oauth", {})
        
        return {
            "client_id": oauth_config.get("client_id", ""),
            "target_principal": oauth_config.get("target_principal", "")
        }
    except Exception as e:
        print(f"⚠️ Warning: Could not load OAuth config, using defaults: {e}")
        return {}


def _get_secret_from_secret_manager(secret_name: str, project_id: Optional[str] = None) -> Optional[str]:
    """Get secret value from Google Secret Manager"""
    try:
        client = secretmanager.SecretManagerServiceClient()
        name = f"projects/{project_id}/secrets/{secret_name}/versions/latest"
        
        response = client.access_secret_version(request={"name": name})
        secret_value = response.payload.data.decode("UTF-8")
        return secret_value
        
    except Exception as e:
        raise ConfigurationError(f"Failed to retrieve secret {secret_name}: {e}")


def get_identity_toolkit_api_key() -> str:
    """Get Identity Toolkit API key from the config loader"""
    config = get_config()
    try:
        google_cloud_config = config.get("google_cloud", {})
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
        api_key = _get_secret_from_secret_manager(secret_name, project_id)
        
        if api_key:
            return api_key
        else:
            raise ConfigurationError(f"Failed to retrieve Identity Toolkit API key from secret: {secret_name}")
            
    except Exception as e:
        raise ConfigurationError(f"Could not load Identity Toolkit API key from secret: {e}")


def get_tenant_id() -> str:
    """Get tenant ID from the config loader"""
    config = get_config()
    try:
        google_cloud_config = config.get("google_cloud", {})
        oauth_config = google_cloud_config.get("oauth", {})
        
        tenant_id = oauth_config.get("tenant_id")
        if not tenant_id:
            raise ConfigurationError("No tenant_id configured in OAuth settings")
        
        return tenant_id
            
    except Exception as e:
        raise ConfigurationError(f"Could not load tenant ID from config: {e}")


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
