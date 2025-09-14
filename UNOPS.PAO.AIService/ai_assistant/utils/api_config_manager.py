#!/usr/bin/env python3
"""
Enhanced Configuration Manager
Loads configuration files once and makes them available throughout the application.
Based on patterns from UNOPS.PAO.AgenticAi
"""

import json
import os
import time
from typing import Any, Dict, List, Optional, Set

from google.cloud import secretmanager


class ApiConfigManager:
    """Singleton configuration manager for entity-specific tools and framework configuration"""
    
    _instance: Optional['ConfigManager'] = None
    _tools_config: Optional[Dict[str, Any]] = None
    _framework_config: Optional[Dict[str, Any]] = None
    _entity_tools_cache: Dict[str, Dict[str, Any]] = {}  # Cache for entity-specific tool configs
    _discovered_entities: Optional[List[str]] = None  # Cache for discovered entities
    
    # Cache for Identity Toolkit API key
    _cached_identity_toolkit_api_key: Optional[str] = None
    _api_key_cache_expiry: float = 0
    _api_key_cache_ttl: int = 3600  # 1 hour TTL
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def load_tools_config(self, tools_config_path: str = None) -> Dict[str, Any]:
        """Load the tools configuration (team's or fallback)"""
        if self._tools_config is None:
            # If no specific path provided, use the configured directory
            if tools_config_path is None:
                tools_config_path = os.path.join(self._config_directory, "tools.json")
            
            # First try team's tools.json in their config directory
            if not os.path.exists(tools_config_path):
                # Fallback: try config/tools/endpoints/ directory structure
                endpoints_dir = os.path.join(self._config_directory, "tools", "endpoints")
                if os.path.exists(endpoints_dir):
                    # Load all JSON files from endpoints directory
                    self._tools_config = self._load_from_endpoints_directory(endpoints_dir)
                else:
                    # Final fallback to package default
                    package_tools = os.path.join(
                        os.path.dirname(__file__), "..", "..", "config", "tools", "tools.json"
                    )
                    if os.path.exists(package_tools):
                        tools_config_path = package_tools
                        print(f"ℹ️ Using package default tools config")
                    else:
                        print(f"❌ No tools config found")
                        self._tools_config = {"entities": []}
                        return self._tools_config
            
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
    
    def _load_from_endpoints_directory(self, endpoints_dir: str) -> Dict[str, Any]:
        """Load entity configurations from config/tools/endpoints/ directory"""
        entities = []
        loaded_configs = []
        failed_configs = []
        
        for json_file in os.listdir(endpoints_dir):
            if json_file.endswith('.json'):
                try:
                    with open(os.path.join(endpoints_dir, json_file), 'r', encoding='utf-8') as f:
                        entity_config = json.load(f)
                        if "entities" in entity_config:
                            entities.extend(entity_config["entities"])
                        loaded_configs.append(json_file)
                except Exception as e:
                    failed_configs.append(f"{json_file}: {e}")
        
        # Summary logging instead of individual file messages
        if loaded_configs:
            print(f"✅ Loaded {len(loaded_configs)} entity configs")
        if failed_configs:
            print(f"❌ Failed to load {len(failed_configs)} configs:")
            for failure in failed_configs:
                print(f"   - {failure}")
        
        return {"entities": entities}
    
    def load_entity_api_config(self, entity_name: str) -> Dict[str, Any]:
        """
        Load entity-specific tools configuration
        
        Args:
            entity_name: The entity name (e.g., "Partner", "Contact")
            
        Returns:
            Dict containing the entity-specific tools configuration
            
        Raises:
            FileNotFoundError: If neither entity-specific nor default config found
        """
        entity_lower = entity_name.lower()
        
        # Check cache first
        if entity_lower in self._entity_tools_cache:
            print(f"✅ Using cached tools config for entity: {entity_name}")
            return self._entity_tools_cache[entity_lower]
        
        # Try entity-specific files with different naming patterns in endpoints directory
        possible_filenames = [
            f"config/tools/endpoints/{entity_lower}_tools.json",  # snake_case
            f"config/tools/endpoints/{entity_lower}-tools.json",  # kebab-case
            f"config/tools/endpoints/{entity_name.lower()}_tools.json",  # exact case
            f"config/tools/endpoints/{entity_name.lower()}-tools.json"   # exact case
        ]
        
        entity_config = None
        used_path = None
        
        # Try to load entity-specific config
        for config_path in possible_filenames:
            try:
                with open(config_path, 'r', encoding='utf-8') as f:
                    entity_config = json.load(f)
                    used_path = config_path
                    print(f"✅ Loaded entity-specific tools config from {config_path}")
                    break
            except FileNotFoundError:
                continue
            except json.JSONDecodeError as e:
                print(f"❌ Invalid JSON in entity config {config_path}: {e}")
                continue
        
        # If no entity-specific config found, try default tools.json
        if entity_config is None:
            try:
                default_config = self.load_tools_config()
                # Filter default config for this entity
                entity_config = self._extract_entity_from_default_config(entity_name, default_config)
                used_path = "config/tools.json (filtered)"
                print(f"⚠️ No entity-specific config found for {entity_name}, using filtered default config")
            except Exception as e:
                print(f"❌ Failed to load default config for entity {entity_name}: {e}")
        
        # If still no config, raise error
        if entity_config is None:
            error_msg = f"❌ No tools configuration found for entity '{entity_name}'. Tried: {', '.join(possible_filenames)} and config/tools.json"
            print(error_msg)
            raise FileNotFoundError(error_msg)
        
        # Normalize entity-specific config format
        # If the config doesn't have "entities" array, wrap it in one
        if entity_config and 'entities' not in entity_config and 'entity' in entity_config:
            entity_config = {'entities': [entity_config]}
            print(f"🔄 Normalized entity-specific config format for {entity_name}")
        
        # Cache the result
        self._entity_tools_cache[entity_lower] = entity_config
        print(f"💾 Cached tools config for entity: {entity_name} (source: {used_path})")
        
        return entity_config
    
    def _extract_entity_from_default_config(self, entity_name: str, default_config: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        """Extract entity-specific configuration from default tools.json"""
        entities = default_config.get('entities', [])
        
        for entity in entities:
            if entity.get('entity', '').lower() == entity_name.lower():
                return {'entities': [entity]}
            # Also check synonyms
            synonyms = entity.get('synonyms', [])
            if any(synonym.lower() == entity_name.lower() for synonym in synonyms):
                return {'entities': [entity]}
        
        return None
    
    def discover_entities_from_files(self) -> List[str]:
        """
        Discover available entities by scanning the config/tools directory
        
        Returns:
            List of entity names discovered from tool files
        """
        if self._discovered_entities is not None:
            return self._discovered_entities
        
        entities = set()
        tools_dir = "config/tools/endpoints"
        
        try:
            if os.path.exists(tools_dir):
                for filename in os.listdir(tools_dir):
                    if filename.endswith('-tools.json') or filename.endswith('_tools.json'):
                        # Extract entity name from filename
                        if filename.endswith('-tools.json'):
                            entity_name = filename.replace('-tools.json', '')
                        else:
                            entity_name = filename.replace('_tools.json', '')
                        
                        # Handle camelCase entity names properly
                        # Convert snake_case or kebab-case to camelCase
                        if '-' in entity_name or '_' in entity_name:
                            # Split by - or _ and capitalize each part
                            parts = entity_name.replace('-', '_').split('_')
                            entity_name = ''.join(part.capitalize() for part in parts)
                        else:
                            # For simple names, just capitalize
                            entity_name = entity_name.capitalize()
                        entities.add(entity_name)
                        
                print(f"🔍 Discovered {len(entities)} entities from tool files: {', '.join(sorted(entities))}")
            else:
                print(f"⚠️ Tools directory not found: {tools_dir}")
                
        except Exception as e:
            print(f"❌ Error discovering entities: {e}")
        
        # Cache the result
        self._discovered_entities = sorted(list(entities))
        return self._discovered_entities
    
    def get_available_entities(self) -> List[str]:
        """
        Get list of all available entities (from files + default config)
        
        Returns:
            List of entity names
        """
        # Get entities from files
        file_entities = set(self.discover_entities_from_files())
        
        # Get entities from default config
        default_entities = set()
        try:
            default_config = self.load_tools_config()
            for entity in default_config.get('entities', []):
                entity_name = entity.get('entity', '')
                if entity_name:
                    # Use the exact entity name from config (preserve camelCase)
                    default_entities.add(entity_name)
        except Exception as e:
            print(f"⚠️ Could not load default config entities: {e}")
        
        # Combine and return
        all_entities = sorted(list(file_entities.union(default_entities)))
        print(f"📋 Total available entities: {len(all_entities)} - {', '.join(all_entities)}")
        return all_entities
    
    def load_framework_config(self, framework_config_path: Optional[str] = None) -> Dict[str, Any]:
        if self._framework_config is None:
            try:
                # Use environment-based configuration loading
                from ai_assistant.utils.framework_config import get_config, get_environment
                try:
                    self._framework_config = get_config()
                    environment = get_environment()
                    print(f"✅ Loaded framework configuration from config/framework/{environment}.json")
                except Exception:
                    # Fallback to direct file loading if framework_config system isn't initialized
                    if framework_config_path is None:
                        import os
                        environment = os.getenv('CURRENT_ENV', 'dev')
                        framework_config_path = f'config/framework/{environment}.json'
                    
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
    
    def get_project_name(self) -> str:
        """Get project name from branding configuration with safe fallback"""
        try:
            branding = self.framework_config.get("branding", {})
            return branding.get("project_name", "AI Assistant")
        except Exception as e:
            print(f"⚠️ Warning: Could not load project_name from config, using default: {e}")
            return "AI Assistant"
    
    def get_application_name(self) -> str:
        """Get application name from branding configuration with safe fallback"""
        try:
            branding = self.framework_config.get("branding", {})
            return branding.get("application_name", "Application")
        except Exception as e:
            print(f"⚠️ Warning: Could not load application_name from config, using default: {e}")
            return "Application"
    
    def get_secret_from_secret_manager(self, secret_name: str, project_id: Optional[str] = None) -> Optional[str]:
        """Get secret value from Google Secret Manager"""
        try:
            if not project_id:
                project_id = os.getenv('GOOGLE_CLOUD_PROJECT')
            
            if not project_id:
                print("❌ No project ID available for Secret Manager access")
                return None
                
            client = secretmanager.SecretManagerServiceClient()
            name = f"projects/{project_id}/secrets/{secret_name}/versions/latest"
            
            print(f"🔐 Retrieving secret: {secret_name} from project: {project_id}")
            response = client.access_secret_version(request={"name": name})
            secret_value = response.payload.data.decode("UTF-8")
            print(f"✅ Successfully retrieved secret: {secret_name}")
            return secret_value
            
        except Exception as e:
            print(f"❌ Failed to retrieve secret {secret_name}: {e}")
            return None
    
    def _convert_connection_string_to_sqlalchemy_url(self, connection_string: str) -> str:
        """
        Convert .NET connection string format to SQLAlchemy URL format
        
        Args:
            connection_string: .NET format like "Username=postgres;Password=pass;Host=host;Port=5432;Database=db;"
        
        Returns:
            str: SQLAlchemy URL format like "postgresql://postgres:pass@host:5432/db"
        """
        try:
            print("🔄 Converting connection string to SQLAlchemy URL format...")
            
            # Parse the connection string
            params = {}
            for pair in connection_string.split(';'):
                if '=' in pair and pair.strip():
                    key, value = pair.split('=', 1)
                    params[key.strip().lower()] = value.strip()
            
            print(f"📋 Parsed connection parameters: {list(params.keys())}")
            
            # Extract required components
            username = params.get('username', '')
            password = params.get('password', '')
            host = params.get('host', 'localhost')
            port = params.get('port', '5432')
            database = params.get('database', '')
            
            if not username or not password or not host or not database:
                missing = []
                if not username: missing.append('username')
                if not password: missing.append('password') 
                if not host: missing.append('host')
                if not database: missing.append('database')
                raise ValueError(f"Missing required connection parameters: {missing}")
            
            # URL encode the password to handle special characters like '/'
            from urllib.parse import quote_plus
            encoded_password = quote_plus(password)
            
            # Build SQLAlchemy URL
            url = f"postgresql://{username}:{encoded_password}@{host}:{port}/{database}"
            
            print("✅ Successfully converted to SQLAlchemy URL")
            print(f"🔗 Final URL format: postgresql://{username}:***@{host}:{port}/{database}")
            return url
            
        except Exception as e:
            print(f"❌ Error converting connection string: {e}")
            print(f"⚠️ Original connection string format: {connection_string[:100]}...")
            # Return original string as fallback - let SQLAlchemy give its own error
            return connection_string

    def get_database_url(self) -> str:
        """Get database URL, with Secret Manager support for test/prod environments"""
        try:
            # Get environment
            environment = os.getenv('CURRENT_ENV', 'dev')
            db_config = self.framework_config.get("database", {})
            
            print(f"🌍 Environment: {environment}")
            
            # For development, use the config file directly
            if environment == 'dev':
                dev_url = db_config.get("url", "sqlite:///./ai_agent.db")
                print("🛠️ Development environment - using config file URL")
                return dev_url
            
            # For test/prod environments, try Secret Manager first if secret_name is configured
            if environment in ['test', 'prod']:
                secret_name = db_config.get("secret_name")
                
                if secret_name:
                    print(f"🔐 Looking for database secret: {secret_name}")
                    secret_url = self.get_secret_from_secret_manager(secret_name)
                    
                    if secret_url:
                        print(f"✅ Retrieved database URL from Secret Manager ({secret_name})")
                        print(f"📋 Raw secret format (first 50 chars): {secret_url[:50]}...")
                        
                        # Check if it's a .NET connection string format (contains semicolons and equals)
                        if ';' in secret_url and '=' in secret_url:
                            print("🔄 Detected .NET connection string format - converting to SQLAlchemy URL...")
                            converted_url = self._convert_connection_string_to_sqlalchemy_url(secret_url)
                            return converted_url
                        else:
                            print("✅ Already in SQLAlchemy URL format")
                            return secret_url
                    else:
                        print(f"⚠️ Failed to get database URL from Secret Manager ({secret_name}), falling back to config")
                else:
                    print(f"⚠️ No secret_name configured for {environment} environment, using config file")
            
            # Fallback to config file
            config_url = db_config.get("url", "sqlite:///./ai_agent.db")
            print(f"ℹ️ Using database URL from config file: {config_url[:50]}...")
            return config_url
            
        except Exception as e:
            print(f"❌ Error getting database URL: {e}")
            print("🔄 Falling back to SQLite...")
            return "sqlite:///./ai_agent.db"
    
    def get_roles(self) -> Dict[str, Any]:
        return self.framework_config.get("roles", {})
    
    def get_defaults(self) -> Dict[str, Any]:
        return self.framework_config.get("defaults", {})
    
    def get_api_base_url(self) -> str:
        """Get API base URL from framework configuration"""
        server_config = self.framework_config.get("server", {})
        return server_config.get("api_base_url", "https://localhost:44426")
    
    def get_gemini_model(self) -> str:
        """Get Gemini model from framework configuration with safe fallback"""
        try:
            runtime_config = self.framework_config.get("runtime", {})
            return runtime_config.get("gemini_model", "gemini-2.5-flash")
        except Exception as e:
            print(f"⚠️ Warning: Could not load gemini_model from config, using default: {e}")
            return "gemini-2.5-flash"
    
    def get_gemini_adhoc_model(self) -> str:
        """Get Gemini adhoc model from framework configuration with safe fallback"""
        try:
            runtime_config = self.framework_config.get("runtime", {})
            return runtime_config.get("gemini_adhoc_model", "gemini-2.0-flash-001")
        except Exception as e:
            print(f"⚠️ Warning: Could not load gemini_adhoc_model from config, using default: {e}")
            return "gemini-2.0-flash-001"

    def get_user_profile_config(self) -> Optional[Dict[str, Any]]:
        """
        Get user profile endpoint configuration from framework config.
        Simple approach: just return the hardcoded endpoint name and entity.
        """
        try:
            user_profile_config = self.framework_config.get("user_profile", {})
            if user_profile_config and "endpoint_name" in user_profile_config and "entity" in user_profile_config:
                print(f"✅ Found user profile config: {user_profile_config}")
                return user_profile_config
            else:
                print("❌ No valid user_profile config found in framework config")
                return None
                
        except Exception as e:
            print(f"❌ Error getting user_profile config: {e}")
            return None

    def get_api_timeout(self) -> int:
        """Get API timeout from runtime config with safe fallback"""
        try:
            runtime_config = self.framework_config.get("runtime", {})
            return runtime_config.get("api_timeout", 30)
        except Exception as e:
            print(f"⚠️ Warning: Could not load api_timeout from config, using default: {e}")
            return 30

    def get_max_retries(self) -> int:
        """Get max retries from runtime config with safe fallback"""
        try:
            runtime_config = self.framework_config.get("runtime", {})
            return runtime_config.get("max_retries", 3)
        except Exception as e:
            print(f"⚠️ Warning: Could not load max_retries from config, using default: {e}")
            return 3

    def get_default_page_size(self) -> int:
        """Get default page size from runtime config with safe fallback"""
        try:
            runtime_config = self.framework_config.get("runtime", {})
            return runtime_config.get("default_page_size", 10)
        except Exception as e:
            print(f"⚠️ Warning: Could not load default_page_size from config, using default: {e}")
            return 10

    def get_default_agent_timeout(self) -> int:
        """Get default agent timeout from runtime config with safe fallback"""
        try:
            runtime_config = self.framework_config.get("runtime", {})
            return runtime_config.get("default_agent_timeout", 60)
        except Exception as e:
            print(f"⚠️ Warning: Could not load default_agent_timeout from config, using default: {e}")
            return 60
    
    def get_entity_api_tools(self, entity_name: str) -> str:
        """
        Generate API endpoints summary filtered for a specific entity using entity-specific config
        
        Args:
            entity_name: The entity to filter for (e.g., "Partner", "Contact", "Interaction")
        
        Returns:
            str: Formatted summary of endpoints for the specific entity
        """
        try:
            # Load entity-specific configuration
            entity_config = self.load_entity_api_config(entity_name)
            entities = entity_config.get('entities', [])
            base_url = self.get_api_base_url()
            
            if not entities:
                return f"❌ No endpoints found for entity: {entity_name}"
            
            # Use the first (and likely only) entity from the entity-specific config
            target_entity = entities[0]
            
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
            
        except FileNotFoundError as e:
            print(f"⚠️ {e}")
            return f"❌ No tools configuration found for entity: {entity_name}"
        except Exception as e:
            print(f"❌ Error loading entity tools for {entity_name}: {e}")
            return f"❌ Error loading configuration for entity: {entity_name}"
    
    def get_api_endpoints_summary(self) -> str:
        """Generate a formatted summary of ALL available API endpoints"""
        summary = "**📡 Available API Endpoints (All Entities):**\n\n"
        
        entities = self.tools_config.get('entities', [])
        base_url = self.get_api_base_url()
        
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
        """
        Get all entities from available tool configurations (both entity-specific and default)
        
        Returns:
            List of entity configurations
        """
        all_entities = []
        available_entity_names = self.get_available_entities()
        
        for entity_name in available_entity_names:
            try:
                entity_config = self.load_entity_api_config(entity_name)
                entities = entity_config.get('entities', [])
                all_entities.extend(entities)
            except Exception as e:
                print(f"⚠️ Could not load config for entity {entity_name}: {e}")
                continue
        
        return all_entities
    
    def get_entity_by_name(self, entity_name: str) -> Optional[Dict[str, Any]]:
        """Get entity configuration by name using entity-specific config"""
        try:
            entity_config = self.load_entity_api_config(entity_name)
            entities = entity_config.get('entities', [])
            
            # Return the first entity (should be the target entity in entity-specific config)
            if entities:
                return entities[0]
            
            return None
            
        except FileNotFoundError:
            print(f"⚠️ No configuration found for entity: {entity_name}")
            return None
        except Exception as e:
            print(f"❌ Error loading entity {entity_name}: {e}")
            return None
    
    def get_entity_synonyms(self, entity_name: str) -> List[str]:
        """Get synonyms for an entity"""
        entity = self.get_entity_by_name(entity_name)
        if entity:
            return entity.get("synonyms", [])
        return []
    
    def get_entity_api_endpoints(self, entity_name: str) -> List[Dict[str, Any]]:
        """Get all endpoints for an entity using entity-specific config"""
        entity = self.get_entity_by_name(entity_name)
        if entity:
            return entity.get("endpoints", [])
        return []
    
    def get_entity_search_metadata(self, entity_name: str) -> Dict[str, Any]:
        """
        Get search metadata for an entity including available fields, operators, and examples
        
        Args:
            entity_name: The entity name (e.g., "Partner", "Contact", "Interaction")
            
        Returns:
            Dict containing searchMetadata from the entity configuration.
            Returns empty dict if no searchMetadata available (graceful fallback for teams without advanced search).
        """
        try:
            entity = self.get_entity_by_name(entity_name)
            if entity and "searchMetadata" in entity:
                search_metadata = entity["searchMetadata"]
                print(f"✅ Found searchMetadata for {entity_name}")
                return search_metadata
            else:
                print(f"ℹ️ No searchMetadata found for {entity_name} - using fallback (entity may not support advanced search)")
                return {}
        except Exception as e:
            print(f"⚠️ Warning: Could not load searchMetadata for {entity_name}: {e} - using fallback")
            return {}
    
    def get_entity_search_fields(self, entity_name: str) -> Dict[str, List[str]]:
        """
        Get available search fields for an entity
        
        Args:
            entity_name: The entity name
            
        Returns:
            Dict with 'directFields' and 'nestedFields' lists.
            Returns empty lists if no searchMetadata available (graceful fallback).
        """
        try:
            search_metadata = self.get_entity_search_metadata(entity_name)
            if not search_metadata:
                print(f"ℹ️ No search fields available for {entity_name} - entity may not support advanced search")
                return {"directFields": [], "nestedFields": {}}
            
            direct_fields = search_metadata.get("directFields", [])
            nested_fields = search_metadata.get("nestedFields", {})
            
            # Ensure nested_fields is a dict, not a list
            if not isinstance(nested_fields, dict):
                print(f"⚠️ Warning: nestedFields for {entity_name} is not a dict, converting to empty dict")
                nested_fields = {}
            
            return {
                "directFields": direct_fields if isinstance(direct_fields, list) else [],
                "nestedFields": nested_fields
            }
        except Exception as e:
            print(f"⚠️ Warning: Could not load search fields for {entity_name}: {e} - using fallback")
            return {"directFields": [], "nestedFields": {}}
    
    def get_entity_search_operators(self, entity_name: str) -> List[str]:
        """
        Get available search operators for an entity
        
        Args:
            entity_name: The entity name
            
        Returns:
            List of available operators.
            Returns basic operators as fallback if no searchMetadata available.
        """
        try:
            search_metadata = self.get_entity_search_metadata(entity_name)
            operators = search_metadata.get("operators", [])
            
            if operators and isinstance(operators, list):
                return operators
            else:
                print(f"ℹ️ No operators found in searchMetadata for {entity_name} - using basic fallback operators")
                # Provide basic fallback operators that work with most systems
                return ["like", "is", "not", "contains", "startsWith", "endsWith"]
        except Exception as e:
            print(f"⚠️ Warning: Could not load search operators for {entity_name}: {e} - using fallback")
            return ["like", "is", "not", "contains", "startsWith", "endsWith"]
    
    def get_entity_search_examples(self, entity_name: str) -> List[Dict[str, str]]:
        """
        Get example search criteria for an entity
        
        Args:
            entity_name: The entity name
            
        Returns:
            List of example criteria objects with field, operator, value, description.
            Returns empty list if no examples available (graceful fallback).
        """
        try:
            search_metadata = self.get_entity_search_metadata(entity_name)
            examples = search_metadata.get("exampleCriteria", [])
            
            if examples and isinstance(examples, list):
                return examples
            else:
                print(f"ℹ️ No search examples found for {entity_name} - entity may not have advanced search examples")
                return []
        except Exception as e:
            print(f"⚠️ Warning: Could not load search examples for {entity_name}: {e} - using fallback")
            return []
    
    def get_entity_date_fields(self, entity_name: str) -> List[str]:
        """
        Get date fields for an entity that can be used with date operators
        
        Args:
            entity_name: The entity name
            
        Returns:
            List of date field names.
            Returns common date fields as fallback if no searchMetadata available.
        """
        try:
            search_metadata = self.get_entity_search_metadata(entity_name)
            date_fields = search_metadata.get("dateFields", [])
            
            if date_fields and isinstance(date_fields, list):
                return date_fields
            else:
                print(f"ℹ️ No date fields found in searchMetadata for {entity_name} - using common fallback date fields")
                # Provide common date field names that most entities might have
                return ["createdDate", "modifiedDate", "date", "updatedDate"]
        except Exception as e:
            print(f"⚠️ Warning: Could not load date fields for {entity_name}: {e} - using fallback")
            return ["createdDate", "modifiedDate", "date", "updatedDate"]
    
    def format_search_guidance(self, entity_name: str) -> str:
        """
        Generate formatted search guidance for an entity based on its searchMetadata
        
        Args:
            entity_name: The entity name
            
        Returns:
            Formatted string with comprehensive search guidance.
            Returns basic guidance if no searchMetadata available (graceful fallback).
        """
        try:
            search_metadata = self.get_entity_search_metadata(entity_name)
            
            if not search_metadata:
                print(f"ℹ️ No searchMetadata for {entity_name} - providing basic search guidance")
                return f"""🔍 **Search Guidance for {entity_name}:**

ℹ️ **Basic Search Available:**
This entity supports basic text search functionality. Advanced search metadata is not configured.

**Recommended Approach:**
- Use simple text search (searchText parameter)
- Use basic operators: like, is, not, contains
- For advanced searches, consult your entity-specific documentation

**Note:** Advanced search with nested fields may not be available for this entity.
"""
            
            guidance = f"🔍 **Search Guidance for {entity_name}:**\n\n"
            
            # Direct fields
            direct_fields = search_metadata.get("directFields", [])
            if direct_fields:
                guidance += f"**Direct Fields:** {', '.join(direct_fields)}\n"
            
            # Nested fields
            nested_fields = search_metadata.get("nestedFields", {})
            if nested_fields and isinstance(nested_fields, dict):
                guidance += "**Nested Fields:**\n"
                for entity_key, fields in nested_fields.items():
                    if isinstance(fields, list):
                        guidance += f"  - {entity_key}: {', '.join(fields)}\n"
            
            # Operators
            operators = search_metadata.get("operators", [])
            if operators:
                guidance += f"\n**Available Operators:** {', '.join(operators)}\n"
            
            # Date fields
            date_fields = search_metadata.get("dateFields", [])
            if date_fields:
                guidance += f"\n**Date Fields:** {', '.join(date_fields)}\n"
            
            # Examples
            examples = search_metadata.get("exampleCriteria", [])
            if examples and isinstance(examples, list):
                guidance += "\n**Example Search Criteria:**\n"
                for example in examples:
                    if isinstance(example, dict):
                        field = example.get("field", "")
                        operator = example.get("operator", "")
                        value = example.get("value", "")
                        description = example.get("description", "")
                        logical_op = example.get("logicalOperator", "")
                        
                        example_json = f'{{"field": "{field}", "operator": "{operator}", "value": "{value}"'
                        if logical_op:
                            example_json += f', "logicalOperator": "{logical_op}"'
                        if description:
                            example_json += f', "description": "{description}"'
                        example_json += '}'
                        
                        guidance += f"  - {example_json}\n"
                        if description:
                            guidance += f"    *{description}*\n"
            
            return guidance
            
        except Exception as e:
            print(f"⚠️ Warning: Could not format search guidance for {entity_name}: {e} - using fallback")
            return f"""🔍 **Search Guidance for {entity_name}:**

⚠️ **Search metadata could not be loaded.**
This entity may support basic search functionality.

**Fallback Recommendations:**
- Use simple text search (searchText parameter)
- Try basic operators: like, is, not, contains, startsWith, endsWith
- Consult your entity-specific documentation for available fields

**Note:** Advanced search capabilities may be limited.
"""
    
    def find_endpoint_by_entity_and_intent(self, entity_name: str, intent: str, extracted_params: Optional[Dict[str, Any]] = None) -> Optional[Dict[str, Any]]:
        """
        Intelligently find the best endpoint for a given entity and intent combination using scoring
        
        Args:
            entity_name: The entity name (e.g., "Contact", "Partner")
            intent: The intent (e.g., "search", "create", "update")
            extracted_params: Optional extracted parameters from user query (e.g., {"id": 123, "type": "OrgUnit"})
        
        Returns:
            Dict containing the best matching endpoint configuration
        """
        endpoints = self.get_entity_api_endpoints(entity_name)
        
        if not endpoints:
            return None

        def score_endpoint_for_intent(endpoint: Dict[str, Any], intent: str, entity_name: str, extracted_params: Optional[Dict[str, Any]] = None) -> int:
            """Score an endpoint based on how well it matches the intent and capabilities"""
            score = 0
            endpoint_name = endpoint.get('name', '').lower()
            description = endpoint.get('description', '').lower()
            url = endpoint.get('url', '').lower()
            method = endpoint.get('method', 'GET').upper()
            when_to_use = endpoint.get('when_to_use', '').lower()
            example_uses = endpoint.get('example_uses', [])
            parameters = endpoint.get('parameters', {})
            
            # Intent to HTTP method mapping
            intent_method_mapping = {
                'search': 'GET', 'list': 'GET', 'get': 'GET', 'find': 'GET', 'retrieve': 'GET',
                'create': 'POST', 'add': 'POST', 'new': 'POST', 'insert': 'POST',
                'update': 'PUT', 'modify': 'PUT', 'edit': 'PUT', 'change': 'PUT',
                'delete': 'DELETE', 'remove': 'DELETE', 'destroy': 'DELETE'
            }
            
            target_method = intent_method_mapping.get(intent.lower(), 'GET')
            
            # Must match HTTP method - this is critical
            if method != target_method:
                return 0  # Wrong method = zero score

            # ✨ NEW: PARAMETER-AWARE SCORING - This is the key improvement!
            param_bonus = 0
            has_id_param = False
            user_provided_id = False
            
            if extracted_params:
                user_provided_id = any(key.lower() in ['id', 'identifier'] for key in extracted_params.keys())
                
                # Check if endpoint accepts the parameters user provided
                for user_param, user_value in extracted_params.items():
                    user_param_lower = user_param.lower()
                    
                    # Direct parameter name match
                    if user_param_lower in [p.get('name', '').lower() for p in parameters if isinstance(p, dict)]:
                        param_bonus += 40  # High bonus for exact parameter match
                    elif user_param_lower in parameters:  # Simple dict format
                        param_bonus += 40
                    elif user_param_lower in url:  # Parameter in URL path
                        param_bonus += 35
                    
                    # ID-specific scoring
                    if user_param_lower in ['id', 'identifier']:
                        has_id_param = True
                        # Look for ID parameters in endpoint
                        if any(p.get('name', '').lower() in ['id', 'identifier'] for p in parameters if isinstance(p, dict)):
                            param_bonus += 50  # Very high bonus for ID parameter match
                        elif 'id' in parameters:  # Simple dict format
                            param_bonus += 50
                        # Check URL patterns for ID
                        if any(pattern in url for pattern in ['/{id}', '/by-id', '/details', '/{identifier}']):
                            param_bonus += 45
                        # Check endpoint name for ID patterns
                        if any(pattern in endpoint_name for pattern in ['byid', 'by_id', 'getby', 'detail']):
                            param_bonus += 35

            # ✨ NEW: SPECIFICITY VS LIST OPERATION SCORING
            specificity_bonus = 0
            if user_provided_id and intent.lower() in ['search', 'get', 'find']:
                # When user provides ID, they want specific item details, not lists
                list_indicators = ['list', 'all', 'eligible', 'available', 'multiple']
                single_item_indicators = ['detail', 'by-id', 'specific', 'get-one', 'single']
                
                # Penalize list operations when ID is provided
                if any(indicator in endpoint_name for indicator in list_indicators):
                    specificity_bonus -= 30  # Strong penalty for list ops with ID
                if any(indicator in description for indicator in list_indicators):
                    specificity_bonus -= 20
                    
                # Reward single-item operations when ID is provided
                if any(indicator in endpoint_name for indicator in single_item_indicators):
                    specificity_bonus += 35  # Strong bonus for specific item ops
                if any(indicator in description for indicator in single_item_indicators):
                    specificity_bonus += 25
                    
                # URL pattern analysis for specificity
                if '/{id}' in url or '/by-id' in url or '/details' in url:
                    specificity_bonus += 30
                elif '/list' in url or '/all' in url or '/search' in url:
                    specificity_bonus -= 15  # Mild penalty for list URLs with ID

            # High-value scoring: Intent keywords in endpoint name (most important)
            intent_lower = intent.lower()
            if intent_lower == endpoint_name:
                score += 50  # Perfect match
            elif intent_lower in endpoint_name:
                score += 30  # Intent is part of name
            elif any(synonym in endpoint_name for synonym in intent_method_mapping.keys() if intent_method_mapping[synonym] == target_method):
                score += 20  # Related intent word in name
            
            # Medium-value scoring: Intent in description and when_to_use
            intent_keywords = [intent_lower] + [k for k, v in intent_method_mapping.items() if v == target_method]
            for keyword in intent_keywords:
                if keyword in description:
                    score += 15
                if keyword in when_to_use:
                    score += 15
            
            # Score based on example uses matching
            for example in example_uses:
                if isinstance(example, str) and intent_lower in example.lower():
                    score += 10
            
            # Special handling for different intents
            if intent_lower in ['search', 'find', 'list', 'get']:
                # For search/list operations, prefer endpoints that support filtering
                if 'search' in endpoint_name or 'find' in endpoint_name:
                    score += 25
                if 'list' in endpoint_name or 'get' in endpoint_name.replace('get', ''):
                    score += 20
                if any(param in parameters for param in ['search', 'query', 'filter', 'term']):
                    score += 15
                # Bonus for pagination support
                if any(param in parameters for param in ['page', 'pagesize', 'top', 'skip', 'limit']):
                    score += 10
                    
            elif intent_lower in ['create', 'add', 'new']:
                # For create operations, prefer endpoints with comprehensive data handling
                if 'create' in endpoint_name or 'add' in endpoint_name:
                    score += 25
                if 'new' in endpoint_name:
                    score += 20
                # Bonus for endpoints that can handle rich data
                if len(parameters) > 3:  # More parameters = more comprehensive
                    score += 10
                    
            elif intent_lower in ['update', 'modify', 'edit']:
                # For update operations, prefer endpoints that handle partial updates
                if 'update' in endpoint_name or 'modify' in endpoint_name:
                    score += 25
                if 'edit' in endpoint_name or 'change' in endpoint_name:
                    score += 20
                # Check if it supports partial updates (fewer required params)
                required_params = sum(1 for p in parameters.values() if isinstance(p, dict) and p.get('required', False))
                if required_params <= 1:  # ID only or no required params
                    score += 15
                    
            elif intent_lower in ['delete', 'remove']:
                # For delete operations, prefer simple ID-based endpoints
                if 'delete' in endpoint_name or 'remove' in endpoint_name:
                    score += 25
                # Prefer endpoints with minimal parameters (just ID)
                if len(parameters) <= 1:
                    score += 15
            
            # URL pattern scoring for intent matching
            url_patterns = {
                'search': ['search', 'find', 'query'],
                'list': ['list', 'all'],
                'get': ['get', 'detail', 'by-id'],
                'create': ['create', 'add', 'new'],
                'update': ['update', 'modify', 'edit'],
                'delete': ['delete', 'remove']
            }
            
            intent_patterns = url_patterns.get(intent_lower, [])
            for pattern in intent_patterns:
                if pattern in url:
                    score += 8
            
            # Bonus for entity-specific patterns
            entity_lower = entity_name.lower()
            if entity_lower in url:
                score += 5
            
            # Penalty for overly complex endpoints when simple ones would do
            if intent_lower in ['get', 'search'] and len(parameters) > 5:
                score -= 5  # Too many parameters for simple operations

            # ✨ APPLY THE NEW BONUSES
            total_score = score + param_bonus + specificity_bonus
            
            return max(0, total_score)  # Ensure non-negative score
        
        # Score all endpoints and find the best match
        candidate_endpoints = []
        for endpoint in endpoints:
            score = score_endpoint_for_intent(endpoint, intent, entity_name, extracted_params)
            if score > 0:
                candidate_endpoints.append({
                    'endpoint': endpoint,
                    'score': score
                })
        
        if candidate_endpoints:
            # Sort by score (highest first) and return the best
            candidate_endpoints.sort(key=lambda x: x['score'], reverse=True)
            best_endpoint = candidate_endpoints[0]['endpoint']
            
            print(f"🎯 [CONFIG] Selected best endpoint for {entity_name}.{intent}:")
            print(f"   Name: {best_endpoint.get('name')}")
            print(f"   URL: {best_endpoint.get('url')}")
            print(f"   Method: {best_endpoint.get('method')}")
            print(f"   Score: {candidate_endpoints[0]['score']}")
            
            # Show extracted params if provided
            if extracted_params:
                print(f"   Params considered: {extracted_params}")
            
            if len(candidate_endpoints) > 1:
                print(f"   Runner-ups:")
                for i, candidate in enumerate(candidate_endpoints[1:3], 1):
                    ep = candidate['endpoint']
                    print(f"     {i}. {ep.get('name')} (score: {candidate['score']})")
            
            return best_endpoint
        
        # No scored matches found, return None
        print(f"⚠️ [CONFIG] No suitable endpoint found for {entity_name}.{intent}")
        return None
    
    def get_entity_detection_config(self) -> str:
        """
        Generate entity detection configuration for dynamic instruction using discovered entities
        """
        entities = self.get_entities()
        available_entity_names = self.get_available_entities()
        
        detection_config = "**🎯 DYNAMIC ENTITY DETECTION CONFIGURATION:**\n\n"
        
        detection_config += f"**Available Entities ({len(available_entity_names)} total):**\n"
        for entity in entities:
            entity_name = entity.get('entity', '')
            description = entity.get('description', '')
            synonyms = entity.get('synonyms', [])
            
            detection_config += f"• **{entity_name}**: {description}\n"
            if synonyms:
                detection_config += f"  - Synonyms: {', '.join(synonyms)}\n"
        
        # Add discovered entities that might not have full configs
        for entity_name in available_entity_names:
            if not any(e.get('entity', '').lower() == entity_name.lower() for e in entities):
                detection_config += f"• **{entity_name}**: Available via entity-specific tools\n"
        
        detection_config += "\n**Intent Patterns:**\n"
        detection_config += "• **search/list/get**: Use for finding or retrieving data\n"
        detection_config += "• **create/add**: Use for creating new records\n" 
        detection_config += "• **update/modify**: Use for updating existing records\n"
        detection_config += "• **delete/remove**: Use for deleting records\n"
        
        return detection_config

    def get_oauth_config(self) -> Dict[str, str]:
        """Get OAuth configuration from google_cloud section"""
        try:
            google_cloud_config = self.framework_config.get("google_cloud", {})
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
            # Check cache first
            current_time = time.time()
            if (self._cached_identity_toolkit_api_key and 
                current_time < self._api_key_cache_expiry):
                print(f"✅ Using cached Identity Toolkit API key (expires in {int(self._api_key_cache_expiry - current_time)}s)")
                return self._cached_identity_toolkit_api_key
            
            google_cloud_config = self.framework_config.get("google_cloud", {})
            oauth_config = google_cloud_config.get("oauth", {})
            
            # Get the secret name from configuration
            secret_name = oauth_config.get("identity_toolkit_api_key_secret")
            if not secret_name:
                print("⚠️ Warning: No identity_toolkit_api_key_secret configured")
                return ""
            
            # Get project ID from configuration
            project_id = google_cloud_config.get("project")
            if not project_id:
                print("⚠️ Warning: No project configured for Google Cloud")
                return ""
            
            # Retrieve the secret from Secret Manager
            print(f"🔐 Retrieving Identity Toolkit API key from secret: {secret_name}")
            api_key = self.get_secret_from_secret_manager(secret_name, project_id)
            
            if api_key:
                print(f"✅ Successfully retrieved Identity Toolkit API key from secret: {secret_name}")
                
                # Cache the result
                self._cached_identity_toolkit_api_key = api_key
                self._api_key_cache_expiry = current_time + self._api_key_cache_ttl
                print(f"💾 Cached Identity Toolkit API key for {self._api_key_cache_ttl}s")
                
                return api_key
            else:
                print(f"❌ Failed to retrieve Identity Toolkit API key from secret: {secret_name}")
                return ""
                
        except Exception as e:
            print(f"⚠️ Warning: Could not load Identity Toolkit API key from secret: {e}")
            return ""
    
    def clear_identity_toolkit_api_key_cache(self) -> None:
        """Clear the cached Identity Toolkit API key to force refresh"""
        self._cached_identity_toolkit_api_key = None
        self._api_key_cache_expiry = 0
        print("🧹 Cleared Identity Toolkit API key cache")
    
    def clear_entity_tools_cache(self, entity_name: Optional[str] = None) -> None:
        """
        Clear entity tools cache
        
        Args:
            entity_name: Specific entity to clear, or None to clear all
        """
        if entity_name:
            entity_lower = entity_name.lower()
            if entity_lower in self._entity_tools_cache:
                del self._entity_tools_cache[entity_lower]
                print(f"🧹 Cleared tools cache for entity: {entity_name}")
            else:
                print(f"⚠️ No cache found for entity: {entity_name}")
        else:
            self._entity_tools_cache.clear()
            print("🧹 Cleared all entity tools cache")
    
    def clear_all_caches(self) -> None:
        """Clear all configuration caches"""
        self._tools_config = None
        self._framework_config = None
        self._entity_tools_cache.clear()
        self._discovered_entities = None
        self.clear_identity_toolkit_api_key_cache()
        print("🧹 Cleared all configuration caches")
    
    def get_cache_info(self) -> Dict[str, Any]:
        """Get information about current cache state"""
        return {
            "default_tools_loaded": self._tools_config is not None,
            "framework_config_loaded": self._framework_config is not None,
            "cached_entity_tools": list(self._entity_tools_cache.keys()),
            "discovered_entities_cached": self._discovered_entities is not None,
            "available_entities_count": len(self.get_available_entities()) if self._discovered_entities else 0,
            "identity_toolkit_api_key_cached": self._cached_identity_toolkit_api_key is not None
        }

    def set_config_directory(self, config_dir: str = "config"):
        """
        Set the base configuration directory path.
        
        Args:
            config_dir: Path to the base config directory 
                       (should contain tools/endpoints/ subdirectory)
        """
        self._config_directory = config_dir
        # Clear cached configs to force reload from new path
        self._tools_config = None
        self._entity_tools_cache.clear()
        self._discovered_entities = None


# Global singleton instance
api_config_manager = ApiConfigManager() 

# Export for backward compatibility
config_manager = api_config_manager

# Export API_BASE_URL for backward compatibility
def get_api_base_url():
    return api_config_manager.get_api_base_url()

# Create a module-level variable for direct access
API_BASE_URL = api_config_manager.get_api_base_url() 