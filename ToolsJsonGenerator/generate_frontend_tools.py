#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Frontend UI Tools Generator
Extends the existing ToolsJsonGenerator approach for frontend Angular components
"""

import json
import os
import sys
import argparse
from pathlib import Path
from typing import Dict, List, Any
from datetime import datetime

# Import the existing generator infrastructure
from llm_generator import ToolsJsonGenerator, load_environment_config

# Import the frontend extractor (note: file has hyphen, import uses underscore)
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
try:
    # Get the absolute path to the frontend extractor file
    script_dir = os.path.dirname(os.path.abspath(__file__))
    frontend_extractor_path = os.path.join(script_dir, 'frontend-extractor.py')
    
    if not os.path.exists(frontend_extractor_path):
        raise FileNotFoundError(f"Frontend extractor not found at: {frontend_extractor_path}")
    
    exec(open(frontend_extractor_path).read())
    FrontendExtractor = globals()['FrontendExtractor']
except Exception as e:
    print(f"[ERROR] Could not load frontend extractor: {e}")
    print(f"[DEBUG] Script directory: {os.path.dirname(os.path.abspath(__file__))}")
    print(f"[DEBUG] Current working directory: {os.getcwd()}")
    print(f"[DEBUG] Looking for: {os.path.join(os.path.dirname(os.path.abspath(__file__)), 'frontend-extractor.py')}")
    sys.exit(1)

# Import tour generator
try:
    from tour_generator import TourGenerator
except ImportError:
    print("[WARNING] Tour generator not available - tours will not be generated")
    TourGenerator = None

class FrontendUIGenerator(ToolsJsonGenerator):
    """Extend the existing ToolsJsonGenerator for frontend UI metadata"""
    
    def __init__(self, project_id: str = None, location: str = None):
        # Initialize with parent class
        super().__init__(project_id, location)
        
    def extract_frontend_metadata(self, angular_project_path: str) -> Dict[str, Any]:
        """Extract metadata from Angular project using the frontend extractor"""
        
        # Use the frontend extractor that was loaded at module level
        
        extractor = FrontendExtractor(angular_project_path)
        entities = extractor.extract_all_entities()
        
        return {
            "ProjectPath": angular_project_path,
            "ExtractedAt": datetime.utcnow().isoformat(),
            "TotalEntities": len(entities),
            "Entities": entities
        }
    
    def create_ui_generation_prompt(self, entity_name: str, entity_metadata: Dict[str, Any]) -> str:
        """Create specialized prompt for UI guidance generation"""
        
        prompt = f"""
You are an expert at creating comprehensive UI guidance documentation for AI assistants.

## TASK
Transform the provided Angular component metadata into detailed UI guidance JSON following the EXACT format specified.

## OUTPUT FORMAT
Generate a valid JSON object with this EXACT structure:
```json
{{
  "entity": "{entity_name}",
  "description": "<Comprehensive description of UI capabilities for this entity>",
  "synonyms": ["<synonym1>", "<synonym2>", "<synonym3>", "<synonym4>", "<synonym5>"],
  "mandatoryFields": ["<field1>", "<field2>", "<field3>"],
  "pages": [
    {{
      "route": "<RoutePattern>",
      "name": "<ComponentName>",
      "description": "<Detailed page description>",
      "capabilities": ["<capability1>", "<capability2>", "<capability3>"],
      "buttons": [
        {{
          "id": "<button_id>",
          "label": "<Button Label>",
          "icon": "<icon_class>",
          "action": "<action_name>",
          "description": "<What this button does>",
          "when_to_use": [
            "<Natural language scenario 1>",
            "<Natural language scenario 2>",
            "<Natural language scenario 3>"
          ]
        }}
      ],
      "forms": [
        {{
          "name": "<form_name>",
          "description": "<Form purpose>",
          "fields": ["<field1>", "<field2>", "<field3>"],
          "validation_rules": ["<rule1>", "<rule2>"]
        }}
      ],
      "tabs": [
        {{
          "name": "<tab_name>",
          "route": "<tab_route>",
          "label": "<tab_label>",
          "description": "<What this tab contains>"
        }}
      ],
      "search": {{
        "placeholder": "<Search placeholder text>",
        "description": "<Search capabilities>",
        "help_text": "<Search tips for users>"
      }},
      "filters": [
        {{
          "name": "<filter_name>",
          "label": "<Filter Label>",
          "description": "<What this filter does>"
        }}
      ],
      "help_guidance": {{
        "when_stuck": "<Comprehensive help when users are confused>",
        "common_tasks": [
          "<Task>: <How to do it>",
          "<Task>: <How to do it>",
          "<Task>: <How to do it>"
        ],
        "troubleshooting": [
          "Problem: <Issue> → Solution: <Fix>",
          "Problem: <Issue> → Solution: <Fix>"
        ],
        "getting_started": "<Step-by-step guide for new users>",
        "keyboard_shortcuts": [
          "<Key>: <Action>",
          "<Key>: <Action>"
        ]
      }}
    }}
  ]
}}
```

## CRITICAL REQUIREMENTS

### 1. ENTITY NAME
Use exactly: "{entity_name}"

### 2. USE ONLY ACTUAL DATA - DO NOT INVENT
- **CRITICAL**: Only use buttons, forms, tabs, and IDs that actually exist in the provided metadata
- If the metadata shows empty arrays (buttons: [], forms: [], tabs: []), keep them empty - DO NOT generate fictional entries
- If an element has no ID in the metadata, DO NOT assign one
- DO NOT create fictional button IDs, form names, or UI element selectors
- **DO NOT include "permissions" arrays** - the frontend doesn't know about permission names, so omit this field entirely
- Base all content STRICTLY on the extracted metadata provided below

### 3. COMPREHENSIVE DESCRIPTIONS
- Page descriptions should explain the business purpose and user workflows
- Button descriptions should be action-oriented and clear (only for buttons that actually exist)
- Help guidance should be practical and specific

### 4. USER-CENTRIC LANGUAGE
- Write for end users, not developers
- Use business terminology, not technical jargon
- Focus on "what" and "how" rather than implementation details

### 5. ACTIONABLE GUIDANCE
- "when_to_use" should be specific scenarios users encounter
- "common_tasks" should be step-by-step instructions
- "troubleshooting" should address real user problems

### 6. HELPFUL GUIDANCE  
- Provide clear guidance on how to use features
- Explain what happens when actions fail or are unavailable
- Provide alternative approaches for common scenarios

### 7. COMPREHENSIVE HELP
- "when_stuck" should address the most common confusion points
- "getting_started" should guide new users through their first task
- "troubleshooting" should solve common problems

## ENTITY METADATA TO TRANSFORM:
```json
{json.dumps(entity_metadata, indent=2)}
```

**REMINDER: DO NOT INVENT FICTIONAL DATA**
- If buttons array is empty in the metadata above, leave it empty in your response
- If forms array is empty in the metadata above, leave it empty in your response  
- If tabs array is empty in the metadata above, leave it empty in your response
- DO NOT include "permissions" fields in buttons - omit this field entirely
- Only describe functionality that actually exists based on the metadata

Generate the comprehensive UI guidance JSON now (JSON only, no explanations):
"""
        return prompt
    
    def generate_ui_tools_json(self, entity_name: str, entity_metadata: Dict[str, Any]) -> str:
        """Generate UI tools JSON for a single entity using Vertex AI"""
        
        print(f"[UI-AI] Generating {entity_name} UI guidance with Vertex AI Gemini...")
        
        # Create the specialized UI prompt
        prompt = self.create_ui_generation_prompt(entity_name, entity_metadata)
        
        # Check token count
        estimated_tokens = len(prompt.split()) * 1.3
        print(f"[UI-STATS] Estimated token count: {estimated_tokens:.0f}")
        
        try:
            # Generate with Vertex AI Gemini using parent class method
            from vertexai.generative_models import GenerationConfig
            
            generation_config = GenerationConfig(
                temperature=0.1,  # Low temperature for consistent output
                top_p=0.8,
                top_k=40,
                max_output_tokens=8192,
            )
            
            response = self.model.generate_content(
                prompt,
                generation_config=generation_config
            )
            
            # Extract the JSON from response
            response_text = response.text.strip()
            
            # Remove markdown code blocks if present
            if response_text.startswith('```json'):
                response_text = response_text[7:]
            if response_text.startswith('```'):
                response_text = response_text[3:]
            if response_text.endswith('```'):
                response_text = response_text[:-3]
            
            response_text = response_text.strip()
            
            # Validate JSON
            try:
                json.loads(response_text)
                return response_text
            except json.JSONDecodeError as e:
                raise Exception(f"Generated UI content is not valid JSON: {str(e)}")
                
        except Exception as e:
            raise Exception(f"Failed to generate UI guidance with Vertex AI: {str(e)}")
    
    def process_frontend_metadata(self, angular_project_path: str, output_dir: str):
        """Main processing pipeline for frontend UI metadata"""
        print("=" * 80)
        print("[UI-GENERATOR] FRONTEND UI TOOLS GENERATOR")
        print("=" * 80)
        print()
        
        # Extract frontend metadata
        print(f"[EXTRACT] Extracting Angular component metadata from: {angular_project_path}")
        frontend_metadata = self.extract_frontend_metadata(angular_project_path)
        
        entities = frontend_metadata.get('Entities', {})
        if not entities:
            print("❌ No entities found with UI documentation")
            return
            
        total_entities = len(entities)
        print(f"   [FOUND] {total_entities} entities with UI documentation")
        print(f"   [TIME] Extracted at: {frontend_metadata.get('ExtractedAt', 'Unknown')}")
        print()
        
        # Create output directory with new UI structure
        ui_tools_dir = Path(output_dir) / "tools" / "ui"
        ui_tools_dir.mkdir(parents=True, exist_ok=True)
        
        # Process each entity
        generated_files = []
        processed_count = 0
        failed_count = 0
        
        for entity_name, entity_data in entities.items():
            print(f"[PROCESS] Generating UI guidance for {entity_name}...")
            
            try:
                # Generate UI tools JSON
                ui_tools_json = self.generate_ui_tools_json(entity_name, entity_data)
                
                # Save entity UI tools file
                ui_tools_file = ui_tools_dir / f"{entity_name.lower()}-ui.json"
                self.save_tools_json(ui_tools_json, str(ui_tools_file))
                generated_files.append(ui_tools_file)
                
                # Count generated pages/components
                try:
                    parsed = json.loads(ui_tools_json)
                    pages = parsed.get('pages', [])
                    print(f"   [OK] Generated {entity_name} UI guidance with {len(pages)} pages -> tools/ui/{entity_name.lower()}-ui.json")
                    processed_count += 1
                except Exception as e:
                    print(f"   [WARNING] Failed to parse UI data for {entity_name}: {e}")
                    processed_count += 1
                    
            except Exception as e:
                print(f"   [ERROR] Failed to process {entity_name}: {e}")
                failed_count += 1
            
            print()
        
        # Show results summary
        print("=" * 80)
        print("[SUCCESS] Frontend UI guidance files generated!")
        print("=" * 80)
        print(f"   Output Directory: {ui_tools_dir}")
        print(f"   Entities Processed: {processed_count}/{total_entities}")
        if failed_count > 0:
            print(f"   Failed: {failed_count}")
        print(f"   Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print()
        
        if generated_files:
            print("[FILES] Generated UI guidance files:")
            for ui_file in generated_files:
                file_size = os.path.getsize(ui_file)
                print(f"   - {ui_file.name} ({file_size:,} bytes)")
        
        if failed_count > 0:
            print(f"\n[WARNING] {failed_count} entities failed to process. Check logs above for details.")
        
        print()
        print("[SUCCESS] Frontend UI guidance ready for AI assistant!")
        
        # Also generate DriverJS tours
        self.generate_tours_from_ui_metadata(ui_tools_dir, output_dir)
        
        print("=" * 80)
    
    def generate_tours_from_ui_metadata(self, ui_tools_dir: Path, output_dir: str):
        """Generate DriverJS tours from UI metadata"""
        if not TourGenerator:
            print("\n[SKIP] Tour generator not available")
            return
            
        print()
        print("🎪 Generating DriverJS tours...")
        
        try:
            # Determine the correct path to Angular tours directory
            # Try to find the ClientApp directory relative to current location
            script_dir = Path(__file__).parent
            
            # Look for ClientApp relative to the script directory
            possible_paths = [
                script_dir.parent / "UNOPS.PAO.ClientApp" / "src" / "app" / "common" / "tours",
                Path("../UNOPS.PAO.ClientApp/src/app/common/tours"),
                Path("UNOPS.PAO.ClientApp/src/app/common/tours")
            ]
            
            tours_dir = None
            for path in possible_paths:
                if path.exists() or path.parent.exists():
                    tours_dir = path
                    break
            
            if not tours_dir:
                print("[ERROR] Could not find UNOPS.PAO.ClientApp/src/app/common/tours directory")
                print(f"[DEBUG] Tried paths: {[str(p) for p in possible_paths]}")
                return
            
            # Ensure tours directory exists
            tours_dir.mkdir(parents=True, exist_ok=True)
            
            print(f"[TOURS] Target directory: {tours_dir}")
            
            # Initialize tour generator and process
            tour_generator = TourGenerator()
            tour_generator.process_ui_metadata_directory(str(ui_tools_dir), str(tours_dir))
            
        except Exception as e:
            print(f"[ERROR] Failed to generate tours: {e}")
            print("[SKIP] Continuing without tours...")

def main():
    """Main function for frontend UI tools generation"""
    parser = argparse.ArgumentParser(description='Generate UI guidance JSON from Angular components using Vertex AI')
    parser.add_argument('--angular-project', '-a', required=True, help='Path to Angular project (UNOPS.PAO.ClientApp)')
    parser.add_argument('--output-dir', '-o', default='../UNOPS.PAO.AIService/config', help='Output directory (default: ../UNOPS.PAO.AIService/config)')
    parser.add_argument('--project', '-p', help='Google Cloud Project ID (or set GOOGLE_CLOUD_PROJECT env var)')
    parser.add_argument('--location', '-l', default='us-central1', help='Google Cloud location (default: us-central1)')
    parser.add_argument('--environment', '-e', help='Environment (dev, staging, production)')
    
    args = parser.parse_args()
    
    # Set environment if specified
    if args.environment:
        os.environ['PAO_ENVIRONMENT'] = args.environment
        print(f"[ENV] Using specified environment: {args.environment}")
    
    try:
        generator = FrontendUIGenerator(project_id=args.project, location=args.location)
        generator.process_frontend_metadata(args.angular_project, args.output_dir)
        return 0
    except Exception as e:
        print(f"[ERROR] {str(e)}", file=sys.stderr)
        print()
        print("[TIPS] Troubleshooting tips:")
        print("   1. Make sure Angular project path is correct")
        print("   2. Ensure components have @uiEntity JSDoc documentation")
        print("   3. Verify Google Cloud authentication")
        print("   4. Check Vertex AI API permissions")
        return 1

if __name__ == "__main__":
    sys.exit(main()) 