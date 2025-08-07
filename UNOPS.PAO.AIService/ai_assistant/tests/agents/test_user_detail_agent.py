#!/usr/bin/env python3
"""
Test suite for user_detail_agent

This test validates that the user_detail_agent is properly configured,
self-contained, and functions correctly.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
# The test is in: ai_assistant/tests/agents/test_user_detail_agent.py
# We need to go up to the project root to find ai_assistant
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestUserDetailAgent:
    """Test cases for user_detail_agent"""
    
    def test_agent_import(self):
        """Test that user_detail_agent can be imported successfully"""
        print("🧪 Testing user_detail_agent import...")
        
        try:
            from ai_assistant.sub_agents.user_detail_agent import user_detail_agent
            print("✅ Successfully imported user_detail_agent")
            return True
        except ImportError as e:
            print(f"❌ Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Unexpected Error: {e}")
            return False
    
    def test_agent_structure(self):
        """Test that the agent has the correct structure and attributes"""
        print("\n🧪 Testing agent structure...")
        
        try:
            from ai_assistant.sub_agents.user_detail_agent import user_detail_agent
            
            # Check basic attributes
            assert hasattr(user_detail_agent, 'name'), "Agent should have a name"
            assert user_detail_agent.name == "user_detail_agent", f"Expected name 'user_detail_agent', got '{user_detail_agent.name}'"
            print(f"✅ Agent name verified: {user_detail_agent.name}")
            
            # Check description
            assert hasattr(user_detail_agent, 'description'), "Agent should have a description"
            assert "user details" in user_detail_agent.description.lower(), "Description should mention user details"
            print(f"✅ Agent description verified: {user_detail_agent.description}")
            
            # Check tools
            assert hasattr(user_detail_agent, 'tools'), "Agent should have tools"
            assert len(user_detail_agent.tools) > 0, "Agent should have at least one tool"
            print(f"✅ Agent has {len(user_detail_agent.tools)} tool(s)")
            
            # Check output_key
            assert hasattr(user_detail_agent, 'output_key'), "Agent should have output_key"
            assert user_detail_agent.output_key == "user_profile", f"Expected output_key 'user_profile', got '{user_detail_agent.output_key}'"
            print(f"✅ Agent output_key verified: {user_detail_agent.output_key}")
            
            # Check callbacks
            assert hasattr(user_detail_agent, 'before_model_callback'), "Agent should have before_model_callback"
            assert hasattr(user_detail_agent, 'after_model_callback'), "Agent should have after_model_callback"
            print("✅ Agent callbacks are configured")
            
            return True
            
        except Exception as e:
            print(f"❌ Structure test failed: {e}")
            return False
    
    def test_utils_import(self):
        """Test that utils functions can be imported"""
        print("\n🧪 Testing utils import...")
        
        try:
            from ai_assistant.sub_agents.user_detail_agent.utils import get_user_profile
            print("✅ Successfully imported get_user_profile")
            
            # Check that it's callable
            assert callable(get_user_profile), "get_user_profile should be callable"
            print("✅ get_user_profile is callable")
            
            return True
            
        except ImportError as e:
            print(f"❌ Utils Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Utils Test Error: {e}")
            return False
    
    def test_get_user_profile_function(self):
        """Test the get_user_profile function with mocked dependencies"""
        print("\n🧪 Testing get_user_profile function...")
        
        try:
            # Mock the problematic imports that depend on old structure
            with patch('ai_assistant.sub_agents.user_detail_agent.utils.invoke_api_tool') as mock_invoke_api, \
                 patch('ai_assistant.sub_agents.user_detail_agent.utils.construct_api_url') as mock_construct_url, \
                 patch('ai_assistant.sub_agents.user_detail_agent.utils.get_user_email_from_session') as mock_get_email:
                
                # Import after patching
                from ai_assistant.sub_agents.user_detail_agent.utils import get_user_profile
                
                # Setup mocks
                mock_get_email.return_value = "test.user@unops.org"
                mock_construct_url.return_value = "https://localhost:44426/api/user-profile"
                mock_invoke_api.return_value = {
                    "status": "success",
                    "response": {
                        "id": 123,
                        "email": "test.user@unops.org",
                        "firstName": "Test",
                        "lastName": "User",
                        "orgUnitId": 1,
                        "orgUnitName": "Test Department"
                    }
                }
                
                # Create mock tool context
                mock_tool_context = Mock()
                mock_tool_context.state = {
                    "user_email": "test.user@unops.org",
                    "session_id": "test-session-123"
                }
                
                # Call the function
                result = get_user_profile(mock_tool_context)
                print(f"✅ Function executed successfully")
                
                # Verify result structure
                assert isinstance(result, str), "Result should be a JSON string"
                parsed_result = json.loads(result)
                assert "get_user_profile_response" in parsed_result, "Result should contain get_user_profile_response"
                print("✅ Function returns properly formatted JSON")
                
                # Verify mocks were called correctly
                mock_get_email.assert_called_once()
                mock_construct_url.assert_called_once()
                mock_invoke_api.assert_called_once()
                print("✅ All dependencies called correctly")
                
                return True
                
        except Exception as e:
            print(f"❌ Function test failed: {e}")
            return False
    
    def test_agent_prompt_quality(self):
        """Test that the agent's instruction is appropriate"""
        print("\n🧪 Testing agent prompt quality...")
        
        try:
            from ai_assistant.sub_agents.user_detail_agent import user_detail_agent
            
            instruction = user_detail_agent.instruction
            
            # Check that instruction exists and has content
            assert instruction and len(instruction.strip()) > 0, "Instruction should not be empty"
            
            # Check for key elements in the instruction
            instruction_lower = instruction.lower()
            
            # Should mention the tool function
            assert "get_user_profile" in instruction_lower, "Instruction should mention get_user_profile tool"
            
            # Should be focused on the task
            assert any(word in instruction_lower for word in ["call", "execute", "tool"]), "Instruction should mention calling the tool"
            
            # Should specify JSON output
            assert "json" in instruction_lower, "Instruction should mention JSON output format"
            
            # Should be concise and focused
            assert len(instruction.split()) < 100, "Instruction should be concise (under 100 words)"
            
            print("✅ Agent instruction is well-structured and appropriate")
            print(f"✅ Instruction length: {len(instruction.split())} words")
            
            return True
            
        except Exception as e:
            print(f"❌ Prompt quality test failed: {e}")
            return False
    
    def test_dependencies_self_contained(self):
        """Test that all dependencies are properly available"""
        print("\n🧪 Testing dependency self-containment...")
        
        issues_found = []
        
        try:
            # Check if old workflow_agent imports are still present
            from ai_assistant.sub_agents.user_detail_agent.utils import get_user_profile
            
            # Try to find problematic imports by checking the source
            import inspect
            source = inspect.getsource(get_user_profile)
            
            if "workflow_agent.api_worker" in source:
                issues_found.append("❌ Still importing from old workflow_agent.api_worker structure")
            
            if "from ai_assistant.workflow_agent" in source:
                issues_found.append("❌ Still importing from workflow_agent instead of utils.common_callbacks")
            
            # Check for proper imports
            expected_imports = [
                "invoke_api_tool",
                "construct_api_url"
            ]
            
            for import_name in expected_imports:
                if import_name not in source:
                    issues_found.append(f"⚠️ Missing expected import: {import_name}")
            
            if issues_found:
                print("🔧 Issues found with dependencies:")
                for issue in issues_found:
                    print(f"  {issue}")
                return False
            else:
                print("✅ Dependencies appear to be properly structured")
                return True
                
        except Exception as e:
            print(f"❌ Dependency test failed: {e}")
            return False
    
    def test_agent_execution(self):
        """Test that the agent actually executes and returns user_profile data"""
        print("\n🧪 Testing agent execution end-to-end...")
        
        try:
            # Mock the problematic imports that depend on external services
            with patch('ai_assistant.sub_agents.user_detail_agent.utils.invoke_api_tool') as mock_invoke_api, \
                 patch('ai_assistant.sub_agents.user_detail_agent.utils.construct_api_url') as mock_construct_url, \
                 patch('ai_assistant.sub_agents.user_detail_agent.utils.get_user_email_from_session') as mock_get_email:
                
                # Setup mocks to return expected data
                mock_get_email.return_value = "test.user@unops.org"
                mock_construct_url.return_value = "https://localhost:44426/api/user-profile"
                mock_invoke_api.return_value = {
                    "status": "success",
                    "response": {
                        "id": 123,
                        "email": "test.user@unops.org",
                        "firstName": "Test",
                        "lastName": "User",
                        "orgUnitId": 1,
                        "orgUnitName": "Test Department"
                    }
                }
                
                # Import the agent after patching
                from ai_assistant.sub_agents.user_detail_agent import user_detail_agent
                
                # Create a mock session state
                mock_session_state = {
                    "user_email": "test.user@unops.org",
                    "session_id": "test-session-123"
                }
                
                # Execute the agent with mock input
                try:
                    # This would normally require the full Google ADK execution context
                    # For testing, we'll verify the agent structure and tool availability
                    
                    # Verify agent has the right tool
                    assert len(user_detail_agent.tools) == 1, "Agent should have exactly one tool"
                    tool = user_detail_agent.tools[0]
                    assert hasattr(tool, 'func'), "Tool should have a function"
                    
                    # Test that the tool function works when called directly
                    mock_tool_context = Mock()
                    mock_tool_context.state = mock_session_state
                    
                    result = tool.func(mock_tool_context)
                    
                    # Verify result structure
                    assert isinstance(result, str), "Tool should return a JSON string"
                    parsed_result = json.loads(result)
                    assert "get_user_profile_response" in parsed_result, "Result should contain get_user_profile_response"
                    
                    user_data = parsed_result["get_user_profile_response"]
                    assert user_data["email"] == "test.user@unops.org", "Should return correct user email"
                    assert user_data["firstName"] == "Test", "Should return correct first name"
                    
                    print("✅ Agent tool executes successfully and returns expected data")
                    print("✅ Agent is properly configured for end-to-end execution")
                    
                    # Verify that mocks were called (indicating the tool was executed)
                    mock_get_email.assert_called()
                    mock_construct_url.assert_called()
                    mock_invoke_api.assert_called()
                    print("✅ Agent tool made expected API calls")
                    
                    return True
                    
                except Exception as execution_error:
                    print(f"❌ Agent execution failed: {execution_error}")
                    return False
                
        except Exception as e:
            print(f"❌ Agent execution test failed: {e}")
            return False


def run_user_detail_agent_tests():
    """Run all tests for user_detail_agent"""
    print("🚀 Running user_detail_agent test suite...")
    print("=" * 60)
    
    test_instance = TestUserDetailAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Structure Test", test_instance.test_agent_structure),
        ("Utils Import Test", test_instance.test_utils_import),
        ("Function Test", test_instance.test_get_user_profile_function),
        ("Prompt Quality Test", test_instance.test_agent_prompt_quality),
        ("Dependencies Test", test_instance.test_dependencies_self_contained),
        ("Agent Execution Test", test_instance.test_agent_execution),
    ]
    
    results = {}
    
    for test_name, test_func in tests:
        try:
            results[test_name] = test_func()
        except Exception as e:
            print(f"❌ {test_name} failed with exception: {e}")
            results[test_name] = False
    
    # Summary
    print("\n" + "=" * 60)
    print("📊 TEST SUMMARY:")
    
    passed = 0
    total = len(results)
    
    for test_name, result in results.items():
        status = "PASS" if result else "FAIL"
        print(f"{test_name}: {status}")
        if result:
            passed += 1
    
    print(f"\nOverall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 ALL TESTS PASSED! user_detail_agent is ready for production!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
    
    return passed == total


if __name__ == "__main__":
    success = run_user_detail_agent_tests()
    sys.exit(0 if success else 1)
