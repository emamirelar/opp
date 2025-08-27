#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
DriverJS Tour Generator
Converts UI metadata to DriverJS tour configurations using intelligent selector mapping
"""

import json
import os
import re
from pathlib import Path
from typing import Dict, List, Any, Optional
from datetime import datetime

class TourGenerator:
    """Generate DriverJS tours from UI metadata using intelligent selector mapping"""
    
    # Button priority for tour ordering
    BUTTON_PRIORITY = {
        'create': 1,    # New Partner, Add Contact
        'edit': 2,      # Edit buttons  
        'import': 3,    # Import actions
        'search': 4,    # Search/filter
        'export': 5,    # Export actions
        'delete': 6,    # Delete actions
        'help': 7       # Help features
    }
    
    def __init__(self):
        self.tour_counter = 0
        
    def extract_button_action_type(self, button_id: str, label: str) -> str:
        """Extract the action type from button metadata for prioritization"""
        button_id_lower = button_id.lower()
        label_lower = label.lower()
        
        # Check for specific action patterns
        if 'create' in button_id_lower or 'new' in label_lower or 'add' in label_lower:
            return 'create'
        elif 'edit' in button_id_lower or 'edit' in label_lower:
            return 'edit'
        elif 'import' in button_id_lower or 'import' in label_lower:
            return 'import'
        elif 'export' in button_id_lower or 'export' in label_lower:
            return 'export'
        elif 'delete' in button_id_lower or 'delete' in label_lower or 'remove' in label_lower:
            return 'delete'
        elif 'search' in button_id_lower or 'filter' in button_id_lower or 'search' in label_lower:
            return 'search'
        else:
            return 'help'
    
    def generate_primeng_selectors(self, button_metadata: Dict[str, Any]) -> List[str]:
        """Generate intelligent selectors for PrimeNG components"""
        button_id = button_metadata.get('id', '')
        label = button_metadata.get('label', '')
        icon = button_metadata.get('icon', '')
        
        selectors = []
        
        # PrimeNG p-button patterns
        if label:
            selectors.extend([
                f'p-button[label="{label}"]',
                f'p-button .p-button-label:contains("{label}")',
                f'button:contains("{label}")',
                f'[aria-label="{label}"]',
                f'[title="{label}"]'
            ])
        
        # Icon-based selectors
        if icon:
            icon_classes = icon.replace(' ', '.')
            selectors.extend([
                f'p-button[icon="{icon}"]',
                f'.{icon_classes}',
                f'i.{icon_classes}',
                f'button .{icon_classes}'
            ])
        
        # ID-based selectors (convert snake_case to kebab-case)
        if button_id:
            kebab_id = button_id.replace('_', '-')
            selectors.extend([
                f'#{kebab_id}',
                f'#{kebab_id}-btn',
                f'[data-test-id="{kebab_id}"]',
                f'.{kebab_id}-button'
            ])
        
        # Generic fallbacks
        selectors.extend([
            f'p-button',
            f'button',
            f'.p-element'
        ])
        
        return selectors
    
    def create_tour_step(self, button_metadata: Dict[str, Any], step_number: int) -> Dict[str, Any]:
        """Create a single tour step from button metadata"""
        selectors = self.generate_primeng_selectors(button_metadata)
        primary_selector = selectors[0] if selectors else 'button'
        
        # Extract description and usage information
        description = button_metadata.get('description', '')
        when_to_use = button_metadata.get('when_to_use', [])
        permissions = button_metadata.get('permissions', [])
        
        # Create rich description
        full_description = description
        if when_to_use:
            full_description += f"<br><br><strong>When to use:</strong> {', '.join(when_to_use[:2])}"
        if permissions:
            full_description += f"<br><small><em>Requires: {', '.join(permissions)}</em></small>"
        
        return {
            "element": primary_selector,
            "popover": {
                "title": button_metadata.get('label', 'Button'),
                "description": full_description,
                "side": "bottom",
                "align": "start"
            },
            "options": {
                "selectors": selectors,  # Include fallback selectors
                "stepNumber": step_number
            }
        }
    
    def create_page_overview_step(self, page_metadata: Dict[str, Any]) -> Dict[str, Any]:
        """Create an overview step for the page"""
        return {
            "popover": {
                "title": f"Welcome to {page_metadata.get('name', 'this page')}",
                "description": page_metadata.get('description', 'Let\'s explore the features available on this page.'),
                "side": "over",
                "align": "center"
            }
        }
    
    def generate_tour_from_ui_metadata(self, entity_name: str, ui_metadata: Dict[str, Any]) -> Dict[str, Any]:
        """Generate a complete DriverJS tour from UI metadata matching existing format"""
        pages = ui_metadata.get('pages', [])
        if not pages:
            return None
        
        # For now, focus on the main page (usually the first one)
        main_page = pages[0]
        buttons = main_page.get('buttons', [])
        
        # Sort buttons by priority
        def get_button_priority(button):
            action_type = self.extract_button_action_type(
                button.get('id', ''), 
                button.get('label', '')
            )
            return self.BUTTON_PRIORITY.get(action_type, 999)
        
        sorted_buttons = sorted(buttons, key=get_button_priority)
        
        # Create tour steps
        steps = []
        
        # Add welcome step (matches existing format)
        steps.append({
            "popover": {
                "titleKey": f"tour.{entity_name.lower()}.steps.welcome.title",
                "descriptionKey": f"tour.{entity_name.lower()}.steps.welcome.description",
                "side": "over",
                "align": "center"
            }
        })
        
        # Add section header step if mentioned in buttons
        header_buttons = [b for b in sorted_buttons if 'header' in b.get('id', '').lower() or 'section' in b.get('id', '').lower()]
        if header_buttons:
            steps.append({
                "element": f".{entity_name.lower()}-section-header",
                "fallbackType": "section-header",
                "popover": {
                    "titleKey": f"tour.{entity_name.lower()}.steps.header.title",
                    "descriptionKey": f"tour.{entity_name.lower()}.steps.header.description",
                    "side": "bottom",
                    "align": "start"
                }
            })
        
        # Add button steps (limit to 6 for good UX)
        for button in sorted_buttons[:6]:
            step = self.create_tour_step_with_i18n(button, entity_name)
            if step:
                steps.append(step)
        
        # Add tour control step (standard across all tours)
        steps.append({
            "element": "app-tour-control",
            "fallbackType": "tour-control",
            "popover": {
                "titleKey": f"tour.{entity_name.lower()}.steps.tourControl.title",
                "descriptionKey": f"tour.{entity_name.lower()}.steps.tourControl.description",
                "side": "bottom",
                "align": "center"
            }
        })
        
        # Create tour configuration matching existing format
        tour_config = {
            "tourId": f"{entity_name.lower()}-tour",
            "titleKey": f"tour.{entity_name.lower()}.title",
            "descriptionKey": f"tour.{entity_name.lower()}.description",
            "entity": entity_name,
            "route": main_page.get('route', ''),
            "showButtons": [
                "next",
                "previous", 
                "close"
            ],
            "allowClose": True,
            "overlayClickNext": False,
            "popoverOffset": 10,
            "steps": steps,
            "generatedAt": datetime.utcnow().isoformat(),
            "version": "1.0"
        }
        
        return tour_config
    
    def create_tour_step_with_i18n(self, button_metadata: Dict[str, Any], entity_name: str) -> Optional[Dict[str, Any]]:
        """Create a tour step with i18n keys matching existing format"""
        button_id = button_metadata.get('id', '')
        label = button_metadata.get('label', '')
        
        if not button_id and not label:
            return None
        
        # Generate CSS selector
        selectors = self.generate_primeng_selectors(button_metadata)
        primary_selector = selectors[0] if selectors else 'button'
        
        # Convert button ID to step key
        if button_id:
            step_key = button_id.replace('_', '').replace('-', '').lower()
        else:
            step_key = label.lower().replace(' ', '').replace('-', '')
        
        # Generate fallback type
        fallback_type = self.generate_fallback_type(button_metadata)
        
        step = {
            "element": primary_selector,
            "fallbackType": fallback_type,
            "popover": {
                "titleKey": f"tour.{entity_name.lower()}.steps.{step_key}.title",
                "descriptionKey": f"tour.{entity_name.lower()}.steps.{step_key}.description",
                "side": "bottom",
                "align": "start"
            }
        }
        
        return step
    
    def generate_fallback_type(self, button_metadata: Dict[str, Any]) -> str:
        """Generate fallback type for button"""
        button_id = button_metadata.get('id', '')
        label = button_metadata.get('label', '')
        
        if 'new' in button_id.lower() or 'create' in button_id.lower() or 'add' in label.lower():
            return "new-button"
        elif 'import' in button_id.lower() or 'import' in label.lower():
            return "import-button"
        elif 'search' in button_id.lower() or 'search' in label.lower():
            return "search-input"
        elif 'filter' in button_id.lower() or 'advanced' in label.lower():
            return "advanced-search"
        elif 'export' in button_id.lower() or 'export' in label.lower():
            return "export-button"
        else:
            return button_id.replace('_', '-') if button_id else "button"
    
    def process_ui_metadata_directory(self, ui_tools_dir: str, output_dir: str):
        """Process all UI metadata files and generate tours"""
        ui_tools_path = Path(ui_tools_dir)
        output_path = Path(output_dir)
        
        # Create output directory
        output_path.mkdir(parents=True, exist_ok=True)
        
        print("=" * 80)
        print("[TOUR-GENERATOR] DRIVERJS TOUR GENERATOR")
        print("=" * 80)
        print()
        
        # Find all UI metadata files
        ui_files = list(ui_tools_path.glob("*-ui.json"))
        
        if not ui_files:
            print("❌ No UI metadata files found")
            return
        
        print(f"[FOUND] {len(ui_files)} UI metadata files")
        print(f"[INPUT] {ui_tools_path}")
        print(f"[OUTPUT] {output_path}")
        print()
        
        generated_tours = []
        tour_registry_entries = []
        
        for ui_file in ui_files:
            try:
                print(f"[PROCESS] {ui_file.name}")
                
                # Load UI metadata
                with open(ui_file, 'r', encoding='utf-8') as f:
                    ui_metadata = json.load(f)
                
                # Extract entity name
                entity_name = ui_metadata.get('entity', ui_file.stem.replace('-ui', ''))
                
                # Generate tour
                tour_config = self.generate_tour_from_ui_metadata(entity_name, ui_metadata)
                
                if tour_config:
                    # Save tour file
                    tour_filename = f"{entity_name.lower()}-tour.json"
                    tour_path = output_path / tour_filename
                    
                    with open(tour_path, 'w', encoding='utf-8') as f:
                        json.dump(tour_config, f, indent=2, ensure_ascii=False)
                    
                    generated_tours.append(tour_path)
                    
                    # Add to registry entries
                    route = tour_config.get('route', '')
                    if route:
                        tour_registry_entries.append({
                            "pattern": route,
                            "tourFile": tour_filename.replace('.json', ''),
                            "description": ui_metadata.get('description', f'{entity_name} page'),
                            "entity": entity_name
                        })
                    
                    step_count = len(tour_config['steps'])
                    print(f"   [OK] Generated tour with {step_count} steps -> {tour_filename}")
                else:
                    print(f"   [SKIP] No buttons found for {entity_name}")
                
            except Exception as e:
                print(f"   [ERROR] Failed to process {ui_file.name}: {e}")
        
        # Update tour registry
        if tour_registry_entries:
            self.update_tour_registry(output_path, tour_registry_entries)
        
        print()
        print("=" * 80)
        print(f"[SUCCESS] Generated {len(generated_tours)} DriverJS tour files!")
        print("=" * 80)
        
        if generated_tours:
            print("[FILES] Generated tour files:")
            for tour_file in generated_tours:
                file_size = tour_file.stat().st_size
                print(f"   - {tour_file.name} ({file_size:,} bytes)")
        
        if tour_registry_entries:
            print()
            print("[REGISTRY] Updated tour registry with new entries:")
            for entry in tour_registry_entries:
                print(f"   - {entry['pattern']} -> {entry['tourFile']}")
    
    def update_tour_registry(self, tours_dir: Path, new_entries: List[Dict[str, Any]]):
        """Update the tour registry with new entries"""
        registry_path = tours_dir / "tour-registry.json"
        
        try:
            # Load existing registry
            if registry_path.exists():
                with open(registry_path, 'r', encoding='utf-8') as f:
                    registry = json.load(f)
            else:
                registry = {"routes": [], "fallbackSelectors": {}}
            
            # Get existing routes to avoid duplicates
            existing_patterns = {route.get('pattern', '') for route in registry.get('routes', [])}
            
            # Add new entries that don't already exist
            routes = registry.get('routes', [])
            added_count = 0
            
            for entry in new_entries:
                pattern = entry.get('pattern', '')
                if pattern and pattern not in existing_patterns:
                    routes.append({
                        "pattern": pattern,
                        "tourFile": entry['tourFile'],
                        "description": entry['description']
                    })
                    existing_patterns.add(pattern)
                    added_count += 1
            
            registry['routes'] = routes
            
            # Ensure fallbackSelectors exist with some defaults
            if not registry.get('fallbackSelectors'):
                registry['fallbackSelectors'] = {
                    "new-button": [
                        "p-button[label*=\"New\"]",
                        "p-button[icon=\"pi pi-plus\"]", 
                        "button[title*=\"New\"]",
                        ".pi-plus",
                        "p-button"
                    ],
                    "import-button": [
                        "p-button[label*=\"Import\"]",
                        "p-button[icon=\"pi pi-file-import\"]",
                        "button[title*=\"Import\"]"
                    ],
                    "search-input": [
                        ".quick-search input",
                        ".search-input",
                        "p-inputtext[placeholder*=\"Search\"]",
                        "input[type=\"search\"]"
                    ],
                    "advanced-search": [
                        ".advanced-search",
                        "p-button[label*=\"Advanced\"]",
                        ".filter-button"
                    ],
                    "section-header": [
                        ".section-header",
                        ".page-header", 
                        "h1, h2, h3"
                    ],
                    "tour-control": [
                        "app-tour-control",
                        ".tour-control-button",
                        "p-button[label*=\"Tour\"]"
                    ]
                }
            
            # Save updated registry
            with open(registry_path, 'w', encoding='utf-8') as f:
                json.dump(registry, f, indent=2, ensure_ascii=False)
            
            print(f"[REGISTRY] Added {added_count} new route(s) to tour registry")
            
        except Exception as e:
            print(f"[ERROR] Failed to update tour registry: {e}")

def main():
    import argparse
    
    parser = argparse.ArgumentParser(description='Generate DriverJS tours from UI metadata')
    parser.add_argument('--ui-tools-dir', '-u', required=True, help='Directory containing UI metadata JSON files')
    parser.add_argument('--output-dir', '-o', required=True, help='Output directory for tour files')
    
    args = parser.parse_args()
    
    generator = TourGenerator()
    generator.process_ui_metadata_directory(args.ui_tools_dir, args.output_dir)

if __name__ == "__main__":
    main() 