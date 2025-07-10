#!/usr/bin/env python3
import os
from dotenv import load_dotenv

print("🧪 Testing environment loading...")
print(f"Before load_dotenv: CURRENT_ENV = {os.getenv('CURRENT_ENV', 'not set')}")

load_dotenv()
print(f"After load_dotenv: CURRENT_ENV = {os.getenv('CURRENT_ENV', 'not set')}")

# Initialize framework config
try:
    from framework_config import initialize_config, get_environment_info
    print("🔧 Initializing framework config...")
    config = initialize_config()
    print(f"✅ Framework config initialized successfully")
    
    env_info = get_environment_info()
    print(f"Framework environment: {env_info.get('environment', 'unknown')}")
    print(f"Config loaded: {env_info.get('config_loaded', False)}")
    
except Exception as e:
    print(f"❌ Error initializing framework config: {e}")

# Test database URL
try:
    from ai_assistant.config_manager import config_manager
    db_url = config_manager.get_database_url()
    print(f"🗄️ Database URL: {db_url[:50]}...")
except Exception as e:
    print(f"❌ Error getting database URL: {e}") 