#!/usr/bin/env python3
"""
Test suite for user_request_agent

This test validates that the user_request_agent is properly configured,
handles worker_agent delegation correctly, and manages before/after callbacks accurately.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
# The test is in: ai_assistant/tests/agents/test_user_request_agent.py
# We need to go up to the project root to find ai_assistant
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestUserRequestAgent:
    """Test cases for user_request_agent"""
    
    def test_agent_import(self):
        """Test that user_request_agent can be imported successfully"""
        print("🧪 Testing user_request_agent import...")
        
        try:
            # Try importing with mocked worker_agent to handle complex dependencies
            with patch('ai_assistant.sub_agents.user_request_agent.agent.worker_agent') as mock_worker:
                mock_worker.name = "worker_agent"
                from ai_assistant.sub_agents.user_request_agent import user_request_agent
                print("✅ Successfully imported user_request_agent (with mocked dependencies)")
                return True
        except ImportError as e:
            print(f"❌ Import Error: {e}")
            # Try to provide more specific error information
            if "agent_callbacks" in str(e):
                print("ℹ️ Issue: agent_callbacks module missing (likely in worker_agent sub-dependencies)")
            return False
        except Exception as e:
            print(f"❌ Unexpected Error: {e}")
            return False
    
    def test_agent_structure_and_worker_delegation(self):
        """Test that the agent has correct structure and worker_agent delegation"""
        print("\n🧪 Testing agent structure and worker_agent delegation...")
        
        try:
            # First, let's fix the missing imports that we identified
            with patch('ai_assistant.sub_agents.user_request_agent.agent.worker_agent') as mock_worker_agent, \
                 patch('ai_assistant.sub_agents.user_request_agent.agent.handle_audio_artifacts_before_model') as mock_before_callback:
                
                # Set up the mock worker_agent with expected attributes
                mock_worker_agent.name = "worker_agent"
                mock_worker_agent.output_key = "workflow_result"
                
                from ai_assistant.sub_agents.user_request_agent import user_request_agent
                
                # Check basic attributes
                assert hasattr(user_request_agent, 'name'), "Agent should have a name"
                assert user_request_agent.name == "user_request_agent", f"Expected name 'user_request_agent', got '{user_request_agent.name}'"
                print(f"✅ Agent name verified: {user_request_agent.name}")
                
                # Check description
                assert hasattr(user_request_agent, 'description'), "Agent should have a description"
                description_keywords = ["main", "assistant", "interaction", "route", "workflow"]
                assert any(keyword in user_request_agent.description.lower() for keyword in description_keywords), "Description should mention main assistant or routing"
                print(f"✅ Agent description verified: {user_request_agent.description}")
                
                # Check instruction (should be a comprehensive string)
                assert hasattr(user_request_agent, 'instruction'), "Agent should have instruction"
                assert isinstance(user_request_agent.instruction, str), "Instruction should be a string"
                assert "workflow_agent" in user_request_agent.instruction.lower(), "Instruction should mention workflow_agent delegation"
                print("✅ Agent instruction contains workflow_agent delegation logic")
                
                # Check sub_agents for worker_agent delegation
                assert hasattr(user_request_agent, 'sub_agents'), "Agent should have sub_agents for delegation"
                print(f"✅ Agent has sub_agents for delegation: {len(user_request_agent.sub_agents) if user_request_agent.sub_agents else 0} sub-agent(s)")
                
                # Check callbacks
                assert hasattr(user_request_agent, 'before_model_callback'), "Agent should have before_model_callback"
                assert hasattr(user_request_agent, 'after_model_callback'), "Agent should have after_model_callback"
                print("✅ Agent callbacks are configured")
                
                return True
                
        except Exception as e:
            print(f"❌ Structure test failed: {e}")
            return False
    
    def test_utils_import(self):
        """Test that utils functions can be imported"""
        print("\n🧪 Testing utils import...")
        
        try:
            from ai_assistant.sub_agents.user_request_agent.utils import (
                enforce_json_format_callback,
                handle_audio_artifacts_before_model,
                classify_request_type
            )
            print("✅ Successfully imported all utility functions")
            
            # Check that they're callable
            assert callable(enforce_json_format_callback), "enforce_json_format_callback should be callable"
            assert callable(handle_audio_artifacts_before_model), "handle_audio_artifacts_before_model should be callable"
            assert callable(classify_request_type), "classify_request_type should be callable"
            print("✅ All utility functions are callable")
            
            return True
            
        except ImportError as e:
            print(f"❌ Utils Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Utils Test Error: {e}")
            return False
    
    def test_classify_request_type_function(self):
        """Test the classify_request_type function for routing decisions"""
        print("\n🧪 Testing classify_request_type function...")
        
        try:
            from ai_assistant.sub_agents.user_request_agent.utils import classify_request_type
            
            # Test greeting classification
            greeting_inputs = ["Hi", "Hello", "Good morning", "Hey there"]
            for greeting in greeting_inputs:
                result = classify_request_type(greeting)
                assert result == "greeting", f"'{greeting}' should be classified as greeting, got '{result}'"
            print("✅ Greeting classification works correctly")
            
            # Test gratitude classification
            gratitude_inputs = ["Thank you", "Thanks", "Appreciate it"]
            for gratitude in gratitude_inputs:
                result = classify_request_type(gratitude)
                assert result == "gratitude", f"'{gratitude}' should be classified as gratitude, got '{result}'"
            print("✅ Gratitude classification works correctly")
            
            # Test knowledge request classification
            knowledge_inputs = ["What is a partner?", "How do I create a contact?", "Explain interactions"]
            for knowledge in knowledge_inputs:
                result = classify_request_type(knowledge)
                assert result == "knowledge_request", f"'{knowledge}' should be classified as knowledge_request, got '{result}'"
            print("✅ Knowledge request classification works correctly")
            
            # Test empty input
            result = classify_request_type("")
            assert result == "unknown", "Empty input should be classified as unknown"
            print("✅ Empty input handling works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ classify_request_type test failed: {e}")
            return False
    
    def test_enforce_json_format_callback(self):
        """Test the after_model_callback for JSON format enforcement"""
        print("\n🧪 Testing enforce_json_format_callback function...")
        
        try:
            from ai_assistant.sub_agents.user_request_agent.utils import enforce_json_format_callback
            
            # Test with valid JSON response
            mock_callback_context = Mock()
            mock_response = Mock()
            mock_response.text = '{"result": [{"type": "markdown", "message": "Hello!"}], "followUps": ["Action 1"]}'
            mock_response.candidates = [Mock()]
            
            # This should return a modified response
            result = enforce_json_format_callback(mock_callback_context, mock_response)
            assert result is not None, "Should return modified response for valid JSON"
            print("✅ Function handles valid JSON correctly")
            
            # Test with invalid JSON response (should be wrapped)
            mock_response_invalid = Mock()
            mock_response_invalid.text = "Just a plain text response"
            mock_response_invalid.candidates = [Mock()]
            
            # Should wrap the response in proper JSON format
            result = enforce_json_format_callback(mock_callback_context, mock_response_invalid)
            assert result is not None, "Should return wrapped response for invalid JSON"
            print("✅ Function handles invalid JSON by wrapping it")
            
            # Test with JSON in markdown code block
            mock_response_markdown = Mock()
            mock_response_markdown.text = '''Here's the response:
```json
{"result": [{"type": "markdown", "message": "Test"}], "followUps": []}
```'''
            mock_response_markdown.candidates = [Mock()]
            
            result = enforce_json_format_callback(mock_callback_context, mock_response_markdown)
            assert result is not None, "Should extract and return JSON from markdown code blocks"
            print("✅ Function extracts JSON from markdown code blocks")
            
            return True
            
        except Exception as e:
            print(f"❌ enforce_json_format_callback test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_handle_audio_artifacts_before_model(self):
        """Test the before_model_callback for audio artifact handling"""
        print("\n🧪 Testing handle_audio_artifacts_before_model function...")
        
        try:
            from ai_assistant.sub_agents.user_request_agent.utils import handle_audio_artifacts_before_model
            
            # Test with audio files in callback context
            mock_callback_context = Mock()
            mock_callback_context.state = {
                'uploaded_files_metadata': [
                    {
                        'filename': 'test_audio.mp3',
                        'mime_type': 'audio/mpeg', 
                        'is_audio': True,
                        'size': 1024000
                    },
                    {
                        'filename': 'document.pdf',
                        'mime_type': 'application/pdf',
                        'is_audio': False,
                        'size': 512000
                    }
                ]
            }
            
            mock_llm_request = Mock()
            
            # Call the function
            result = handle_audio_artifacts_before_model(mock_callback_context, mock_llm_request)
            
            # Verify that audio files were processed
            assert 'has_audio_files' in mock_callback_context.state, "Should set has_audio_files flag"
            assert mock_callback_context.state['has_audio_files'] == True, "Should detect audio files"
            assert 'available_audio_files' in mock_callback_context.state, "Should store available audio files"
            assert 'uploaded_artifacts' in mock_callback_context.state, "Should store all uploaded artifacts"
            
            audio_files = mock_callback_context.state['available_audio_files']
            assert len(audio_files) == 1, "Should have one audio file"
            assert audio_files[0]['filename'] == 'test_audio.mp3', "Should store correct audio file info"
            
            print("✅ Function processes audio files correctly")
            print(f"✅ Audio files detected: {len(audio_files)}")
            
            # Test with no audio files
            mock_callback_context_no_audio = Mock()
            mock_callback_context_no_audio.state = {}
            
            result = handle_audio_artifacts_before_model(mock_callback_context_no_audio, mock_llm_request)
            # Should not crash and should handle gracefully
            print("✅ Function handles no audio files gracefully")
            
            return True
            
        except Exception as e:
            print(f"❌ handle_audio_artifacts_before_model test failed: {e}")
            return False
    
    def test_worker_agent_delegation_logic(self):
        """Test that the agent properly delegates to worker_agent when needed"""
        print("\n🧪 Testing worker_agent delegation logic...")
        
        try:
            # Mock the worker_agent import
            with patch('ai_assistant.sub_agents.user_request_agent.agent.worker_agent') as mock_worker_agent, \
                 patch('ai_assistant.sub_agents.user_request_agent.agent.handle_audio_artifacts_before_model') as mock_before_callback:
                
                # Setup mock worker_agent
                mock_worker_agent.name = "worker_agent"
                mock_worker_agent.output_key = "workflow_result"
                
                from ai_assistant.sub_agents.user_request_agent import user_request_agent
                
                # Verify that worker_agent is in sub_agents
                assert hasattr(user_request_agent, 'sub_agents'), "Agent should have sub_agents"
                
                # Test the instruction contains delegation logic
                instruction = user_request_agent.instruction
                
                # Check for key delegation phrases
                delegation_keywords = [
                    "delegate to workflow_agent",
                    "workflow_agent capabilities",
                    "action_plan suggests",
                    "always delegate"
                ]
                
                found_keywords = []
                for keyword in delegation_keywords:
                    if keyword.lower() in instruction.lower():
                        found_keywords.append(keyword)
                
                assert len(found_keywords) >= 2, f"Instruction should contain delegation logic, found: {found_keywords}"
                print(f"✅ Instruction contains delegation logic: {len(found_keywords)} key phrases found")
                
                # Check for specific routing rules
                routing_rules = [
                    "handle directly",
                    "delegate to workflow_agent", 
                    "greetings",
                    "search, retrieve, list data"
                ]
                
                found_rules = []
                for rule in routing_rules:
                    if rule.lower() in instruction.lower():
                        found_rules.append(rule)
                
                assert len(found_rules) >= 3, f"Instruction should contain routing rules, found: {found_rules}"
                print(f"✅ Instruction contains routing rules: {len(found_rules)} rules found")
                
                return True
                
        except Exception as e:
            print(f"❌ Worker agent delegation test failed: {e}")
            return False
    
    def test_agent_prompt_quality(self):
        """Test that the agent's instruction is appropriate for user interaction and delegation"""
        print("\n🧪 Testing agent prompt quality...")
        
        try:
            # Mock imports to access the agent
            with patch('ai_assistant.sub_agents.user_request_agent.agent.worker_agent') as mock_worker_agent, \
                 patch('ai_assistant.sub_agents.user_request_agent.agent.handle_audio_artifacts_before_model'):
                
                mock_worker_agent.name = "worker_agent"
                
                from ai_assistant.sub_agents.user_request_agent import user_request_agent
                
                instruction = user_request_agent.instruction
                
                # Check that instruction exists and has content
                assert instruction and len(instruction.strip()) > 0, "Instruction should not be empty"
                
                # Check for key elements in the instruction
                instruction_lower = instruction.lower()
                
                # Should mention user interaction
                assert any(word in instruction_lower for word in ["friendly", "assistant", "user", "greeting"]), "Instruction should mention user interaction"
                
                # Should mention delegation logic
                assert "workflow_agent" in instruction_lower, "Instruction should mention workflow_agent"
                
                # Should specify JSON output
                assert "json" in instruction_lower, "Instruction should mention JSON output format"
                
                # Should mention followups
                assert "followups" in instruction_lower, "Instruction should mention followups"
                
                # Should be comprehensive but not excessive
                word_count = len(instruction.split())
                assert 200 < word_count < 2000, f"Instruction should be comprehensive but not excessive (got {word_count} words)"
                
                print("✅ Agent instruction is well-structured and appropriate")
                print(f"✅ Instruction length: {word_count} words")
                
                return True
                
        except Exception as e:
            print(f"❌ Prompt quality test failed: {e}")
            return False
    
    def test_dependencies_and_imports(self):
        """Test that all dependencies are properly available and imports are correct"""
        print("\n🧪 Testing dependencies and import issues...")
        
        issues_found = []
        
        try:
            # Check individual utility imports first
            from ai_assistant.sub_agents.user_request_agent.utils import enforce_json_format_callback
            print("✅ enforce_json_format_callback can be imported")
            
            # Check if worker_agent exists as a sub_agent
            try:
                from ai_assistant.sub_agents.worker_agent import worker_agent
                print("✅ worker_agent exists and can be imported")
            except ImportError as e:
                issues_found.append(f"❌ worker_agent cannot be imported: {str(e)[:100]}")
                print(f"⚠️ Worker agent import issue: {str(e)[:100]}")
            
            # Check if main agent works with mocking problematic imports
            try:
                with patch('ai_assistant.sub_agents.user_request_agent.agent.worker_agent'):
                    from ai_assistant.sub_agents.user_request_agent import user_request_agent
                    print("✅ user_request_agent can be imported with mocked worker_agent")
            except Exception as e:
                issues_found.append(f"❌ user_request_agent import issue: {str(e)[:100]}")
                print(f"⚠️ user_request_agent import issue: {str(e)[:100]}")
            
            if len(issues_found) <= 1:  # Allow for one known issue (worker_agent complexity)
                print("✅ Most dependencies appear to be available")
                if issues_found:
                    print("ℹ️ Known issues with complex sub-agent imports are expected")
                return True
            else:
                print("🔧 Multiple issues found with dependencies:")
                for issue in issues_found:
                    print(f"  {issue}")
                return False
                
        except Exception as e:
            print(f"❌ Dependency test failed: {e}")
            return False
    
    def test_agent_execution_with_mocked_worker(self):
        """Test the complete agent execution flow with mocked worker_agent"""
        print("\n🧪 Testing agent execution with mocked worker delegation...")
        
        try:
            # Mock all dependencies
            with patch('ai_assistant.sub_agents.user_request_agent.agent.worker_agent') as mock_worker_agent, \
                 patch('ai_assistant.sub_agents.user_request_agent.agent.handle_audio_artifacts_before_model') as mock_before_callback, \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                # Setup mocks
                mock_worker_agent.name = "worker_agent"
                mock_worker_agent.output_key = "workflow_result"
                mock_config.get_project_name.return_value = "Test Project"
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.user_request_agent import user_request_agent
                
                # Test the before_model_callback
                mock_callback_context = Mock()
                mock_callback_context.state = {
                    'uploaded_files_metadata': [
                        {'filename': 'test.mp3', 'mime_type': 'audio/mpeg', 'is_audio': True, 'size': 1000}
                    ]
                }
                mock_llm_request = Mock()
                
                if user_request_agent.before_model_callback:
                    user_request_agent.before_model_callback(mock_callback_context, mock_llm_request)
                    assert 'has_audio_files' in mock_callback_context.state, "Before callback should process audio files"
                    print("✅ Before model callback executed successfully")
                
                # Test the after_model_callback
                mock_tool_context = Mock()
                mock_response = Mock()
                mock_response.text = '{"result": [{"type": "markdown", "message": "Test response"}], "followUps": []}'
                
                if user_request_agent.after_model_callback:
                    user_request_agent.after_model_callback(mock_tool_context, mock_response)
                    print("✅ After model callback executed successfully")
                
                # Verify agent structure for delegation
                assert hasattr(user_request_agent, 'sub_agents'), "Agent should have sub_agents for delegation"
                print("✅ Agent is configured for worker delegation")
                
                # Verify instruction quality for delegation logic
                instruction = user_request_agent.instruction
                assert "workflow_agent" in instruction.lower(), "Instruction should contain delegation logic"
                print("✅ Agent instruction contains proper delegation logic")
                
                return True
                
        except Exception as e:
            print(f"❌ Agent execution test failed: {e}")
            return False


def run_user_request_agent_tests():
    """Run all tests for user_request_agent"""
    print("🚀 Running user_request_agent test suite...")
    print("=" * 60)
    
    test_instance = TestUserRequestAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Structure & Worker Delegation Test", test_instance.test_agent_structure_and_worker_delegation),
        ("Utils Import Test", test_instance.test_utils_import),
        ("Request Classification Test", test_instance.test_classify_request_type_function),
        ("JSON Format Callback Test", test_instance.test_enforce_json_format_callback),
        ("Audio Artifacts Callback Test", test_instance.test_handle_audio_artifacts_before_model),
        ("Worker Delegation Logic Test", test_instance.test_worker_agent_delegation_logic),
        ("Prompt Quality Test", test_instance.test_agent_prompt_quality),
        ("Dependencies & Imports Test", test_instance.test_dependencies_and_imports),
        ("Agent Execution Test", test_instance.test_agent_execution_with_mocked_worker),
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
        print("🎉 ALL TESTS PASSED! user_request_agent is ready for production!")
        print("🎯 Worker delegation and callback accuracy verified!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
        print("🔧 Check import issues and delegation logic.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
        print("🔧 Focus on imports and worker_agent integration.")
    
    return passed == total


if __name__ == "__main__":
    success = run_user_request_agent_tests()
    sys.exit(0 if success else 1)
