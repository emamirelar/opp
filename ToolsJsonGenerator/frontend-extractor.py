#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Frontend UI Metadata Extractor
Extracts UI documentation from Angular components using JSDoc-style comments
"""

import json
import os
import re
import ast
from typing import Dict, List, Any, Optional
from pathlib import Path
import glob

class FrontendExtractor:
    """Extract UI metadata from Angular TypeScript components"""
    
    def __init__(self, angular_project_path: str):
        self.angular_project_path = Path(angular_project_path)
        self.src_path = self.angular_project_path / "src" / "app"
        
    def extract_all_entities(self) -> Dict[str, Any]:
        """Extract all entity-specific UI metadata"""
        entities = {}
        
        # Find all component files
        component_files = list(self.src_path.glob("**/*.component.ts"))
        
        for component_file in component_files:
            try:
                component_metadata = self.extract_component_metadata(component_file)
                if component_metadata and component_metadata.get('entity'):
                    entity_name = component_metadata['entity']
                    
                    if entity_name not in entities:
                        entities[entity_name] = {
                            "entity": entity_name,
                            "description": component_metadata.get('description', ''),
                            "synonyms": component_metadata.get('synonyms', []),
                            "mandatoryFields": component_metadata.get('mandatoryFields', []),
                            "pages": []
                        }
                    
                    # Add page information
                    if component_metadata.get('route'):
                        entities[entity_name]['pages'].append({
                            "route": component_metadata['route'],
                            "name": component_metadata['name'],
                            "description": component_metadata['description'],
                            "capabilities": component_metadata.get('capabilities', []),
                            "buttons": component_metadata.get('buttons', []),
                            "forms": component_metadata.get('forms', []),
                            "tabs": component_metadata.get('tabs', []),
                            "search": component_metadata.get('search', {}),
                            "filters": component_metadata.get('filters', []),
                            "help_guidance": component_metadata.get('help_guidance', {})
                        })
                        
            except Exception as e:
                print(f"Error processing {component_file}: {e}")
                continue
                
        return entities
    
    def extract_component_metadata(self, file_path: Path) -> Optional[Dict[str, Any]]:
        """Extract metadata from a single component file"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Extract JSDoc comment block
            jsdoc_match = re.search(r'/\*\*\s*(.*?)\s*\*/', content, re.DOTALL)
            if not jsdoc_match:
                return None
                
            jsdoc_content = jsdoc_match.group(1)
            
            # Parse JSDoc tags
            metadata = self.parse_jsdoc_tags(jsdoc_content)
            
            # Extract component class name
            class_match = re.search(r'export class (\w+)', content)
            if class_match:
                metadata['name'] = class_match.group(1)
            
            # Extract component selector
            selector_match = re.search(r"selector:\s*['\"]([^'\"]+)['\"]", content)
            if selector_match:
                metadata['selector'] = selector_match.group(1)
            
            # Extract template methods for buttons/actions
            metadata['buttons'] = self.extract_buttons(content)
            metadata['forms'] = self.extract_forms(content)
            
            return metadata if metadata.get('entity') else None
            
        except Exception as e:
            print(f"Error extracting from {file_path}: {e}")
            return None
    
    def parse_jsdoc_tags(self, jsdoc_content: str) -> Dict[str, Any]:
        """Parse JSDoc tags from comment content"""
        metadata = {}
        
        # Remove asterisks and clean up
        lines = [line.strip().lstrip('*').strip() for line in jsdoc_content.split('\n')]
        current_tag = None
        current_content = []
        
        for line in lines:
            if line.startswith('@'):
                # Save previous tag
                if current_tag:
                    self.save_jsdoc_tag(metadata, current_tag, current_content)
                
                # Start new tag
                parts = line.split(' ', 1)
                current_tag = parts[0][1:]  # Remove @
                current_content = [parts[1]] if len(parts) > 1 else []
            else:
                if line.strip():
                    current_content.append(line)
        
        # Save last tag
        if current_tag:
            self.save_jsdoc_tag(metadata, current_tag, current_content)
            
        return metadata
    
    def save_jsdoc_tag(self, metadata: Dict[str, Any], tag: str, content: List[str]) -> None:
        """Save a JSDoc tag to metadata"""
        content_str = ' '.join(content).strip()
        
        if tag == 'uiEntity':
            metadata['entity'] = content_str
        elif tag == 'description':
            metadata['description'] = content_str
        elif tag == 'route':
            metadata['route'] = content_str
        elif tag == 'capabilities':
            metadata['capabilities'] = [cap.strip() for cap in content_str.split(',')]
        elif tag == 'synonyms':
            metadata['synonyms'] = [syn.strip() for syn in content_str.split(',')]
        elif tag == 'mandatoryFields':
            metadata['mandatoryFields'] = [field.strip() for field in content_str.split(',')]
        elif tag == 'help_when_stuck':
            if 'help_guidance' not in metadata:
                metadata['help_guidance'] = {}
            metadata['help_guidance']['when_stuck'] = content_str
        elif tag == 'common_tasks':
            if 'help_guidance' not in metadata:
                metadata['help_guidance'] = {}
            # Parse list items
            tasks = []
            for line in content:
                if line.strip().startswith('-'):
                    tasks.append(line.strip()[1:].strip())
            metadata['help_guidance']['common_tasks'] = tasks
        elif tag == 'tabs':
            # Parse tab definitions
            metadata['tabs'] = self.parse_tabs(content_str)
    
    def parse_tabs(self, content: str) -> List[Dict[str, str]]:
        """Parse tab definitions from JSDoc content"""
        tabs = []
        # Expected format: "Details:/path1,Contacts:/path2,Interactions:/path3"
        for tab_def in content.split(','):
            if ':' in tab_def:
                name, route = tab_def.split(':', 1)
                tabs.append({
                    "name": name.strip(),
                    "route": route.strip(),
                    "label": f"title.{name.strip().lower()}"
                })
        return tabs
    
    def extract_buttons(self, content: str) -> List[Dict[str, Any]]:
        """Extract button metadata from component methods"""
        buttons = []
        
        # Look for @uiButton comments
        button_pattern = r'/\*\*\s*\*\s*@uiButton\s+(\w+)\s*(.*?)\s*\*/\s*(\w+)\s*\('
        
        for match in re.finditer(button_pattern, content, re.DOTALL):
            action = match.group(1)
            button_doc = match.group(2)
            method_name = match.group(3)
            
            button_metadata = self.parse_button_doc(button_doc)
            button_metadata.update({
                "id": f"{action}-btn",
                "action": action,
                "method": method_name
            })
            
            buttons.append(button_metadata)
        
        return buttons
    
    def parse_button_doc(self, doc_content: str) -> Dict[str, Any]:
        """Parse button documentation from JSDoc content"""
        metadata = {}
        lines = [line.strip().lstrip('*').strip() for line in doc_content.split('\n')]
        
        for line in lines:
            if line.startswith('@description'):
                metadata['description'] = line.split(' ', 1)[1]
            elif line.startswith('@when_to_use'):
                uses = line.split(' ', 1)[1]
                metadata['when_to_use'] = [use.strip() for use in uses.split(',')]
            elif line.startswith('@permissions'):
                perms = line.split(' ', 1)[1]
                metadata['permissions'] = [perm.strip() for perm in perms.split(',')]
            elif line.startswith('@label'):
                metadata['label'] = line.split(' ', 1)[1]
            elif line.startswith('@icon'):
                metadata['icon'] = line.split(' ', 1)[1]
        
        return metadata
    
    def extract_forms(self, content: str) -> List[Dict[str, Any]]:
        """Extract form metadata from component"""
        forms = []
        
        # Look for FormGroup definitions
        form_pattern = r'(\w+):\s*FormGroup\s*=.*?this\.fb\.group\s*\(\s*\{(.*?)\}\s*\)'
        
        for match in re.finditer(form_pattern, content, re.DOTALL):
            form_name = match.group(1)
            form_fields_content = match.group(2)
            
            # Extract field names
            field_pattern = r'(\w+):\s*\[.*?\]'
            fields = [field_match.group(1) for field_match in re.finditer(field_pattern, form_fields_content)]
            
            forms.append({
                "name": form_name,
                "fields": fields,
                "description": f"Form for {form_name.replace('Form', '').replace('Group', '')}"
            })
        
        return forms

def main():
    """Main function to extract frontend metadata"""
    if len(sys.argv) < 2:
        print("Usage: python frontend-extractor.py <angular-project-path>")
        return
    
    project_path = sys.argv[1]
    extractor = FrontendExtractor(project_path)
    
    # Extract all entities
    entities = extractor.extract_all_entities()
    
    # Save each entity to separate files
    output_dir = Path("../UNOPS.PAO.AIService/config/tools")
    output_dir.mkdir(exist_ok=True)
    
    for entity_name, entity_data in entities.items():
        output_file = output_dir / f"{entity_name.lower()}-ui.json"
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(entity_data, f, indent=2, ensure_ascii=False)
        print(f"Generated: {output_file}")

if __name__ == "__main__":
    import sys
    main() 