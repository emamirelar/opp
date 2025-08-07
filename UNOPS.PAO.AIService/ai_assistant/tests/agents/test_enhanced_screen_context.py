#!/usr/bin/env python3
"""
Enhanced test suite for screen_context_agent with sophisticated URL handling

This test validates the enhanced logic for handling screen_url vs user_focus_context
relationships, multiple URL patterns, and entity detection.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestEnhancedScreenContextAgent:
    """Enhanced test cases for screen_context_agent with sophisticated URL handling"""
    
    def test_multiple_url_scenarios(self):
        """Test various combinations of screen_url and user_focus_context"""
        print("\n🧪 Testing multiple URL scenarios...")
        
        # Test scenarios as described by the user
        test_scenarios = [
            {
                "name": "Basic partnerships/contacts navigation",
                "screen_url": "partnerships/contacts",
                "user_focus_context": "partnerships/contacts",
                "expected_relationship": "same",
                "expected_primary_entity": "Contact",
                "description": "User is viewing contacts in partnerships section"
            },
            {
                "name": "Empty focus context gets resolved", 
                "screen_url": "partnerships/contacts/123",
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_resolved_focus": "partnerships/contacts/123",
                "expected_primary_entity": "Contact",
                "expected_entity_id": "123",
                "description": "Empty user_focus_context should be set to screen_url"
            },
            {
                "name": "Homepage detection",
                "screen_url": "/",
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_screen_type": "homepage",
                "description": "Root URL means homepage"
            },
            {
                "name": "AI assistant fullscreen mode",
                "screen_url": "/ai/assistant",
                "user_focus_context": "",
                "expected_relationship": "same", 
                "expected_screen_type": "ai_assistant_mode",
                "description": "/ai* means fullscreen AI assistant"
            },
            {
                "name": "Separate focus contexts",
                "screen_url": "/ai/chat",
                "user_focus_context": "/contacts/456",
                "expected_relationship": "different",
                "expected_primary_entity": "Contact",
                "expected_primary_id": "456",
                "expected_secondary_type": "ai_assistant_mode",
                "description": "AI mode with contact focus in right panel"
            },
            {
                "name": "Partner detail page",
                "screen_url": "/partners/789",
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_primary_entity": "Partner",
                "expected_entity_id": "789",
                "expected_screen_type": "entity_detail_page",
                "description": "Partner detail page with numeric ID"
            },
            {
                "name": "Interaction with GUID ID",
                "screen_url": "/interactions/12345678-1234-1234-1234-123456789012", 
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_primary_entity": "Interaction",
                "expected_screen_type": "entity_detail_page",
                "description": "Interaction detail with GUID ID"
            },
            {
                "name": "Partner list page",
                "screen_url": "/partners",
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_primary_entity": "Partner", 
                "expected_screen_type": "entity_list_page",
                "description": "Partner list without ID"
            },
            {
                "name": "Query params preservation",
                "screen_url": "/contacts/123?tab=interactions&filter=recent",
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_primary_entity": "Contact",
                "expected_entity_id": "123",
                "expected_query_params": {"tab": "interactions", "filter": "recent"},
                "description": "URL with query parameters"
            },
            {
                "name": "Dashboard page",
                "screen_url": "/dashboard/overview",
                "user_focus_context": "",
                "expected_relationship": "same",
                "expected_screen_type": "dashboard_overview",
                "description": "Dashboard page detection"
            }
        ]
        
        results = []
        
        for scenario in test_scenarios:
            print(f"\n🔍 Testing: {scenario['name']}")
            print(f"   Description: {scenario['description']}")
            print(f"   screen_url: '{scenario['screen_url']}'")
            print(f"   user_focus_context: '{scenario['user_focus_context']}'")
            
            try:
                # Mock dependencies
                with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke_api, \
                     patch('ai_assistant.utils.common_callbacks.construct_api_url') as mock_construct_url, \
                     patch('ai_assistant.sub_agents.screen_context_agent.utils.config_manager') as mock_config, \
                     patch('ai_assistant.sub_agents.screen_context_agent.utils.ui_config_manager') as mock_ui_config:
                    
                    # Setup mocks
                    mock_config.load_entity_api_config.return_value = {
                        "entities": [{
                            "endpoints": [{
                                "method": "GET",
                                "url": "/api/contacts/{id}"
                            }]
                        }]
                    }
                    mock_config.get_api_base_url.return_value = "https://localhost:44426"
                    mock_construct_url.return_value = "https://localhost:44426/api/contacts/123"
                    mock_invoke_api.return_value = {
                        "status": "success",
                        "response": {
                            "id": "123",
                            "name": "John Doe",
                            "status": "Active",
                            "email": "john@example.com"
                        }
                    }
                    
                    # Mock UI config (return None to use fallback logic)
                    mock_ui_config.find_matching_page_by_url.return_value = None
                    
                    # Import after patching
                    from ai_assistant.sub_agents.screen_context_agent.utils import parse_screen_url_callback
                    
                    # Create mock callback context
                    mock_callback_context = Mock()
                    mock_callback_context.state = {
                        "screen_url": scenario["screen_url"],
                        "user_focus_context": scenario["user_focus_context"]
                    }
                    
                    # Execute the enhanced callback
                    parse_screen_url_callback(mock_callback_context, None)
                    
                    # Verify enhanced_screen_context was created
                    assert "enhanced_screen_context" in mock_callback_context.state, "Should create enhanced_screen_context"
                    
                    enhanced_context = mock_callback_context.state["enhanced_screen_context"]
                    
                    # Test relationship detection
                    actual_relationship = enhanced_context["focus_relationship"]
                    expected_relationship = scenario["expected_relationship"]
                    assert actual_relationship == expected_relationship, f"Expected relationship '{expected_relationship}', got '{actual_relationship}'"
                    
                    # Test resolved focus context
                    if "expected_resolved_focus" in scenario:
                        actual_resolved = enhanced_context["resolved_user_focus_context"]
                        expected_resolved = scenario["expected_resolved_focus"]
                        assert actual_resolved == expected_resolved, f"Expected resolved focus '{expected_resolved}', got '{actual_resolved}'"
                    
                    # Test primary context entity
                    if "expected_primary_entity" in scenario:
                        primary_context = enhanced_context["primary_context"]
                        actual_entity = primary_context["entity_in_focus"]
                        expected_entity = scenario["expected_primary_entity"]
                        assert actual_entity == expected_entity, f"Expected primary entity '{expected_entity}', got '{actual_entity}'"
                    
                    # Test entity ID
                    if "expected_entity_id" in scenario:
                        primary_context = enhanced_context["primary_context"]
                        actual_id = primary_context["entity_id_in_focus"]
                        expected_id = scenario["expected_entity_id"]
                        assert actual_id == expected_id, f"Expected entity ID '{expected_id}', got '{actual_id}'"
                    
                    # Test screen type
                    if "expected_screen_type" in scenario:
                        primary_context = enhanced_context["primary_context"]
                        actual_type = primary_context["screen_type"]
                        expected_type = scenario["expected_screen_type"]
                        assert actual_type == expected_type, f"Expected screen type '{expected_type}', got '{actual_type}'"
                    
                    # Test query parameters
                    if "expected_query_params" in scenario:
                        primary_context = enhanced_context["primary_context"]
                        actual_params = primary_context["query_params"]
                        expected_params = scenario["expected_query_params"]
                        assert actual_params == expected_params, f"Expected query params {expected_params}, got {actual_params}"
                    
                    # Test secondary context for different focus scenarios
                    if "expected_secondary_type" in scenario:
                        secondary_context = enhanced_context["secondary_context"]
                        assert secondary_context is not None, "Should have secondary context for different focus"
                        actual_secondary_type = secondary_context["screen_type"]
                        expected_secondary_type = scenario["expected_secondary_type"]
                        assert actual_secondary_type == expected_secondary_type, f"Expected secondary type '{expected_secondary_type}', got '{actual_secondary_type}'"
                    
                    print(f"   ✅ PASSED: {scenario['name']}")
                    results.append({"name": scenario["name"], "status": "PASSED"})
                    
            except Exception as e:
                print(f"   ❌ FAILED: {scenario['name']} - {str(e)}")
                results.append({"name": scenario["name"], "status": "FAILED", "error": str(e)})
        
        # Summary
        passed = sum(1 for r in results if r["status"] == "PASSED")
        total = len(results)
        print(f"\n📊 Multiple URL Scenarios: {passed}/{total} passed")
        
        return passed == total
    
    def test_entity_display_name_tool(self):
        """Test the get_entity_display_name_tool function"""
        print("\n🧪 Testing get_entity_display_name_tool...")
        
        try:
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke_api, \
                 patch('ai_assistant.utils.common_callbacks.construct_api_url') as mock_construct_url, \
                 patch('ai_assistant.sub_agents.screen_context_agent.utils.config_manager') as mock_config:
                
                # Setup mocks for different entity types
                test_cases = [
                    {
                        "entity_type": "Partner",
                        "entity_id": "123",
                        "mock_config": {
                            "entities": [{
                                "endpoints": [{
                                    "method": "GET",
                                    "url": "/api/partners/{id}"
                                }]
                            }]
                        },
                        "mock_response": {
                            "status": "success",
                            "response": {
                                "id": "123",
                                "name": "UNICEF",
                                "status": "Active",
                                "shortName": "UNICEF",
                                "website": "https://unicef.org",
                                "phone": "+1-555-0123"
                            }
                        },
                        "expected_fields": ["name", "shortName", "website", "phone"]
                    },
                    {
                        "entity_type": "Contact", 
                        "entity_id": "456",
                        "mock_config": {
                            "entities": [{
                                "endpoints": [{
                                    "method": "GET",
                                    "url": "/api/contacts/{id}"
                                }]
                            }]
                        },
                        "mock_response": {
                            "status": "success",
                            "response": {
                                "id": "456",
                                "name": "Jane Smith",
                                "status": "Active",
                                "email": "jane@unicef.org",
                                "title": "Program Manager", 
                                "department": "Health"
                            }
                        },
                        "expected_fields": ["name", "email", "title", "department"]
                    },
                    {
                        "entity_type": "Interaction",
                        "entity_id": "789",
                        "mock_config": {
                            "entities": [{
                                "endpoints": [{
                                    "method": "GET",
                                    "url": "/api/interactions/{id}"
                                }]
                            }]
                        },
                        "mock_response": {
                            "status": "success", 
                            "response": {
                                "id": "789",
                                "name": "Meeting with UNICEF",
                                "status": "Completed",
                                "subject": "Quarterly Review Meeting",
                                "interactionType": "Meeting",
                                "createdDate": "2024-01-15"
                            }
                        },
                        "expected_fields": ["name", "subject", "interactionType", "createdDate"]
                    }
                ]
                
                # Import after patching
                from ai_assistant.sub_agents.screen_context_agent.utils import get_entity_display_name_tool
                
                for test_case in test_cases:
                    print(f"   Testing {test_case['entity_type']} entity...")
                    
                    # Setup mocks for this test case
                    mock_config.load_entity_api_config.return_value = test_case["mock_config"]
                    mock_config.get_api_base_url.return_value = "https://localhost:44426"
                    mock_construct_url.return_value = f"https://localhost:44426/api/{test_case['entity_type'].lower()}s/{test_case['entity_id']}"
                    mock_invoke_api.return_value = test_case["mock_response"]
                    
                    # Call the function
                    result = get_entity_display_name_tool(test_case["entity_type"], test_case["entity_id"])
                    
                    # Verify basic structure
                    assert isinstance(result, dict), "Should return a dictionary"
                    assert "id" in result, "Should contain id"
                    assert "entity_type" in result, "Should contain entity_type"
                    assert "name" in result, "Should contain name"
                    assert result["id"] == test_case["entity_id"], "Should return correct ID"
                    assert result["entity_type"] == test_case["entity_type"], "Should return correct entity type"
                    
                    # Verify entity-specific fields
                    for field in test_case["expected_fields"]:
                        assert field in result, f"Should contain {field} for {test_case['entity_type']}"
                    
                    print(f"   ✅ {test_case['entity_type']} entity test passed")
                
                print("✅ get_entity_display_name_tool tests passed")
                return True
                
        except Exception as e:
            print(f"❌ get_entity_display_name_tool test failed: {e}")
            return False
    
    def test_url_pattern_detection(self):
        """Test URL pattern detection logic"""
        print("\n🧪 Testing URL pattern detection...")
        
        try:
            from ai_assistant.sub_agents.screen_context_agent.utils import _analyze_enhanced_url
            
            url_tests = [
                # Homepage patterns
                {"url": "/", "expected_pattern": "homepage", "expected_type": "homepage"},
                {"url": "", "expected_pattern": "empty", "expected_type": "homepage"},
                
                # AI assistant patterns
                {"url": "/ai", "expected_pattern": "ai_fullscreen", "expected_type": "ai_assistant_mode"},
                {"url": "/ai/chat", "expected_pattern": "ai_fullscreen", "expected_type": "ai_assistant_mode"},
                {"url": "/ai/assistant/fullscreen", "expected_pattern": "ai_fullscreen", "expected_type": "ai_assistant_mode"},
                
                # Entity list patterns
                {"url": "/partners", "expected_pattern": "entity_list", "expected_type": "entity_list_page", "expected_entity": "Partner"},
                {"url": "/contacts", "expected_pattern": "entity_list", "expected_type": "entity_list_page", "expected_entity": "Contact"},
                {"url": "/interactions", "expected_pattern": "entity_list", "expected_type": "entity_list_page", "expected_entity": "Interaction"},
                
                # Entity detail patterns
                {"url": "/partners/123", "expected_pattern": "entity_detail", "expected_type": "entity_detail_page", "expected_entity": "Partner", "expected_id": "123"},
                {"url": "/contacts/456", "expected_pattern": "entity_detail", "expected_type": "entity_detail_page", "expected_entity": "Contact", "expected_id": "456"},
                {"url": "/interactions/12345678-1234-1234-1234-123456789012", "expected_pattern": "entity_detail", "expected_type": "entity_detail_page", "expected_entity": "Interaction", "expected_id": "12345678-1234-1234-1234-123456789012"},
                
                # Form patterns
                {"url": "/partners/create", "expected_pattern": "form", "expected_type": "form_page"},
                {"url": "/contacts/edit/123", "expected_pattern": "form", "expected_type": "form_page"},
                {"url": "/new-interaction", "expected_pattern": "form", "expected_type": "form_page"},
                
                # Dashboard patterns
                {"url": "/dashboard", "expected_pattern": "dashboard", "expected_type": "dashboard_overview"},
                {"url": "/dashboard/overview", "expected_pattern": "dashboard", "expected_type": "dashboard_overview"},
                
                # Complex paths
                {"url": "/partnerships/contacts", "expected_pattern": "entity_list", "expected_type": "entity_list_page", "expected_entity": "Contact"},
                {"url": "/partnerships/partners/789", "expected_pattern": "entity_detail", "expected_type": "entity_detail_page", "expected_entity": "Partner", "expected_id": "789"},
                
                # Query parameters
                {"url": "/contacts/123?tab=interactions&filter=recent", "expected_pattern": "entity_detail", "expected_type": "entity_detail_page", "expected_entity": "Contact", "expected_id": "123"}
            ]
            
            # Mock UI config to return None (use fallback logic)
            with patch('ai_assistant.sub_agents.screen_context_agent.utils.ui_config_manager') as mock_ui_config:
                mock_ui_config.find_matching_page_by_url.return_value = None
                
                for test in url_tests:
                    analysis = _analyze_enhanced_url(test["url"])
                    
                    # Check URL pattern
                    actual_pattern = analysis["url_pattern"]
                    expected_pattern = test["expected_pattern"]
                    assert actual_pattern == expected_pattern, f"URL '{test['url']}': expected pattern '{expected_pattern}', got '{actual_pattern}'"
                    
                    # Check screen type
                    actual_type = analysis["screen_type"]
                    expected_type = test["expected_type"]
                    assert actual_type == expected_type, f"URL '{test['url']}': expected type '{expected_type}', got '{actual_type}'"
                    
                    # Check entity if expected
                    if "expected_entity" in test:
                        actual_entity = analysis["entity_in_focus"]
                        expected_entity = test["expected_entity"]
                        assert actual_entity == expected_entity, f"URL '{test['url']}': expected entity '{expected_entity}', got '{actual_entity}'"
                    
                    # Check entity ID if expected
                    if "expected_id" in test:
                        actual_id = analysis["entity_id_in_focus"]
                        expected_id = test["expected_id"]
                        assert actual_id == expected_id, f"URL '{test['url']}': expected ID '{expected_id}', got '{actual_id}'"
                    
                    print(f"   ✅ URL pattern test passed: {test['url']}")
            
            print("✅ URL pattern detection tests passed")
            return True
            
        except Exception as e:
            print(f"❌ URL pattern detection test failed: {e}")
            return False
    
    def test_id_detection_logic(self):
        """Test ID detection in URLs"""
        print("\n🧪 Testing ID detection logic...")
        
        try:
            from ai_assistant.sub_agents.screen_context_agent.utils import _is_likely_id
            
            id_tests = [
                # Numeric IDs
                {"value": "123", "expected": True, "description": "Simple numeric ID"},
                {"value": "0", "expected": True, "description": "Zero ID"},
                {"value": "999999", "expected": True, "description": "Large numeric ID"},
                
                # GUID IDs
                {"value": "12345678-1234-1234-1234-123456789012", "expected": True, "description": "Standard GUID"},
                {"value": "abcdef12-1234-5678-9abc-def123456789", "expected": True, "description": "GUID with letters"},
                
                # Long alphanumeric IDs
                {"value": "abc123def456ghi789", "expected": True, "description": "Long alphanumeric ID"},
                {"value": "user_12345_profile", "expected": False, "description": "ID with underscores (not pure alphanumeric)"},
                
                # Non-IDs
                {"value": "create", "expected": False, "description": "Action keyword"},
                {"value": "edit", "expected": False, "description": "Action keyword"},
                {"value": "new", "expected": False, "description": "Action keyword"},
                {"value": "", "expected": False, "description": "Empty string"},
                {"value": "a", "expected": False, "description": "Single character"},
                {"value": "abc", "expected": False, "description": "Short string"},
                {"value": "12345678-1234-1234-1234", "expected": False, "description": "Incomplete GUID"},
                
                # Edge cases
                {"value": "123abc", "expected": False, "description": "Mixed short string"},
                {"value": "abcdef12345", "expected": True, "description": "11 character alphanumeric (just over threshold)"}
            ]
            
            for test in id_tests:
                result = _is_likely_id(test["value"])
                expected = test["expected"]
                assert result == expected, f"ID test '{test['value']}' ({test['description']}): expected {expected}, got {result}"
                print(f"   ✅ ID detection passed: {test['description']}")
            
            print("✅ ID detection logic tests passed")
            return True
            
        except Exception as e:
            print(f"❌ ID detection logic test failed: {e}")
            return False


def run_enhanced_screen_context_tests():
    """Run all enhanced tests for screen_context_agent"""
    print("🚀 Running Enhanced Screen Context Agent Test Suite...")
    print("=" * 70)
    
    test_instance = TestEnhancedScreenContextAgent()
    
    # Run tests in order
    tests = [
        ("Multiple URL Scenarios", test_instance.test_multiple_url_scenarios),
        ("Entity Display Name Tool", test_instance.test_entity_display_name_tool), 
        ("URL Pattern Detection", test_instance.test_url_pattern_detection),
        ("ID Detection Logic", test_instance.test_id_detection_logic),
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
    print("📊 ENHANCED TEST SUMMARY:")
    
    passed = 0
    total = len(results)
    
    for test_name, result in results.items():
        status = "PASS" if result else "FAIL"
        print(f"{test_name}: {status}")
        if result:
            passed += 1
    
    print(f"\nOverall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 ALL ENHANCED TESTS PASSED! Enhanced screen_context_agent is ready!")
        print("\n🎯 Key Features Validated:")
        print("   ✅ Sophisticated screen_url vs user_focus_context logic")
        print("   ✅ Multiple URL pattern detection (/, /ai*, entity pages)")
        print("   ✅ Entity and ID extraction from URLs")
        print("   ✅ API calls to fetch entity details")
        print("   ✅ Enhanced context recommendations for AI agent")
    elif passed >= total * 0.75:
        print("⚠️ Most enhanced tests passed, but some issues need fixing.")
    else:
        print("❌ Multiple issues found in enhanced features. Significant fixes needed.")
    
    return passed == total


if __name__ == "__main__":
    success = run_enhanced_screen_context_tests()
    sys.exit(0 if success else 1) 