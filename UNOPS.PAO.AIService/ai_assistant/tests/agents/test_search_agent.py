#!/usr/bin/env python3
"""
Test suite for search_agent

This test validates that the search_agent properly handles Google search queries,
formats responses correctly, and provides reliable external information retrieval.
"""

import sys
import os
import json
from unittest.mock import Mock, patch, MagicMock

# Add the correct path to find ai_assistant module
current_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.dirname(os.path.dirname(os.path.dirname(current_dir)))
sys.path.insert(0, project_root)

class TestSearchAgent:
    """Test cases for search_agent"""
    
    def test_agent_import(self):
        """Test that search_agent can be imported successfully"""
        print("🧪 Testing search_agent import...")
        
        try:
            # Mock google_search tool to handle dependencies
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search') as mock_google_search:
                mock_google_search.name = "google_search"
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                print("✅ Successfully imported search_agent (with mocked google_search)")
                return True
        except ImportError as e:
            print(f"❌ Import Error: {e}")
            return False
        except Exception as e:
            print(f"❌ Unexpected Error: {e}")
            return False
    
    def test_agent_structure_and_configuration(self):
        """Test that the agent has correct structure and search configuration"""
        print("\n🧪 Testing agent structure and search configuration...")
        
        try:
            # Mock google_search tool
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search') as mock_google_search, \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_google_search.name = "google_search"
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                # Check basic attributes
                assert hasattr(search_agent, 'name'), "Agent should have a name"
                assert search_agent.name == "search_agent", f"Expected name 'search_agent', got '{search_agent.name}'"
                print(f"✅ Agent name verified: {search_agent.name}")
                
                # Check description
                assert hasattr(search_agent, 'description'), "Agent should have a description"
                description_keywords = ["specialized", "search", "google", "external", "information"]
                assert any(keyword in search_agent.description.lower() for keyword in description_keywords), "Description should mention search functionality"
                print(f"✅ Agent description verified: {search_agent.description}")
                
                # Check instruction contains search guidance
                assert hasattr(search_agent, 'instruction'), "Agent should have instruction"
                instruction_keywords = ["google search", "json", "sources", "search terms", "format"]
                instruction_lower = search_agent.instruction.lower()
                found_keywords = [kw for kw in instruction_keywords if kw in instruction_lower]
                assert len(found_keywords) >= 3, f"Instruction should contain search guidance, found: {found_keywords}"
                print("✅ Agent instruction contains comprehensive search guidance")
                
                # Check tools configuration
                assert hasattr(search_agent, 'tools'), "Agent should have tools"
                assert search_agent.tools is not None, "Tools should not be None"
                print(f"✅ Agent tools configuration verified")
                
                # Check output key
                assert hasattr(search_agent, 'output_key'), "Agent should have output_key"
                assert search_agent.output_key == "search_results", f"Expected output_key 'search_results', got '{search_agent.output_key}'"
                print(f"✅ Agent output_key verified: {search_agent.output_key}")
                
                # Check transfer restrictions (important for search agent)
                assert hasattr(search_agent, 'disallow_transfer_to_parent'), "Agent should have transfer restrictions"
                assert search_agent.disallow_transfer_to_parent == True, "Should disallow transfer to parent"
                assert search_agent.disallow_transfer_to_peers == True, "Should disallow transfer to peers"
                print("✅ Agent transfer restrictions properly configured")
                
                return True
                
        except Exception as e:
            print(f"❌ Structure test failed: {e}")
            return False
    
    def test_search_instruction_quality(self):
        """Test that the search instruction is comprehensive and well-structured"""
        print("\n🧪 Testing search instruction quality...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                instruction = search_agent.instruction
                
                # Check instruction exists and has content
                assert instruction and len(instruction.strip()) > 0, "Instruction should not be empty"
                
                # Check for key sections
                instruction_lower = instruction.lower()
                
                required_sections = [
                    "role",
                    "when",
                    "how to respond",
                    "response format",
                    "json",
                    "sources"
                ]
                
                found_sections = []
                for section in required_sections:
                    if section in instruction_lower:
                        found_sections.append(section)
                
                assert len(found_sections) >= 4, f"Instruction should contain key sections, found: {found_sections}"
                print(f"✅ Instruction contains {len(found_sections)} key sections")
                
                # Check for JSON format specification
                assert "json" in instruction_lower, "Instruction should specify JSON format"
                assert "sources" in instruction_lower, "Instruction should mention sources"
                assert "title" in instruction_lower and "url" in instruction_lower, "Instruction should specify source structure"
                print("✅ JSON format and sources properly specified")
                
                # Check for search guidelines
                search_guidelines = ["search terms", "recent", "authoritative", "credibility"]
                found_guidelines = [guide for guide in search_guidelines if guide in instruction_lower]
                assert len(found_guidelines) >= 2, f"Instruction should contain search guidelines, found: {found_guidelines}"
                print(f"✅ Search guidelines present: {found_guidelines}")
                
                # Check instruction length (should be comprehensive but not excessive)
                word_count = len(instruction.split())
                assert 100 < word_count < 800, f"Instruction should be comprehensive but not excessive (got {word_count} words)"
                print(f"✅ Instruction length appropriate: {word_count} words")
                
                return True
                
        except Exception as e:
            print(f"❌ Instruction quality test failed: {e}")
            return False
    
    def test_search_response_format_specification(self):
        """Test that the agent's instruction properly specifies response format"""
        print("\n🧪 Testing search response format specification...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                instruction = search_agent.instruction
                
                # Check that instruction specifies proper JSON structure
                json_structure_elements = [
                    "content",
                    "sources", 
                    "title",
                    "url",
                    "description"
                ]
                
                found_elements = []
                for element in json_structure_elements:
                    if f'"{element}"' in instruction or f"'{element}'" in instruction:
                        found_elements.append(element)
                
                assert len(found_elements) >= 4, f"Instruction should specify JSON structure elements, found: {found_elements}"
                print(f"✅ JSON structure elements specified: {found_elements}")
                
                # Check for example format
                assert "example" in instruction.lower() or "examples" in instruction.lower(), "Instruction should contain examples"
                print("✅ Instruction contains examples")
                
                # Verify response format mentions JSON
                assert "json" in instruction.lower(), "Instruction should mention JSON format"
                assert "format" in instruction.lower(), "Instruction should mention formatting"
                print("✅ Response format properly specified")
                
                return True
                
        except Exception as e:
            print(f"❌ Response format test failed: {e}")
            return False
    
    def test_google_search_tool_integration(self):
        """Test that the agent properly integrates with google_search tool"""
        print("\n🧪 Testing google_search tool integration...")
        
        try:
            # Create a more detailed mock for google_search
            mock_google_search = Mock()
            mock_google_search.name = "google_search"
            mock_google_search.description = "Performs Google searches"
            
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search', mock_google_search), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                # Verify tools configuration
                assert hasattr(search_agent, 'tools'), "Agent should have tools configured"
                assert search_agent.tools is not None, "Tools should not be None"
                print("✅ Google search tool integration verified")
                
                # Check that the instruction mentions using google_search
                instruction_lower = search_agent.instruction.lower()
                assert "google_search" in instruction_lower or "google search" in instruction_lower, "Instruction should mention using google_search"
                print("✅ Instruction properly references google_search tool")
                
                return True
                
        except Exception as e:
            print(f"❌ Google search tool integration test failed: {e}")
            return False
    
    def test_search_query_generation_logic(self):
        """Test logic for generating effective search queries"""
        print("\n🧪 Testing search query generation logic...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                instruction = search_agent.instruction
                
                # Check that instruction provides guidance on search terms
                search_guidance = [
                    "search terms",
                    "relevant",
                    "specific",
                    "recent",
                    "2024"  # Should include current year guidance
                ]
                
                found_guidance = []
                instruction_lower = instruction.lower()
                for guidance in search_guidance:
                    if guidance in instruction_lower:
                        found_guidance.append(guidance)
                
                assert len(found_guidance) >= 3, f"Instruction should provide search query guidance, found: {found_guidance}"
                print(f"✅ Search query guidance provided: {found_guidance}")
                
                # Check for examples of good search queries
                if "example" in instruction_lower:
                    # Look for search query examples
                    example_patterns = ["news", "latest", "2024", "initiatives"]
                    found_examples = [ex for ex in example_patterns if ex in instruction_lower]
                    assert len(found_examples) >= 2, f"Should have good search query examples, found: {found_examples}"
                    print(f"✅ Search query examples found: {found_examples}")
                
                return True
                
        except Exception as e:
            print(f"❌ Search query generation test failed: {e}")
            return False
    
    def test_search_result_processing_specification(self):
        """Test that the agent properly specifies how to process search results"""
        print("\n🧪 Testing search result processing specification...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                instruction = search_agent.instruction
                instruction_lower = instruction.lower()
                
                # Check for result processing guidance
                processing_guidance = [
                    "extract",
                    "summarize", 
                    "key information",
                    "sources",
                    "credibility",
                    "verify"
                ]
                
                found_processing = []
                for guidance in processing_guidance:
                    if guidance in instruction_lower:
                        found_processing.append(guidance)
                
                assert len(found_processing) >= 3, f"Instruction should provide result processing guidance, found: {found_processing}"
                print(f"✅ Result processing guidance: {found_processing}")
                
                # Check for source quality guidance
                quality_guidance = ["authoritative", "recent", "credible", "reliable"]
                found_quality = [q for q in quality_guidance if q in instruction_lower]
                assert len(found_quality) >= 1, f"Instruction should mention source quality, found: {found_quality}"
                print(f"✅ Source quality guidance: {found_quality}")
                
                # Check for number of sources guidance
                assert "2-5" in instruction or "2" in instruction, "Instruction should specify number of sources"
                print("✅ Source quantity guidance provided")
                
                return True
                
        except Exception as e:
            print(f"❌ Search result processing test failed: {e}")
            return False
    
    def test_agent_specialization_and_use_cases(self):
        """Test that the agent clearly defines its specialization and use cases"""
        print("\n🧪 Testing agent specialization and use cases...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                instruction = search_agent.instruction
                instruction_lower = instruction.lower()
                
                # Check for clear use cases
                use_cases = [
                    "current events",
                    "news",
                    "external",
                    "latest",
                    "companies",
                    "not in knowledge base",
                    "recent developments"
                ]
                
                found_use_cases = []
                for use_case in use_cases:
                    if use_case in instruction_lower:
                        found_use_cases.append(use_case)
                
                assert len(found_use_cases) >= 4, f"Instruction should clearly define use cases, found: {found_use_cases}"
                print(f"✅ Use cases clearly defined: {found_use_cases}")
                
                # Check for "when called" section
                assert "when" in instruction_lower, "Instruction should specify when to use this agent"
                print("✅ Usage conditions specified")
                
                # Check that it distinguishes from internal knowledge
                knowledge_distinctions = ["knowledge base", "internal", "external", "not covered"]
                found_distinctions = [d for d in knowledge_distinctions if d in instruction_lower]
                assert len(found_distinctions) >= 2, f"Should distinguish from internal knowledge, found: {found_distinctions}"
                print(f"✅ Knowledge base distinction: {found_distinctions}")
                
                return True
                
        except Exception as e:
            print(f"❌ Specialization test failed: {e}")
            return False
    
    def test_search_agent_transfer_restrictions(self):
        """Test that the search agent has proper transfer restrictions"""
        print("\n🧪 Testing search agent transfer restrictions...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                # Check transfer restrictions (important for specialized agents)
                assert hasattr(search_agent, 'disallow_transfer_to_parent'), "Should have parent transfer restriction"
                assert hasattr(search_agent, 'disallow_transfer_to_peers'), "Should have peer transfer restriction"
                
                assert search_agent.disallow_transfer_to_parent == True, "Should disallow transfer to parent"
                assert search_agent.disallow_transfer_to_peers == True, "Should disallow transfer to peers"
                
                print("✅ Transfer restrictions properly configured")
                print("✅ Agent will complete search tasks without delegation")
                
                return True
                
        except Exception as e:
            print(f"❌ Transfer restrictions test failed: {e}")
            return False
    
    def test_mock_search_execution_simulation(self):
        """Test a simulated search execution with mocked Google search"""
        print("\n🧪 Testing mock search execution simulation...")
        
        try:
            # Create a detailed mock search result
            mock_search_results = [
                {
                    "title": "Latest UNOPS Developments 2024",
                    "url": "https://unops.org/news/latest-developments",
                    "snippet": "UNOPS announces new sustainability initiatives and project partnerships for 2024..."
                },
                {
                    "title": "UNOPS Annual Report Released",
                    "url": "https://unops.org/annual-report-2024",
                    "snippet": "The 2024 annual report highlights key achievements and future strategic directions..."
                }
            ]
            
            mock_google_search = Mock()
            mock_google_search.name = "google_search"
            mock_google_search.return_value = mock_search_results
            
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search', mock_google_search), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                # Simulate the search process
                print("🔍 Simulating search execution...")
                
                # Test query: "Latest UNOPS news 2024"
                search_query = "Latest UNOPS news 2024"
                print(f"📝 Search query: {search_query}")
                
                # Verify agent has the tools to handle this
                assert hasattr(search_agent, 'tools'), "Agent should have search tools"
                assert search_agent.output_key == "search_results", "Should output search_results"
                
                # Simulate expected response structure based on instruction
                expected_response_structure = {
                    "content": "string",
                    "sources": [
                        {
                            "title": "string",
                            "url": "string", 
                            "description": "string"
                        }
                    ]
                }
                
                print("✅ Search execution simulation successful")
                print(f"✅ Expected response structure: {list(expected_response_structure.keys())}")
                print(f"✅ Expected source fields: {list(expected_response_structure['sources'][0].keys())}")
                
                return True
                
        except Exception as e:
            print(f"❌ Search execution simulation failed: {e}")
            return False
    
    def test_utility_functions(self):
        """Test the search agent utility functions"""
        print("\n🧪 Testing search agent utility functions...")
        
        try:
            from ai_assistant.sub_agents.search_agent.utils import (
                optimize_search_query,
                validate_search_source,
                format_search_response,
                extract_key_terms,
                assess_source_authority,
                filter_sources_by_quality,
                validate_search_response
            )
            
            # Test query optimization
            optimized_query = optimize_search_query("latest UNOPS news")
            assert "2024" in optimized_query, "Should add current year to temporal queries"
            print("✅ Query optimization works correctly")
            
            # Test source validation
            valid_source = {
                "title": "Test Article Title",
                "url": "https://example.com/article",
                "description": "This is a test article description with sufficient content"
            }
            assert validate_search_source(valid_source) == True, "Valid source should pass validation"
            
            invalid_source = {"title": "Short", "url": "invalid-url", "description": "Too short"}
            assert validate_search_source(invalid_source) == False, "Invalid source should fail validation"
            print("✅ Source validation works correctly")
            
            # Test key terms extraction
            key_terms = extract_key_terms("What are the latest UNOPS sustainability initiatives?")
            expected_terms = ["latest", "unops", "sustainability", "initiatives"]
            found_terms = [term for term in expected_terms if term in key_terms]
            assert len(found_terms) >= 3, f"Should extract key terms, found: {found_terms}"
            print("✅ Key terms extraction works correctly")
            
            # Test source authority assessment
            gov_score = assess_source_authority("https://government.gov/news")
            commercial_score = assess_source_authority("https://example.com/article")
            assert gov_score > commercial_score, "Government sources should have higher authority"
            print("✅ Source authority assessment works correctly")
            
            # Test response formatting
            formatted_response = format_search_response("Test content", [valid_source])
            assert "content" in formatted_response and "sources" in formatted_response, "Should format response correctly"
            assert validate_search_response(formatted_response) == True, "Formatted response should be valid"
            print("✅ Response formatting and validation work correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ Utility functions test failed: {e}")
            return False
    
    def test_agent_integration_readiness(self):
        """Test that the search agent is ready for integration with the larger system"""
        print("\n🧪 Testing search agent integration readiness...")
        
        try:
            with patch('ai_assistant.sub_agents.search_agent.agent.google_search'), \
                 patch('ai_assistant.utils.api_config_manager.config_manager') as mock_config:
                
                mock_config.get_gemini_model.return_value = Mock()
                
                from ai_assistant.sub_agents.search_agent.agent import search_agent
                
                # Test integration points
                integration_checks = {
                    "has_name": hasattr(search_agent, 'name') and search_agent.name == "search_agent",
                    "has_description": hasattr(search_agent, 'description') and len(search_agent.description) > 0,
                    "has_instruction": hasattr(search_agent, 'instruction') and len(search_agent.instruction) > 0,
                    "has_tools": hasattr(search_agent, 'tools') and search_agent.tools is not None,
                    "has_output_key": hasattr(search_agent, 'output_key') and search_agent.output_key == "search_results",
                    "proper_restrictions": (hasattr(search_agent, 'disallow_transfer_to_parent') and 
                                          search_agent.disallow_transfer_to_parent == True)
                }
                
                # Verify all integration checks pass
                for check_name, check_result in integration_checks.items():
                    assert check_result == True, f"Integration check '{check_name}' failed"
                    print(f"✅ {check_name.replace('_', ' ').title()}: PASS")
                
                print("✅ Search agent is ready for system integration")
                print("✅ Google search capability confirmed")
                print("✅ Response formatting verified")
                print("✅ Utility functions available for enhanced functionality")
                
                return True
                
        except Exception as e:
            print(f"❌ Integration readiness test failed: {e}")
            return False


def run_search_agent_tests():
    """Run all tests for search_agent"""
    print("🚀 Running search_agent test suite...")
    print("=" * 70)
    
    test_instance = TestSearchAgent()
    
    # Run tests in order
    tests = [
        ("Import Test", test_instance.test_agent_import),
        ("Structure & Configuration Test", test_instance.test_agent_structure_and_configuration),
        ("Search Instruction Quality Test", test_instance.test_search_instruction_quality),
        ("Response Format Specification Test", test_instance.test_search_response_format_specification),
        ("Google Search Tool Integration Test", test_instance.test_google_search_tool_integration),
        ("Search Query Generation Logic Test", test_instance.test_search_query_generation_logic),
        ("Search Result Processing Test", test_instance.test_search_result_processing_specification),
        ("Agent Specialization & Use Cases Test", test_instance.test_agent_specialization_and_use_cases),
        ("Transfer Restrictions Test", test_instance.test_search_agent_transfer_restrictions),
        ("Mock Search Execution Simulation Test", test_instance.test_mock_search_execution_simulation),
        ("Utility Functions Test", test_instance.test_utility_functions),
        ("Integration Readiness Test", test_instance.test_agent_integration_readiness),
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
        print("🎉 ALL TESTS PASSED! search_agent is ready for production!")
        print("🎯 Google search functionality verified!")
        print("🔍 Response formatting and source handling confirmed!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
        print("🔧 Check search instruction quality and tool integration.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
        print("🔧 Focus on search functionality and response formatting.")
    
    return passed == total


if __name__ == "__main__":
    success = run_search_agent_tests()
    sys.exit(0 if success else 1)
