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
        print("Loading environment from:", env_file_path)
        try:
            with open(env_file_path, 'r', encoding='utf-8') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        key, value = line.split('=', 1)
                        env_vars[key.strip()] = value.strip()
                        # Also set in current process environment
                        os.environ[key.strip()] = value.strip()
            print("   Loaded", len(env_vars), "environment variables")
        except Exception as e:
                          print("   WARNING Failed to load", env_file_path, ":", e)
    return env_vars

def auto_detect_environment() -> str:
    """Auto-detect which environment to use based on various indicators"""
    # Check environment variable first
    if 'ENVIRONMENT' in os.environ:
        return os.environ['ENVIRONMENT']
    
    # Check for development indicators
    if 'DEVELOPMENT' in os.environ or 'DEV' in os.environ:
        return 'dev'
    
    # Check for staging indicators
    if 'STAGING' in os.environ:
        return 'staging'
    
    # Check for production indicators
    if 'PRODUCTION' in os.environ or 'PROD' in os.environ:
        return 'production'
    
    # Check hostname patterns (if available)
    hostname = os.environ.get('COMPUTERNAME', os.environ.get('HOSTNAME', ''))
    if 'dev' in hostname.lower():
        return 'dev'
    elif 'staging' in hostname.lower() or 'stage' in hostname.lower():
        return 'staging'
    elif 'prod' in hostname.lower():
        return 'production'
    
    # Default to development for local machines
    return 'dev'

def setup_environment(environment: str = None) -> str:
    """Setup environment configuration"""
    if not environment:
        environment = auto_detect_environment()
    
    print("Environment:", environment)
    
    # Look for environment files in multiple locations
    environments_dir = Path(__file__).parent / "environments"
    env_file = environments_dir / (environment + ".env")
    
    if env_file.exists():
        load_environment_file(str(env_file))
    else:
        print("Warning: Environment file not found:", env_file)
        
    return environment


class ToolsJsonGenerator:
    def __init__(self, project_id: str = None, location: str = None):
        """Initialize the generator with Vertex AI"""
        self.project_id = project_id or os.getenv('GOOGLE_CLOUD_PROJECT')
        self.location = location or os.getenv('GOOGLE_CLOUD_LOCATION', 'us-central1')
        
        if not self.project_id:
            raise Exception("Google Cloud Project ID is required. Set GOOGLE_CLOUD_PROJECT env var or pass --project")
        
        print("Initializing Vertex AI...")
        print("   Project ID:", self.project_id)
        print("   Location:", self.location)
        
        try:
            vertexai.init(project=self.project_id, location=self.location)
            self.model = GenerativeModel("gemini-2.0-flash-exp")
            print("   ✅ Vertex AI initialized successfully")
        except Exception as e:
            print("   ❌ Failed to initialize Vertex AI:", str(e))
            raise

    def create_generation_prompt(self, metadata: Dict[str, Any]) -> str:
        """Create the prompt for Gemini to generate entity-specific tools JSON"""
        
        # Check if we have search metadata
        search_metadata_section = ""
        if 'SearchMetadata' in metadata and metadata['SearchMetadata']:
            search_metadata_json = json.dumps(metadata['SearchMetadata'], indent=2)
            search_metadata_section = "\n\n## SEARCH METADATA AVAILABLE\nThe following search metadata has been extracted from AdvancedSearchHelper:\n```json\n" + search_metadata_json + "\n```"
        
        # Create metadata JSON string 
        metadata_json = json.dumps(metadata, indent=2)
        
        prompt = """
You are an expert at converting .NET Web API controller metadata into structured entity configuration for AI agents.

## TASK
Transform the provided controller metadata into an entity-specific JSON configuration following the EXACT format specified.
""" + search_metadata_section + """

## OUTPUT FORMAT
Generate a valid JSON object with this EXACT structure. Use COMPACT JSON formatting (minimal whitespace) to reduce token count:

```json
{"entity":"<EntityName>","description":"<Description of what this entity manages>","synonyms":["<synonym1>","<synonym2>","<synonym3>","<synonym4>","<synonym5>"],"mandatoryFields":["<field1>","<field2>"],"baseUrl":"<BaseAPIPath>","endpoints":[{"name":"<MethodName>","url":"<URLTemplateWithParameters>","method":"<HTTPMethod>","intent":["<intent1>","<intent2>","<intent3>"],"description":"<Detailed description>","searchCapability":{"type":"<simple|advanced|none>","searchFields":["<field1>","<field2>"],"supportsPagination":<boolean>,"supportsSort":<boolean>},"parameters":{"<bodyParamName>":{"type":"<type>","required":<boolean>,"description":"<description>","default":"<defaultValue>"}},"exampleBody":{"<field1>":"<example_value1>","<field2>":"<example_value2>"},"exampleUrl":"<ConcreteExampleURL>","requiredFields":["<field1>","<field2>"],"example_uses":["<Natural language example 1>","<Natural language example 2>"],"when_to_use":"<Clear guidance>","scoring":{"searchBonus":<number>,"intentMatch":<number>,"complexity":"<simple|medium|complex>"}}}],"commonPatterns":{"pagination":{"pageIndex":"1-based page number","pageSize":"items per page","orderBy":"field name for sorting","ascending":"sort direction boolean"},"search":{"simple":"use <SimpleSearchEndpoint> with searchText","advanced":"use <AdvancedSearchEndpoint> with searchCriteria"}}}
```

## URL TEMPLATE EXAMPLES:
- Simple GET by ID: "/api/partner/{id}"
- Search with text: "/api/partner/search?searchText={searchText}&pageIndex={pageIndex}&pageSize={pageSize}"
- Advanced search: "/api/partner/advanced-search?pageIndex={pageIndex}&pageSize={pageSize}&orderBy={orderBy}&ascending={ascending}"
- List with pagination: "/api/partner?pageIndex={pageIndex}&pageSize={pageSize}&orderBy={orderBy}&ascending={ascending}"
- Category filter: "/api/partner/by-category/{categoryId}?pageIndex={pageIndex}&pageSize={pageSize}"

**CRITICAL: Use compact JSON format with minimal whitespace to reduce token count and avoid generation issues.**

## CRITICAL REQUIREMENTS

### 1. ENTITY INFORMATION
- **entity**: Extract from controller name (remove "Controller" suffix)
- **description**: Concise business purpose description
- **synonyms**: 3-5 alternative terms users might use
- **mandatoryFields**: Core required fields for the entity (2-3 max)
- **baseUrl**: Base API path (e.g., "/api/partner")

### 2. INTENT-BASED ENDPOINT CLASSIFICATION
- **intent**: Array of user intentions this endpoint serves
  * Search endpoints: ["search", "find", "lookup", "query"]
  * Create endpoints: ["create", "add", "new", "register"]
  * Update endpoints: ["update", "modify", "edit", "change"]
  * Delete endpoints: ["delete", "remove", "destroy"]
  * List endpoints: ["list", "get", "browse", "show"]
  * Get by ID: ["get", "retrieve", "show", "details"]

### 3. SEARCH CAPABILITY METADATA
- **type**: "simple" (text search), "advanced" (criteria), "none" (not a search)
- **searchFields**: Array of fields that can be searched
- **supportsPagination**: Boolean for pagination support
- **supportsSort**: Boolean for sorting support

### 4. SCORING SYSTEM
- **searchBonus**: Points for search relevance (0-100)
  * Simple text search: 100
  * Advanced search: 80
  * List endpoints: 20
  * Get by ID: 10
- **intentMatch**: Points for intent matching (0-100)
- **complexity**: "simple", "medium", "complex"

### 5. PARAMETERS OPTIMIZATION
- **parameters**: ONLY body parameters (not query parameters in URL)
- **requiredFields**: Array of mandatory fields for POST/PUT
- **exampleBody**: Realistic example for POST/PUT/PATCH
- **exampleUrl**: Concrete example showing parameter substitution

### 6. ENDPOINTS
- **name**: Use the exact method name from metadata (CRITICAL: Match controller method names exactly)
  * SearchPartners (not SearchForPartner)
  * AdvancedSearchPartners
  * SearchContacts
  * AdvancedSearchContacts
  * SearchInteractions
  * AdvancedSearchInteractions
- **url**: Create intelligent URL templates:
  * For path parameters: Use {paramName} format (e.g., "/api/partner/{id}")
  * For query parameters: Include in URL template (e.g., "/api/partner/search?searchText={searchText}&pageIndex={pageIndex}&pageSize={pageSize}")
  * For search endpoints: Always include search parameters in URL template
  * For pagination: Include pageIndex, pageSize in URL template when available
  * For sorting: Include orderBy, ascending parameters in URL template when available
- **parameters**: Simplified parameters object - only include body parameters and complex objects, NOT query parameters already in URL template
- **exampleBody**: ONLY for POST/PUT/PATCH methods - provide realistic example request body with actual field names and sample values

### 7. TYPES MAPPING
Map .NET types to JSON schema types:
- string/String → "string"
- int/Int32/long/Int64 → "integer"
- bool/Boolean → "boolean"
- float/double/decimal → "float"
- DateTime → "string"
- object/model → "object"
- array/list → "array"

### 8. COMMON PATTERNS SECTION
Generate a commonPatterns section that includes:
```json
"commonPatterns": {
  "pagination": {
    "pageIndex": "1-based page number (default: 1)",
    "pageSize": "items per page (default: 20)", 
    "orderBy": "field name for sorting",
    "ascending": "sort direction boolean"
  },
  "search": {
    "simple": "use SearchPartners with searchText parameter",
    "advanced": "use AdvancedSearchPartners with searchCriteria JSON array"
  }
}
```

### 9. CRITICAL INSTRUCTIONS
- **REMOVE searchMetadata**: Do not include the old searchMetadata section
- **URL templates**: Include ALL query parameters in URL templates
- **Parameters**: Only include request body parameters
- **Intent arrays**: Be specific about user intentions each endpoint serves
- **Search types**: Correctly identify simple vs advanced vs none
- **Scoring values**: Use realistic scoring values (searchBonus: 100 for simple search, 80 for advanced, 20 for list, 10 for get-by-id)
- **Example URLs**: Show concrete examples with actual parameter values

## CONTROLLER METADATA TO TRANSFORM:
```json
""" + metadata_json + """
```

Generate the entity configuration JSON now (JSON only, no explanations):

**FINAL INSTRUCTION: Output ONLY valid, compact JSON with minimal whitespace. Do not include any explanations, markdown formatting, or code blocks. Start directly with opening brace and end with closing brace.**
"""
        return prompt

    def generate_tools_json(self, metadata: Dict[str, Any]) -> str:
        """Generate tools.json using Vertex AI"""
        
        print("[AI] Generating tools.json with Vertex AI Gemini...")
        
        try:
            prompt = self.create_generation_prompt(metadata)
            
            generation_config = GenerationConfig(
                temperature=0.1,
                top_p=0.8,
                top_k=40,
                candidate_count=1,
                max_output_tokens=8192,
            )
            
            print("   [PROMPT] Sending generation request...")
            response = self.model.generate_content(
                prompt,
                generation_config=generation_config,
            )
            
            if not response.text:
                raise Exception("Empty response from Gemini API")
            
            tools_json = response.text.strip()
            
            # Clean up the response - remove any markdown formatting if present
            if tools_json.startswith('```json'):
                tools_json = tools_json.replace('```json', '').replace('```', '').strip()
            elif tools_json.startswith('```'):
                tools_json = tools_json.replace('```', '').strip()
            
            # Validate JSON
            try:
                parsed = json.loads(tools_json)
                # Re-format for consistency
                tools_json = json.dumps(parsed, indent=2, ensure_ascii=False)
                print("   [SUCCESS] Generated valid JSON tools configuration")
                return tools_json
            except json.JSONDecodeError as e:
                print("   [ERROR] Generated invalid JSON:", str(e))
                print("   [RESPONSE] Raw response:", tools_json[:500])
                raise Exception("Generated invalid JSON from Gemini API")
                
        except Exception as e:
            print("   [ERROR] Failed to generate tools.json:", str(e))
            raise

    def load_api_metadata(self, metadata_path: str) -> Dict[str, Any]:
        """Load the API metadata file"""
        print("Loading API metadata...")
        try:
            with open(metadata_path, 'r', encoding='utf-8') as f:
                metadata = json.load(f)
            print("   ✅ Successfully loaded metadata")
            return metadata
        except Exception as e:
            raise Exception("Failed to load API metadata from " + metadata_path + ": " + str(e))

    def save_tools_json(self, tools_json: str, output_path: str):
        """Save the generated tools.json"""
        print("Saving tools.json...")
        try:
            os.makedirs(os.path.dirname(output_path), exist_ok=True)
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(tools_json)
            print("   ✅ Successfully saved to:", output_path)
        except Exception as e:
            raise Exception("Failed to save tools.json to " + output_path + ": " + str(e))

    def process(self, input_path: str, output_path: str):
        """Main processing pipeline - processes controllers individually from single metadata file"""
        print("=" * 80)
        print("[GENERATOR] TOOLS.JSON GENERATOR - AI Agent Configuration Builder")
        print("=" * 80)
        print()
        
        # Load the API metadata
        print("Loading API metadata from:", input_path)
        metadata = self.load_api_metadata(input_path)
        
        controllers = metadata.get('Controllers', [])
        if not controllers:
            raise Exception("No controllers found in metadata file: " + input_path)
        
        total_endpoints = sum(len(c.get('Methods', [])) for c in controllers)
        print("   Found", len(controllers), "controllers with", total_endpoints, "total endpoints")
        print("   Extracted at:", metadata.get('ExtractedAt', 'Unknown'))
        print()
        
        # Create tools/endpoints subdirectory for backend API tools
        output_dir = os.path.dirname(output_path)
        tools_dir = os.path.join(output_dir, "tools", "endpoints")
        os.makedirs(tools_dir, exist_ok=True)
        
        # Also create main tools.json in the tools directory for config_manager fallback
        main_tools_dir = os.path.join(output_dir, "tools")
        os.makedirs(main_tools_dir, exist_ok=True)
        
        # Process each controller separately
        individual_tool_files = []
        processed_count = 0
        failed_count = 0
        
        for controller in controllers:
            controller_name = controller.get('Name', 'Unknown')
            clean_name = controller_name.replace('Controller', '').lower()
            
            try:
                print("Processing controller:", controller_name)
                
                # Generate tools JSON for this specific controller
                controller_tools_json = self.generate_tools_json(controller)
                
                # Save individual controller tools file
                controller_tools_file = os.path.join(tools_dir, clean_name + "-tools.json")
                self.save_tools_json(controller_tools_json, controller_tools_file)
                
                individual_tool_files.append({
                    "controller": controller_name,
                    "file": controller_tools_file,
                    "entity": clean_name
                })
                
                processed_count += 1
                print("   ✅ Generated:", clean_name + "-tools.json")
                
            except Exception as e:
                print("   ❌ Failed to process", controller_name, ":", str(e))
                failed_count += 1
                continue
        
        print()
        print("=" * 80)
        print("PROCESSING SUMMARY")
        print("=" * 80)
        print("   📊 Total controllers:", len(controllers))
        print("   ✅ Successfully processed:", processed_count)
        print("   ❌ Failed:", failed_count)
        print()
        
        if individual_tool_files:
            print("   📄 Generated individual tool files:")
            for tool_file in individual_tool_files:
                print("     -", tool_file["entity"] + "-tools.json")
        
        # Create a combined tools.json for fallback compatibility
        combined_tools = {
            "metadata": {
                "generated_at": datetime.now().isoformat(),
                "total_controllers": len(controllers),
                "processed_controllers": processed_count,
                "failed_controllers": failed_count
            },
            "individual_files": individual_tool_files
        }
        
        combined_tools_file = os.path.join(main_tools_dir, "tools.json")
        self.save_tools_json(json.dumps(combined_tools, indent=2), combined_tools_file)

        print("   📄 Combined fallback: tools/tools.json")


def main():
    parser = argparse.ArgumentParser()
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
        print("Using specified environment:", args.environment)
    
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
        print("[ERROR]", str(e), file=sys.stderr)
        print()
        print("[TIPS] Troubleshooting tips:")
        print("   1. Make sure Google Cloud SDK is installed")
        print("   2. Run: gcloud auth application-default login")
        print("   3. Verify your environment configuration files")
        print("   4. Check if your project has Vertex AI API enabled")
        return 1

if __name__ == "__main__":
    sys.exit(main())
