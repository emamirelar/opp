#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
LLM Generator for Tools.json
Transforms extracted API metadata into AI agent tools.json using Gemini API
"""

import json
import os
import sys
import argparse
from pathlib import Path
from typing import Dict, List, Any
from datetime import datetime
import vertexai
from vertexai.generative_models import GenerativeModel, GenerationConfig

# Fix encoding for Windows console
if sys.platform.startswith('win'):
    # Set encoding for stdout and stderr to handle Unicode
    import io
    if hasattr(sys.stdout, 'reconfigure'):
        try:
            sys.stdout.reconfigure(encoding='utf-8')
            sys.stderr.reconfigure(encoding='utf-8')
        except:
            # Fallback: replace stdout/stderr with UTF-8 compatible versions
            sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
            sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

def load_environment_file(env_file_path: str) -> Dict[str, str]:
    """Load environment variables from a .env file"""
    env_vars = {}
    if os.path.exists(env_file_path):
        print(f"[FILE] Loading environment from: {env_file_path}")
        try:
            with open(env_file_path, 'r', encoding='utf-8') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        key, value = line.split('=', 1)
                        env_vars[key.strip()] = value.strip()
                        # Also set in current process environment
                        os.environ[key.strip()] = value.strip()
            print(f"   [OK] Loaded {len(env_vars)} environment variables")
        except Exception as e:
                          print(f"   [WARNING] Failed to load {env_file_path}: {e}")
    return env_vars

def auto_detect_environment() -> str:
    """Auto-detect which environment to use based on various indicators"""
    
    # Check if user explicitly set environment
    if 'PAO_ENVIRONMENT' in os.environ:
        return os.environ['PAO_ENVIRONMENT']
    
    # Check common CI/CD environment variables
    if 'GITHUB_REF' in os.environ:
        ref = os.environ['GITHUB_REF']
        if 'main' in ref or 'master' in ref:
            return 'production'
        elif 'staging' in ref or 'stage' in ref:
            return 'staging'
        else:
            return 'dev'
    
    if 'BUILD_SOURCEBRANCH' in os.environ:  # Azure DevOps
        branch = os.environ['BUILD_SOURCEBRANCH']
        if 'main' in branch or 'master' in branch:
            return 'production'
        elif 'staging' in branch or 'stage' in branch:
            return 'staging'
        else:
            return 'dev'
    
    # Default to dev environment
    return 'dev'

def load_environment_config() -> Dict[str, str]:
    """Load environment configuration with automatic detection"""
    
    # Get script directory
    script_dir = Path(__file__).parent
    environments_dir = script_dir / "environments"
    
    # Auto-detect environment
    environment = auto_detect_environment()
    print(f"[INFO] Auto-detected environment: {environment}")
    
    # Try to load environment-specific file first
    env_file = environments_dir / f"{environment}.env"
    env_vars = load_environment_file(str(env_file))
    
    # If no environment-specific file found, try generic .env
    if not env_vars and os.path.exists(script_dir / ".env"):
        env_vars = load_environment_file(str(script_dir / ".env"))
    
    # If still no config found, show available options
    if not env_vars:
        available_envs = []
        if environments_dir.exists():
            for env_file in environments_dir.glob("*.env"):
                available_envs.append(env_file.stem)
        
        if available_envs:
            print(f"[WARNING] No environment config loaded. Available environments: {', '.join(available_envs)}")
            print(f"[TIP] Set PAO_ENVIRONMENT={available_envs[0]} to use a specific environment")
        else:
            print("[WARNING] No environment configuration files found.")
    
    return env_vars

class ToolsJsonGenerator:
    def __init__(self, project_id: str = None, location: str = None):
        """Initialize the generator with Vertex AI"""
        
        # Load environment configuration automatically
        env_config = load_environment_config()
        
        # Use provided parameters, then environment variables, then defaults
        self.project_id = project_id or os.getenv('GOOGLE_CLOUD_PROJECT')
        self.location = location or os.getenv('GOOGLE_CLOUD_LOCATION', 'us-central1')
        
        if not self.project_id:
            raise ValueError(
                "Google Cloud Project ID is required.\n"
                "Solutions:\n"
                "  1. Set GOOGLE_CLOUD_PROJECT environment variable\n"
                "  2. Pass --project parameter\n"
                "  3. Configure environment files (dev.env, staging.env, production.env)\n"
                "  4. Run: gcloud auth application-default login"
            )
        
        print(f"[CLOUD] Using Google Cloud Project: {self.project_id}")
        print(f"[LOCATION] Using location: {self.location}")
        
        # Initialize Vertex AI
        try:
            vertexai.init(project=self.project_id, location=self.location)
            self.model = GenerativeModel('gemini-2.5-flash')
            print("[OK] Vertex AI initialized successfully")
        except Exception as e:
            raise Exception(f"Failed to initialize Vertex AI: {str(e)}\n"
                          f"Make sure you have proper authentication set up:\n"
                          f"  1. Run: gcloud auth application-default login\n"
                          f"  2. Or set up service account key\n"
                          f"  3. Verify project access permissions")
        
    def load_api_metadata(self, input_path: str) -> Dict[str, Any]:
        """Load the extracted API metadata from JSON file"""
        try:
            with open(input_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        except Exception as e:
            raise Exception(f"Failed to load API metadata from {input_path}: {str(e)}")
    
    def create_generation_prompt(self, metadata: Dict[str, Any]) -> str:
        """Create the prompt for Gemini to generate entity-specific tools JSON"""
        
        prompt = f"""
You are an expert at converting .NET Web API controller metadata into structured entity configuration for AI agents.

## TASK
Transform the provided controller metadata into an entity-specific JSON configuration following the EXACT format specified.

## OUTPUT FORMAT
Generate a valid JSON object with this EXACT structure:
```json
{{
  "entity": "<EntityName>",
  "description": "<Description of what this entity manages>",
  "synonyms": ["<synonym1>", "<synonym2>", "<synonym3>", "<synonym4>", "<synonym5>"],
  "mandatoryFields": ["<field1>", "<field2>", "<field3>"],
  "endpoints": [
    {{
      "name": "<MethodName>",
      "url": "<FullRouteURL>",
      "method": "<HTTPMethod>",
      "description": "<Detailed description>",
      "parameters": {{
        "<paramName>": {{ "type": "<type>", "required": <boolean>, "description": "<description>" }}
      }},
      "example_uses": [
        "<Natural language example 1>",
        "<Natural language example 2>",
        "<Natural language example 3>"
      ],
      "when_to_use": "<Clear guidance on when to use this endpoint>"
    }}
  ]
}}
```

## CRITICAL REQUIREMENTS

### 1. ENTITY NAME
- Extract from controller name by removing "Controller" suffix
- Examples: "PartnerController" → "Partner", "ContactController" → "Contact"

### 2. DESCRIPTION  
- Describe what the entity represents and its business purpose
- Keep it concise but informative

### 3. SYNONYMS
- Generate 3-5 alternative terms users might use for this entity
- Consider common business terms, abbreviations, related concepts

### 4. MANDATORY FIELDS
- Identify required/mandatory fields from POST/PUT endpoint parameters
- Look for parameters marked as required in the metadata
- Focus on business-critical fields

### 5. ENDPOINTS
- **name**: Use the exact method name from metadata
- **url**: Combine BaseRoute + Method Route (replace {{id}} with {{id}})
- **method**: HTTP method (GET, POST, PUT, DELETE)
- **description**: Detailed business description of what the endpoint does
- **parameters**: Flat object with each parameter as a property
- **example_uses**: 3+ natural language examples of when to use this endpoint
- **when_to_use**: Clear guidance on the use case for this endpoint

### 6. PARAMETER MAPPING
- string → "string"
- int/integer/long → "integer"  
- bool/boolean → "boolean"
- float/double/decimal → "float"
- DateTime → "string"
- object/model → "object"
- array/list → "array"

## CONTROLLER METADATA TO TRANSFORM:
```json
{json.dumps(metadata, indent=2)}
```

Generate the entity configuration JSON now (JSON only, no explanations):
"""
        return prompt

    def generate_tools_json(self, metadata: Dict[str, Any]) -> str:
        """Generate tools.json using Vertex AI"""
        
        print("[AI] Generating tools.json with Vertex AI Gemini...")
        print(f"   Project: {self.project_id}")
        print(f"   Location: {self.location}")
        
        # Create the prompt
        prompt = self.create_generation_prompt(metadata)
        
        # Check token count (approximate)
        estimated_tokens = len(prompt.split()) * 1.3  # Rough estimate
        print(f"[STATS] Estimated token count: {estimated_tokens:.0f}")
        
        if estimated_tokens > 900000:  # Conservative limit for Gemini Pro 1.5
            print("[WARNING] Large payload detected, consider chunking for production use")
        
        try:
            # Generate with Vertex AI Gemini
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
                raise Exception(f"Generated content is not valid JSON: {str(e)}")
                
        except Exception as e:
            raise Exception(f"Failed to generate with Vertex AI Gemini: {str(e)}")
    
    def save_tools_json(self, tools_json: str, output_path: str):
        """Save the generated tools.json to file"""
        try:
            # Pretty print the JSON
            parsed = json.loads(tools_json)
            formatted = json.dumps(parsed, indent=2, ensure_ascii=False)
            
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(formatted)
                
        except Exception as e:
            raise Exception(f"Failed to save tools.json to {output_path}: {str(e)}")
    


    def process(self, input_path: str, output_path: str):
        """Main processing pipeline - processes controllers individually from single metadata file"""
        print("=" * 80)
        print("[GENERATOR] TOOLS.JSON GENERATOR - AI Agent Configuration Builder")
        print("=" * 80)
        print()
        
        # Load the API metadata
        print(f"[LOAD] Loading API metadata from: {input_path}")
        metadata = self.load_api_metadata(input_path)
        
        controllers = metadata.get('Controllers', [])
        if not controllers:
            raise Exception(f"No controllers found in metadata file: {input_path}")
        
        total_endpoints = sum(len(c.get('Methods', [])) for c in controllers)
        print(f"   [FOUND] {len(controllers)} controllers with {total_endpoints} total endpoints")
        print(f"   [TIME] Extracted at: {metadata.get('ExtractedAt', 'Unknown')}")
        print()
        
        # Create tools subdirectory 
        output_dir = os.path.dirname(output_path)
        tools_dir = os.path.join(output_dir, "tools")
        os.makedirs(tools_dir, exist_ok=True)
        
        # Process each controller separately
        individual_tool_files = []
        processed_count = 0
        failed_count = 0
        
        for controller in controllers:
            controller_name = controller.get('Name', 'Unknown')
            method_count = len(controller.get('Methods', []))
            
            # Generate clean controller name for file (remove "Controller" suffix)
            clean_name = controller_name.replace('Controller', '').lower()
            
            print(f"[PROCESS] Processing {controller_name}...")
            print(f"   [STATS] {method_count} endpoints")
            
            try:
                # Create single-controller metadata for LLM processing
                single_controller_metadata = {
                    "AssemblyName": metadata.get('AssemblyName', 'Unknown'),
                    "AssemblyVersion": metadata.get('AssemblyVersion', 'Unknown'), 
                    "ExtractedAt": metadata.get('ExtractedAt', 'Unknown'),
                    "Controllers": [controller]
                }
                
                # Generate tools for this controller
                controller_tools_json = self.generate_tools_json(single_controller_metadata)
                
                # Save individual controller tools file in tools subdirectory
                controller_tools_file = os.path.join(tools_dir, f"{clean_name}-tools.json")
                self.save_tools_json(controller_tools_json, controller_tools_file)
                individual_tool_files.append(controller_tools_file)
                
                # Count generated endpoints
                try:
                    parsed = json.loads(controller_tools_json)
                    endpoints = parsed.get('endpoints', [])
                    entity_name = parsed.get('entity', clean_name)
                    print(f"   [OK] Generated {entity_name} entity with {len(endpoints)} endpoints -> tools/{clean_name}-tools.json")
                    processed_count += 1
                except Exception as e:
                    print(f"   [WARNING] Failed to parse entity data for {controller_name}: {e}")
                    processed_count += 1  # Still count as processed since file was saved
                    
            except Exception as e:
                print(f"   [ERROR] Failed to process {controller_name}: {e}")
                failed_count += 1
            
            print()
        
        # Show results summary
        print("=" * 80)
        print("[SUCCESS] Individual controller tools.json files generated!")
        print("=" * 80)
        print(f"   Output Directory: {tools_dir}")
        print(f"   Controllers Processed: {processed_count}/{len(controllers)}")
        if failed_count > 0:
            print(f"   Failed: {failed_count}")
        print(f"   Total Endpoints: {total_endpoints}")
        print(f"   Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print()
        
        if individual_tool_files:
            print("[FILES] Generated tool files:")
            for tool_file in individual_tool_files:
                file_size = os.path.getsize(tool_file)
                print(f"   - {os.path.basename(tool_file)} ({file_size:,} bytes)")
        
        if failed_count > 0:
            print(f"\n[WARNING] {failed_count} controllers failed to process. Check logs above for details.")
        
        print()
        print("[SUCCESS] Individual tool files ready for easier management and debugging!")
        print("=" * 80)

def main():
    parser = argparse.ArgumentParser(description='Generate tools.json for AI agent from API metadata using Vertex AI')
    parser.add_argument('--input', '-i', required=True, help='Input API metadata JSON file')
    parser.add_argument('--output', '-o', default='tools.json', help='Output tools.json file (default: tools.json)')
    parser.add_argument('--project', '-p', help='Google Cloud Project ID (or set GOOGLE_CLOUD_PROJECT env var)')
    parser.add_argument('--location', '-l', default='us-central1', help='Google Cloud location (default: us-central1)')
    parser.add_argument('--environment', '-e', help='Environment to use (dev, staging, production). Auto-detected if not specified.')
    parser.add_argument('--timeout', '-t', type=int, default=120, help='Timeout in seconds for LLM requests (default: 120)')
    parser.add_argument('--skip-llm', action='store_true', help='Skip LLM generation and use basic template')
    
    args = parser.parse_args()
    
    # Set environment if specified
    if args.environment:
        os.environ['PAO_ENVIRONMENT'] = args.environment
        print(f"[ENV] Using specified environment: {args.environment}")
    
    try:
        # Handle skip-llm option
        if args.skip_llm:
            print("[SKIP] LLM generation skipped, using basic template")
            # TODO: Implement basic template generation if needed
            return 0
            
        generator = ToolsJsonGenerator(project_id=args.project, location=args.location)
        generator.process(args.input, args.output)
        return 0
    except Exception as e:
        print(f"[ERROR] {str(e)}", file=sys.stderr)
        print()
        print("[TIPS] Troubleshooting tips:")
        print("   1. Make sure Google Cloud SDK is installed")
        print("   2. Run: gcloud auth application-default login")
        print("   3. Verify your environment configuration files")
        print("   4. Check if your project has Vertex AI API enabled")
        return 1

if __name__ == "__main__":
    sys.exit(main()) 