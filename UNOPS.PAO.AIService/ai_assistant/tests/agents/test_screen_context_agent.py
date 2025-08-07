#!/usr/bin/env python3
"""
Test suite for screen_context_agent

This test validates that the screen_context_agent is properly configured,
self-contained, and functions correctly.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
# The test is in: ai_assistant/tests/agents/test_screen_context_agent.py
# We need to go up to the project root to find ai_assistant
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestScreenContextAgent:
    """Test cases for screen_context_agent"""
    
    def test_agent_import(self):
        """Test that screen_context_agent can be imported successfully"""
        print("🧪 Testing screen_context_agent import...")
        
        try:
            from ai_assistant.sub_agents.screen_context_agent import screen_context_agent
            print("✅ Successfully imported screen_context_agent")
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
            from ai_assistant.sub_agents.screen_context_agent import screen_context_agent
            
            # Check basic attributes
            assert hasattr(screen_context_agent, 'name'), "Agent should have a name"
            assert screen_context_agent.name == "screen_context_agent", f"Expected name 'screen_context_agent', got '{screen_context_agent.name}'"
            print(f"✅ Agent name verified: {screen_context_agent.name}")
            
            # Check description
            assert hasattr(screen_context_agent, 'description'), "Agent should have a description"
            assert "screen context" in screen_context_agent.description.lower(), "Description should mention screen context"
            print(f"✅ Agent description verified: {screen_context_agent.description}")
            
            # Check tools
            assert hasattr(screen_context_agent, 'tools'), "Agent should have tools"
            assert len(screen_context_agent.tools) > 0, "Agent should have at least one tool"
            print(f"✅ Agent has {len(screen_context_agent.tools)} tool(s)")
            
            # Check output_key
            assert hasattr(screen_context_agent, 'output_key'), "Agent should have output_key"
            assert screen_context_agent.output_key == "screen_context", f"Expected output_key 'screen_context', got '{screen_context_agent.output_key}'"
            print(f"✅ Agent output_key verified: {screen_context_agent.output_key}")
            
            # Check callbacks
            assert hasattr(screen_context_agent, 'before_model_callback'), "Agent should have before_model_callback"
            assert hasattr(screen_context_agent, 'after_model_callback'), "Agent should have after_model_callback"
            print("✅ Agent callbacks are configured")
            
            return True
            
        except Exception as e:
            print(f"❌ Structure test failed: {e}")
            return False
    
    def test_utils_import(self):
        """Test that utils functions can be imported"""
        print("\n🧪 Testing utils import...")
        
        try:
            from ai_assistant.sub_agents.screen_context_agent.utils import parse_screen_url_callback, get_entity_display_name_tool
            print("✅ Successfully imported parse_screen_url_callback and get_entity_display_name_tool")
            
            # Check that they're callable
            assert callable(parse_screen_url_callback), "parse_screen_url_callback should be callable"
            assert callable(get_entity_display_name_tool), "get_entity_display_name_tool should be callable"
            print("✅ Both functions are callable")
            
            return True
            
        except ImportError as e:
            print(f"❌ Utils Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Utils Test Error: {e}")
            return False
    
    def test_parse_screen_url_callback_function(self):
        """Test the parse_screen_url_callback function with mocked dependencies"""
        print("\n🧪 Testing parse_screen_url_callback function...")
        
        try:
            # Mock the UI config manager that this function depends on
            with patch('ai_assistant.sub_agents.screen_context_agent.utils.ui_config_manager') as mock_ui_config:
                
                # Setup mock UI config manager
                mock_ui_config.find_matching_page_by_url.return_value = {
                    "screen_type": "entity_detail_page",
                    "entity_name": "Partner",
                    "entity_id": "123",
                    "screen_metadata": {
                        "title": "Partner Details",
                        "type": "entity_detail_page"
                    },
                    "ui_schema_file": "partners-ui.json"
                }
                
                # Import after patching
                from ai_assistant.sub_agents.screen_context_agent.utils import parse_screen_url_callback
                
                # Create mock callback context
                mock_callback_context = Mock()
                mock_callback_context.state = {
                    "screen_url": "/partners/123"
                }
                
                # Call the function
                result = parse_screen_url_callback(mock_callback_context, None)
                print(f"✅ Function executed successfully")
                
                # Verify result is None (callbacks should return None)
                assert result is None, "Callback should return None"
                
                # Verify data is stored in state
                assert "basic_screen_context" in mock_callback_context.state, "Should store basic_screen_context in state"
                
                basic_context = mock_callback_context.state["basic_screen_context"]
                
                # Verify result structure
                assert isinstance(basic_context, dict), "Result should be a dictionary"
                assert "current_url" in basic_context, "Should contain current_url"
                assert basic_context["current_url"] == "/partners/123", "Should return correct URL"
                assert basic_context["screen_type"] == "entity_detail_page", "Should detect entity detail page"
                assert basic_context["entity_in_focus"] == "Partner", "Should detect Partner entity"
                
                print("✅ Function returns properly structured screen context")
                
                # Verify mock was called
                mock_ui_config.find_matching_page_by_url.assert_called_once_with("/partners/123")
                print("✅ UI config manager called correctly")
                
                return True
                
        except Exception as e:
            print(f"❌ parse_screen_url_callback test failed: {e}")
            return False
    
    def test_get_entity_display_name_tool_function(self):
        """Test the get_entity_display_name_tool function with mocked dependencies"""
        print("\n🧪 Testing get_entity_display_name_tool function...")
        
        try:
            # Mock the common_callbacks functions that are imported inside the function
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke_api, \
                 patch('ai_assistant.utils.common_callbacks.construct_api_url') as mock_construct_url, \
                 patch('ai_assistant.sub_agents.screen_context_agent.utils.config_manager') as mock_config:
                
                # Setup mocks
                mock_config.load_entity_api_config.return_value = {
                    "entities": [{
                        "endpoints": [{
                            "method": "GET",
                            "url": "/api/partners/{id}"
                        }]
                    }]
                }
                mock_config.get_api_base_url.return_value = "https://localhost:44426"
                mock_construct_url.return_value = "https://localhost:44426/api/partners/123"
                mock_invoke_api.return_value = {
                    "status": "success",
                    "response": {
                        "id": "123",
                        "name": "ABC Corp",
                        "status": "Active"
                    }
                }
                
                # Import after patching
                from ai_assistant.sub_agents.screen_context_agent.utils import get_entity_display_name_tool
                
                # Call the function
                result = get_entity_display_name_tool("Partner", "123")
                print(f"✅ Function executed successfully")
                
                # Verify result structure
                assert isinstance(result, dict), "Result should be a dictionary"
                assert result["id"] == "123", "Should return correct ID"
                assert result["name"] == "ABC Corp", "Should return correct name"
                assert result["status"] == "Active", "Should return correct status"
                assert result["entity_type"] == "Partner", "Should return correct entity type"
                
                print("✅ Function returns properly formatted entity display info")
                
                # Verify mocks were called correctly
                mock_config.load_entity_api_config.assert_called_once_with("Partner")
                mock_construct_url.assert_called_once()
                mock_invoke_api.assert_called_once()
                print("✅ All dependencies called correctly")
                
                return True
                
        except Exception as e:
            print(f"❌ get_entity_display_name_tool test failed: {e}")
            return False
    
    def test_agent_prompt_quality(self):
        """Test that the agent's instruction is appropriate"""
        print("\n🧪 Testing agent prompt quality...")
        
        try:
            from ai_assistant.sub_agents.screen_context_agent import screen_context_agent
            
            instruction = screen_context_agent.instruction
            
            # Check that instruction exists and has content
            assert instruction and len(instruction.strip()) > 0, "Instruction should not be empty"
            
            # Check for key elements in the instruction
            instruction_lower = instruction.lower()
            
            # Should mention the tool function
            assert "get_entity_display_name_tool" in instruction_lower, "Instruction should mention get_entity_display_name_tool"
            
            # Should mention screen context processing
            assert any(word in instruction_lower for word in ["screen", "context", "url"]), "Instruction should mention screen context processing"
            
            # Should specify JSON output
            assert "json" in instruction_lower, "Instruction should mention JSON output format"
            
            # Should mention entity detection logic
            assert "entity_detail_page" in instruction_lower, "Instruction should mention entity_detail_page logic"
            
            # Should be comprehensive but not excessive
            word_count = len(instruction.split())
            assert 100 < word_count < 1000, f"Instruction should be comprehensive but not excessive (got {word_count} words)"
            
            print("✅ Agent instruction is well-structured and appropriate")
            print(f"✅ Instruction length: {word_count} words")
            
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
            from ai_assistant.sub_agents.screen_context_agent.utils import get_entity_display_name_tool
            
            # Try to find problematic imports by checking the source
            import inspect
            source = inspect.getsource(get_entity_display_name_tool)
            
            if "workflow_agent.api_worker" in source:
                issues_found.append("❌ Still importing from old workflow_agent.api_worker structure")
            
            if "from ai_assistant.workflow_agent" in source:
                issues_found.append("❌ Still importing from workflow_agent instead of utils.common_callbacks")
            
            # Check for proper imports that should exist
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
        """Test that the agent actually executes and returns screen_context data"""
        print("\n🧪 Testing agent execution end-to-end...")
        
        try:
            # Mock the external dependencies
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke_api, \
                 patch('ai_assistant.utils.common_callbacks.construct_api_url') as mock_construct_url, \
                 patch('ai_assistant.sub_agents.screen_context_agent.utils.config_manager') as mock_config, \
                 patch('ai_assistant.sub_agents.screen_context_agent.utils.ui_config_manager') as mock_ui_config:
                
                # Setup mocks for entity display name fetching
                mock_config.load_entity_api_config.return_value = {
                    "entities": [{
                        "endpoints": [{
                            "method": "GET",
                            "url": "/api/partners/{id}"
                        }]
                    }]
                }
                mock_config.get_api_base_url.return_value = "https://localhost:44426"
                mock_construct_url.return_value = "https://localhost:44426/api/partners/123"
                mock_invoke_api.return_value = {
                    "status": "success",
                    "response": {
                        "id": "123",
                        "name": "ABC Corp",
                        "status": "Active"
                    }
                }
                
                # Setup mock for URL parsing (this will trigger the before_model_callback)
                mock_ui_config.find_matching_page_by_url.return_value = {
                    "screen_type": "entity_detail_page",
                    "entity_name": "Partner",
                    "entity_id": "123",
                    "screen_metadata": {
                        "title": "Partner Details",
                        "type": "entity_detail_page"
                    },
                    "ui_schema_file": "partners-ui.json"
                }
                
                # Import the agent after patching
                from ai_assistant.sub_agents.screen_context_agent import screen_context_agent
                
                # Create a mock session state
                mock_session_state = {
                    "screen_url": "/partners/123"
                }
                
                # Execute the agent components
                try:
                    # Verify agent has the right tool
                    assert len(screen_context_agent.tools) == 1, "Agent should have exactly one tool"
                    tool = screen_context_agent.tools[0]
                    assert hasattr(tool, 'func'), "Tool should have a function"
                    
                    # Test the tool function directly
                    result = tool.func("Partner", "123")
                    
                    # Verify result structure
                    assert isinstance(result, dict), "Tool should return a dictionary"
                    assert "name" in result, "Result should contain name"
                    assert result["name"] == "ABC Corp", "Should return correct entity name"
                    assert result["entity_type"] == "Partner", "Should return correct entity type"
                    
                    print("✅ Agent tool executes successfully and returns expected data")
                    
                    # Test the before_model_callback
                    mock_callback_context = Mock()
                    mock_callback_context.state = mock_session_state
                    
                    callback_result = screen_context_agent.before_model_callback(mock_callback_context, None)
                    
                    # Verify callback returns None
                    assert callback_result is None, "Callback should return None"
                    
                    # Verify data is stored in state  
                    assert "basic_screen_context" in mock_callback_context.state, "Should store basic_screen_context in state"
                    
                    basic_context = mock_callback_context.state["basic_screen_context"]
                    assert basic_context["screen_type"] == "entity_detail_page", "Should detect entity detail page"
                    assert basic_context["entity_in_focus"] == "Partner", "Should detect Partner entity"
                    
                    print("✅ Agent before_model_callback executes successfully")
                    print("✅ Agent is properly configured for end-to-end execution")
                    
                    # Verify that mocks were called (indicating the components were executed)
                    mock_ui_config.find_matching_page_by_url.assert_called()
                    mock_invoke_api.assert_called()
                    print("✅ Agent components made expected calls")
                    
                    return True
                    
                except Exception as execution_error:
                    print(f"❌ Agent execution failed: {execution_error}")
                    return False
                
        except Exception as e:
            print(f"❌ Agent execution test failed: {e}")
            return False


def run_screen_context_agent_tests():
    """Run all tests for screen_context_agent"""
    print("🚀 Running screen_context_agent test suite...")
    print("=" * 60)
    
    test_instance = TestScreenContextAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Structure Test", test_instance.test_agent_structure),
        ("Utils Import Test", test_instance.test_utils_import),
        ("URL Parsing Test", test_instance.test_parse_screen_url_callback_function),
        ("Entity Display Name Test", test_instance.test_get_entity_display_name_tool_function),
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
        print("🎉 ALL TESTS PASSED! screen_context_agent is ready for production!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
    
    return passed == total


if __name__ == "__main__":
    success = run_screen_context_agent_tests()
    sys.exit(0 if success else 1)
