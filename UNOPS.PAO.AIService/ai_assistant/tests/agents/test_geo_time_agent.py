#!/usr/bin/env python3
"""
Test suite for geo_time_agent

This test validates that the geo_time_agent is properly configured,
self-contained, and functions correctly.
"""

import sys
import os
import json
from datetime import datetime, timezone
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
# The test is in: ai_assistant/tests/agents/test_geo_time_agent.py
# We need to go up to the project root to find ai_assistant
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestGeoTimeAgent:
    """Test cases for geo_time_agent"""
    
    def test_agent_import(self):
        """Test that geo_time_agent can be imported successfully"""
        print("🧪 Testing geo_time_agent import...")
        
        try:
            from ai_assistant.sub_agents.geo_time_agent import geo_time_agent
            print("✅ Successfully imported geo_time_agent")
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
            from ai_assistant.sub_agents.geo_time_agent import geo_time_agent
            
            # Check basic attributes
            assert hasattr(geo_time_agent, 'name'), "Agent should have a name"
            assert geo_time_agent.name == "geo_time_agent", f"Expected name 'geo_time_agent', got '{geo_time_agent.name}'"
            print(f"✅ Agent name verified: {geo_time_agent.name}")
            
            # Check description
            assert hasattr(geo_time_agent, 'description'), "Agent should have a description"
            description_keywords = ["time", "location", "geo", "current"]
            assert any(keyword in geo_time_agent.description.lower() for keyword in description_keywords), "Description should mention time or location"
            print(f"✅ Agent description verified: {geo_time_agent.description}")
            
            # Check instruction (should be a string for this agent)
            assert hasattr(geo_time_agent, 'instruction'), "Agent should have instruction"
            assert isinstance(geo_time_agent.instruction, str), "Instruction should be a string"
            assert "geo-time" in geo_time_agent.instruction.lower() or "time" in geo_time_agent.instruction.lower(), "Instruction should mention time/geo functionality"
            print("✅ Agent instruction is properly configured")
            
            # Check output_key
            assert hasattr(geo_time_agent, 'output_key'), "Agent should have output_key"
            assert geo_time_agent.output_key == "user_geo_stats", f"Expected output_key 'user_geo_stats', got '{geo_time_agent.output_key}'"
            print(f"✅ Agent output_key verified: {geo_time_agent.output_key}")
            
            # Check callbacks
            assert hasattr(geo_time_agent, 'before_model_callback'), "Agent should have before_model_callback"
            assert callable(geo_time_agent.before_model_callback), "before_model_callback should be callable"
            print("✅ Agent before_model_callback is configured")
            
            return True
            
        except Exception as e:
            print(f"❌ Structure test failed: {e}")
            return False
    
    def test_utils_import(self):
        """Test that utils functions can be imported"""
        print("\n🧪 Testing utils import...")
        
        try:
            from ai_assistant.sub_agents.geo_time_agent.utils import (
                get_current_time_info,
                get_user_location_info,
                get_geo_time_info_before_model
            )
            print("✅ Successfully imported all utility functions")
            
            # Check that they're callable
            assert callable(get_current_time_info), "get_current_time_info should be callable"
            assert callable(get_user_location_info), "get_user_location_info should be callable"
            assert callable(get_geo_time_info_before_model), "get_geo_time_info_before_model should be callable"
            print("✅ All utility functions are callable")
            
            return True
            
        except ImportError as e:
            print(f"❌ Utils Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Utils Test Error: {e}")
            return False
    
    def test_get_current_time_info_function(self):
        """Test the get_current_time_info function"""
        print("\n🧪 Testing get_current_time_info function...")
        
        try:
            # Import the function
            from ai_assistant.sub_agents.geo_time_agent.utils import get_current_time_info
            
            # Call the function
            result = get_current_time_info()
            print(f"✅ Function executed successfully")
            
            # Verify result structure
            assert isinstance(result, dict), "Result should be a dictionary"
            
            # Check required fields
            required_fields = ["utc_time", "utc_timestamp", "formatted_utc", "date", "time", "day_of_week"]
            for field in required_fields:
                assert field in result, f"Result should contain {field}"
            
            # Verify time format
            assert "UTC" in result["formatted_utc"], "Formatted UTC should contain 'UTC'"
            assert isinstance(result["utc_timestamp"], float), "UTC timestamp should be a float"
            
            # Verify date format (YYYY-MM-DD)
            date_parts = result["date"].split("-")
            assert len(date_parts) == 3, "Date should be in YYYY-MM-DD format"
            assert len(date_parts[0]) == 4, "Year should be 4 digits"
            
            print("✅ Function returns properly formatted time information")
            print(f"✅ Current time: {result['formatted_utc']}")
            
            return True
            
        except Exception as e:
            print(f"❌ get_current_time_info test failed: {e}")
            return False
    
    def test_get_user_location_info_function(self):
        """Test the get_user_location_info function with mocked HTTP request"""
        print("\n🧪 Testing get_user_location_info function...")
        
        try:
            # Mock the requests.get call to avoid external dependency
            with patch('ai_assistant.sub_agents.geo_time_agent.utils.requests.get') as mock_get:
                
                # Setup mock response for successful location detection
                mock_response = Mock()
                mock_response.status_code = 200
                mock_response.json.return_value = {
                    "status": "success",
                    "country": "United States",
                    "countryCode": "US",
                    "regionName": "New York",
                    "city": "New York",
                    "timezone": "America/New_York",
                    "lat": 40.7128,
                    "lon": -74.0060,
                    "isp": "Test ISP"
                }
                mock_get.return_value = mock_response
                
                # Import after patching
                from ai_assistant.sub_agents.geo_time_agent.utils import get_user_location_info
                
                # Call the function
                result = get_user_location_info()
                print(f"✅ Function executed successfully")
                
                # Verify result structure
                assert isinstance(result, dict), "Result should be a dictionary"
                
                # Check expected fields for successful response
                expected_fields = ["country", "country_code", "region", "city", "timezone", "latitude", "longitude", "source"]
                for field in expected_fields:
                    assert field in result, f"Result should contain {field}"
                
                # Verify specific values
                assert result["country"] == "United States", "Should return correct country"
                assert result["city"] == "New York", "Should return correct city"
                assert result["source"] == "ip_geolocation", "Should indicate ip_geolocation source"
                
                print("✅ Function returns properly formatted location information")
                print(f"✅ Location: {result['city']}, {result['country']}")
                
                # Verify HTTP request was made correctly
                mock_get.assert_called_once_with('http://ip-api.com/json/', timeout=5)
                print("✅ HTTP request made correctly")
                
                return True
                
        except Exception as e:
            print(f"❌ get_user_location_info test failed: {e}")
            return False
    
    def test_get_user_location_info_failure_handling(self):
        """Test the get_user_location_info function failure handling"""
        print("\n🧪 Testing get_user_location_info failure handling...")
        
        try:
            # Mock the requests.get call to simulate failure
            with patch('ai_assistant.sub_agents.geo_time_agent.utils.requests.get') as mock_get:
                
                # Setup mock to raise an exception
                mock_get.side_effect = Exception("Network error")
                
                # Import after patching
                from ai_assistant.sub_agents.geo_time_agent.utils import get_user_location_info
                
                # Call the function
                result = get_user_location_info()
                print(f"✅ Function handled failure gracefully")
                
                # Verify result structure for failure case
                assert isinstance(result, dict), "Result should be a dictionary"
                assert result["location_available"] == False, "Should indicate location not available"
                assert "error" in result, "Should contain error information"
                assert result["source"] == "error", "Should indicate error source"
                
                print("✅ Function returns proper error response")
                print(f"✅ Error handled: {result.get('error', 'Unknown error')}")
                
                return True
                
        except Exception as e:
            print(f"❌ get_user_location_info failure test failed: {e}")
            return False
    
    def test_get_geo_time_info_before_model_function(self):
        """Test the get_geo_time_info_before_model function with mocked dependencies"""
        print("\n🧪 Testing get_geo_time_info_before_model function...")
        
        try:
            # Mock the requests.get call for location info
            with patch('ai_assistant.sub_agents.geo_time_agent.utils.requests.get') as mock_get:
                
                # Setup mock response for location
                mock_response = Mock()
                mock_response.status_code = 200
                mock_response.json.return_value = {
                    "status": "success",
                    "country": "Bangladesh",
                    "countryCode": "BD",
                    "regionName": "Dhaka",
                    "city": "Dhaka",
                    "timezone": "Asia/Dhaka",
                    "lat": 23.8103,
                    "lon": 90.4125,
                    "isp": "Test ISP BD"
                }
                mock_get.return_value = mock_response
                
                # Import after patching
                from ai_assistant.sub_agents.geo_time_agent.utils import get_geo_time_info_before_model
                
                # Create mock tool context
                mock_tool_context = Mock()
                mock_tool_context.state = {}
                
                # Call the function with correct signature
                result = get_geo_time_info_before_model(mock_tool_context, None)
                print(f"✅ Function executed successfully")
                
                # Verify that data was stored in tool context state
                assert 'user_geo_stats' in mock_tool_context.state, "Should store user_geo_stats in state"
                
                geo_stats = mock_tool_context.state['user_geo_stats']
                assert isinstance(geo_stats, dict), "Geo stats should be a dictionary"
                
                # Check required top-level fields
                required_fields = ["timestamp", "time_info", "location_info", "status"]
                for field in required_fields:
                    assert field in geo_stats, f"Geo stats should contain {field}"
                
                # Verify time_info structure
                time_info = geo_stats["time_info"]
                assert "utc_time" in time_info, "Should contain time information"
                assert "formatted_utc" in time_info, "Should contain formatted time"
                
                # Verify location_info structure
                location_info = geo_stats["location_info"]
                assert "country" in location_info, "Should contain location information"
                assert location_info["country"] == "Bangladesh", "Should return correct country"
                
                # Verify status
                assert geo_stats["status"] == "success", "Should indicate success status"
                
                print("✅ Function stores properly formatted geo-time data")
                print(f"✅ Location detected: {location_info.get('city', 'Unknown')}, {location_info.get('country', 'Unknown')}")
                
                return True
                
        except Exception as e:
            print(f"❌ get_geo_time_info_before_model test failed: {e}")
            return False
    
    def test_agent_prompt_quality(self):
        """Test that the agent's instruction is appropriate"""
        print("\n🧪 Testing agent prompt quality...")
        
        try:
            from ai_assistant.sub_agents.geo_time_agent import geo_time_agent
            
            instruction = geo_time_agent.instruction
            
            # Check that instruction exists and has content
            assert instruction and len(instruction.strip()) > 0, "Instruction should not be empty"
            
            # Check for key elements in the instruction
            instruction_lower = instruction.lower()
            
            # Should mention geo-time functionality
            assert any(word in instruction_lower for word in ["geo", "time", "location", "datetime"]), "Instruction should mention geo-time functionality"
            
            # Should mention JSON output
            assert "json" in instruction_lower, "Instruction should mention JSON output format"
            
            # Should be concise and focused
            word_count = len(instruction.split())
            assert 10 < word_count < 100, f"Instruction should be concise and focused (got {word_count} words)"
            
            print("✅ Agent instruction is well-structured and appropriate")
            print(f"✅ Instruction length: {word_count} words")
            print(f"✅ Instruction: {instruction}")
            
            return True
            
        except Exception as e:
            print(f"❌ Prompt quality test failed: {e}")
            return False
    
    def test_dependencies_self_contained(self):
        """Test that all dependencies are properly available"""
        print("\n🧪 Testing dependency self-containment...")
        
        issues_found = []
        
        try:
            # Check imports in the utils functions
            from ai_assistant.sub_agents.geo_time_agent.utils import get_geo_time_info_before_model
            
            # Try to find problematic imports by checking the source
            import inspect
            source = inspect.getsource(get_geo_time_info_before_model)
            
            if "workflow_agent" in source:
                issues_found.append("❌ Still importing from old workflow_agent structure")
            
            # Check for proper function functionality (imports are at module level, which is fine)
            # The function should use datetime and json functionality
            function_features = [
                "datetime",  # Should use datetime functionality
                "json"       # Should use JSON functionality
            ]
            
            # Note: All imports (requests, json, datetime) are at module level, which is appropriate
            print("ℹ️ Note: All imports (requests, json, datetime) are at module level (appropriate design)")
            
            if issues_found:
                print("🔧 Issues found with dependencies:")
                for issue in issues_found:
                    print(f"  {issue}")
                return False
            else:
                print("✅ Dependencies appear to be properly structured")
                print("✅ External dependencies: requests (for IP geolocation)")
                return True
                
        except Exception as e:
            print(f"❌ Dependency test failed: {e}")
            return False
    
    def test_agent_execution(self):
        """Test that the agent actually executes and returns user_geo_stats data"""
        print("\n🧪 Testing agent execution end-to-end...")
        
        try:
            # Mock the external HTTP request dependency
            with patch('ai_assistant.sub_agents.geo_time_agent.utils.requests.get') as mock_get:
                
                # Setup mocks for location detection
                mock_response = Mock()
                mock_response.status_code = 200
                mock_response.json.return_value = {
                    "status": "success",
                    "country": "United States",
                    "countryCode": "US",
                    "regionName": "California",
                    "city": "San Francisco",
                    "timezone": "America/Los_Angeles",
                    "lat": 37.7749,
                    "lon": -122.4194,
                    "isp": "Test ISP CA"
                }
                mock_get.return_value = mock_response
                
                # Import the agent after patching
                from ai_assistant.sub_agents.geo_time_agent import geo_time_agent
                
                # Create a mock session state
                mock_session_state = {}
                
                # Execute the agent components
                try:
                    # Test the before_model_callback
                    mock_tool_context = Mock()
                    mock_tool_context.state = mock_session_state
                    
                    callback_result = geo_time_agent.before_model_callback(mock_tool_context, None)
                    
                    # Verify callback executed successfully and stored data
                    assert 'user_geo_stats' in mock_tool_context.state, "Should store user_geo_stats in state"
                    
                    geo_stats = mock_tool_context.state['user_geo_stats']
                    assert isinstance(geo_stats, dict), "Geo stats should be a dictionary"
                    assert "time_info" in geo_stats, "Should contain time information"
                    assert "location_info" in geo_stats, "Should contain location information"
                    assert geo_stats["status"] == "success", "Should indicate success"
                    
                    # Verify time information
                    time_info = geo_stats["time_info"]
                    assert "utc_time" in time_info, "Should have UTC time"
                    assert "day_of_week" in time_info, "Should have day of week"
                    
                    # Verify location information
                    location_info = geo_stats["location_info"]
                    assert location_info["city"] == "San Francisco", "Should detect correct city"
                    assert location_info["country"] == "United States", "Should detect correct country"
                    
                    print("✅ Agent before_model_callback executes successfully")
                    print(f"✅ Time info collected: {time_info['formatted_utc']}")
                    print(f"✅ Location detected: {location_info['city']}, {location_info['country']}")
                    
                    # Verify instruction quality
                    instruction = geo_time_agent.instruction
                    assert isinstance(instruction, str), "Instruction should be a string"
                    assert len(instruction) > 10, "Instruction should be substantial"
                    
                    print("✅ Agent instruction is properly configured")
                    print("✅ Agent is properly configured for end-to-end execution")
                    
                    # Verify that mocks were called (indicating external requests were made)
                    mock_get.assert_called_with('http://ip-api.com/json/', timeout=5)
                    print("✅ Agent made expected external API calls")
                    
                    return True
                    
                except Exception as execution_error:
                    print(f"❌ Agent execution failed: {execution_error}")
                    return False
                
        except Exception as e:
            print(f"❌ Agent execution test failed: {e}")
            return False


def run_geo_time_agent_tests():
    """Run all tests for geo_time_agent"""
    print("🚀 Running geo_time_agent test suite...")
    print("=" * 60)
    
    test_instance = TestGeoTimeAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Structure Test", test_instance.test_agent_structure),
        ("Utils Import Test", test_instance.test_utils_import),
        ("Current Time Info Test", test_instance.test_get_current_time_info_function),
        ("User Location Info Test", test_instance.test_get_user_location_info_function),
        ("Location Failure Handling Test", test_instance.test_get_user_location_info_failure_handling),
        ("Before Model Callback Test", test_instance.test_get_geo_time_info_before_model_function),
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
        print("🎉 ALL TESTS PASSED! geo_time_agent is ready for production!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
    
    return passed == total


if __name__ == "__main__":
    success = run_geo_time_agent_tests()
    sys.exit(0 if success else 1)
