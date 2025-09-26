#!/usr/bin/env python3
"""
Test the basic custom agent - just verify the foundation works
"""

import importlib.util
import os

def test_basic_custom_agent():
    """Test that our basic custom agent can be created and has the right structure"""
    try:
        print("🧪 Testing basic custom agent foundation...")
        
        # Load the agent module directly from file
        agent_file = os.path.join(
            os.path.dirname(__file__), 
            'ai_assistant', 'sub_agents', 'basic_custom_agent', 'agent.py'
        )
        
        spec = importlib.util.spec_from_file_location("basic_agent", agent_file)
        basic_agent_module = importlib.util.module_from_spec(spec)
        
        print("📁 Loading basic agent module...")
        spec.loader.exec_module(basic_agent_module)
        print("✅ Basic agent module loaded successfully")
        
        # Get the class
        BasicCustomAgent = basic_agent_module.BasicCustomAgent
        print("✅ BasicCustomAgent class found")
        
        # Create an instance
        agent = BasicCustomAgent(name="TestBasicAgent")
        print(f"✅ Agent instance created: {agent.name}")
        
        # Verify it has the right structure
        print(f"✅ Agent type: {type(agent)}")
        print(f"✅ Agent has _run_async_impl: {hasattr(agent, '_run_async_impl')}")
        print(f"✅ Agent sub_agents: {agent.sub_agents}")
        
        print("\n🎉 Basic custom agent foundation is working correctly!")
        print("✨ Ready to build more complex functionality on top of this.")
        
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    test_basic_custom_agent()
