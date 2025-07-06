#!/usr/bin/env python3
"""
Simple Configuration Loader

Loads configuration from framework_config_<environment>.json files.
Environment is determined by the ENVIRONMENT variable.
"""

import json
import os
import logging
from typing import Dict, Any, Optional
from pathlib import Path

logger = logging.getLogger(__name__)

class ConfigurationError(Exception):
    """Raised when configuration loading fails"""
    pass

class ConfigLoader:
    """Simple configuration loader for JSON files"""
    
    def __init__(self, config_dir: str = "config"):
        self.config_dir = Path(config_dir)
        self._config: Optional[Dict[str, Any]] = None
        self._environment: str = "dev"
    
    def load_config(self, environment: str = None) -> Dict[str, Any]:
        """Load configuration for the specified environment"""
        if environment is None:
            environment = os.getenv('ENVIRONMENT', 'dev')
        
        self._environment = environment
        config_file = self.config_dir / f"framework_config_{environment}.json"
        
        if not config_file.exists():
            raise ConfigurationError(f"Configuration file not found: {config_file}")
        
        try:
            with open(config_file, 'r', encoding='utf-8') as f:
                self._config = json.load(f)
            
            logger.info(f"Configuration loaded successfully for environment: {environment}")
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

# Global configuration instance
_config_loader: Optional[ConfigLoader] = None

def initialize_config(environment: str = None) -> Dict[str, Any]:
    """Initialize the global configuration loader"""
    global _config_loader
    _config_loader = ConfigLoader()
    return _config_loader.load_config(environment)

def get_config() -> Dict[str, Any]:
    """Get the global configuration"""
    global _config_loader
    if _config_loader is None:
        raise ConfigurationError("Configuration not initialized. Call initialize_config() first.")
    return _config_loader.get_config()

def get_environment() -> str:
    """Get the current environment"""
    global _config_loader
    if _config_loader is None:
        return "dev"  # Default fallback
    return _config_loader.get_environment()

def validate_config() -> Dict[str, Any]:
    """Validate the current configuration"""
    try:
        config = get_config()
        issues = []
        
        # Basic validation
        required_sections = ['branding', 'server', 'database', 'cache', 'google_drive']
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

def get_environment_info() -> Dict[str, Any]:
    """Get detailed environment information"""
    try:
        config = get_config()
        environment = get_environment()
        
        return {
            "environment": environment,
            "config_loaded": True,
            "branding": config.get("branding", {}),
            "server": config.get("server", {}),
            "features": config.get("features", {}),
            "google_drive_enabled": config.get("google_drive", {}).get("enabled", False),
            "cache_enabled": config.get("cache", {}).get("enable_cache", True)
        }
    except ConfigurationError:
        return {
            "environment": "unknown",
            "config_loaded": False,
            "error": "Configuration not initialized"
        }
