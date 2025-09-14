#!/usr/bin/env python3
"""
Test script to verify environment configuration loading
"""

import os
import sys
from pathlib import Path

# Add the current directory to Python path so we can import the main module
sys.path.insert(0, str(Path(__file__).parent))

from llm_generator import load_environment_config, auto_detect_environment

def main():
    print("=" * 60)
    print("🧪 ENVIRONMENT CONFIGURATION TEST")
    print("=" * 60)
    print()
    
    # Test auto-detection
    environment = auto_detect_environment()
    print(f"🔍 Auto-detected environment: {environment}")
    print()
    
    # Test environment loading
    env_vars = load_environment_config()
    print()
    
    # Show current environment variables
    print("📋 Current Google Cloud Configuration:")
    print(f"   GOOGLE_CLOUD_PROJECT: {os.getenv('GOOGLE_CLOUD_PROJECT', 'Not set')}")
    print(f"   GOOGLE_CLOUD_LOCATION: {os.getenv('GOOGLE_CLOUD_LOCATION', 'Not set')}")
    print(f"   PAO_ENVIRONMENT: {os.getenv('PAO_ENVIRONMENT', 'Not set')}")
    print()
    
    # Test manual environment override
    print("🎯 Testing manual environment override...")
    original_env = os.getenv('PAO_ENVIRONMENT')
    
    for test_env in ['dev', 'staging', 'production']:
        os.environ['PAO_ENVIRONMENT'] = test_env
        detected = auto_detect_environment()
        print(f"   Set PAO_ENVIRONMENT={test_env} → Detected: {detected}")
    
    # Restore original environment
    if original_env:
        os.environ['PAO_ENVIRONMENT'] = original_env
    else:
        os.environ.pop('PAO_ENVIRONMENT', None)
    
    print()
    print("✅ Environment configuration test completed!")
    print("=" * 60)

if __name__ == "__main__":
    main() 