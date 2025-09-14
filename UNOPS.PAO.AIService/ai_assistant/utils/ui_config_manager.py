#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
UI Configuration Manager
Manages frontend UI guidance and configuration for the AI assistant
"""

import json
import os
from typing import Dict, List, Any, Optional
from pathlib import Path
from .api_config_manager import config_manager

class UIConfigManager:
    """
    Singleton manager for UI guidance configuration
    Handles loading and caching of UI guidance from config/tools/ui/ directory
    """
    
    _instance = None
    _ui_config_cache: Dict[str, Dict[str, Any]] = {}
    _discovered_ui_entities: Optional[List[str]] = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(UIConfigManager, cls).__new__(cls)
        return cls._instance
    
    def construct_full_url(self, route: str) -> str:
        """
        Construct full URL from route using api_base_url from config
        
        Args:
            route: Route path (e.g., "/partnerships/contacts")
            
        Returns:
            Full URL (e.g., "https://localhost:44426/#/partnerships/contacts")
        """
        try:
            api_base_url = config_manager.get_api_base_url()
            # Remove trailing slash from base URL if present
            base_url = api_base_url.rstrip('/')
            # Ensure route starts with /
            clean_route = route if route.startswith('/') else f'/{route}'
            # Construct full URL with hash routing
            full_url = f"{base_url}#{clean_route}"
            return full_url
        except Exception as e:
            print(f"⚠️ Error constructing full URL for route {route}: {e}")
            # Fallback to just the route
            return route
    
    def load_entity_ui_config(self, entity_name: str) -> Optional[Dict[str, Any]]:
        """
        Load UI guidance configuration for a specific entity
        
        Args:
            entity_name: Name of the entity (e.g., 'Partner', 'Contact', 'AiAssistant')
            
        Returns:
            Dict containing UI guidance configuration or None if not found
        """
        entity_lower = entity_name.lower()
        
        # Check cache first
        if entity_lower in self._ui_config_cache:
            print(f"✅ Using cached UI config for entity: {entity_name}")
            return self._ui_config_cache[entity_lower]
        
        # Try entity-specific UI files with different naming patterns
        possible_filenames = [
            f"config/tools/ui/{entity_lower}-ui.json",  # kebab-case (preferred)
            f"config/tools/ui/{entity_lower}_ui.json",  # snake_case
            f"config/tools/ui/{entity_name.lower()}-ui.json",  # exact case
            f"config/tools/ui/{entity_name.lower()}_ui.json"   # exact case
        ]
        
        ui_config = None
        used_path = None
        
        # Try to load entity-specific UI config
        for config_path in possible_filenames:
            try:
                with open(config_path, 'r', encoding='utf-8') as f:
                    ui_config = json.load(f)
                    used_path = config_path
                    print(f"✅ Loaded UI config from {config_path}")
                    break
            except FileNotFoundError:
                continue
            except json.JSONDecodeError as e:
                print(f"❌ Invalid JSON in UI config {config_path}: {e}")
                continue
        
        if ui_config is None:
            print(f"⚠️ No UI configuration found for entity: {entity_name}")
            return None
        
        # Cache the loaded config
        self._ui_config_cache[entity_lower] = ui_config
        print(f"📝 Cached UI config for {entity_name}")
        
        return ui_config
    
    def get_entity_ui_guidance(self, entity_name: str) -> Optional[Dict[str, Any]]:
        """
        Get comprehensive UI guidance for an entity
        
        Args:
            entity_name: Name of the entity
            
        Returns:
            Dict containing UI guidance information
        """
        ui_config = self.load_entity_ui_config(entity_name)
        if not ui_config:
            return None
        
        # Process pages to add full URLs
        pages = ui_config.get("pages", [])
        enhanced_pages = []
        for page in pages:
            enhanced_page = page.copy()
            route = page.get("route", "")
            if route:
                enhanced_page["full_url"] = self.construct_full_url(route)
            
            # Process tabs within pages to add full URLs
            tabs = page.get("tabs", [])
            if tabs:
                enhanced_tabs = []
                for tab in tabs:
                    enhanced_tab = tab.copy()
                    tab_route = tab.get("route", "")
                    if tab_route:
                        enhanced_tab["full_url"] = self.construct_full_url(tab_route)
                    enhanced_tabs.append(enhanced_tab)
                enhanced_page["tabs"] = enhanced_tabs
            
            enhanced_pages.append(enhanced_page)
            
        return {
            "entity": ui_config.get("entity", entity_name),
            "description": ui_config.get("description", ""),
            "synonyms": ui_config.get("synonyms", []),
            "pages": enhanced_pages,
            "total_pages": len(enhanced_pages),
            "has_buttons": any(page.get("buttons", []) for page in enhanced_pages),
            "has_forms": any(page.get("forms", []) for page in enhanced_pages),
            "has_tabs": any(page.get("tabs", []) for page in enhanced_pages)
        }
    
    def get_ui_pages_for_entity(self, entity_name: str) -> List[Dict[str, Any]]:
        """
        Get all UI pages/components for an entity
        
        Args:
            entity_name: Name of the entity
            
        Returns:
            List of page configurations with full URLs
        """
        ui_config = self.load_entity_ui_config(entity_name)
        if not ui_config:
            return []
        
        # Process pages to add full URLs
        pages = ui_config.get("pages", [])
        enhanced_pages = []
        for page in pages:
            enhanced_page = page.copy()
            route = page.get("route", "")
            if route:
                enhanced_page["full_url"] = self.construct_full_url(route)
            
            # Process tabs within pages to add full URLs
            tabs = page.get("tabs", [])
            if tabs:
                enhanced_tabs = []
                for tab in tabs:
                    enhanced_tab = tab.copy()
                    tab_route = tab.get("route", "")
                    if tab_route:
                        enhanced_tab["full_url"] = self.construct_full_url(tab_route)
                    enhanced_tabs.append(enhanced_tab)
                enhanced_page["tabs"] = enhanced_tabs
            
            enhanced_pages.append(enhanced_page)
            
        return enhanced_pages
    
    def get_ui_buttons_for_entity(self, entity_name: str) -> List[Dict[str, Any]]:
        """
        Get all buttons across all pages for an entity
        
        Args:
            entity_name: Name of the entity
            
        Returns:
            List of button configurations
        """
        pages = self.get_ui_pages_for_entity(entity_name)
        buttons = []
        
        for page in pages:
            page_buttons = page.get("buttons", [])
            for button in page_buttons:
                # Add page context to button info
                button_with_context = button.copy()
                button_with_context["page_name"] = page.get("name", "Unknown")
                button_with_context["page_route"] = page.get("route", "")
                buttons.append(button_with_context)
                
        return buttons
    
    def get_screen_help_for_entity(self, entity_name: str, screen_type: str = None) -> Dict[str, Any]:
        """
        Get help guidance for a specific entity screen
        
        Args:
            entity_name: Name of the entity
            screen_type: Optional screen type filter (e.g., 'list', 'detail')
            
        Returns:
            Dict containing help guidance
        """
        pages = self.get_ui_pages_for_entity(entity_name)
        help_info = {
            "entity": entity_name,
            "screen_type": screen_type,
            "help_guidance": [],
            "common_tasks": [],
            "troubleshooting": [],
            "getting_started": ""
        }
        
        for page in pages:
            page_help = page.get("help_guidance", {})
            if page_help:
                help_info["help_guidance"].append({
                    "page": page.get("name", "Unknown"),
                    "route": page.get("route", ""),
                    "when_stuck": page_help.get("when_stuck", ""),
                    "common_tasks": page_help.get("common_tasks", []),
                    "troubleshooting": page_help.get("troubleshooting", []),
                    "getting_started": page_help.get("getting_started", "")
                })
        
        return help_info
    
    def discover_ui_entities(self) -> List[str]:
        """
        Discover available UI entities by scanning the config/tools/ui directory
        
        Returns:
            List of entity names with UI configurations
        """
        if self._discovered_ui_entities is not None:
            return self._discovered_ui_entities
        
        entities = set()
        ui_dir = "config/tools/ui"
        
        try:
            if os.path.exists(ui_dir):
                for filename in os.listdir(ui_dir):
                    if filename.endswith('-ui.json') or filename.endswith('_ui.json'):
                        # Extract entity name from filename
                        if filename.endswith('-ui.json'):
                            entity_name = filename.replace('-ui.json', '')
                        else:
                            entity_name = filename.replace('_ui.json', '')
                        
                        # Capitalize the entity name
                        entity_name = entity_name.capitalize()
                        entities.add(entity_name)
                        
                print(f"🎨 Discovered {len(entities)} UI entities: {', '.join(sorted(entities))}")
            else:
                print(f"⚠️ UI directory not found: {ui_dir}")
                
        except Exception as e:
            print(f"❌ Error discovering UI entities: {e}")
        
        self._discovered_ui_entities = sorted(entities)
        return self._discovered_ui_entities
    
    def get_available_ui_entities(self) -> List[str]:
        """
        Get list of all available UI entities
        
        Returns:
            List of entity names with UI configurations
        """
        return self.discover_ui_entities()
    
    def clear_ui_cache(self):
        """Clear the UI configuration cache"""
        self._ui_config_cache.clear()
        self._discovered_ui_entities = None
        print("🧹 UI configuration cache cleared")
    
    def find_entity_by_synonym(self, search_term: str) -> Optional[str]:
        """
        Find an entity by searching through synonyms
        
        Args:
            search_term: The term to search for
            
        Returns:
            Entity name if found, None otherwise
        """
        search_lower = search_term.lower()
        
        for entity_name in self.get_available_ui_entities():
            ui_config = self.load_entity_ui_config(entity_name)
            if not ui_config:
                continue
                
            # Check entity name
            if entity_name.lower() == search_lower:
                return entity_name
                
            # Check synonyms
            synonyms = ui_config.get("synonyms", [])
            for synonym in synonyms:
                if synonym.lower() == search_lower:
                    return entity_name
        
        return None

    def find_matching_page_by_url(self, screen_url: str) -> Optional[Dict[str, Any]]:
        """
        Find matching page and entity by screen URL from UI schemas
        
        Args:
            screen_url: The screen URL to match (e.g., "/partnerships/partners/123")
            
        Returns:
            Dict containing entity, page info, and parsed URL info, or None if no match
        """
        if not screen_url:
            return None
            
        # Clean the URL path
        url_path = screen_url.strip('/')
        if not url_path:
            return {
                "entity_name": None,
                "entity_id": None,
                "screen_type": "homepage",
                "page_info": None,
                "ui_schema_file": None
            }
        
        # Try to find exact or partial matches in all UI entities
        for entity_name in self.get_available_ui_entities():
            ui_config = self.load_entity_ui_config(entity_name)
            if not ui_config:
                continue
                
            pages = ui_config.get("pages", [])
            for page in pages:
                route = page.get("route", "").strip("/")
                
                # Exact match
                if route == url_path:
                    return self._build_screen_context_from_page(
                        entity_name, page, screen_url, ui_config, None
                    )
                
                # Partial match - check if URL starts with route
                if route and url_path.startswith(route):
                    # Extract potential entity ID
                    remaining_path = url_path[len(route):].strip("/")
                    entity_id = remaining_path if remaining_path and "/" not in remaining_path else None
                    
                    return self._build_screen_context_from_page(
                        entity_name, page, screen_url, ui_config, entity_id
                    )
        
        # No exact match found, try path-based entity detection
        return self._fallback_path_analysis(screen_url)
    
    def _build_screen_context_from_page(self, entity_name: str, page: Dict[str, Any], 
                                       screen_url: str, ui_config: Dict[str, Any], 
                                       entity_id: Optional[str]) -> Dict[str, Any]:
        """Build screen context from UI page configuration"""
        
        # Determine screen type
        route = page.get("route", "")
        screen_type = "entity_list_page"
        
        if entity_id:
            screen_type = "entity_detail_page"
        elif any(keyword in route.lower() for keyword in ["create", "new", "edit", "form"]):
            screen_type = "form_page"
        elif "dashboard" in route.lower():
            screen_type = "dashboard_overview"
        elif any(keyword in route.lower() for keyword in ["ai", "assistant", "chat"]):
            screen_type = "ai_specific_page"
        
        # Build screen metadata from page info
        screen_metadata = {
            "title": page.get("name", entity_name),
            "type": screen_type,
            "entity_type_singular": entity_name.lower() if entity_name else None,
            "description": page.get("description", ui_config.get("description", "")),
            "available_actions": page.get("capabilities", [])
        }
        
        return {
            "entity_name": entity_name,
            "entity_id": entity_id,
            "screen_type": screen_type,
            "page_info": page,
            "screen_metadata": screen_metadata,
            "ui_schema_file": f"{entity_name.lower()}-ui.json" if entity_name else None
        }
    
    def _fallback_path_analysis(self, screen_url: str) -> Dict[str, Any]:
        """Fallback analysis when no exact UI page match is found"""
        
        path_parts = [part for part in screen_url.strip('/').split('/') if part]
        
        if not path_parts:
            return {
                "entity_name": None,
                "entity_id": None,
                "screen_type": "homepage",
                "page_info": None,
                "screen_metadata": {"title": "Homepage", "type": "homepage"},
                "ui_schema_file": None
            }
        
        # Try to detect entity by path patterns
        entity_patterns = {
            'partners': 'Partner',
            'partnerships': 'Partner',
            'contacts': 'Contact', 
            'interactions': 'Interaction',
            'aiprompts': 'AiPrompt',
            'ai': None,
            'assistant': None,
            'dashboard': None
        }
        
        entity_name = None
        entity_id = None
        screen_type = "unknown"
        
        for i, part in enumerate(path_parts):
            if part.lower() in entity_patterns:
                entity_name = entity_patterns[part.lower()]
                
                # Check if there's an ID after the entity
                if i + 1 < len(path_parts):
                    next_part = path_parts[i + 1]
                    # Check if it looks like an ID
                    if (next_part.isdigit() or len(next_part) > 10):
                        entity_id = next_part
                        screen_type = "entity_detail_page"
                        break
                
                screen_type = "entity_list_page" if entity_name else "ai_specific_page"
                break
        
        # Special cases
        if any(keyword in screen_url.lower() for keyword in ['ai', 'assistant', 'chat']):
            screen_type = "ai_specific_page"
        elif 'dashboard' in screen_url.lower():
            screen_type = "dashboard_overview"
        
        return {
            "entity_name": entity_name,
            "entity_id": entity_id,
            "screen_type": screen_type,
            "page_info": None,
            "screen_metadata": {
                "title": f"{entity_name} Page" if entity_name else "Application Page",
                "type": screen_type
            },
            "ui_schema_file": None
        }

# Create global instance
ui_config_manager = UIConfigManager() 