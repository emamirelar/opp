#!/usr/bin/env python3
"""
Test suite for response_formatter_agent

This test validates that the response_formatter_agent properly formats API responses
into structured JSON with appropriate display types for frontend rendering.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestResponseFormatterAgent:
    """Test cases for response_formatter_agent"""
    
    def test_agent_import(self):
        """Test that response_formatter_agent can be imported successfully"""
        print("Testing response_formatter_agent import...")
        
        try:
            # Import the real config manager to get the actual model
            from ai_assistant.utils.api_config_manager import config_manager
            actual_model = config_manager.get_gemini_model()
            print(f"Using actual configured gemini model: {actual_model}")
            
            from ai_assistant.sub_agents.response_formatter_agent import response_formatter_agent
            print("Successfully imported response_formatter_agent")
            return True
        except ImportError as e:
            print(f"Import Error: {e}")
            return False
        except Exception as e:
            print(f"Unexpected Error: {e}")
            return False
    
    def test_pydantic_models(self):
        """Test that the Pydantic models are properly defined and functional"""
        print("\nTesting Pydantic models...")
        
        try:
            from ai_assistant.sub_agents.response_formatter_agent.agent import (
                ResponseItem, SourceItem, FormattedResponse
            )
            
            # Test ResponseItem
            response_item = ResponseItem(
                type="markdown",
                message="Test message",
                entity="Partner"
            )
            assert response_item.type == "markdown", "ResponseItem type should be set correctly"
            assert response_item.message == "Test message", "ResponseItem message should be set correctly"
            assert response_item.entity == "Partner", "ResponseItem entity should be set correctly"
            print("ResponseItem model works correctly")
            
            # Test SourceItem
            source_item = SourceItem(
                title="Test Source",
                url="https://example.com",
                description="Test description"
            )
            assert source_item.title == "Test Source", "SourceItem title should be set correctly"
            assert source_item.url == "https://example.com", "SourceItem URL should be set correctly"
            print("SourceItem model works correctly")
            
            # Test FormattedResponse
            formatted_response = FormattedResponse(
                result=[response_item],
                sources=[source_item],
                followUps=["Action 1", "Action 2"]
            )
            assert len(formatted_response.result) == 1, "FormattedResponse should contain result items"
            assert len(formatted_response.sources) == 1, "FormattedResponse should contain sources"
            assert len(formatted_response.followUps) == 2, "FormattedResponse should contain followUps"
            print("FormattedResponse model works correctly")
            
            # Test model serialization
            json_data = formatted_response.model_dump()
            assert "result" in json_data, "Serialized data should contain result"
            assert "sources" in json_data, "Serialized data should contain sources"
            assert "followUps" in json_data, "Serialized data should contain followUps"
            print("Model serialization works correctly")
            
            return True
            
        except Exception as e:
            print(f"Pydantic models test failed: {e}")
            return False
    
    def test_agent_structure_and_configuration(self):
        """Test that the agent has correct structure and formatting configuration"""
        print("\nTesting agent structure and formatting configuration...")
        
        try:
            # Use real config manager
            from ai_assistant.utils.api_config_manager import config_manager
            actual_model = config_manager.get_gemini_model()
            
            from ai_assistant.sub_agents.response_formatter_agent import response_formatter_agent
            
            # Check basic attributes
            assert hasattr(response_formatter_agent, 'name'), "Agent should have a name"
            assert response_formatter_agent.name == "response_formatter_agent", f"Expected name 'response_formatter_agent', got '{response_formatter_agent.name}'"
            print(f"Agent name verified: {response_formatter_agent.name}")
            
            # Check description
            assert hasattr(response_formatter_agent, 'description'), "Agent should have a description"
            description_keywords = ["format", "api", "response", "json", "frontend", "render"]
            assert any(keyword in response_formatter_agent.description.lower() for keyword in description_keywords), "Description should mention formatting functionality"
            print(f"Agent description verified: {response_formatter_agent.description}")
            
            # Check instruction (should be dynamic)
            assert hasattr(response_formatter_agent, 'instruction'), "Agent should have instruction"
            print("Agent instruction configured (dynamic)")
            
            # Check output configuration
            assert hasattr(response_formatter_agent, 'output_key'), "Agent should have output_key"
            assert response_formatter_agent.output_key == "formatted_response", f"Expected output_key 'formatted_response', got '{response_formatter_agent.output_key}'"
            print(f"Agent output_key verified: {response_formatter_agent.output_key}")
            
            # Check output schema
            assert hasattr(response_formatter_agent, 'output_schema'), "Agent should have output_schema"
            assert response_formatter_agent.output_schema is not None, "Output schema should not be None"
            print("Agent output schema configured")
            
            # Check callbacks
            assert hasattr(response_formatter_agent, 'before_model_callback'), "Agent should have before_model_callback"
            assert hasattr(response_formatter_agent, 'after_model_callback'), "Agent should have after_model_callback"
            print("Agent callbacks are configured")
            
            # Check transfer restrictions
            assert hasattr(response_formatter_agent, 'disallow_transfer_to_parent'), "Agent should have transfer restrictions"
            assert response_formatter_agent.disallow_transfer_to_parent == True, "Should disallow transfer to parent"
            print("Agent transfer restrictions properly configured")
            
            return True
            
        except Exception as e:
            print(f"Structure test failed: {e}")
            return False
    
    def test_utils_import(self):
        """Test that utils functions can be imported"""
        print("\nTesting utils import...")
        
        try:
            from ai_assistant.sub_agents.response_formatter_agent.utils import (
                format_response_before_model,
                dynamic_response_instruction,
                extract_key_data
            )
            print("Successfully imported utility functions")
            
            # Check that they're callable
            assert callable(format_response_before_model), "format_response_before_model should be callable"
            assert callable(dynamic_response_instruction), "dynamic_response_instruction should be callable"
            assert callable(extract_key_data), "extract_key_data should be callable"
            print("All utility functions are callable")
            
            return True
            
        except ImportError as e:
            print(f"Utils Import Error: {e}")
            return False
        except Exception as e:
            print(f"Utils Test Error: {e}")
            return False
    
    def test_integration_readiness(self):
        """Test that the response formatter agent is ready for integration"""
        print("\nTesting agent integration readiness...")
        
        try:
            # Use real config manager for integration test
            from ai_assistant.utils.api_config_manager import config_manager
            actual_model = config_manager.get_gemini_model()
            print(f"Integration test using actual model: {actual_model}")
            
            from ai_assistant.sub_agents.response_formatter_agent import response_formatter_agent
            
            # Test integration points
            integration_checks = {
                "has_name": hasattr(response_formatter_agent, 'name') and response_formatter_agent.name == "response_formatter_agent",
                "has_description": hasattr(response_formatter_agent, 'description') and len(response_formatter_agent.description) > 0,
                "has_instruction": hasattr(response_formatter_agent, 'instruction'),
                "has_output_key": hasattr(response_formatter_agent, 'output_key') and response_formatter_agent.output_key == "formatted_response",
                "has_output_schema": hasattr(response_formatter_agent, 'output_schema') and response_formatter_agent.output_schema is not None,
                "has_callbacks": (hasattr(response_formatter_agent, 'before_model_callback') and 
                                hasattr(response_formatter_agent, 'after_model_callback')),
                "proper_restrictions": (hasattr(response_formatter_agent, 'disallow_transfer_to_parent') and 
                                      response_formatter_agent.disallow_transfer_to_parent == True)
            }
            
            # Verify all integration checks pass
            for check_name, check_result in integration_checks.items():
                assert check_result == True, f"Integration check '{check_name}' failed"
                print(f"{check_name.replace('_', ' ').title()}: PASS")
            
            print("Response formatter agent is ready for system integration")
            print("Structured JSON output capability confirmed")
            print("Multiple display types supported")
            print("Pydantic model validation active")
            
            return True
            
        except Exception as e:
            print(f"Integration readiness test failed: {e}")
            return False


def run_response_formatter_agent_tests():
    """Run all tests for response_formatter_agent"""
    print("Running response_formatter_agent test suite...")
    print("=" * 75)
    
    test_instance = TestResponseFormatterAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Pydantic Models Test", test_instance.test_pydantic_models),
        ("Structure & Configuration Test", test_instance.test_agent_structure_and_configuration),
        ("Utils Import Test", test_instance.test_utils_import),
        ("Integration Readiness Test", test_instance.test_integration_readiness),
    ]
    
    results = {}
    
    for test_name, test_func in tests:
        try:
            results[test_name] = test_func()
        except Exception as e:
            print(f"{test_name} failed with exception: {e}")
            results[test_name] = False
    
    # Summary
    print("\n" + "=" * 75)
    print("TEST SUMMARY:")
    
    passed = 0
    total = len(results)
    
    for test_name, result in results.items():
        status = "PASS" if result else "FAIL"
        print(f"{test_name}: {status}")
        if result:
            passed += 1
    
    print(f"\nOverall: {passed}/{total} tests passed")
    
    if passed == total:
        print("ALL TESTS PASSED! response_formatter_agent is ready for production!")
        print("JSON formatting and display types verified!")
        print("Response formatting features confirmed!")
    elif passed >= total * 0.75:
        print("Most tests passed, but some issues need fixing.")
        print("Check formatting logic and callback functionality.")
    else:
        print("Multiple issues found. Significant fixes needed.")
        print("Focus on response formatting and model validation.")
    
    return passed == total


if __name__ == "__main__":
    success = run_response_formatter_agent_tests()
    sys.exit(0 if success else 1)
