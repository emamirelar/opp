#!/usr/bin/env python3
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
                "team_service": "Your Team AI Service",
                "framework": "UNOPS AI Agent Framework"
            }
        
        # Add your custom endpoints to the framework
        app.include_router(custom_router)
        
        print("🚀 Starting your team's AI service...")
        print("📁 Framework config loaded from: ./config/")
        print("🌐 Web interface: http://localhost:8000/dev-ui")
        
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
