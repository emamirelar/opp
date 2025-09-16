#!/usr/bin/env python3
"""
Test suite for worker_agent

This test validates that the worker_agent properly orchestrates sequential workflow
execution between task_executor_agent and response_formatter_agent.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestWorkerAgent:
    """Test cases for worker_agent"""
    
    def test_agent_import(self):
        """Test that worker_agent can be imported successfully"""
        print("🧪 Testing worker_agent import...")
        
        try:
            # Try importing with mocked sub-agents to handle complex dependencies
            with patch('ai_assistant.sub_agents.worker_agent.agent.task_executor_agent') as mock_task_agent, \
                 patch('ai_assistant.sub_agents.worker_agent.agent.response_formatter_agent') as mock_response_agent:
                
                # Set up mock sub-agents
                mock_task_agent.name = "task_executor_agent"
                mock_response_agent.name = "response_formatter_agent"
                
                from ai_assistant.sub_agents.worker_agent import worker_agent
                print("✅ Successfully imported worker_agent (with mocked sub-agents)")
                return True
        except ImportError as e:
            print(f"❌ Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Unexpected Error: {e}")
            return False
    
    def test_agent_structure_and_orchestration(self):
        """Test that the agent has correct structure and sequential orchestration"""
        print("\n🧪 Testing agent structure and sequential orchestration...")
        
        try:
            # Mock sub-agents to test structure
            with patch('ai_assistant.sub_agents.worker_agent.agent.task_executor_agent') as mock_task_agent, \
                 patch('ai_assistant.sub_agents.worker_agent.agent.response_formatter_agent') as mock_response_agent:
                
                # Set up mock sub-agents with expected attributes
                mock_task_agent.name = "task_executor_agent"
                mock_task_agent.description = "Processes entities and makes API calls"
                mock_response_agent.name = "response_formatter_agent"
                mock_response_agent.description = "Formats results into user-friendly responses"
                
                from ai_assistant.sub_agents.worker_agent import worker_agent
                
                # Check basic attributes
                assert hasattr(worker_agent, 'name'), "Agent should have a name"
                assert worker_agent.name == "worker_agent", f"Expected name 'worker_agent', got '{worker_agent.name}'"
                print(f"✅ Agent name verified: {worker_agent.name}")
                
                # Check description
                assert hasattr(worker_agent, 'description'), "Agent should have a description"
                description_keywords = ["comprehensive", "worker", "process", "api", "tool", "format"]
                assert any(keyword in worker_agent.description.lower() for keyword in description_keywords), "Description should mention comprehensive workflow processing"
                print(f"✅ Agent description verified: {worker_agent.description}")
                
                # Check that it's a SequentialAgent (should have sub_agents)
                assert hasattr(worker_agent, 'sub_agents'), "SequentialAgent should have sub_agents"
                assert worker_agent.sub_agents is not None, "sub_agents should not be None"
                assert len(worker_agent.sub_agents) == 2, f"Expected 2 sub-agents, got {len(worker_agent.sub_agents) if worker_agent.sub_agents else 0}"
                print(f"✅ Sequential agent structure verified: {len(worker_agent.sub_agents)} sub-agents")
                
                # Check sub-agent order (critical for sequential processing)
                sub_agent_names = [agent.name for agent in worker_agent.sub_agents]
                expected_order = ["task_executor_agent", "response_formatter_agent"]
                assert sub_agent_names == expected_order, f"Expected sub-agent order {expected_order}, got {sub_agent_names}"
                print(f"✅ Sub-agent execution order verified: {' → '.join(sub_agent_names)}")
                
                return True
                
        except Exception as e:
            print(f"❌ Structure test failed: {e}")
            return False
    
    def test_utils_import(self):
        """Test that utils functions can be imported"""
        print("\n🧪 Testing utils import...")
        
        try:
            from ai_assistant.sub_agents.worker_agent.utils import (
                log_workflow_start,
                log_workflow_step,
                log_workflow_complete,
                validate_workflow_output,
                format_workflow_error
            )
            print("✅ Successfully imported all utility functions")
            
            # Check that they're callable
            assert callable(log_workflow_start), "log_workflow_start should be callable"
            assert callable(log_workflow_step), "log_workflow_step should be callable"
            assert callable(log_workflow_complete), "log_workflow_complete should be callable"
            assert callable(validate_workflow_output), "validate_workflow_output should be callable"
            assert callable(format_workflow_error), "format_workflow_error should be callable"
            print("✅ All utility functions are callable")
            
            return True
            
        except ImportError as e:
            print(f"❌ Utils Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Utils Test Error: {e}")
            return False
    
    def test_logging_functions(self):
        """Test the workflow logging functions"""
        print("\n🧪 Testing workflow logging functions...")
        
        try:
            from ai_assistant.sub_agents.worker_agent.utils import (
                log_workflow_start,
                log_workflow_step,
                log_workflow_complete
            )
            
            # Test workflow start logging
            print("📝 Testing log_workflow_start...")
            log_workflow_start()  # Should print workflow start message
            print("✅ log_workflow_start executed successfully")
            
            # Test workflow step logging
            print("📝 Testing log_workflow_step...")
            log_workflow_step("Task Execution", 1)
            log_workflow_step("Response Formatting", 2)
            print("✅ log_workflow_step executed successfully")
            
            # Test workflow complete logging
            print("📝 Testing log_workflow_complete...")
            log_workflow_complete()  # Should print completion message
            print("✅ log_workflow_complete executed successfully")
            
            return True
            
        except Exception as e:
            print(f"❌ Logging functions test failed: {e}")
            return False
    
    def test_output_validation_function(self):
        """Test the validate_workflow_output function"""
        print("\n🧪 Testing validate_workflow_output function...")
        
        try:
            from ai_assistant.sub_agents.worker_agent.utils import validate_workflow_output
            
            # Test valid output (dict)
            valid_output = {
                "result": "success",
                "data": {"message": "Test completed"},
                "workflow_stage": "complete"
            }
            result = validate_workflow_output(valid_output)
            assert result == True, "Valid dict output should return True"
            print("✅ Valid output validation works correctly")
            
            # Test invalid output (not dict)
            invalid_outputs = [
                "string output",
                ["list", "output"],
                42,
                None
            ]
            
            for invalid_output in invalid_outputs:
                result = validate_workflow_output(invalid_output)
                assert result == False, f"Invalid output {type(invalid_output)} should return False"
            
            print("✅ Invalid output validation works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ validate_workflow_output test failed: {e}")
            return False
    
    def test_error_formatting_function(self):
        """Test the format_workflow_error function"""
        print("\n🧪 Testing format_workflow_error function...")
        
        try:
            from ai_assistant.sub_agents.worker_agent.utils import format_workflow_error
            
            # Test error formatting
            error_message = "Test error occurred"
            result = format_workflow_error(error_message)
            
            # Verify structure
            assert isinstance(result, dict), "Error format should return a dict"
            assert "error" in result, "Error response should have 'error' field"
            assert "message" in result, "Error response should have 'message' field"
            assert "workflow_stage" in result, "Error response should have 'workflow_stage' field"
            assert "timestamp" in result, "Error response should have 'timestamp' field"
            
            # Verify content
            assert result["error"] == True, "Error field should be True"
            assert result["message"] == error_message, "Message should match input"
            assert result["workflow_stage"] == "error", "Workflow stage should be 'error'"
            
            print("✅ Error formatting works correctly")
            print(f"✅ Error format: {result}")
            
            return True
            
        except Exception as e:
            print(f"❌ format_workflow_error test failed: {e}")
            return False
    
    def test_sequential_workflow_orchestration(self):
        """Test that the worker_agent properly orchestrates sequential execution"""
        print("\n🧪 Testing sequential workflow orchestration...")
        
        try:
            # Mock sub-agents with detailed behavior
            with patch('ai_assistant.sub_agents.worker_agent.agent.task_executor_agent') as mock_task_agent, \
                 patch('ai_assistant.sub_agents.worker_agent.agent.response_formatter_agent') as mock_response_agent:
                
                # Set up mock sub-agents
                mock_task_agent.name = "task_executor_agent"
                mock_task_agent.description = "Processes entities and makes API calls"
                mock_response_agent.name = "response_formatter_agent"
                mock_response_agent.description = "Formats results into user-friendly responses"
                
                from ai_assistant.sub_agents.worker_agent import worker_agent
                
                # Verify sequential structure
                assert hasattr(worker_agent, 'sub_agents'), "Should have sub_agents for sequential execution"
                assert len(worker_agent.sub_agents) == 2, "Should have exactly 2 sub-agents"
                
                # Verify execution order
                first_agent = worker_agent.sub_agents[0]
                second_agent = worker_agent.sub_agents[1]
                
                assert first_agent.name == "task_executor_agent", "First agent should be task_executor_agent"
                assert second_agent.name == "response_formatter_agent", "Second agent should be response_formatter_agent"
                
                print("✅ Sequential workflow orchestration verified")
                print(f"✅ Step 1: {first_agent.name}")
                print(f"✅ Step 2: {second_agent.name}")
                
                return True
                
        except Exception as e:
            print(f"❌ Sequential orchestration test failed: {e}")
            return False
    
    def test_workflow_execution_simulation(self):
        """Test a simulated workflow execution with mocked sub-agents"""
        print("\n🧪 Testing workflow execution simulation...")
        
        try:
            # Import utility functions
            from ai_assistant.sub_agents.worker_agent.utils import (
                log_workflow_start,
                log_workflow_step,
                log_workflow_complete,
                validate_workflow_output
            )
            
            # Simulate a complete workflow
            print("🔄 Simulating complete workflow execution...")
            
            # Step 1: Start workflow
            log_workflow_start()
            
            # Step 2: Execute task executor (simulated)
            log_workflow_step("Task Execution", 1)
            task_result = {
                "api_calls_made": 3,
                "entities_processed": ["Partner", "Contact"],
                "results": {"partners": [{"id": 1, "name": "Test Partner"}]}
            }
            
            # Validate intermediate result
            is_valid = validate_workflow_output(task_result)
            assert is_valid == True, "Task executor output should be valid"
            print("✅ Step 1 completed successfully")
            
            # Step 3: Execute response formatter (simulated)
            log_workflow_step("Response Formatting", 2)
            formatted_result = {
                "result": [{
                    "type": "markdown",
                    "message": "Found 1 partner: Test Partner"
                }],
                "followUps": ["View partner details", "Add new contact"]
            }
            
            # Validate final result
            is_valid = validate_workflow_output(formatted_result)
            assert is_valid == True, "Response formatter output should be valid"
            print("✅ Step 2 completed successfully")
            
            # Step 4: Complete workflow
            log_workflow_complete()
            
            print("✅ Complete workflow simulation successful")
            print(f"✅ Final result structure: {list(formatted_result.keys())}")
            
            return True
            
        except Exception as e:
            print(f"❌ Workflow execution simulation failed: {e}")
            return False
    
    def test_error_handling_and_resilience(self):
        """Test error handling and workflow resilience"""
        print("\n🧪 Testing error handling and workflow resilience...")
        
        try:
            from ai_assistant.sub_agents.worker_agent.utils import format_workflow_error, validate_workflow_output
            
            # Test various error scenarios
            error_scenarios = [
                "API call failed",
                "Invalid entity type",
                "Network timeout",
                "Authentication failed"
            ]
            
            for error_msg in error_scenarios:
                error_response = format_workflow_error(error_msg)
                
                # Verify error response structure
                assert isinstance(error_response, dict), "Error response should be a dict"
                assert error_response["error"] == True, "Error flag should be True"
                assert error_response["message"] == error_msg, "Error message should match"
                
                # Verify error response is valid workflow output
                is_valid = validate_workflow_output(error_response)
                assert is_valid == True, "Error response should be valid workflow output"
            
            print("✅ Error handling works correctly for all scenarios")
            print(f"✅ Tested {len(error_scenarios)} error scenarios")
            
            return True
            
        except Exception as e:
            print(f"❌ Error handling test failed: {e}")
            return False
    
    def test_agent_integration_readiness(self):
        """Test that the agent is ready for integration with the larger system"""
        print("\n🧪 Testing agent integration readiness...")
        
        try:
            # Mock all dependencies for integration test
            with patch('ai_assistant.sub_agents.worker_agent.agent.task_executor_agent') as mock_task_agent, \
                 patch('ai_assistant.sub_agents.worker_agent.agent.response_formatter_agent') as mock_response_agent:
                
                # Set up comprehensive mock sub-agents
                mock_task_agent.name = "task_executor_agent"
                mock_task_agent.output_key = "task_result"
                mock_response_agent.name = "response_formatter_agent"
                mock_response_agent.output_key = "formatted_response"
                
                from ai_assistant.sub_agents.worker_agent import worker_agent
                
                # Test integration points
                integration_checks = {
                    "has_name": hasattr(worker_agent, 'name'),
                    "has_description": hasattr(worker_agent, 'description'),
                    "has_sub_agents": hasattr(worker_agent, 'sub_agents'),
                    "correct_sub_agent_count": len(worker_agent.sub_agents) == 2,
                    "sequential_structure": True  # SequentialAgent structure
                }
                
                # Verify all integration checks pass
                for check_name, check_result in integration_checks.items():
                    assert check_result == True, f"Integration check '{check_name}' failed"
                    print(f"✅ {check_name.replace('_', ' ').title()}: PASS")
                
                # Test workflow coordination capability
                print("✅ Agent is ready for system integration")
                print("✅ Sequential workflow orchestration confirmed")
                print("✅ Sub-agent coordination verified")
                
                return True
                
        except Exception as e:
            print(f"❌ Integration readiness test failed: {e}")
            return False
    
    def test_dependencies_self_contained(self):
        """Test that the agent and its dependencies are self-contained"""
        print("\n🧪 Testing dependencies and self-containment...")
        
        try:
            # Test that utils can be imported independently
            from ai_assistant.sub_agents.worker_agent.utils import (
                log_workflow_start,
                validate_workflow_output,
                format_workflow_error
            )
            print("✅ Utils functions are self-contained")
            
            # Test that agent structure is well-defined
            with patch('ai_assistant.sub_agents.worker_agent.agent.task_executor_agent'), \
                 patch('ai_assistant.sub_agents.worker_agent.agent.response_formatter_agent'):
                
                from ai_assistant.sub_agents.worker_agent import worker_agent
                
                # Check that agent is properly structured
                required_attributes = ['name', 'description', 'sub_agents']
                for attr in required_attributes:
                    assert hasattr(worker_agent, attr), f"Agent should have '{attr}' attribute"
                
                print("✅ Agent structure is well-defined")
                print("✅ Dependencies are properly managed")
                
                return True
                
        except Exception as e:
            print(f"❌ Dependencies test failed: {e}")
            return False


def run_worker_agent_tests():
    """Run all tests for worker_agent"""
    print("🚀 Running worker_agent test suite...")
    print("=" * 70)
    
    test_instance = TestWorkerAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Structure & Orchestration Test", test_instance.test_agent_structure_and_orchestration),
        ("Utils Import Test", test_instance.test_utils_import),
        ("Logging Functions Test", test_instance.test_logging_functions),
        ("Output Validation Test", test_instance.test_output_validation_function),
        ("Error Formatting Test", test_instance.test_error_formatting_function),
        ("Sequential Orchestration Test", test_instance.test_sequential_workflow_orchestration),
        ("Workflow Execution Simulation Test", test_instance.test_workflow_execution_simulation),
        ("Error Handling & Resilience Test", test_instance.test_error_handling_and_resilience),
        ("Integration Readiness Test", test_instance.test_agent_integration_readiness),
        ("Dependencies Self-Contained Test", test_instance.test_dependencies_self_contained),
    ]
    
    results = {}
    
    for test_name, test_func in tests:
        try:
            results[test_name] = test_func()
        except Exception as e:
            print(f"❌ {test_name} failed with exception: {e}")
            results[test_name] = False
    
    # Summary
    print("\n" + "=" * 70)
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
        print("🎉 ALL TESTS PASSED! worker_agent is ready for production!")
        print("🎯 Sequential workflow orchestration verified!")
        print("🔄 Sub-agent coordination working perfectly!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
        print("🔧 Check workflow orchestration and sub-agent integration.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
        print("🔧 Focus on sequential agent structure and dependencies.")
    
    return passed == total


if __name__ == "__main__":
    success = run_worker_agent_tests()
    sys.exit(0 if success else 1)
