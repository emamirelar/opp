#!/usr/bin/env python3
"""
Enhanced Configuration Manager
Loads configuration files once and makes them available throughout the application.
Based on patterns from UNOPS.PAO.AgenticAi
"""

import json
import os
from typing import Dict, Any, Optional, List
from functools import lru_cache

# Load base URL from environment variable
API_BASE_URL = os.getenv('API_BASE_URL', 'https://localhost:44426')


class ConfigManager:
    """Singleton configuration manager for tools.json and framework_config.json"""
    
    _instance: Optional['ConfigManager'] = None
    _tools_config: Optional[Dict[str, Any]] = None
    _framework_config: Optional[Dict[str, Any]] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def load_tools_config(self, tools_config_path: str = 'config/tools.json') -> Dict[str, Any]:
        if self._tools_config is None:
            try:
                with open(tools_config_path, 'r', encoding='utf-8') as f:
                    self._tools_config = json.load(f)
                print(f"✅ Loaded tools configuration from {tools_config_path}")
            except FileNotFoundError:
                print(f"❌ Tools config file not found: {tools_config_path}")
                self._tools_config = {"entities": []}
            except json.JSONDecodeError as e:
                print(f"❌ Invalid JSON in tools config: {e}")
                self._tools_config = {"entities": []}
        return self._tools_config
    
    def load_framework_config(self, framework_config_path: str = None) -> Dict[str, Any]:
        if self._framework_config is None:
            try:
                # Use environment-based configuration loading
                from framework_config import get_config, get_environment
                try:
                    self._framework_config = get_config()
                    environment = get_environment()
                    print(f"✅ Loaded framework configuration from config/framework_config_{environment}.json")
                except Exception as e:
                    # Fallback to direct file loading if framework_config system isn't initialized
                    if framework_config_path is None:
                        import os
                        environment = os.getenv('ENVIRONMENT', 'dev')
                        framework_config_path = f'config/framework_config_{environment}.json'
                    
                    with open(framework_config_path, 'r', encoding='utf-8') as f:
                        self._framework_config = json.load(f)
                    print(f"✅ Loaded framework configuration from {framework_config_path}")
                    
            except FileNotFoundError:
                config_path = framework_config_path or 'config/framework_config.json'
                print(f"❌ Framework config file not found: {config_path}")
                self._framework_config = {}
            except json.JSONDecodeError as e:
                print(f"❌ Invalid JSON in framework config: {e}")
                self._framework_config = {}
        return self._framework_config
    
    @property
    def tools_config(self) -> Dict[str, Any]:
        if self._tools_config is None:
            self.load_tools_config()
        return self._tools_config
    
    @property
    def framework_config(self) -> Dict[str, Any]:
        if self._framework_config is None:
            self.load_framework_config()
        return self._framework_config
    
    def get_cache_rules(self) -> Dict[str, List[str]]:
        return self.framework_config.get("cache", {}).get("rules", {})
    
    def get_url_patterns(self) -> Dict[str, str]:
        return self.framework_config.get("cache", {}).get("url_patterns", {})
    
    def get_cache_ttl(self) -> Dict[str, int]:
        return self.framework_config.get("cache", {}).get("ttl", {})
    
    def get_support_info(self) -> Dict[str, str]:
        return self.framework_config.get("support", {})
    
    def get_branding(self) -> Dict[str, str]:
        return self.framework_config.get("branding", {})
    
    def get_roles(self) -> Dict[str, Any]:
        return self.framework_config.get("roles", {})
    
    def get_defaults(self) -> Dict[str, Any]:
        return self.framework_config.get("defaults", {})
    
    def get_entity_specific_tools(self, entity_name: str) -> str:
        """
        Generate API endpoints summary filtered for a specific entity
        
        Args:
            entity_name: The entity to filter for (e.g., "Partner", "Contact", "Interaction")
        
        Returns:
            str: Formatted summary of endpoints for the specific entity
        """
        entities = self.tools_config.get('entities', [])
        base_url = API_BASE_URL
        
        # Find the specific entity
        target_entity = None
        for entity in entities:
            if entity.get('entity', '').lower() == entity_name.lower():
                target_entity = entity
                break
            # Also check synonyms
            synonyms = entity.get('synonyms', [])
            if any(synonym.lower() == entity_name.lower() for synonym in synonyms):
                target_entity = entity
                break
        
        if not target_entity:
            return f"❌ No endpoints found for entity: {entity_name}"
        
        summary = f"📡 **API Endpoints for {target_entity.get('entity', entity_name)}:**\n\n"
        
        entity_desc = target_entity.get('description', '')
        synonyms = target_entity.get('synonyms', [])
        endpoints = target_entity.get('endpoints', [])
        
        summary += f"**Entity Description:** {entity_desc}\n"
        if synonyms:
            summary += f"**Synonyms:** {', '.join(synonyms)}\n"
        summary += "\n"
        
        for endpoint in endpoints:
            name = endpoint.get('name', '')
            url = endpoint.get('url', '')
            method = endpoint.get('method', 'GET')
            desc = endpoint.get('description', '')
            when_to_use = endpoint.get('when_to_use', '')
            example_uses = endpoint.get('example_uses', [])
            
            # Build full URL
            full_url = f"{base_url}{url}" if not url.startswith('http') else url
            summary += f"**{name}:** `{method} {full_url}`\n"
            summary += f"  - Description: {desc}\n"
            
            if when_to_use:
                summary += f"  - When to use: {when_to_use}\n"
                
            if example_uses:
                summary += f"  - Examples: {', '.join(example_uses)}\n"
                
            # Add parameters
            parameters = endpoint.get('parameters', {})
            if parameters:
                required_params = [k for k, v in parameters.items() if v.get('required', False)]
                optional_params = [k for k, v in parameters.items() if not v.get('required', False)]
                
                if required_params:
                    summary += f"  - Required parameters: {', '.join(required_params)}\n"
                if optional_params:
                    summary += f"  - Optional parameters: {', '.join(optional_params)}\n"
            
            summary += "\n"
        
        return summary
    
    def get_api_endpoints_summary(self) -> str:
        """Generate a formatted summary of ALL available API endpoints"""
        summary = "**📡 Available API Endpoints (All Entities):**\n\n"
        
        entities = self.tools_config.get('entities', [])
        base_url = API_BASE_URL
        
        if entities:
            for entity in entities:
                entity_name = entity.get('entity', '')
                description = entity.get('description', '')
                synonyms = entity.get('synonyms', [])
                endpoints = entity.get('endpoints', [])
                
                if endpoints:
                    summary += f"**🔧 {entity_name} Endpoints:**\n"
                    summary += f"*{description}*\n"
                    if synonyms:
                        summary += f"*Synonyms: {', '.join(synonyms)}*\n"
                    
                    for endpoint in endpoints:
                        name = endpoint.get('name', '')
                        url = endpoint.get('url', '')
                        method = endpoint.get('method', 'GET')
                        desc = endpoint.get('description', '')
                        when_to_use = endpoint.get('when_to_use', '')
                        example_uses = endpoint.get('example_uses', [])
                        
                        # Build full URL
                        full_url = f"{base_url}{url}" if not url.startswith('http') else url
                        summary += f"• `{method} {full_url}` - {desc}\n"
                        
                        if when_to_use:
                            summary += f"  *When to use: {when_to_use}*\n"
                            
                        if example_uses:
                            summary += f"  *Examples: {', '.join(example_uses[:2])}*\n"
                            
                        # Add parameters
                        parameters = endpoint.get('parameters', {})
                        if parameters:
                            required_params = [k for k, v in parameters.items() if v.get('required', False)]
                            optional_params = [k for k, v in parameters.items() if not v.get('required', False)]
                            
                            if required_params:
                                summary += f"  *Required: {', '.join(required_params)}*\n"
                            if optional_params:
                                summary += f"  *Optional: {', '.join(optional_params)}*\n"
                    
                    summary += "\n"
        
        return summary
    
    def get_entities(self) -> List[Dict[str, Any]]:
        """Get all entities from tools configuration"""
        return self.tools_config.get("entities", [])
    
    def get_entity_by_name(self, entity_name: str) -> Optional[Dict[str, Any]]:
        """Get entity configuration by name"""
        entities = self.get_entities()
        for entity in entities:
            if entity.get("entity", "").lower() == entity_name.lower():
                return entity
            # Also check synonyms
            synonyms = entity.get('synonyms', [])
            if any(synonym.lower() == entity_name.lower() for synonym in synonyms):
                return entity
        return None
    
    def get_entity_synonyms(self, entity_name: str) -> List[str]:
        """Get synonyms for an entity"""
        entity = self.get_entity_by_name(entity_name)
        if entity:
            return entity.get("synonyms", [])
        return []
    
    def get_entity_endpoints(self, entity_name: str) -> List[Dict[str, Any]]:
        """Get all endpoints for an entity"""
        entity = self.get_entity_by_name(entity_name)
        if entity:
            return entity.get("endpoints", [])
        return []
    
    def find_endpoint_by_entity_and_intent(self, entity_name: str, intent: str) -> Optional[Dict[str, Any]]:
        """
        Find the best endpoint for a given entity and intent combination
        
        Args:
            entity_name: The entity name (e.g., "Contact", "Partner")
            intent: The intent (e.g., "search", "create", "update")
        
        Returns:
            Dict containing the best matching endpoint configuration
        """
        endpoints = self.get_entity_endpoints(entity_name)
        
        # Intent to HTTP method mapping
        intent_method_mapping = {
            'search': 'GET',
            'list': 'GET', 
            'get': 'GET',
            'create': 'POST',
            'add': 'POST',
            'update': 'PUT',
            'modify': 'PUT',
            'delete': 'DELETE',
            'remove': 'DELETE'
        }
        
        target_method = intent_method_mapping.get(intent.lower(), 'GET')
        
        # First, try to find exact method match
        for endpoint in endpoints:
            if endpoint.get('method', 'GET').upper() == target_method.upper():
                # Additional checks for better matching
                endpoint_name = endpoint.get('name', '').lower()
                if intent.lower() in endpoint_name:
                    return endpoint
        
        # Fallback to first endpoint with matching method
        for endpoint in endpoints:
            if endpoint.get('method', 'GET').upper() == target_method.upper():
                return endpoint
        
        # Final fallback to first available endpoint
        if endpoints:
            return endpoints[0]
        
        return None
    
    def get_entity_detection_config(self) -> str:
        """
        Generate entity detection configuration for dynamic instruction
        """
        entities = self.get_entities()
        detection_config = "**🎯 DYNAMIC ENTITY DETECTION CONFIGURATION:**\n\n"
        
        detection_config += "**Available Entities:**\n"
        for entity in entities:
            entity_name = entity.get('entity', '')
            description = entity.get('description', '')
            synonyms = entity.get('synonyms', [])
            
            detection_config += f"• **{entity_name}**: {description}\n"
            if synonyms:
                detection_config += f"  - Synonyms: {', '.join(synonyms)}\n"
        
        detection_config += "\n**Intent Patterns:**\n"
        detection_config += "• **search/list/get**: Use for finding or retrieving data\n"
        detection_config += "• **create/add**: Use for creating new records\n" 
        detection_config += "• **update/modify**: Use for updating existing records\n"
        detection_config += "• **delete/remove**: Use for deleting records\n"
        
        return detection_config


# Global singleton instance
config_manager = ConfigManager() 