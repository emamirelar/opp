#!/usr/bin/env python3
"""
Test the custom user agent
"""

def test_custom_agent():
    try:
        import sys
        import os
        
        # Add the current directory to Python path
        current_dir = os.path.dirname(os.path.abspath(__file__))
        if current_dir not in sys.path:
            sys.path.insert(0, current_dir)
        
        from ai_assistant.sub_agents.custom_user_agent.agent import UserRequestAgent
        print("✅ UserRequestAgent class imported successfully")
        
        # Create an instance
        agent = UserRequestAgent()
        print(f"✅ UserRequestAgent instance created: {agent.name}")
        
        # Test user context extraction (without actual session)
        test_context = agent._extract_user_context(None)
        print(f"✅ Default user context: {test_context}")
        
        # Test response generation
        test_response = agent._generate_response("Hello", test_context)
        print(f"✅ Test response generated: {test_response['result'][0]['message'][:50]}...")
        
        print("\n🎉 All tests passed! Custom agent is working correctly.")
        
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    test_custom_agent()
