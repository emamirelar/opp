#!/usr/bin/env python3
"""
UNOPS AI Agent Framework - Team Config Initialization

This script creates the expected config folder structure with example files
for teams to customize for their specific domain.

Usage:
    python tools/init-team-config.py
    python tools/init-team-config.py --output-dir my-ai-service
"""

import os
import json
import shutil
import argparse
from pathlib import Path

def create_config_structure(output_dir: str = "."):
    """Create the expected config folder structure with example files"""
    
    base_path = Path(output_dir)
    config_path = base_path / "config"
    
    # Create directory structure
    framework_path = config_path / "framework"
    endpoints_path = config_path / "tools" / "endpoints" 
    ui_path = config_path / "tools" / "ui"
    
    framework_path.mkdir(parents=True, exist_ok=True)
    endpoints_path.mkdir(parents=True, exist_ok=True)
    ui_path.mkdir(parents=True, exist_ok=True)
    
    print(f"📁 Created config directory structure in: {config_path}")
    
    # Copy example files from the package
    package_examples = Path(__file__).parent.parent / "config" / "team-examples"
    
    if package_examples.exists():
        # Copy example entity configurations
        for example_dir in package_examples.iterdir():
            if example_dir.is_dir():
                for json_file in example_dir.glob("*.json"):
                    dest_file = endpoints_path / f"example-{json_file.name}"
                    shutil.copy2(json_file, dest_file)
                    print(f"📄 Copied: {dest_file.name}")
    
    # Create example framework configs
    create_example_framework_configs(framework_path)
    
    # Create example UI config
    create_example_ui_config(ui_path)

    create_example_env_file(base_path)

    create_deployment_scripts(base_path)
    
    print(f"\n✅ Team AI service structure created successfully!")
    print(f"📁 Location: {config_path.absolute()}")
    print(f"\n📋 Next steps:")
    print(f"1. Framework is already installed via pip")
    print(f"2. Edit config/framework/ files with your team's API details")
    print(f"3. Customize entity configs in config/tools/endpoints/")
    print(f"4. Copy .env.example to .env and configure")
    print(f"5. Run: python main.py")
    print(f"\n🌐 Your AI service will be available at:")
    print(f"   Web Interface: http://localhost:8000/dev-ui")
    print(f"   API Documentation: http://localhost:8000/docs")

def create_example_framework_configs(framework_path: Path):
    """Create example framework configuration files"""
    
    # Base configuration template
    base_config = {
        "environment": "dev",
        "cache": {
            "rules": {
                "Contact": ["screen_context"],
                "Partner": ["screen_context"],
                "Interaction": ["screen_context"],
                "User": ["user_profile"],
                "UserData": ["user_profile"],
                "UserPreferences": ["user_profile"]
            },
            "url_patterns": {
                "contacts": "Contact",
                "partners": "Partner", 
                "interactions": "Interaction",
                "user": "User",
                "userdata": "UserData",
                "preferences": "UserPreferences"
            },
            "ttl": {
                "user_profile": 3600,
                "screen_context": 600,
                "general": 1800
            },
            "enable_cache": True,
            "cache_backend": "memory"
        },
        "google_cloud": {
            "use_vertex_ai": True,
            "project": "your-google-cloud-project",
            "location": "europe-west4",
            "oauth": {
                "identity_toolkit_api_key_secret": "IdentityToolkitAPIKey",
                "client_id": "your-oauth-client-id.apps.googleusercontent.com",
                "target_principal": "your-service@your-project.iam.gserviceaccount.com"
            }
        },
        "database": {
            "url": "postgresql://user:password@localhost:5432/your_database"
        },
        "artifacts": {
            "service_type": "InMemoryArtifactService",
            "gcs_bucket_name": "your-bucket-name"
        },
        "server": {
            "serve_web_interface": True,
            "api_base_url": "https://localhost:44426/",
            "is_development": True,
            "host": "0.0.0.0",
            "port": 8000,
            "allow_origins": [
                "http://localhost:3000",
                "http://localhost:8000",
                "http://localhost:8080",
                "https://localhost:44426"
            ]
        },
        "google_drive": {
            "enabled": True,
            "project_id": "your-google-cloud-project",
            "oauth_enabled": True,
            "oauth_use_existing_client": True,
            "oauth_client_secret_name": "IdentityToolkitAPIKey",
            "oauth_redirect_ports": [8080, 8081, 8082, 9000, 9001, 9876],
            "secret_name": "GoogleDriveAI",
            "service_account_email": "google-drive-ai@your-project.iam.gserviceaccount.com",
            "scopes": [
                "https://www.googleapis.com/auth/drive",
                "https://www.googleapis.com/auth/documents"
            ]
        },
        "developer": {
            "email": "your-email@unops.org"
        },
        "agent": {
            "enable_google_drive_agent": True,
            "enable_workflow_agent": True,
            "enable_search_agent": True,
            "enable_contextual_agent": True
        },
        "action_logging": {
            "enabled": True,
            "auto_log_actions": True,
            "log_followup_suggestions": True,
            "retention_days": 90,
            "enable_analytics": True
        },
        "support": {
            "email": "your-support@unops.org",
            "response_template": "For technical support with {application_name}, please send an email to {support_email}.",
            "hours": "During business hours",
            "response_time": "24-48 hours"
        },
        "branding": {
            "application_name": "Application",
            "project_name": "UNOPS Your Domain",
            "organization": "UNOPS",
            "organization_full": "United Nations Office for Project Services",
            "description": "AI Agent for your specific domain"
        },
        "runtime": {
            "gemini_model": "gemini-2.5-flash",
            "gemini_adhoc_model": "gemini-2.0-flash-001",
            "api_timeout": 300,
            "default_page_size": 10,
            "max_retries": 3,
            "user_profile_cache_ttl": 3600,
            "screen_context_cache_ttl": 7200,
            "default_agent_timeout": 60
        },
        "description": "Development environment configuration"
    }
    
    # Create dev.json
    dev_config = base_config.copy()
    with open(framework_path / "dev.json", 'w', encoding='utf-8') as f:
        json.dump(dev_config, f, indent=2, ensure_ascii=False)
    
    # Create test.json
    test_config = base_config.copy()
    test_config["environment"] = "test"
    test_config["server"]["port"] = 8001
    test_config["server"]["is_development"] = True
    test_config["database"]["url"] = "postgresql://user:password@localhost:5432/your_database_test"
    test_config["description"] = "Test environment configuration"
    
    with open(framework_path / "test.json", 'w', encoding='utf-8') as f:
        json.dump(test_config, f, indent=2, ensure_ascii=False)
    
    # Create prod.json
    prod_config = base_config.copy()
    prod_config["environment"] = "prod"
    prod_config["server"]["is_development"] = False
    prod_config["server"]["allow_origins"] = ["https://your-frontend.unops.org"]
    prod_config["artifacts"]["service_type"] = "GcsArtifactService"
    prod_config["description"] = "Production environment configuration"
    
    with open(framework_path / "prod.json", 'w', encoding='utf-8') as f:
        json.dump(prod_config, f, indent=2, ensure_ascii=False)
    
    print(f"📄 Created: framework/dev.json")
    print(f"📄 Created: framework/test.json") 
    print(f"📄 Created: framework/prod.json")

def create_example_ui_config(ui_path: Path):
    """Create example UI configuration file"""
    
    ui_config = {
        "version": "1.0",
        "description": "UI configuration for team-specific customizations",
        "theme": {
            "primary_color": "#0066cc",
            "secondary_color": "#f0f0f0",
            "application_logo": "/assets/team-logo.png"
        },
        "navigation": {
            "menu_items": [
                {
                    "label": "Dashboard",
                    "path": "/dashboard",
                    "icon": "dashboard"
                },
                {
                    "label": "Your Entities", 
                    "path": "/entities",
                    "icon": "list"
                },
                {
                    "label": "Reports",
                    "path": "/reports", 
                    "icon": "analytics"
                }
            ]
        },
        "dashboard": {
            "widgets": [
                {
                    "type": "summary_cards",
                    "entities": ["YourPrimaryEntity", "YourSecondaryEntity"]
                },
                {
                    "type": "recent_activity",
                    "limit": 10
                }
            ]
        }
    }
    
    with open(ui_path / "example-ui.json", 'w', encoding='utf-8') as f:
        json.dump(ui_config, f, indent=2, ensure_ascii=False)
    
    print(f"📄 Created: tools/ui/example-ui.json")

def create_team_main_py(output_dir: str = "."):
    """Create example main.py for teams"""
    
    main_py_content = '''#!/usr/bin/env python3
"""
Your Team's AI Service

This uses the UNOPS AI Agent Framework (installed via pip) and automatically
loads configurations from your config/ folder structure.
"""

import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Import the framework (installed via pip)
try:
    # Import framework components
    from ai_assistant.utils.framework_config import initialize_config
    # Import the framework's app creation function instead of the app directly
    import sys
    import os
    
    # Add the framework package to the Python path if needed
    framework_path = os.path.dirname(__file__)
    if framework_path not in sys.path:
        sys.path.insert(0, framework_path)
    
    if __name__ == "__main__":
        import uvicorn
        
        # *** CRITICAL: SPECIFY YOUR CONFIG FOLDER LOCATION HERE ***
        config = initialize_config(config_dir="./config")
        
        # Import and create the framework's app after config is initialized
        from ai_assistant.app import create_app
        app = create_app()
        
        # Optional: Add custom endpoints for your team
        from fastapi import APIRouter
        
        # Example: Custom health endpoint for your team
        custom_router = APIRouter(prefix="/api/team", tags=["team"])
        
        @custom_router.get("/health")
        async def team_health():
            """Team-specific health check"""
            return {
                "status": "ok", 
                "team_service": "AI Assistant",
                "framework": "UNOPS AI Agent Framework"
            }
        
        # Add your custom endpoints to the framework
        app.include_router(custom_router)
        
        print("🚀 Starting the AI service...")
        print("📁 Framework config loaded from: ./config/")
        print("🌐 Web interface: http://localhost:8000/dev-ui?app=ai_assistant")
        
        uvicorn.run(
            app,  # Use the framework's configured app
            host=os.getenv('HOST', '0.0.0.0'),
            port=int(os.getenv('PORT', 8000)),
            reload=os.getenv('IS_DEVELOPMENT', 'false').lower() == 'true',
            log_level="info"
        )
        
except ImportError as e:
    print("❌ UNOPS AI Agent Framework not found!")
    print("📦 Install with: pip install git+https://github.com/UNOPS-ITG/unops-ai-agent-appintelligence.git")
    print(f"Error details: {e}")
    
except Exception as e:
    print(f"❌ Failed to start service: {e}")
    print("💡 Make sure your config files are properly set up:")
    print("   - config/framework/dev.json (required)")
    print("   - config/tools/endpoints/ (required)")
    print("   - .env file with CURRENT_ENV=dev")
'''
    
    main_py_path = Path(output_dir) / "main.py"
    with open(main_py_path, 'w', encoding='utf-8') as f:
        f.write(main_py_content)
    
    print(f"📄 Created: main.py")

def copy_deployment_files(output_dir: str = "."):
    """Copy deployment files for teams"""
    
    # Create deployment directory
    deployment_path = Path(output_dir) / "deployment"
    deployment_path.mkdir(exist_ok=True)
    
    # Get the package deployment directory
    package_deployment = Path(__file__).parent.parent.parent / "deployment"
    
    # Copy team deployment files
    team_files = [
        "team-deploy-simple.bat",
        "team-setup-gcloud.bat", 
        "README-DEPLOYMENT.md"
    ]
    
    for file_name in team_files:
        src_file = package_deployment / file_name
        if src_file.exists():
            dest_file = deployment_path / file_name
            shutil.copy2(src_file, dest_file)
            print(f"📄 Copied: deployment/{file_name}")
    
    print(f"📁 Created: deployment folder with team scripts")

def copy_essential_scripts(output_dir: str = "."):
    """Copy essential scripts for teams"""
    
    # Get the package root directory  
    package_root = Path(__file__).parent.parent.parent
    
    # Essential scripts to copy
    scripts = [
        "update_packages.bat",
        "run_app.bat",
        "upgrade_framework.bat"
    ]
    
    for script in scripts:
        src_file = package_root / script
        if src_file.exists():
            dest_file = Path(output_dir) / script
            shutil.copy2(src_file, dest_file)
            print(f"📄 Copied: {script}")
    
    print(f"✅ Essential scripts copied")

def create_example_env_file(base_path: str = "."):
    """Create .env.example file for teams"""
    
    env_content = '''# UNOPS AI Agent Framework - Environment Configuration
# Copy this file to .env and customize with your team's values

GOOGLE_GENAI_USE_VERTEXAI=TRUE
GOOGLE_CLOUD_PROJECT=project_id
GOOGLE_CLOUD_LOCATION=location
# =====================================
# For local development testing
# =====================================
DEV_EMAIL=xyz@unops.org
CURRENT_ENV=dev
'''
    
    dest_env = Path(base_path) / ".env.example"
    with open(dest_env, 'w', encoding='utf-8') as f:
        f.write(env_content)
    
    print(f"📄 Created: .env.example")

def create_deployment_scripts(base_path: str = "."):
    """Copy deployment scripts and cloud files to base folder"""
    
    # Get the package deployment directory and root
    package_deployment = Path(__file__).parent.parent.parent / "deployment"
    package_root = Path(__file__).parent.parent.parent
    base_path_obj = Path(base_path)
    
    # Team deployment files to copy from deployment folder
    team_files = [
        "team-deploy-simple.bat",
        "team-setup-gcloud.bat"
    ]
    
    for file_name in team_files:
        src_file = package_deployment / file_name
        if src_file.exists():
            dest_file = base_path_obj / file_name
            shutil.copy2(src_file, dest_file)
            print(f"📄 Copied: {file_name}")
        else:
            print(f"⚠️ Warning: {file_name} not found in package deployment folder")
    
    # Copy cloud deployment files from package root
    cloud_files = [
        "cloudbuild.yaml",
        "Dockerfile"
    ]
    
    for file_name in cloud_files:
        # Create generic versions directly in base folder
        dest_file = base_path_obj / file_name
        if file_name == "cloudbuild.yaml":
            create_generic_cloudbuild(dest_file)
        elif file_name == "Dockerfile":
            create_generic_dockerfile(dest_file)
        print(f"📄 Created: {file_name}")
    
    # Create deployment README in base folder
    readme_file = base_path_obj / "DEPLOYMENT-GUIDE.md"
    create_deployment_readme(readme_file)
    print(f"📄 Created: DEPLOYMENT-GUIDE.md")
    
    print(f"✅ Deployment files created in base folder")

def create_generic_cloudbuild(dest_file: Path):
    """Create a generic cloudbuild.yaml for teams"""
    cloudbuild_content = '''# Cloud Build configuration for Google Cloud Run deployment
# Edit the variables below for your team's service

steps:
  # Build the Docker image
  - name: 'gcr.io/cloud-builders/docker'
    args: [
      'build', 
      '-t', 'gcr.io/$PROJECT_ID/your-team-ai-service:$SHORT_SHA',
      '-t', 'gcr.io/$PROJECT_ID/your-team-ai-service:latest',
      '.'
    ]

  # Push the image to Google Container Registry
  - name: 'gcr.io/cloud-builders/docker'
    args: [
      'push', 
      'gcr.io/$PROJECT_ID/your-team-ai-service:$SHORT_SHA'
    ]

  # Deploy to Cloud Run
  - name: 'gcr.io/google.com/cloudsdktool/cloud-sdk'
    entrypoint: 'gcloud'
    args: [
      'run', 'deploy', 'your-team-ai-service',
      '--image', 'gcr.io/$PROJECT_ID/your-team-ai-service:$SHORT_SHA',
      '--platform', 'managed',
      '--region', 'europe-west4',
      '--port', '8080',
      '--memory', '2Gi',
      '--cpu', '1',
      '--concurrency', '80',
      '--timeout', '300',
      '--max-instances', '10',
      '--min-instances', '1',
      '--set-env-vars', 'CURRENT_ENV=prod',
      '--set-env-vars', 'GOOGLE_CLOUD_PROJECT=$PROJECT_ID',
      '--set-env-vars', 'GOOGLE_CLOUD_LOCATION=europe-west4',
      '--set-env-vars', 'GOOGLE_GENAI_USE_VERTEXAI=TRUE'
    ]

# Images to be pushed to Google Container Registry
images:
  - 'gcr.io/$PROJECT_ID/your-team-ai-service:$SHORT_SHA'
  - 'gcr.io/$PROJECT_ID/your-team-ai-service:latest'

# Build options
options:
  machineType: 'E2_HIGHCPU_8'
  diskSizeGb: '100'

# Timeout for the entire build (20 minutes)
timeout: '1200s'
'''
    
    with open(dest_file, 'w', encoding='utf-8') as f:
        f.write(cloudbuild_content)

def create_generic_dockerfile(dest_file: Path):
    """Create a generic Dockerfile for teams"""
    dockerfile_content = '''# Use Python 3.11 slim image for better performance
FROM python:3.11-slim

# Set environment variables
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1

# Set work directory
WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \\
    build-essential \\
    gcc \\
    g++ \\
    libpq-dev \\
    curl \\
    && rm -rf /var/lib/apt/lists/*

# Copy requirements first to leverage Docker layer caching
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir --upgrade pip && \\
    pip install --no-cache-dir -r requirements.txt

# Copy application code
COPY . .

# Create a non-root user
RUN useradd --create-home --shell /bin/bash appuser && \\
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080 (Cloud Run expects this)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \\
    CMD curl -f http://localhost:8080/framework/info || exit 1

# Command to run the application
CMD ["python", "-m", "uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8080"]
'''
    
    with open(dest_file, 'w', encoding='utf-8') as f:
        f.write(dockerfile_content)

def create_deployment_readme(dest_file: Path):
    """Create deployment guide in base folder"""
    readme_content = '''# 🚀 Deployment Guide

## Quick Cloud Run Deployment

### 1. Configure Your Team Settings
Edit these files with your team's Google Cloud project details:

**`team-setup-gcloud.bat`:**
```batch
set PROJECT_ID=your-google-cloud-project  
set REGION=your-preferred-region
```

**`team-deploy-simple.bat`:**
```batch
set SERVICE_NAME=your-team-ai-service
set PROJECT_ID=your-google-cloud-project
set REGION=your-preferred-region
```

### 2. Deploy Options

**Option A: Simple Deployment**
```bash
# Setup and deploy
./team-setup-gcloud.bat
./team-deploy-simple.bat
```

**Option B: Cloud Build (CI/CD)**
```bash
# Edit cloudbuild.yaml with your service name
# Then trigger build
gcloud builds submit --config cloudbuild.yaml
```

### 3. Local Docker Testing
```bash
# Build and test locally
docker build -t your-team-ai .
docker run -p 8080:8080 your-team-ai
```

## Files Included
- `team-setup-gcloud.bat` - Google Cloud setup
- `team-deploy-simple.bat` - Simple deployment script  
- `cloudbuild.yaml` - Cloud Build configuration
- `Dockerfile` - Container configuration

## Support
Check the main framework documentation for detailed deployment instructions.
'''
    
    with open(dest_file, 'w', encoding='utf-8') as f:
        f.write(readme_content)

def main():
    parser = argparse.ArgumentParser(description='Initialize UNOPS AI Agent config structure')
    parser.add_argument('--output-dir', '-o', default='.', help='Output directory (default: current)')
    parser.add_argument('--include-main', action='store_true', help='Also create main.py, deployment scripts, and essential batch files')
    
    args = parser.parse_args()
    
    print("🚀 UNOPS AI Agent Framework - Config Initialization")
    print("=" * 55)
    
    create_config_structure(args.output_dir)
    
    if args.include_main:
        print(f"\n📄 Creating team files...")
        create_team_main_py(args.output_dir)
        copy_deployment_files(args.output_dir)
        copy_essential_scripts(args.output_dir)

if __name__ == "__main__":
    main() 