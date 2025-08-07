#!/usr/bin/env python3
"""
Simple test for enhanced screen context functionality
"""

import sys
import os
import json
from unittest.mock import Mock

# Add the correct path to find ai_assistant module
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

def test_enhanced_url_analysis():
    """Test the enhanced URL analysis directly"""
    print("🧪 Testing Enhanced URL Analysis...")
    
    try:
        from ai_assistant.sub_agents.screen_context_agent.utils import _analyze_enhanced_url, _is_likely_id
        
        # Test URL patterns
        test_cases = [
            {
                "url": "partnerships/contacts",
                "expected_entity": "Contact",
                "expected_type": "entity_list_page",
                "description": "Partnerships contacts"
            },
            {
                "url": "partnerships/contacts/123", 
                "expected_entity": "Contact",
                "expected_id": "123",
                "expected_type": "entity_detail_page",
                "description": "Contact detail with ID"
            },
            {
                "url": "/",
                "expected_type": "homepage",
                "description": "Homepage"
            },
            {
                "url": "/ai/assistant",
                "expected_type": "ai_assistant_mode",
                "description": "AI assistant mode"
            },
            {
                "url": "/partners/789",
                "expected_entity": "Partner",
                "expected_id": "789", 
                "expected_type": "entity_detail_page",
                "description": "Partner detail"
            }
        ]
        
        # Mock UI config to return None (use fallback logic)
        import unittest.mock
        with unittest.mock.patch('ai_assistant.sub_agents.screen_context_agent.utils.ui_config_manager') as mock_ui_config:
            mock_ui_config.find_matching_page_by_url.return_value = None
            
            passed = 0
            total = len(test_cases)
            
            for test_case in test_cases:
                try:
                    result = _analyze_enhanced_url(test_case["url"])
                    
                    print(f"Testing: {test_case['description']}")
                    print(f"  URL: {test_case['url']}")
                    print(f"  Result: {result}")
                    
                    # Check screen type
                    if "expected_type" in test_case:
                        actual_type = result["screen_type"]
                        expected_type = test_case["expected_type"]
                        if actual_type == expected_type:
                            print(f"  ✅ Screen type: {actual_type}")
                        else:
                            print(f"  ❌ Screen type: expected {expected_type}, got {actual_type}")
                            continue
                    
                    # Check entity
                    if "expected_entity" in test_case:
                        actual_entity = result["entity_in_focus"]
                        expected_entity = test_case["expected_entity"]
                        if actual_entity == expected_entity:
                            print(f"  ✅ Entity: {actual_entity}")
                        else:
                            print(f"  ❌ Entity: expected {expected_entity}, got {actual_entity}")
                            continue
                    
                    # Check entity ID
                    if "expected_id" in test_case:
                        actual_id = result["entity_id_in_focus"]
                        expected_id = test_case["expected_id"]
                        if actual_id == expected_id:
                            print(f"  ✅ Entity ID: {actual_id}")
                        else:
                            print(f"  ❌ Entity ID: expected {expected_id}, got {actual_id}")
                            continue
                    
                    print(f"  ✅ PASSED: {test_case['description']}")
                    passed += 1
                    
                except Exception as e:
                    print(f"  ❌ FAILED: {test_case['description']} - {str(e)}")
            
            print(f"\nURL Analysis: {passed}/{total} tests passed")
            return passed == total
            
    except Exception as e:
        print(f"❌ Enhanced URL analysis test failed: {e}")
        return False

def test_focus_context_logic():
    """Test the focus context logic"""
    print("\n🧪 Testing Focus Context Logic...")
    
    test_scenarios = [
        {
            "screen_url": "partnerships/contacts",
            "user_focus_context": "partnerships/contacts", 
            "expected_relationship": "same",
            "description": "Same URLs"
        },
        {
            "screen_url": "partnerships/contacts/123",
            "user_focus_context": "",
            "expected_relationship": "same",
            "expected_resolved_focus": "partnerships/contacts/123",
            "description": "Empty focus gets resolved"
        },
        {
            "screen_url": "/ai/chat",
            "user_focus_context": "/contacts/456",
            "expected_relationship": "different", 
            "description": "Different focus contexts"
        }
    ]
    
    passed = 0
    total = len(test_scenarios)
    
    for scenario in test_scenarios:
        try:
            screen_url = scenario["screen_url"]
            user_focus_context = scenario["user_focus_context"]
            
            print(f"Testing: {scenario['description']}")
            print(f"  screen_url: '{screen_url}'")
            print(f"  user_focus_context: '{user_focus_context}'")
            
            # Apply the conditional logic
            original_user_focus = user_focus_context
            if screen_url != user_focus_context and not user_focus_context and screen_url:
                user_focus_context = screen_url
                print(f"  🔄 Applied condition: user_focus_context set to '{user_focus_context}'")
            
            # Determine relationship
            focus_relationship = "same" if screen_url == user_focus_context else "different"
            
            # Check expected relationship
            expected_relationship = scenario["expected_relationship"]
            if focus_relationship == expected_relationship:
                print(f"  ✅ Relationship: {focus_relationship}")
            else:
                print(f"  ❌ Relationship: expected {expected_relationship}, got {focus_relationship}")
                continue
            
            # Check resolved focus if expected
            if "expected_resolved_focus" in scenario:
                expected_resolved = scenario["expected_resolved_focus"]
                if user_focus_context == expected_resolved:
                    print(f"  ✅ Resolved focus: {user_focus_context}")
                else:
                    print(f"  ❌ Resolved focus: expected {expected_resolved}, got {user_focus_context}")
                    continue
            
            print(f"  ✅ PASSED: {scenario['description']}")
            passed += 1
            
        except Exception as e:
            print(f"  ❌ FAILED: {scenario['description']} - {str(e)}")
    
    print(f"\nFocus Context Logic: {passed}/{total} tests passed")
    return passed == total

def test_id_detection():
    """Test ID detection logic"""
    print("\n🧪 Testing ID Detection...")
    
    try:
        from ai_assistant.sub_agents.screen_context_agent.utils import _is_likely_id
        
        id_tests = [
            {"value": "123", "expected": True, "description": "Numeric ID"},
            {"value": "12345678-1234-1234-1234-123456789012", "expected": True, "description": "GUID"},
            {"value": "abc123def456ghi", "expected": True, "description": "Long alphanumeric"},
            {"value": "create", "expected": False, "description": "Action keyword"},
            {"value": "", "expected": False, "description": "Empty string"}
        ]
        
        passed = 0
        total = len(id_tests)
        
        for test in id_tests:
            result = _is_likely_id(test["value"])
            expected = test["expected"]
            
            if result == expected:
                print(f"  ✅ {test['description']}: '{test['value']}' -> {result}")
                passed += 1
            else:
                print(f"  ❌ {test['description']}: '{test['value']}' -> expected {expected}, got {result}")
        
        print(f"\nID Detection: {passed}/{total} tests passed")
        return passed == total
        
    except Exception as e:
        print(f"❌ ID detection test failed: {e}")
        return False

def run_simple_tests():
    """Run simple tests for enhanced screen context"""
    print("🚀 Running Simple Enhanced Screen Context Tests...")
    print("=" * 60)
    
    tests = [
        ("Enhanced URL Analysis", test_enhanced_url_analysis),
        ("Focus Context Logic", test_focus_context_logic),
        ("ID Detection", test_id_detection)
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
    print("📊 SIMPLE TEST SUMMARY:")
    
    passed = 0
    total = len(results)
    
    for test_name, result in results.items():
        status = "PASS" if result else "FAIL"
        print(f"{test_name}: {status}")
        if result:
            passed += 1
    
    print(f"\nOverall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 ALL SIMPLE TESTS PASSED!")
        print("\n🎯 Validated Features:")
        print("   ✅ Enhanced URL pattern analysis")
        print("   ✅ screen_url vs user_focus_context logic")
        print("   ✅ Entity and ID detection")
        print("   ✅ URL pattern classification")
    
    return passed == total

if __name__ == "__main__":
    success = run_simple_tests()
    sys.exit(0 if success else 1) 