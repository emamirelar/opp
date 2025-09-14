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

logger = logging.getLogger(__name__)

class ConfigurationError(Exception):
    """Raised when configuration loading fails"""
    pass

# Global config instance
_config_loader: Optional['ConfigLoader'] = None

def set_config_directory(config_dir: str = "config"):
    """
    Set the configuration directory path for the framework.
    
    Teams should call this in their main.py to specify where their config folder is located.
    
    Args:
        config_dir: Path to the config directory (default: "config")
                   Framework expects: config/framework/ and config/tools/
    """
    global _config_loader
    framework_dir = os.path.join(config_dir, "framework")
    _config_loader = ConfigLoader(framework_dir)
    logger.debug(f"Framework config directory: {framework_dir}")
    
    # Also update the API config manager to use the same base path
    from ai_assistant.utils.api_config_manager import config_manager
    config_manager.set_config_directory(config_dir)

class ConfigLoader:
    """Simple configuration loader for JSON files"""
    
    def __init__(self, config_dir: str = "config/framework"):
        self.config_dir = Path(config_dir)
        
        # Check if team has their own config directory
        if not self.config_dir.exists():
            # Fallback to package config directory for development
            package_config = Path(__file__).parent.parent.parent / "config" / "framework"
            if package_config.exists():
                self.config_dir = package_config
                logger.info(f"ℹ️ Using package config directory: {self.config_dir}")
            else:
                logger.warning(f"⚠️ Config directory not found: {config_dir}")
        else:
            logger.info(f"✅ Using team config directory: {self.config_dir}")
            
        self._config: Optional[Dict[str, Any]] = None
        self._environment: str = "dev"
    
    def load_config(self, environment: str = None) -> Dict[str, Any]:
        """Load configuration for the specified environment"""
        print(f"Loading config for environment: {os.getenv('CURRENT_ENV')}")
        if environment is None:
            environment = os.getenv('CURRENT_ENV', 'dev')

        print(f"Loading config for environment: {environment}")
        
        self._environment = environment
        config_file = self.config_dir / f"{environment}.json"
        
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

def get_config() -> Dict[str, Any]:
    """Get the current configuration"""
    global _config_loader
    if _config_loader is None:
        # Auto-initialize with default path if not set by team
        set_config_directory("config")
    return _config_loader.load_config()

def initialize_config(environment: str = None, config_dir: str = "config") -> Dict[str, Any]:
    """
    Initialize the configuration system
    
    Args:
        environment: Environment to load (dev, test, prod)
        config_dir: Base config directory path (team's config folder)
    """
    global _config_loader
    
    # Set the config directory
    set_config_directory(config_dir)
    
    if environment:
        _config_loader.set_environment(environment)
    return _config_loader.load_config()

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
