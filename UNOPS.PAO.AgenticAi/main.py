#!/usr/bin/env python3
"""
FastAPI application for Opportunity+ AI Agent
Production-ready deployment with database persistence
"""

import os
import uvicorn
from dotenv import load_dotenv
from google.adk.cli.fast_api import get_fast_api_app

# Load environment variables
load_dotenv()

# Get the directory where main.py is located (project root)
AGENT_DIR = os.path.dirname(os.path.abspath(__file__))

# PostgreSQL database URL from environment variable
# For deployment, this should be set in environment variables
SESSION_DB_URL = os.getenv('DATABASE_URL', 'postgresql://postgres:0Y%2FX3YNxHLL0fL4T@localhost:5433/anusha')

# CORS allowed origins - customize for your deployment
ALLOWED_ORIGINS = [
    "http://localhost",
    "http://localhost:3000",
    "http://localhost:8000",
    "http://localhost:8080",
    "http://127.0.0.1:8000",
    "*"  # For development only - restrict for production
]

# Enable web interface
SERVE_WEB_INTERFACE = True

print("🚀 Starting Opportunity+ AI Agent FastAPI Server")
print("=" * 60)
print(f"📁 Agent Directory: {AGENT_DIR}")
print(f"🗄️ Database: {'PostgreSQL' if 'postgresql' in SESSION_DB_URL else 'SQLite' if 'sqlite' in SESSION_DB_URL else 'In-Memory'}")
print(f"🌐 Web Interface: {'Enabled' if SERVE_WEB_INTERFACE else 'Disabled'}")

# Create the FastAPI app instance
app = get_fast_api_app(
    agent_dir=AGENT_DIR,
    session_db_url=SESSION_DB_URL,
    allow_origins=ALLOWED_ORIGINS,
    web=SERVE_WEB_INTERFACE,
    trace_to_cloud=False  # Set to True for cloud tracing in production
)

# Add custom health check endpoint
@app.get("/health")
async def health_check():
    """Health check endpoint for deployment monitoring"""
    return {
        "status": "healthy",
        "service": "opportunity-ai-agent",
        "version": "1.0.0",
        "database": "connected" if SESSION_DB_URL else "in-memory"
    }

# Add API information endpoint
@app.get("/api/info")
async def api_info():
    """API information and available endpoints"""
    return {
        "service": "Opportunity+ AI Agent",
        "version": "1.0.0",
        "agent": "opportunity_ai_agent",
        "endpoints": {
            "create_session": "POST /apps/opportunity_ai_agent/users/{user_id}/sessions",
            "get_session": "GET /apps/opportunity_ai_agent/users/{user_id}/sessions/{session_id}",
            "list_sessions": "GET /apps/opportunity_ai_agent/users/{user_id}/sessions",
            "run_sse": "POST /apps/opportunity_ai_agent/users/{user_id}/sessions/{session_id}/run_sse",
            "health": "GET /health",
            "web_ui": "GET /" if SERVE_WEB_INTERFACE else None
        },
        "example_usage": {
            "create_session_with_state": {
                "url": "POST /apps/opportunity_ai_agent/users/user/sessions",
                "payload": {
                    "state": {
                        "user_name": "Your Name",
                        "user_email": "your.email@example.com",
                        "user_role": "Your Role"
                    }
                }
            }
        }
    }

if __name__ == "__main__":
    # Use the PORT environment variable (for Cloud Run/deployment), defaulting to 8000
    port = int(os.environ.get("PORT", 8000))
    
    print(f"\n🌟 Server starting on http://0.0.0.0:{port}")
    print("📋 Available endpoints:")
    print(f"  • Web UI: http://localhost:{port}/")
    print(f"  • API Info: http://localhost:{port}/api/info")
    print(f"  • Health Check: http://localhost:{port}/health")
    print(f"  • Create Session: POST http://localhost:{port}/apps/opportunity_ai_agent/users/user/sessions")
    print(f"  • Chat Stream: POST http://localhost:{port}/apps/opportunity_ai_agent/users/user/sessions/{{session_id}}/run_sse")
    print(f"\n💡 Example session creation with state:")
    print(f"curl -X POST http://localhost:{port}/apps/opportunity_ai_agent/users/user/sessions \\")
    print(f'  -H "Content-Type: application/json" \\')
    print(f'  -d \'{{"state": {{"user_name": "Anusha", "user_role": "Partnership Manager"}}}}\'')
    print("\n🛑 Press Ctrl+C to stop the server")
    
    uvicorn.run(
        app, 
        host="0.0.0.0", 
        port=port,
        log_level="info",
        reload=False  # Set to True for development auto-reload
    )
