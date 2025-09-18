#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
UI Tools for AI Assistant
Tools that provide UI guidance, screen help, and frontend assistance
"""

import json
from typing import Dict, List, Any, Optional
from ..utils.ui_config_manager import UIConfigManager
ui_config_manager = UIConfigManager()

def get_ui_guidance_for_entity(entity_name: str) -> str:
    """
    Get comprehensive UI guidance for a specific entity
    
    Args:
        entity_name: Name of the entity (e.g., 'Partner', 'Contact', 'AiAssistant')
        
    Returns:
        JSON string containing UI guidance information
    """
    try:
        print(f"🎨 [UI-TOOL] Getting UI guidance for entity: {entity_name}")
        
        # First try to find entity by exact name or synonym
        actual_entity = ui_config_manager.find_entity_by_synonym(entity_name)
        if not actual_entity:
            actual_entity = entity_name
        
        guidance = ui_config_manager.get_entity_ui_guidance(actual_entity)
        
        if not guidance:
            available_entities = ui_config_manager.get_available_ui_entities()
            return json.dumps({
                "error": f"No UI guidance found for entity: {entity_name}",
                "available_entities": available_entities,
                "suggestion": "Try one of the available entities listed above"
            })
        
        print(f"✅ [UI-TOOL] Found UI guidance for {actual_entity}")
        return json.dumps(guidance, indent=2)
        
    except Exception as e:
        print(f"❌ [UI-TOOL] Error getting UI guidance: {e}")
        return json.dumps({
            "error": f"Failed to get UI guidance: {str(e)}",
            "entity": entity_name
        })

def get_screen_help(entity_name: str, screen_type: Optional[str] = None) -> str:
    """
    Get contextual help for a specific screen or entity
    
    Args:
        entity_name: Name of the entity
        screen_type: Optional screen type (e.g., 'list', 'detail', 'form')
        
    Returns:
        JSON string containing screen help information
    """
    try:
        print(f"🎨 [UI-TOOL] Getting screen help for {entity_name}, screen_type: {screen_type}")
        
        # Find entity by name or synonym
        actual_entity = ui_config_manager.find_entity_by_synonym(entity_name)
        if not actual_entity:
            actual_entity = entity_name
        
        help_info = ui_config_manager.get_screen_help_for_entity(actual_entity, screen_type)
        
        if not help_info.get("help_guidance"):
            return json.dumps({
                "error": f"No screen help found for entity: {entity_name}",
                "entity": actual_entity,
                "screen_type": screen_type,
                "suggestion": "Try asking about available UI entities or check if the entity name is correct"
            })
        
        print(f"✅ [UI-TOOL] Found screen help for {actual_entity}")
        return json.dumps(help_info, indent=2)
        
    except Exception as e:
        print(f"❌ [UI-TOOL] Error getting screen help: {e}")
        return json.dumps({
            "error": f"Failed to get screen help: {str(e)}",
            "entity": entity_name,
            "screen_type": screen_type
        })

def get_ui_buttons_for_entity(entity_name: str) -> str:
    """
    Get all available buttons/actions for an entity's UI
    
    Args:
        entity_name: Name of the entity
        
    Returns:
        JSON string containing button information
    """
    try:
        print(f"🎨 [UI-TOOL] Getting UI buttons for entity: {entity_name}")
        
        # Find entity by name or synonym
        actual_entity = ui_config_manager.find_entity_by_synonym(entity_name)
        if not actual_entity:
            actual_entity = entity_name
        
        buttons = ui_config_manager.get_ui_buttons_for_entity(actual_entity)
        
        if not buttons:
            return json.dumps({
                "entity": actual_entity,
                "buttons": [],
                "message": f"No buttons found for {actual_entity}. This entity may not have interactive UI elements documented.",
                "total_buttons": 0
            })
        
        result = {
            "entity": actual_entity,
            "buttons": buttons,
            "total_buttons": len(buttons),
            "pages_with_buttons": list(set(btn.get("page_name", "Unknown") for btn in buttons))
        }
        
        print(f"✅ [UI-TOOL] Found {len(buttons)} buttons for {actual_entity}")
        return json.dumps(result, indent=2)
        
    except Exception as e:
        print(f"❌ [UI-TOOL] Error getting UI buttons: {e}")
        return json.dumps({
            "error": f"Failed to get UI buttons: {str(e)}",
            "entity": entity_name
        })

def get_ui_pages_for_entity(entity_name: str) -> str:
    """
    Get all UI pages/screens for an entity
    
    Args:
        entity_name: Name of the entity
        
    Returns:
        JSON string containing page information
    """
    try:
        print(f"🎨 [UI-TOOL] Getting UI pages for entity: {entity_name}")
        
        # Find entity by name or synonym
        actual_entity = ui_config_manager.find_entity_by_synonym(entity_name)
        if not actual_entity:
            actual_entity = entity_name
        
        pages = ui_config_manager.get_ui_pages_for_entity(actual_entity)
        
        if not pages:
            return json.dumps({
                "entity": actual_entity,
                "pages": [],
                "message": f"No UI pages found for {actual_entity}",
                "total_pages": 0
            })
        
        # Simplify page info for cleaner response
        simplified_pages = []
        for page in pages:
            simplified_pages.append({
                "name": page.get("name", "Unknown"),
                "route": page.get("route", ""),
                "description": page.get("description", ""),
                "capabilities": page.get("capabilities", []),
                "has_buttons": len(page.get("buttons", [])) > 0,
                "has_forms": len(page.get("forms", [])) > 0,
                "has_tabs": len(page.get("tabs", [])) > 0,
                "button_count": len(page.get("buttons", [])),
                "form_count": len(page.get("forms", [])),
                "tab_count": len(page.get("tabs", []))
            })
        
        result = {
            "entity": actual_entity,
            "pages": simplified_pages,
            "total_pages": len(pages)
        }
        
        print(f"✅ [UI-TOOL] Found {len(pages)} pages for {actual_entity}")
        return json.dumps(result, indent=2)
        
    except Exception as e:
        print(f"❌ [UI-TOOL] Error getting UI pages: {e}")
        return json.dumps({
            "error": f"Failed to get UI pages: {str(e)}",
            "entity": entity_name
        })

def get_available_ui_entities() -> str:
    """
    Get list of all entities that have UI guidance available
    
    Returns:
        JSON string containing available UI entities
    """
    try:
        print(f"🎨 [UI-TOOL] Getting available UI entities")
        
        entities = ui_config_manager.get_available_ui_entities()
        
        # Get summary info for each entity
        entity_summaries = []
        for entity in entities:
            guidance = ui_config_manager.get_entity_ui_guidance(entity)
            if guidance:
                entity_summaries.append({
                    "entity": entity,
                    "description": guidance.get("description", "")[:100] + "..." if len(guidance.get("description", "")) > 100 else guidance.get("description", ""),
                    "synonyms": guidance.get("synonyms", []),
                    "total_pages": guidance.get("total_pages", 0),
                    "has_buttons": guidance.get("has_buttons", False),
                    "has_forms": guidance.get("has_forms", False),
                    "has_tabs": guidance.get("has_tabs", False)
                })
        
        result = {
            "available_entities": entities,
            "total_entities": len(entities),
            "entity_details": entity_summaries
        }
        
        print(f"✅ [UI-TOOL] Found {len(entities)} available UI entities")
        return json.dumps(result, indent=2)
        
    except Exception as e:
        print(f"❌ [UI-TOOL] Error getting available UI entities: {e}")
        return json.dumps({
            "error": f"Failed to get available UI entities: {str(e)}"
        })

def search_ui_by_keyword(keyword: str) -> str:
    """
    Search for UI guidance by keyword across all entities
    
    Args:
        keyword: Keyword to search for (in entity names, descriptions, synonyms)
        
    Returns:
        JSON string containing search results
    """
    try:
        print(f"🎨 [UI-TOOL] Searching UI by keyword: {keyword}")
        
        keyword_lower = keyword.lower()
        results = []
        
        entities = ui_config_manager.get_available_ui_entities()
        for entity in entities:
            guidance = ui_config_manager.get_entity_ui_guidance(entity)
            if not guidance:
                continue
            
            # Search in entity name
            if keyword_lower in entity.lower():
                results.append({
                    "entity": entity,
                    "match_type": "entity_name",
                    "description": guidance.get("description", ""),
                    "relevance_score": 10
                })
                continue
            
            # Search in synonyms
            synonyms = guidance.get("synonyms", [])
            synonym_match = any(keyword_lower in synonym.lower() for synonym in synonyms)
            if synonym_match:
                results.append({
                    "entity": entity,
                    "match_type": "synonym",
                    "description": guidance.get("description", ""),
                    "synonyms": synonyms,
                    "relevance_score": 8
                })
                continue
            
            # Search in description
            description = guidance.get("description", "")
            if keyword_lower in description.lower():
                results.append({
                    "entity": entity,
                    "match_type": "description",
                    "description": description,
                    "relevance_score": 5
                })
        
        # Sort by relevance score (highest first)
        results.sort(key=lambda x: x.get("relevance_score", 0), reverse=True)
        
        search_result = {
            "keyword": keyword,
            "results": results,
            "total_matches": len(results)
        }
        
        print(f"✅ [UI-TOOL] Found {len(results)} matches for keyword: {keyword}")
        return json.dumps(search_result, indent=2)
        
    except Exception as e:
        print(f"❌ [UI-TOOL] Error searching UI by keyword: {e}")
        return json.dumps({
            "error": f"Failed to search UI by keyword: {str(e)}",
            "keyword": keyword
        }) 