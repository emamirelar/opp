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
        """Generate a complete DriverJS tour from UI metadata"""
        pages = ui_metadata.get('pages', [])
        if not pages:
            return None
        
        # For now, focus on the main page (usually the first one)
        main_page = pages[0]
        buttons = main_page.get('buttons', [])
        
        if not buttons:
            return None
        
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
        
        # Add overview step
        steps.append(self.create_page_overview_step(main_page))
        
        # Add button steps
        for i, button in enumerate(sorted_buttons[:6], 1):  # Limit to 6 buttons for good UX
            step = self.create_tour_step(button, i)
            steps.append(step)
        
        # Add completion step
        steps.append({
            "popover": {
                "title": "Tour Complete!",
                "description": f"You've learned the key features of {entity_name}. You can always restart this tour from the help menu.",
                "side": "over",
                "align": "center"
            }
        })
        
        # Create tour configuration
        tour_config = {
            "tourId": f"{entity_name.lower()}-overview",
            "title": f"{entity_name} Overview Tour",
            "description": ui_metadata.get('description', f'Learn how to use {entity_name} features'),
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
                    
                    step_count = len(tour_config['steps'])
                    print(f"   [OK] Generated tour with {step_count} steps -> {tour_filename}")
                else:
                    print(f"   [SKIP] No buttons found for {entity_name}")
                
            except Exception as e:
                print(f"   [ERROR] Failed to process {ui_file.name}: {e}")
        
        print()
        print("=" * 80)
        print(f"[SUCCESS] Generated {len(generated_tours)} DriverJS tour files!")
        print("=" * 80)
        
        if generated_tours:
            print("[FILES] Generated tour files:")
            for tour_file in generated_tours:
                file_size = tour_file.stat().st_size
                print(f"   - {tour_file.name} ({file_size:,} bytes)")

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