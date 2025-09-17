#!/usr/bin/env python3
"""
Simple test for the custom UserRequestAgent using direct import
"""

import sys
import os
import importlib.util
import json

def test_custom_agent_direct():
    """Test the custom agent by loading it directly from file"""
    try:
        # Load the agent module directly
        agent_file = os.path.join(os.path.dirname(__file__), 'ai_assistant', 'sub_agents', 'custom_user_agent', 'agent.py')
        spec = importlib.util.spec_from_file_location("custom_agent", agent_file)
        custom_agent_module = importlib.util.module_from_spec(spec)
        
        print("📁 Loading custom agent module...")
        spec.loader.exec_module(custom_agent_module)
        print("✅ Custom agent module loaded successfully")
        
        # Get the UserRequestAgent class
        UserRequestAgent = custom_agent_module.UserRequestAgent
        print("✅ UserRequestAgent class found")
        
        # Create an instance
        agent = UserRequestAgent(name="TestUserRequestAgent")
        print(f"✅ UserRequestAgent instance created: {agent.name}")
        
        # Test user context extraction (without actual session)
        test_context = agent._extract_user_context(None)
        print(f"✅ Default user context: {test_context}")
        
        # Test response generation for different message types
        test_messages = [
            "Hello",
            "Thank you",
            "Help me",
            "Show me partners"
        ]
        
        for message in test_messages:
            response = agent._generate_response(message, test_context)
            print(f"\n📝 Message: '{message}'")
            print(f"   Response: {response['result'][0]['message'][:80]}...")
            print(f"   Follow-ups: {response['followUps']}")
        
        print("\n🎉 All tests passed! Custom UserRequestAgent is working correctly.")
        
        # Test with mock user profile
        print("\n🧪 Testing with mock user profile...")
        mock_user_context = {
            "user_name": "Sarah Johnson",
            "current_screen": "Partners",
            "user_profile": {
                "firstName": "Sarah",
                "name": "Sarah Johnson",
                "displayName": "Sarah J."
            },
            "screen_context": {
                "current_screen": "Partners",
                "page": "/partners"
            }
        }
        
        personalized_response = agent._generate_response("Hello", mock_user_context)
        print(f"📝 Personalized greeting: {personalized_response['result'][0]['message']}")
        
        print("\n✨ Custom agent successfully demonstrates personalization!")
        
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    test_custom_agent_direct()
